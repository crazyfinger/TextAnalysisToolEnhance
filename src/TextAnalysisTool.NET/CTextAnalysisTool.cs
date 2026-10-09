using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ToolBox;

namespace TextAnalysisTool.NET;

internal sealed class CTextAnalysisTool : IDisposable
{
	private enum LoadSource
	{
		None,
		File,
		Clipboard,
		DragDropText,
		ResourceStream
	}

	private enum FiltersFileOperations
	{
		Load = 1,
		Append,
		Save
	}

	private LineCollection m_lineCollection;

	private readonly LineCollectionDisplay m_lineCollectionDisplay;

	private readonly FilterCollection m_filterCollection;

	private readonly FilterCollectionDisplay m_filterCollectionDisplay;

	private readonly MainWindow m_mainWindow;

	private LoadSource m_loadSource;

	private string m_lineSourceLong;

	private string m_lineSourceShort;

	private string m_currentFileName;

	private string m_filterFileName;

	private Filter m_findFilter;

	private readonly ArrayList m_plugins;

	public const int MaxPathShownInError = 50;

	public const int MaxPathShownInMenus = 70;

	private bool m_disposed;

	public FilterCollection FilterCollection => m_filterCollection;

	public string FilterFileName
	{
		get
		{
			return m_filterFileName;
		}
		private set
		{
			m_filterFileName = value;
			SetMainWindowTitle();
		}
	}

	public IList Plugins => ArrayList.ReadOnly(m_plugins);

	public bool ReloadLinesPossible => m_currentFileName != null;

	public bool FindNextMatchPossible => m_findFilter != null;

	public bool LinesLoaded => m_lineCollection.Count != 0;

	public bool LinesVisible => m_lineCollectionDisplay.ListBox.LinesVisible;

	public int NumberLinesLoaded => m_lineCollection.Count;

	public int NumberLinesSelected => m_lineCollectionDisplay.ListBox.NumberLinesSelected;

	public int NumberLinesFiltered => m_filterCollection.FilteredLines.Count;

	public bool FiltersPresent => m_filterCollection.Filters.Count != 0;

	public static string ReleaseVersionString
	{
		get
		{
			int build = Assembly.GetExecutingAssembly().GetName().Version.Build;
			return new DateTime(2000, 1, 1).AddDays(build).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}
	}

	public bool CurrentEncodingIsSystemDefault { get; private set; }

	public Encoding CurrentEncoding { get; set; }

	public CTextAnalysisTool()
	{
		m_lineCollection = new LineCollection();
		m_lineCollectionDisplay = new LineCollectionDisplay(this);
		m_filterCollection = new FilterCollection(this);
		m_filterCollection.DirtyStatusChanged += filterCollection_DirtyStatusChanged;
		m_filterCollectionDisplay = new FilterCollectionDisplay(this, m_filterCollection, m_lineCollectionDisplay);
		m_mainWindow = new MainWindow(this, m_lineCollectionDisplay, m_filterCollectionDisplay);
		m_loadSource = LoadSource.None;
		m_lineSourceLong = "[No file loaded]";
		m_lineSourceShort = "[No file loaded]";
		UpdateUIFonts(Settings.FontSize, Settings.ZoomPercent);
		SetCurrentEncoding(Settings.DefaultEncodingIsSystemDefault ? null : Settings.DefaultEncodingName);
		m_plugins = new ArrayList();
		SetMainWindowTitle();
	}

	private void filterCollection_DirtyStatusChanged(object sender, DirtyStatusChangedEventArgs e)
	{
		SetMainWindowTitle();
	}

	private string GetDisplayedFileName(string fileFullPath, string fileName)
	{
		string text = fileName;
		if (Settings.FileNameDisplayStyle == FileNameDisplayStyle.FullPath)
		{
			text = fileFullPath;
		}
		else if (Settings.FileNameDisplayStyle == FileNameDisplayStyle.RetainPathEnd)
		{
			int num = Math.Min(fileFullPath.Length, Settings.FileNameDisplayLength);
			text = fileFullPath.Substring(fileFullPath.Length - num);
			if (num < fileFullPath.Length)
			{
				text = "..." + text;
			}
		}
		else if (Settings.FileNameDisplayStyle == FileNameDisplayStyle.RetainPathRootAndEnd)
		{
			text = fileFullPath.ShortenPath(Settings.FileNameDisplayLength);
		}
		return text;
	}

	private void LoadLines(LoadSource source, string data)
	{
		m_currentFileName = null;
		string message = "An unexpected error occurred: {0}.";
		Cursor cursor = m_mainWindow.Cursor;
		try
		{
			m_mainWindow.Cursor = Cursors.WaitCursor;
			switch (source)
			{
			case LoadSource.File:
			{
				message = "An error occurred while attempting to read the file: {0}.";
				FileInfo fileInfo = new FileInfo(data);
				string fullName = fileInfo.FullName;
				m_loadSource = LoadSource.File;
				m_lineSourceLong = fullName;
				m_lineSourceShort = fileInfo.Name;
				TextReader textReader = null;
				foreach (PluginWrapper plugin in m_plugins)
				{
					message = string.Format("An error occurred while attempting to read the file (via plug-in {0}): {1}.", plugin.Name, "{0}");
					if (plugin.IsFileTypeSupported(fullName))
					{
						textReader = plugin.GetReaderForFile(fullName);
						if (textReader != null)
						{
							break;
						}
					}
				}
				if (textReader == null)
				{
					message = "An error occurred while attempting to read the file: {0}.";
					textReader = new StreamReader(fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite), CurrentEncoding, detectEncodingFromByteOrderMarks: true);
				}
				using (textReader)
				{
					m_lineCollection = new LineCollection(textReader);
				}
				m_currentFileName = fullName;
				break;
			}
			case LoadSource.Clipboard:
				message = "An error occurred while attempting to exchange data with the clipboard: {0}.";
				m_loadSource = LoadSource.Clipboard;
				m_lineSourceLong = "Clipboard";
				m_lineSourceShort = "Clipboard";
				try
				{
					using TextReader reader3 = new StringReader((string)Clipboard.GetDataObject().GetData(DataFormats.UnicodeText));
					SetCurrentEncoding("utf-8");
					m_lineCollection = new LineCollection(reader3);
				}
				catch (ArgumentNullException)
				{
					throw new Exception("The data on the clipboard is in an unsupported format.");
				}
				break;
			case LoadSource.DragDropText:
			{
				message = "An error occurred while attempting to access the dropped data: {0}.";
				m_loadSource = LoadSource.DragDropText;
				m_lineSourceLong = "Drag-and-dropped text";
				m_lineSourceShort = "Drag-and-dropped text";
				using (TextReader reader2 = new StringReader(data))
				{
					SetCurrentEncoding("utf-8");
					m_lineCollection = new LineCollection(reader2);
				}
				break;
			}
			case LoadSource.ResourceStream:
			{
				m_loadSource = LoadSource.ResourceStream;
				m_lineSourceLong = data;
				m_lineSourceShort = data;
				using (TextReader reader = new StreamReader(Resource.GetStream(data)))
				{
					SetCurrentEncoding("utf-8");
					m_lineCollection = new LineCollection(reader);
				}
				break;
			}
			}
			m_mainWindow.StatusMessage = "Source: " + m_lineSourceLong;
		}
		catch (Exception e)
		{
			ShowMessageBox(message, e, MessageBoxIcon.Hand);
			m_lineCollection = new LineCollection();
			m_mainWindow.StatusMessage = "[No file loaded]";
			m_loadSource = LoadSource.None;
		}
		finally
		{
			m_mainWindow.Cursor = cursor;
		}
		SetMainWindowTitle();
		LinesChanged();
		if (m_mainWindow != null && !m_mainWindow.IsDisposed)
		{
			m_lineCollectionDisplay.ListBox.Focus();
		}
	}

	public void LoadLinesFromFile(string fileName)
	{
		LoadLines(LoadSource.File, fileName);
	}

	public void LoadLinesFromClipboard()
	{
		LoadLines(LoadSource.Clipboard, null);
	}

	public void LoadLinesFromDragDropText(string lines)
	{
		LoadLines(LoadSource.DragDropText, lines);
	}

	public void LoadLinesFromResourceStream(string streamName)
	{
		LoadLines(LoadSource.ResourceStream, streamName);
	}

	public void ReloadLines()
	{
		m_lineCollectionDisplay.ListBox.Reload(m_currentFileName);
	}

	private void GetLoadSourceNames(out string shortName, out string longName)
	{
		shortName = m_lineSourceShort;
		longName = m_lineSourceLong;
		if (m_loadSource == LoadSource.None)
		{
			shortName = "[No file loaded]";
			longName = "[No file loaded]";
		}
		else if (m_loadSource == LoadSource.File)
		{
			shortName = GetDisplayedFileName(m_lineSourceLong, m_lineSourceShort);
			longName = m_lineSourceLong;
		}
	}

	public void SetMainWindowTitle()
	{
		string text = null;
		if (!string.IsNullOrEmpty(m_filterFileName))
		{
			try
			{
				text = Path.GetFileName(m_filterFileName);
			}
			catch
			{
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "[No filter file]";
		}
		GetLoadSourceNames(out var shortName, out var _);
		m_mainWindow.Text = string.Format("{0} - {1}{2} - {3}", shortName, text, FilterCollection.IsDirty ? "*" : "", "TextAnalysisTool.NET");
	}

	private void LoadOrSaveFilters(FiltersFileOperations op, string fileName)
	{
		if (FiltersFileOperations.Save == op)
		{
			try
			{
				using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8))
				{
					using XmlTextWriter xmlTextWriter = new XmlTextWriter(w);
					xmlTextWriter.Formatting = Formatting.Indented;
					xmlTextWriter.WriteStartDocument(standalone: true);
					xmlTextWriter.WriteStartElement("TextAnalysisTool.NET");
					xmlTextWriter.WriteAttributeString("version", ReleaseVersionString);
					xmlTextWriter.WriteAttributeString("showOnlyFilteredLines", m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines.ToString());
					xmlTextWriter.WriteStartElement("filters");
					foreach (Filter filter2 in m_filterCollection.Filters)
					{
						filter2.ToXml(xmlTextWriter);
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndDocument();
				}
				FilterFileName = fileName;
				FilterCollection.ClearDirtyFlag();
				return;
			}
			catch (Exception e)
			{
				ShowMessageBox("An error occurred while attempting to write the file: {0}.", e, MessageBoxIcon.Hand);
				return;
			}
		}
		try
		{
			ArrayList arrayList = new ArrayList();
			bool flag = false;
			bool showOnlyFilteredLines = false;
			bool flag2 = false;
			using (TextReader input = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				using XmlTextReader reader = new XmlTextReader(input);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(reader);
				Utility.ThrowExceptionIf("TextAnalysisTool.NET" != xmlDocument.DocumentElement.Name, null, typeof(ApplicationException));
				Utility.ThrowExceptionIf(xmlDocument.DocumentElement.Attributes.GetNamedItem("version") == null, null, typeof(ApplicationException));
				XmlNode namedItem = xmlDocument.DocumentElement.Attributes.GetNamedItem("showOnlyFilteredLines");
				if (namedItem != null)
				{
					flag = true;
					showOnlyFilteredLines = bool.Parse(namedItem.Value);
				}
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("filters");
				Utility.ThrowExceptionIf(1 != xmlNodeList.Count, null, typeof(ApplicationException));
				string xpath = string.Format("{0}[@{1} and @{2} and @{3}]", "filter", "type", "enabled", "excluding");
				foreach (XmlNode item in xmlNodeList[0].SelectNodes(xpath))
				{
					Filter filter = Filter.FromXml(item);
					if (filter == null)
					{
						flag2 = true;
					}
					else
					{
						arrayList.Add(filter);
					}
				}
			}
			if (flag2)
			{
				ShowMessageBox("One or more filter types were unrecognized and have not been loaded.", MessageBoxIcon.Asterisk);
			}
			if (op == FiltersFileOperations.Load)
			{
				m_filterCollection.ReplaceAll(arrayList);
				FilterCollection.ClearDirtyFlag();
			}
			else
			{
				m_filterCollection.AddRange(arrayList);
			}
			if (flag)
			{
				m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines = showOnlyFilteredLines;
			}
			FilterFileName = fileName;
		}
		catch (Exception ex)
		{
			ShowMessageBox(details: (!(ex is ApplicationException) && !(ex is XmlException) && !(ex is FormatException) && !(ex is OverflowException) && !(ex is ArgumentOutOfRangeException)) ? ex.Message : string.Format("The file is not a valid {0} filter file.", "TextAnalysisTool.NET"), message: "An error occurred while attempting to read the file: {0}.", parameter: fileName.ShortenPath(50), icon: MessageBoxIcon.Hand);
			m_filterCollection.RemoveAll();
		}
	}

	public void SaveFiltersToFile(string fileName)
	{
		LoadOrSaveFilters(FiltersFileOperations.Save, fileName);
	}

	public void LoadFiltersFromFile(string fileName, bool replaceFilters = true)
	{
		FiltersFileOperations op = (replaceFilters ? FiltersFileOperations.Load : FiltersFileOperations.Append);
		LoadOrSaveFilters(op, fileName);
	}

	public void SaveCurrentLinesToFile(string fileName)
	{
		Cursor cursor = m_mainWindow.Cursor;
		try
		{
			m_mainWindow.Cursor = Cursors.WaitCursor;
			using TextWriter writer = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
			m_lineCollectionDisplay.ListBox.OutputLinesToStream(writer);
		}
		catch (Exception e)
		{
			ShowMessageBox("An error occurred while attempting to write the file: {0}.", e, MessageBoxIcon.Hand);
		}
		finally
		{
			m_mainWindow.Cursor = cursor;
		}
	}

	public void CopySelectedLinesToClipboard()
	{
		try
		{
			DataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.Html, ConvertToHtmlClipboardFormat(m_lineCollectionDisplay.ListBox.GetSelectedLinesAsString(formatAsHtml: true)));
			dataObject.SetData(DataFormats.UnicodeText, autoConvert: false, m_lineCollectionDisplay.ListBox.GetSelectedLinesAsString(formatAsHtml: false));
			Clipboard.SetDataObject(dataObject, copy: true);
		}
		catch (ExternalException e)
		{
			ShowMessageBox("An error occurred while attempting to exchange data with the clipboard: {0}.", e, MessageBoxIcon.Hand);
		}
	}

	private static MemoryStream ConvertToHtmlClipboardFormat(string s)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		int num = 12;
		string text = "<html>\n<body>\n<!--StartFragment -->\n";
		string text2 = "<!--EndFragment -->\n</body>\n</html>";
		int num2 = "Version:1.0\nStartHTML:{0:d10}\nEndHTML:{1:d10}\nStartFragment:{2:d10}\nEndFragment:{3:d10}\n".Length + num;
		int num3 = num2 + text.Length + bytes.Length + text2.Length;
		int num4 = num2 + text.Length;
		int num5 = num4 + bytes.Length;
		string s2 = $"Version:1.0\nStartHTML:{num2:d10}\nEndHTML:{num3:d10}\nStartFragment:{num4:d10}\nEndFragment:{num5:d10}\n";
		byte[][] array = new byte[4][]
		{
			Encoding.UTF8.GetBytes(s2),
			Encoding.UTF8.GetBytes(text),
			bytes,
			Encoding.UTF8.GetBytes(text2)
		};
		int num6 = 0;
		byte[][] array2 = array;
		foreach (byte[] array3 in array2)
		{
			num6 += array3.Length;
		}
		MemoryStream memoryStream = new MemoryStream(num6);
		array2 = array;
		foreach (byte[] array4 in array2)
		{
			memoryStream.Write(array4, 0, array4.Length);
		}
		return memoryStream;
	}

	public void ShowFindDialog(bool findReverse, Filter filter = null)
	{
		using FindDialog findDialog = new FindDialog(m_lineCollectionDisplay.ListBox.GetSelectedLineText(), findReverse);
		if (filter != null)
		{
			findDialog.ConfigureFromFilter(filter);
		}
		if (DialogResult.OK == findDialog.ShowDialog(m_mainWindow))
		{
			m_findFilter = findDialog.FilterObject;
			m_findFilter.InitializeAccepts();
			FindNextMatch(findDialog.Reverse);
			m_findFilter.UninitializeAccepts();
		}
	}

	public void FindNextMatch(bool reverse)
	{
		m_lineCollectionDisplay.ListBox.FindNextMatch(m_findFilter, reverse);
	}

	private int ArgbToColorDialogInt(Color color)
	{
		return (color.B << 16) | (color.G << 8) | color.R;
	}

	private int[] GetCustomColors(bool backgroundColors)
	{
		List<int> list = new List<int>();
		foreach (Filter filter in m_filterCollection.Filters)
		{
			Color color = (backgroundColors ? filter.BackgroundColor : filter.ForegroundColor);
			if (!color.IsKnownColor)
			{
				int item = ArgbToColorDialogInt(color);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	public void ShowAddFilterDialog()
	{
		using FilterDialog filterDialog = new FilterDialog(m_lineCollectionDisplay.ListBox.GetSelectedLineText(), GetCustomColors);
		if (DialogResult.OK == filterDialog.ShowDialog(m_mainWindow))
		{
			m_filterCollection.Add(filterDialog.FilterObject);
		}
	}

	public void ShowEditFilterDialog(Filter editFilter)
	{
		using FilterDialog filterDialog = new FilterDialog(null, editFilter, GetCustomColors);
		if (DialogResult.OK == filterDialog.ShowDialog(m_mainWindow))
		{
			m_filterCollection.Replace(editFilter, filterDialog.FilterObject);
		}
	}

	public void ShowEditFilterDialog(List<Filter> selectedFilters)
	{
		using FilterDialog filterDialog = new FilterDialog(null, selectedFilters, GetCustomColors);
		if (DialogResult.OK == filterDialog.ShowDialog(m_mainWindow))
		{
			List<Filter> filterList = filterDialog.FilterList;
			m_filterCollection.Replace(selectedFilters, filterList);
		}
	}

	public void ShowAboutDialog()
	{
		using AboutDialog aboutDialog = new AboutDialog(ReleaseVersionString);
		aboutDialog.ShowDialog(m_mainWindow);
	}

	public void UpdateLinesLength()
	{
		m_lineCollection.RefreshLinesLength();
		m_filterCollection.RefreshLinesLength();
	}

	public void LinesChanged()
	{
		FilterAndDisplayLines(updateFilterDisplay: true);
		m_mainWindow.SetStatusTotalLines();
		m_mainWindow.SetStatusSelection();
	}

	public void FiltersChanged(bool updateFilterDisplay)
	{
		FilterAndDisplayLines(updateFilterDisplay);
	}

	public void ReplaceLine(int lineNumber, string newText)
	{
		string singleLine = newText.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
		m_lineCollection.ReplaceLineText(lineNumber, singleLine);
		m_lineCollection.RefreshLinesLength();
		LinesChanged();
	}

	public void OnRefineComputed(int generation, LineCollection refinedLines)
	{
		if (m_mainWindow == null || m_mainWindow.IsDisposed)
		{
			return;
		}
		if (m_mainWindow.InvokeRequired)
		{
			m_mainWindow.BeginInvoke((MethodInvoker)delegate
			{
				ApplyRefineResult(generation, refinedLines);
			});
			return;
		}
		ApplyRefineResult(generation, refinedLines);
	}

	private void ApplyRefineResult(int generation, LineCollection refinedLines)
	{
		if (!m_filterCollection.IsCurrentRefineGeneration(generation))
		{
			return;
		}
		m_filterCollection.SetFilteredLines(refinedLines);
		m_lineCollectionDisplay.ListBox.DisplayLines(m_lineCollection, refinedLines, showOnlyFilteredLinesChanging: false);
		m_mainWindow.SetStatusFilteredLines();
	}

	private void FilterAndDisplayLines(bool updateFilterDisplay)
	{
		Cursor cursor = m_mainWindow.Cursor;
		try
		{
			m_mainWindow.Cursor = Cursors.WaitCursor;
			try
			{
				m_mainWindow.SuspendRedrawForm();
				m_filterCollection.FilterLines(m_lineCollection);
			}
			finally
			{
				m_mainWindow.ResumeRedrawForm();
			}
			if (updateFilterDisplay)
			{
				m_filterCollectionDisplay.UpdateDisplay();
			}
			m_lineCollectionDisplay.ListBox.DisplayLines(m_lineCollection, m_filterCollection.FilteredLines, showOnlyFilteredLinesChanging: false);
			m_mainWindow.SetStatusFilteredLines();
		}
		finally
		{
			m_mainWindow.Cursor = cursor;
		}
	}

	public void Run(string[] args)
	{
		string[] files = Directory.GetFiles(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "TATPlugin_*.dll");
		foreach (string text in files)
		{
			try
			{
				Type[] types = Assembly.LoadFrom(text).GetTypes();
				foreach (Type t in types)
				{
					try
					{
						m_plugins.Add(new PluginWrapper(t));
					}
					catch (Exception)
					{
					}
				}
			}
			catch (Exception e)
			{
				ShowMessageBox($"An error occurred while attempting to load plug-in file {text}: {{0}}.", e, MessageBoxIcon.Hand);
			}
		}
		m_plugins.Sort();
		m_mainWindow.CreatePluginSettingsMenu(m_plugins);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		int result = 0;
		if (args.Length != 0)
		{
			Regex regex = new Regex("^[/-](?<option>\\w+)(:(?<value>.+))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			files = args;
			foreach (string text2 in files)
			{
				Match match = regex.Match(text2);
				if (match.Success)
				{
					string b = match.Result("${option}").ToLower();
					string text3 = match.Result("${value}");
					if (string.Equals("filters", b) && !string.IsNullOrEmpty(text3))
					{
						LoadFiltersFromFile(text3, replaceFilters: false);
						flag2 = true;
					}
					else if (string.Equals("config", b) && !string.IsNullOrEmpty(text3))
					{
						if (!flag3)
						{
							OptionsBehaviorProperties newOptions = new OptionsBehaviorProperties();
							if (Settings.ImportConfiguration(text3, out newOptions))
							{
								m_mainWindow.UpdateSettingsChanged(newOptions);
							}
							flag3 = true;
						}
						else
						{
							ShowMessageBox("A /Config flag has already been supplied on the command line. Ignoring file: {0}.", text3, MessageBoxIcon.Exclamation);
						}
					}
					else if (string.Equals("line", b) && !string.IsNullOrEmpty(text3))
					{
						if (!int.TryParse(text3, out result) || result <= 0)
						{
							ShowMessageBox("The value of the /Line command line argument could not be parsed into a positive integer. Ignoring.", MessageBoxIcon.Exclamation);
						}
					}
					else if (string.Equals("clipboard", b))
					{
						if (!flag)
						{
							LoadLinesFromClipboard();
							flag = true;
						}
					}
					else
					{
						ShowMessageBox("The following command line argument is not recognized and will be ignored: {0}.", text2, MessageBoxIcon.Hand);
					}
				}
				else if (!flag)
				{
					LoadLinesFromFile(text2);
					flag = true;
				}
			}
		}
		if (!flag && !flag2)
		{
			LinesChanged();
		}
		if (flag && result > 0)
		{
			m_lineCollectionDisplay.ListBox.SelectLineNumber(Math.Min(result, m_lineCollection.Count));
		}
		m_filterCollection.ClearDirtyFlag();
		m_mainWindow.UpdateRecentFilesMenus();
		m_mainWindow.UpdateRecentFilterFilesMenus();
		Application.Run(m_mainWindow);
	}

	public static void ShowMessageBox(string message, MessageBoxIcon icon)
	{
		MessageBox.Show(message, "TextAnalysisTool.NET", MessageBoxButtons.OK, icon);
	}

	public static DialogResult ShowMessageBox(string message, MessageBoxIcon icon, MessageBoxButtons buttons)
	{
		return MessageBox.Show(message, "TextAnalysisTool.NET", buttons, icon);
	}

	public static void ShowMessageBox(string message, string details, MessageBoxIcon icon)
	{
		ShowMessageBox(string.Format(message, details), icon);
	}

	public static void ShowMessageBox(string message, string parameter, string details, MessageBoxIcon icon)
	{
		ShowMessageBox(string.Format(message, parameter) + "\n" + details, icon);
	}

	public static void ShowMessageBox(string message, Exception e, MessageBoxIcon icon)
	{
		ShowMessageBox(message, e.Message, icon);
	}

	private static void UnhandledException(Exception e)
	{
		try
		{
			string message = ((!(e is SecurityException)) ? $"Sorry, an unhandled exception has occurred and the application is about to terminate!\n\nPlease report this problem and include the following details:\n\n{Assembly.GetExecutingAssembly().FullName} - {ReleaseVersionString}\n{e.ToString()}" : "Sorry, a system security exception has occurred and the application is about to terminate!\n\nThe most common cause of this problem is attempting to run the application from a network share (by default the .NET Framework does not allow this).  Please copy the executable to your machine and try again.  If the error persists, please report this problem.");
			ShowMessageBox(message, MessageBoxIcon.Hand);
			Application.Exit();
		}
		catch (Exception)
		{
		}
	}

	private static void AddUnhandledExceptionHandler()
	{
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
	}

	private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		UnhandledException(e.ExceptionObject as Exception);
	}

	private static void OnThreadException(object sender, ThreadExceptionEventArgs t)
	{
		UnhandledException(t.Exception);
	}

	public static Font SafeCreateFont(string fontFamily, float size, int zoomPercent = 100, Font defaultFont = null)
	{
		Font font = defaultFont;
		try
		{
			font = new Font(fontFamily, size * ((float)zoomPercent / 100f));
		}
		catch
		{
			if (font == null)
			{
				font = new Font(Settings.DefaultFontFamily, 10f);
			}
		}
		return font;
	}

	public void UpdateUIFonts(float newFontSize, int zoomPercent = 100)
	{
		Font font = new Font(m_filterCollectionDisplay.Font.FontFamily, newFontSize * ((float)zoomPercent / 100f));
		m_filterCollectionDisplay.Font = font;
		m_filterCollectionDisplay.Refresh();
		if (m_mainWindow != null && !m_mainWindow.IsDisposed)
		{
			m_mainWindow.UpdateUIFonts(newFontSize, zoomPercent);
		}
	}

	public void SetCurrentEncoding(string encodingName)
	{
		CurrentEncoding = Encoding.Default;
		CurrentEncodingIsSystemDefault = true;
		try
		{
			if (!string.IsNullOrEmpty(encodingName) && string.Compare(encodingName, "System Default", ignoreCase: true) != 0)
			{
				CurrentEncoding = Encoding.GetEncoding(encodingName);
				CurrentEncodingIsSystemDefault = false;
			}
		}
		catch
		{
		}
		if (ReloadLinesPossible)
		{
			ReloadLines();
		}
	}

	public static Encoding SafeGetEncoding(string encodingName)
	{
		try
		{
			return string.IsNullOrEmpty(encodingName) ? Encoding.Default : Encoding.GetEncoding(encodingName);
		}
		catch
		{
			return Encoding.Default;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (disposing)
			{
				m_lineCollectionDisplay.Dispose();
				m_filterCollectionDisplay.Dispose();
				m_mainWindow.Dispose();
			}
			m_disposed = true;
		}
	}

	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			AddUnhandledExceptionHandler();
			Application.ThreadException += OnThreadException;
			Application.EnableVisualStyles();
			Settings.Load();
			new CTextAnalysisTool().Run(args);
			Settings.Save();
		}
		catch (Exception e)
		{
			UnhandledException(e);
		}
	}
}
