using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using ToolBox;

namespace TextAnalysisTool.NET;

internal static class Settings
{
	private const string registryKey = "Software\\DavidAns\\TextAnalysisTool.NET";

	private const string numberOfRecentFilesString = "NumberOfRecentFiles";

	private const string numberOfRecentFiltersString = "NumberOfRecentFilters";

	private const string recentFileString = "RecentFile";

	private const string recentFilterFileString = "RecentFilterFile";

	private static bool windowPositionValid;

	private static int windowX;

	private static int windowY;

	private static int windowWidth;

	private static int windowHeight;

	private const float NewUserFontSize = 8f;

	public static readonly string DefaultFontFamily;

	public const float MaxFontSize = 50f;

	public const float MinFontSize = 5f;

	public const int DefaultZoomPercent = 100;

	public const int MaxZoomPercent = 400;

	public const int MinZoomPercent = 10;

	public const int ZoomStep = 10;

	public const float DefaultFontSize = 10f;

	public const bool DefaultBeepOnSearchEnd = true;

	public const PasteTextOverwriteEnum DefaultPasteTextOverwrite = PasteTextOverwriteEnum.AlwaysAsk;

	public const bool DefaultShowLineNumbers = true;

	public const bool DefaultShowFilterToolTip = true;

	public const CopyLineNumbersState DefaultCopyLineNumbers = CopyLineNumbersState.WhenVisible;

	public const ShowMarkersState DefaultShowMarkersColumn = ShowMarkersState.WhenInUse;

	public const FiltersDirtiedState DefaultFiltersDirtied = FiltersDirtiedState.Always;

	public const bool DefaultRememberFilterColumnLayout = true;

	public const string DefaultFilterColumnLayout = "";

	public const int DefaultMaxNumberOfRecentItems = 5;

	public const FileNameDisplayStyle DefaultFileNameDisplayStyle = FileNameDisplayStyle.FileNameOnly;

	public const int DefaultFileNameDisplayLength = 0;

	public const int DefaultTabSize = 4;

	public const int DefaultSplitterPositionPercent = 75;

	public const DockStyle DefaultFilterListLocation = DockStyle.Bottom;

	public const string DefaultDefaultEncodingName = "System Default";

	public const int DefaultFindDialogWidth = 0;

	public const int DefaultFilterDialogWidth = 0;

	public const int MaxMaxNumberOfRecentItems = 20;

	public const int MaxTabSize = 100;

	public const int MaxFileNameDisplayLength = int.MaxValue;

	public const int MaxFindDialogWidth = 4000;

	public const int MaxFilterDialogWidth = 4000;

	public const string DefaultWindowTextName = "WindowText";

	public const string DefaultGrayTextName = "GrayText";

	public const string DefaultHighlightTextName = "HighlightText";

	public const string DefaultWindowName = "Window";

	public const string DefaultGrayBackgroundName = "MenuBar";

	public const string DefaultHighlightName = "Highlight";

	public const string DefaultFilterHighlightName = "ControlDark";

	public const string DefaultFilterHighlightBoxName = "ControlDarkDark";

	public static readonly Color DefaultWindowText;

	public static readonly Color DefaultGrayText;

	public static readonly Color DefaultHighlightText;

	public static readonly Color DefaultWindow;

	public static readonly Color DefaultGrayBackground;

	public static readonly Color DefaultHighlight;

	public static readonly Color DefaultFilterHighlight;

	public static readonly Color DefaultFilterHighlightBox;

	private const string importExportVersionString = "version";

	private const string importExportSettingsString = "settings";

	private const string importExportSettingString = "setting";

	private static int zoomPercentValue;

	private static string defaultEncodingNameValue;

	public static FormStartPosition WindowPositionStart
	{
		get
		{
			if (!windowPositionValid)
			{
				return FormStartPosition.WindowsDefaultBounds;
			}
			return FormStartPosition.Manual;
		}
	}

	public static FormWindowState WindowPositionState { get; set; }

	public static Rectangle WindowPositionRectangle
	{
		get
		{
			return new Rectangle(windowX, windowY, windowWidth, windowHeight);
		}
		set
		{
			windowX = value.X;
			windowY = value.Y;
			windowWidth = value.Width;
			windowHeight = value.Height;
		}
	}

	public static int SplitterPositionPercent { get; set; }

	public static DockStyle FilterListLocation { get; set; }

	public static string LastFileDirectory { get; set; }

	public static string LastFilterDirectory { get; set; }

	public static List<string> RecentFiles { get; private set; }

	public static int NumberOfRecentFiles => RecentFiles.Count;

	public static List<string> RecentFilterFiles { get; private set; }

	public static int NumberOfRecentFilters => RecentFilterFiles.Count;

	public static int MaxNumberOfRecentItems { get; set; }

	public static FileNameDisplayStyle FileNameDisplayStyle { get; set; }

	public static int FileNameDisplayLength { get; set; }

	public static int TabSize { get; private set; }

	public static int FindDialogWidth { get; set; }

	public static int FilterDialogWidth { get; set; }

	public static float FontSize { get; set; }

	public static string FontFamily { get; set; }

	public static int ZoomPercent
	{
		get
		{
			return zoomPercentValue;
		}
		set
		{
			zoomPercentValue = Math.Min(value, 400);
			zoomPercentValue = Math.Max(zoomPercentValue, 10);
		}
	}

	public static Color WindowText { get; private set; }

	public static Color GrayText { get; private set; }

	public static Color GrayBackground { get; private set; }

	public static Color HighlightText { get; private set; }

	public static Color Window { get; private set; }

	public static Color Highlight { get; private set; }

	public static Color FilterHighlight { get; private set; }

	public static Color FilterHighlightBox { get; private set; }

	public static ShowMarkersState ShowMarkersColumn { get; set; }

	public static FiltersDirtiedState FiltersDirtied { get; set; }

	public static bool RememberFilterColumnLayout { get; set; }

	public static string FilterColumnLayout { get; set; }

	public static bool BeepOnSearchEnd { get; private set; }

	public static PasteTextOverwriteEnum PasteTextOverwrite { get; set; }

	public static bool ShowLineNumbers { get; set; }

	public static bool ShowFilterToolTip { get; set; }

	public static CopyLineNumbersState CopyLineNumbers { get; private set; }

	public static bool DefaultEncodingIsSystemDefault { get; private set; }

	public static string DefaultEncodingName
	{
		get
		{
			if (DefaultEncodingIsSystemDefault)
			{
				return Encoding.Default.WebName;
			}
			return defaultEncodingNameValue;
		}
		set
		{
			try
			{
				defaultEncodingNameValue = Encoding.GetEncoding(value).WebName;
				DefaultEncodingIsSystemDefault = false;
			}
			catch
			{
				defaultEncodingNameValue = null;
				DefaultEncodingIsSystemDefault = true;
			}
		}
	}

	static Settings()
	{
		DefaultEncodingIsSystemDefault = true;
		DefaultWindowText = Color.FromName("WindowText");
		DefaultGrayText = Color.FromName("GrayText");
		DefaultHighlightText = Color.FromName("HighlightText");
		DefaultWindow = Color.FromName("Window");
		DefaultGrayBackground = Color.FromName("MenuBar");
		DefaultHighlight = Color.FromName("Highlight");
		DefaultFilterHighlight = Color.FromName("ControlDark");
		DefaultFilterHighlightBox = Color.FromName("ControlDarkDark");
		DefaultFontFamily = System.Drawing.FontFamily.GenericMonospace.Name;
		SetDefaultValues();
	}

	public static void SetDefaultValues()
	{
		WindowPositionState = FormWindowState.Normal;
		SplitterPositionPercent = 75;
		FilterListLocation = DockStyle.Bottom;
		LastFileDirectory = "";
		LastFilterDirectory = "";
		ZoomPercent = 100;
		windowPositionValid = false;
		FromOptions(ToDefaultOptions());
	}

	public static void Load()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["WindowState"] = "WindowPositionState";
		dictionary["WindowColor"] = "Window";
		dictionary["HighlightColor"] = "Highlight";
		dictionary["HighlightTextColor"] = "HighlightText";
		dictionary["GrayBackgroundColor"] = "GrayBackground";
		dictionary["GrayTextColor"] = "GrayText";
		dictionary["WindowTextColor"] = "WindowText";
		dictionary["DefaultEncoding"] = "DefaultEncodingName";
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\DavidAns\\TextAnalysisTool.NET");
		string[] valueNames = registryKey.GetValueNames();
		PropertyInfo propertyInfo = null;
		object obj = null;
		try
		{
			typeof(Settings).GetProperties();
			string[] array = valueNames;
			foreach (string text in array)
			{
				bool flag = false;
				propertyInfo = typeof(Settings).GetProperty(text);
				if (propertyInfo == null && dictionary.ContainsKey(text))
				{
					propertyInfo = typeof(Settings).GetProperty(dictionary[text]);
					flag = true;
				}
				if (!(propertyInfo != null) || !propertyInfo.CanWrite)
				{
					continue;
				}
				object obj2 = registryKey.GetValue(text);
				TypeConverter typeConverter = null;
				if (obj2.GetType() != propertyInfo.PropertyType)
				{
					if (propertyInfo.PropertyType == typeof(Rectangle) && obj2.GetType() == typeof(string))
					{
						typeConverter = new StringRectangleConverter();
					}
					else if (propertyInfo.PropertyType == typeof(Color) && (obj2.GetType() == typeof(string) || obj2.GetType() == typeof(int)))
					{
						typeConverter = new StringColorConverter();
					}
					else if (propertyInfo.PropertyType == typeof(bool) && obj2.GetType() == typeof(int))
					{
						typeConverter = new BooleanDwordConverter();
					}
					if (typeConverter == null)
					{
						typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
					}
					obj2 = typeConverter.ConvertFrom(obj2);
				}
				propertyInfo.SetValue(null, obj2, null);
				if (flag)
				{
					registryKey.DeleteValue(text);
				}
			}
		}
		catch (Exception)
		{
		}
		if (WindowPositionRectangle.Left == 0 && WindowPositionRectangle.Right == 0 && WindowPositionRectangle.Top == 0 && WindowPositionRectangle.Bottom == 0)
		{
			WindowPositionState = FormWindowState.Normal;
			windowPositionValid = false;
		}
		else
		{
			try
			{
				Utility.ThrowExceptionIf(!Screen.FromRectangle(WindowPositionRectangle).WorkingArea.Contains((WindowPositionRectangle.Left + WindowPositionRectangle.Right) / 2, (WindowPositionRectangle.Top + WindowPositionRectangle.Bottom) / 2), null);
				windowPositionValid = true;
			}
			catch
			{
				WindowPositionState = FormWindowState.Normal;
				windowPositionValid = false;
			}
		}
		obj = registryKey.GetValue("NumberOfRecentFiles");
		int val = ((obj is int && (int)obj >= 1 && (int)obj <= MaxNumberOfRecentItems) ? ((int)obj) : 0);
		val = Math.Min(MaxNumberOfRecentItems, val);
		RecentFiles = new List<string>();
		for (int j = 0; j < val; j++)
		{
			string name = string.Format("{0}{1}", "RecentFile", j);
			obj = registryKey.GetValue(name);
			if (obj is string)
			{
				RecentFiles.Add((string)obj);
			}
		}
		obj = registryKey.GetValue("NumberOfRecentFilters");
		int val2 = ((obj is int) ? ((int)obj) : 0);
		val2 = Math.Min(MaxNumberOfRecentItems, val2);
		RecentFilterFiles = new List<string>();
		for (int k = 0; k < val2; k++)
		{
			string name2 = string.Format("{0}{1}", "RecentFilterFile", k);
			obj = registryKey.GetValue(name2);
			if (obj is string)
			{
				RecentFilterFiles.Add((string)obj);
			}
		}
	}

	public static void Save()
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\DavidAns\\TextAnalysisTool.NET");
			PropertyInfo[] properties = typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			foreach (PropertyInfo propertyInfo in properties)
			{
				object value = propertyInfo.GetValue(null, null);
				if (!(propertyInfo.PropertyType == typeof(List<string>)) && value != null)
				{
					registryKey.SetValue(propertyInfo.Name, value);
				}
			}
			for (int j = 0; j < RecentFiles.Count; j++)
			{
				string name = string.Format("{0}{1}", "RecentFile", j);
				registryKey.SetValue(name, RecentFiles[j]);
			}
			for (int k = 0; k < RecentFilterFiles.Count; k++)
			{
				string name2 = string.Format("{0}{1}", "RecentFilterFile", k);
				registryKey.SetValue(name2, RecentFilterFiles[k]);
			}
		}
		catch (Exception e)
		{
			CTextAnalysisTool.ShowMessageBox("An error occurred while attempting to save preferences: {0}.", e, MessageBoxIcon.Hand);
		}
	}

	public static void ExportConfiguration(string fileName, OptionsBehaviorProperties optionObj)
	{
		try
		{
			XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
			PropertyInfo[] properties = optionObj.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				XmlAttributes xmlAttributes = new XmlAttributes();
				object[] customAttributes = propertyInfo.GetCustomAttributes(inherit: true);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					Attribute attribute = (Attribute)customAttributes[j];
					if (attribute is XmlIgnoreAttribute)
					{
						xmlAttributes = null;
						break;
					}
					if (attribute is XmlElementAttribute)
					{
						xmlAttributes.XmlElements.Add((XmlElementAttribute)attribute);
					}
				}
				if (xmlAttributes != null)
				{
					xmlAttributeOverrides.Add(typeof(OptionsBehaviorProperties), propertyInfo.Name, xmlAttributes);
				}
			}
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(OptionsBehaviorProperties), xmlAttributeOverrides);
			TextWriter textWriter = new StreamWriter(fileName);
			xmlSerializer.Serialize(textWriter, optionObj);
			textWriter.Flush();
			textWriter.Close();
		}
		catch (Exception e)
		{
			CTextAnalysisTool.ShowMessageBox("An error occurred while attempting to write the file: {0}.", e, MessageBoxIcon.Hand);
		}
	}

	public static bool ImportConfiguration(string fileName, out OptionsBehaviorProperties newOptions)
	{
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(OptionsBehaviorProperties));
			FileStream fileStream = new FileStream(fileName, FileMode.Open);
			XmlReader xmlReader = XmlReader.Create(fileStream);
			newOptions = (OptionsBehaviorProperties)xmlSerializer.Deserialize(xmlReader);
			fileStream.Close();
			return true;
		}
		catch (Exception ex)
		{
			CTextAnalysisTool.ShowMessageBox(details: (!(ex is ApplicationException) && !(ex is XmlException) && !(ex is FormatException) && !(ex is OverflowException) && !(ex is ArgumentOutOfRangeException)) ? ex.Message : string.Format("The file is not a valid {0} configuration file.", "TextAnalysisTool.NET"), message: "An error occurred while attempting to read the file: {0}.", parameter: fileName.ShortenPath(50), icon: MessageBoxIcon.Hand);
			newOptions = null;
			return false;
		}
	}

	public static OptionsBehaviorProperties ToOptions()
	{
		return new OptionsBehaviorProperties
		{
			MaxNumberOfRecentItems = MaxNumberOfRecentItems,
			FileNameDisplayStyle = FileNameDisplayStyle,
			FileNameDisplayLength = FileNameDisplayLength,
			BeepOnSearchEnd = BeepOnSearchEnd,
			PasteTextOverwrite = PasteTextOverwrite,
			ShowLineNumbers = ShowLineNumbers,
			ShowFilterToolTip = ShowFilterToolTip,
			CopyLineNumbers = CopyLineNumbers,
			ShowMarkersColumn = ShowMarkersColumn,
			FiltersDirtied = FiltersDirtied,
			RememberFilterColumnLayout = RememberFilterColumnLayout,
			FilterColumnLayout = FilterColumnLayout,
			TabSize = TabSize,
			WindowTextColor = WindowText,
			WindowColor = Window,
			GrayTextColor = GrayText,
			GrayBackgroundColor = GrayBackground,
			HighlightTextColor = HighlightText,
			HighlightColor = Highlight,
			FilterHighlightColor = FilterHighlight,
			FilterHighlightBoxColor = FilterHighlightBox,
			LineFont = CTextAnalysisTool.SafeCreateFont(FontFamily, FontSize),
			DefaultEncodingName = (DefaultEncodingIsSystemDefault ? "System Default" : DefaultEncodingName)
		};
	}

	public static OptionsBehaviorProperties ToDefaultOptions()
	{
		return new OptionsBehaviorProperties
		{
			MaxNumberOfRecentItems = 5,
			FileNameDisplayStyle = FileNameDisplayStyle,
			FileNameDisplayLength = 0,
			BeepOnSearchEnd = true,
			PasteTextOverwrite = PasteTextOverwriteEnum.AlwaysAsk,
			ShowLineNumbers = true,
			ShowFilterToolTip = true,
			CopyLineNumbers = CopyLineNumbersState.WhenVisible,
			ShowMarkersColumn = ShowMarkersState.WhenInUse,
			FiltersDirtied = FiltersDirtiedState.Always,
			RememberFilterColumnLayout = true,
			FilterColumnLayout = "",
			TabSize = 4,
			WindowTextColor = DefaultWindowText,
			WindowColor = DefaultWindow,
			GrayTextColor = DefaultGrayText,
			GrayBackgroundColor = DefaultGrayBackground,
			HighlightTextColor = DefaultHighlightText,
			HighlightColor = DefaultHighlight,
			FilterHighlightColor = DefaultFilterHighlight,
			FilterHighlightBoxColor = DefaultFilterHighlightBox,
			LineFont = new Font(DefaultFontFamily, 10f),
			DefaultEncodingName = DefaultEncodingName
		};
	}

	public static void FromOptions(OptionsBehaviorProperties options)
	{
		MaxNumberOfRecentItems = options.MaxNumberOfRecentItems;
		FileNameDisplayStyle = options.FileNameDisplayStyle;
		FileNameDisplayLength = options.FileNameDisplayLength;
		BeepOnSearchEnd = options.BeepOnSearchEnd;
		PasteTextOverwrite = options.PasteTextOverwrite;
		ShowLineNumbers = options.ShowLineNumbers;
		ShowFilterToolTip = options.ShowFilterToolTip;
		CopyLineNumbers = options.CopyLineNumbers;
		ShowMarkersColumn = options.ShowMarkersColumn;
		FiltersDirtied = options.FiltersDirtied;
		RememberFilterColumnLayout = options.RememberFilterColumnLayout;
		FilterColumnLayout = options.FilterColumnLayout;
		TabSize = options.TabSize;
		WindowText = options.WindowTextColor;
		Window = options.WindowColor;
		GrayText = options.GrayTextColor;
		GrayBackground = options.GrayBackgroundColor;
		HighlightText = options.HighlightTextColor;
		Highlight = options.HighlightColor;
		FilterHighlight = options.FilterHighlightColor;
		FilterHighlightBox = options.FilterHighlightBoxColor;
		FontFamily = options.LineFont.FontFamily.Name;
		FontSize = options.LineFont.Size;
		DefaultEncodingName = options.DefaultEncodingName;
	}
}
