using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TextAnalysisTool.NET.Plugin;
using ToolBox;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class LineCollectionDisplayListBox : ListBox
{
	public class ZoomLevelEventArgs : EventArgs
	{
		public float ZoomLevel { get; private set; }

		public ZoomLevelEventArgs(float zoomLevel)
		{
			ZoomLevel = zoomLevel;
		}
	}

	public class ShowOnlyFilteredLinesEventArgs : EventArgs
	{
		public bool ShowOnlyFilteredLines { get; private set; }

		public ShowOnlyFilteredLinesEventArgs(bool showOnlyFilteredLines)
		{
			ShowOnlyFilteredLines = showOnlyFilteredLines;
		}
	}

	private const int LBS_HASSTRINGS = 64;

	private const int LBS_NODATA = 8192;

	private const int LB_SETSEL = 389;

	private const int LB_GETSEL = 391;

	private const int LB_GETSELCOUNT = 400;

	private const int LB_GETSELITEMS = 401;

	private const int LB_GETCARETINDEX = 415;

	private const int LB_SETCOUNT = 423;

	private const int LB_ERR = -1;

	private const int LB_ERRSPACE = -2;

	private const int WM_CHAR = 258;

	private const int WM_MOUSEWHEEL = 522;

	private const int WS_VSCROLL = 2097152;

	private const int WHEEL_DELTA = 120;

	private const int WM_HSCROLL = 276;

	private const int SB_THUMBPOSITION = 4;

	private const int SB_HORZ = 0;

	private const int MaxWidthOfTextInPixels = 32000;

	private const char WideCharacter = '宽';

	private readonly CTextAnalysisTool m_textAnalysisTool;

	private readonly VScrollBar m_scrollBar;

	private LineCollection m_sourceLines;

	private LineCollection m_filteredLines;

	private bool m_showOnlyFilteredLines;

	private int m_maximumLineNumberDigits;

	private readonly SolidBrush m_brush;

	private readonly StringFormat m_stringFormat;

	private int m_currentlySelectedLineNumber;

	private int m_accumulatedWheelDelta;

	private ToolTip m_toolTip;

	private int m_toolTipLineNumber = -1;

	private int m_maxWidthOfTextInCharacters;

	private float m_characterWidthInPixels;

	private float m_asciiCharWidthInPixels;

	private float m_cjkCharWidthInPixels;

	private int m_cachedMetaDataWidth = -1;

	private int m_cachedMetaDataDigits = -1;

	private bool m_cachedIncludeMarkers;

	private Font m_cachedMetaDataFont;

	private ToolStripMenuItem m_markerMenu;

	private ToolStripMenuItem m_hideBeforeItem;

	private ToolStripMenuItem m_hideAfterItem;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams obj = base.CreateParams;
			obj.Style |= 8192;
			obj.Style &= -65;
			obj.Style &= -2097153;
			return obj;
		}
	}

	public bool ShowOnlyFilteredLines
	{
		get
		{
			return m_showOnlyFilteredLines;
		}
		set
		{
			if (m_showOnlyFilteredLines != value)
			{
				m_showOnlyFilteredLines = value;
				RefreshDisplay();
				this.ShowOnlyFilteredLinesChanged?.Invoke(this, new ShowOnlyFilteredLinesEventArgs(value));
			}
		}
	}

	private static readonly LineCollection s_emptyLineCollection = new LineCollection();

	public bool LinesVisible => CurrentDisplayLines.Count != 0;

	public LineCollection SourceLines => m_sourceLines ?? s_emptyLineCollection;

	public event EventHandler MarkersChanged;

	public int NumberLinesSelected => base.SelectedIndices.Count;

	private LineCollection CurrentDisplayLines
	{
		get
		{
			if (!m_showOnlyFilteredLines)
			{
				return m_sourceLines ?? s_emptyLineCollection;
			}
			return m_filteredLines ?? s_emptyLineCollection;
		}
	}

	public float FontSize
	{
		get
		{
			return Font.Size;
		}
		set
		{
			if (Font.Size != value)
			{
				Font = CTextAnalysisTool.SafeCreateFont(Font.FontFamily.Name, value, Settings.ZoomPercent, Font);
				RefreshDisplay();
				m_textAnalysisTool.UpdateUIFonts(Settings.FontSize, Settings.ZoomPercent);
			}
		}
	}

	public override Font Font
	{
		set
		{
			BeginUpdate();
			int num = 100;
			Size sizeCjk = TextRenderer.MeasureText(string.Empty.PadRight(num, '宽'), value);
			Size sizeAscii = TextRenderer.MeasureText(string.Empty.PadRight(num, '0'), value);
			ItemHeight = sizeCjk.Height;
			m_cjkCharWidthInPixels = (float)sizeCjk.Width / (float)num;
			m_asciiCharWidthInPixels = (float)sizeAscii.Width / (float)num;
			m_characterWidthInPixels = m_asciiCharWidthInPixels;
			m_maxWidthOfTextInCharacters = Math.Max(100, (int)(31800f / m_asciiCharWidthInPixels));
			base.Font = value;
			EndUpdate();
			this.ZoomLevelChanged?.Invoke(this, new ZoomLevelEventArgs(ZoomPercent));
		}
	}

	public int ZoomPercent
	{
		get
		{
			return Settings.ZoomPercent;
		}
		set
		{
			int zoomPercent = Settings.ZoomPercent;
			Settings.ZoomPercent = value;
			if (Settings.ZoomPercent != zoomPercent)
			{
				Font = CTextAnalysisTool.SafeCreateFont(Font.FontFamily.Name, Settings.FontSize, Settings.ZoomPercent, Font);
				RefreshDisplay();
				m_textAnalysisTool.UpdateUIFonts(Settings.FontSize, Settings.ZoomPercent);
			}
		}
	}

	public event EventHandler<ZoomLevelEventArgs> ZoomLevelChanged;

	public event EventHandler<ShowOnlyFilteredLinesEventArgs> ShowOnlyFilteredLinesChanged;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern int GetScrollPos(IntPtr hWnd, int nBar);

	private int CallSendMessage(uint msg, int wParam, int lParam)
	{
		return ThrowExceptionOnFailedSendMessageCall(SendMessage(base.Handle, msg, new IntPtr(wParam), new IntPtr(lParam)).ToInt32());
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, int[] lParam);

	private int CallSendMessage(uint msg, int wParam, int[] lParam)
	{
		return ThrowExceptionOnFailedSendMessageCall(SendMessage(base.Handle, msg, new IntPtr(wParam), lParam).ToInt32());
	}

	private static int ThrowExceptionOnFailedSendMessageCall(int result)
	{
		Utility.ThrowExceptionIf(-1 == result || -2 == result, "Call to SendMessage(ListBox, ...) failed");
		return result;
	}

	public LineCollectionDisplayListBox(CTextAnalysisTool textAnalysisTool, VScrollBar scrollBar)
	{
		m_textAnalysisTool = textAnalysisTool;
		m_scrollBar = scrollBar;
		m_brush = new SolidBrush(Color.Empty);
		Font = CTextAnalysisTool.SafeCreateFont(Settings.FontFamily, Settings.FontSize, Settings.ZoomPercent);
		BackColor = Settings.Window;
		DrawMode = DrawMode.OwnerDrawFixed;
		base.IntegralHeight = false;
		base.HorizontalScrollbar = true;
		base.HorizontalExtent = 0;
		base.ResizeRedraw = true;
		base.ScrollAlwaysVisible = true;
		SelectionMode = SelectionMode.MultiExtended;
		m_stringFormat = new StringFormat(StringFormat.GenericDefault);
		m_stringFormat.FormatFlags |= StringFormatFlags.NoFontFallback;
		m_currentlySelectedLineNumber = -1;
		m_markerMenu = new ToolStripMenuItem("Toggle &Marker");
		for (int i = 0; i < 8; i++)
		{
			m_markerMenu.DropDownItems.Add(MarkerMenu(i));
		}
		m_hideBeforeItem = new ToolStripMenuItem("Hide Lines &Before This");
		m_hideBeforeItem.Click += (s, e2) => OnHideLines(hideBefore: true);
		m_hideAfterItem = new ToolStripMenuItem("Hide Lines &After This");
		m_hideAfterItem.Click += (s, e2) => OnHideLines(hideBefore: false);
		ContextMenuStrip = new ContextMenuStrip();
		ContextMenuStrip.Items.Add(m_hideBeforeItem);
		ContextMenuStrip.Items.Add(m_hideAfterItem);
		ContextMenuStrip.Items.Add(m_markerMenu);
		ContextMenuStrip.Opening += ContextMenuStrip_Opening;
		base.MouseUp += ContextMenuStripPositioning;
		ConfigureToolTipHelpers();
	}

	private void ContextMenuStripPositioning(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Control) != Keys.Control && 0 <= e.Location.Y && e.Location.Y < base.ClientRectangle.Height)
		{
			int num = e.Location.Y / ItemHeight;
			num = base.TopIndex + num;
			if (num < CurrentDisplayLines.Count && Array.BinarySearch(GetSelectedIndexes(), num) < 0)
			{
				SelectIndex(num);
			}
		}
	}

	private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
	{
		int[] selectedLines = GetSelectedLineNumbers();
		bool hasSelection = selectedLines != null && selectedLines.Length > 0;
		bool isMultiSelect = selectedLines != null && selectedLines.Length > 1;

		ToolStripDropDown menu = (ToolStripDropDown)sender;
		menu.Items.Clear();
		AddPluginMenuItems(menu);

		if (isMultiSelect)
		{
			int minLine = int.MaxValue;
			int maxLine = int.MinValue;
			for (int i = 0; i < selectedLines.Length; i++)
			{
				if (selectedLines[i] < minLine)
				{
					minLine = selectedLines[i];
				}
				if (selectedLines[i] > maxLine)
				{
					maxLine = selectedLines[i];
				}
			}
			ToolStripMenuItem hideRangeItem = new ToolStripMenuItem($"Hide Lines {minLine} to {maxLine}");
			hideRangeItem.Click += (s, e2) => m_textAnalysisTool.AddHideLinesRangeFilter(minLine, maxLine);
			menu.Items.Add(hideRangeItem);
		}

		menu.Items.Add(m_hideBeforeItem);
		menu.Items.Add(m_hideAfterItem);
		menu.Items.Add(m_markerMenu);
		m_hideBeforeItem.Enabled = hasSelection;
		m_hideAfterItem.Enabled = hasSelection;
		m_markerMenu.Enabled = hasSelection;
		foreach (ToolStripItem item in m_markerMenu.DropDownItems)
		{
			if (item is FullyEnabledMenuItem fullyEnabledMenuItem)
			{
				fullyEnabledMenuItem.Enabled = hasSelection;
			}
		}
	}

	private void OnHideLines(bool hideBefore)
	{
		int lineNumber = GetSelectedLineNumber();
		if (lineNumber < 0)
		{
			return;
		}
		m_textAnalysisTool.AddHideLinesFilter(lineNumber, hideBefore);
	}

	private void AddPluginMenuItems(ToolStripDropDown menu)
	{
		if (m_textAnalysisTool == null)
		{
			return;
		}
		string lineText = GetSelectedLineText();
		if (string.IsNullOrEmpty(lineText))
		{
			return;
		}
		bool addedAny = false;
		foreach (object pluginObj in m_textAnalysisTool.Plugins)
		{
			if (pluginObj is not PluginWrapper plugin)
			{
				continue;
			}
			IList<PluginContextMenuItem> items = plugin.GetLineContextMenuItems(lineText);
			if (items == null || items.Count == 0)
			{
				continue;
			}
			foreach (PluginContextMenuItem item in items)
			{
				if (item == null || string.IsNullOrEmpty(item.Text))
				{
					continue;
				}
				if (item.IsSeparator)
				{
					menu.Items.Add(new ToolStripSeparator());
					continue;
				}
				ToolStripMenuItem menuItem = BuildPluginMenuItem(item);
				menu.Items.Add(menuItem);
				addedAny = true;
			}
		}
		if (addedAny)
		{
			menu.Items.Add(new ToolStripSeparator());
		}
	}

	private ToolStripMenuItem BuildPluginMenuItem(PluginContextMenuItem item)
	{
		ToolStripMenuItem menuItem = new ToolStripMenuItem(item.Text);
		if (item.ForeColor != Color.Empty && item.ForeColor != Color.Transparent)
		{
			menuItem.Image = CreateColorBadge(item.ForeColor, item.BackColor);
		}
		if (item.SubItems != null && item.SubItems.Count > 0)
		{
			foreach (PluginContextMenuItem subItem in item.SubItems)
			{
				if (subItem == null)
				{
					continue;
				}
				if (subItem.IsSeparator)
				{
					menuItem.DropDownItems.Add(new ToolStripSeparator());
				}
				else
				{
					menuItem.DropDownItems.Add(BuildPluginMenuItem(subItem));
				}
			}
		}
		if (!string.IsNullOrEmpty(item.FilterPattern))
		{
			string pattern = item.FilterPattern;
			bool caseSensitive = item.CaseSensitive;
			Color foreColor = item.ForeColor;
			Color backColor = item.BackColor;
			string description = item.Description;
			bool promptForColor = item.PromptForColor;
			menuItem.Click += (s, e2) =>
			{
				if (promptForColor)
				{
					using ColorDialog dlg = new ColorDialog();
					dlg.Color = (foreColor != Color.Empty && foreColor != Color.Transparent) ? foreColor : Color.Black;
					dlg.FullOpen = true;
					if (dlg.ShowDialog(this) == DialogResult.OK)
					{
						m_textAnalysisTool.AddFilterFromPattern(pattern, caseSensitive, dlg.Color, backColor, description);
					}
				}
				else
				{
					m_textAnalysisTool.AddFilterFromPattern(pattern, caseSensitive, foreColor, backColor, description);
				}
			};
		}
		else if (item.ClickAction != null)
		{
			Action action = item.ClickAction;
			menuItem.Click += (s, e2) => action();
		}
		return menuItem;
	}

	private static Image CreateColorBadge(Color foreColor, Color backColor)
	{
		Bitmap bmp = new Bitmap(16, 16);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.Clear(Color.Transparent);
			if (backColor != Color.Empty && backColor != Color.Transparent)
			{
				using Brush backBrush = new SolidBrush(backColor);
				g.FillRectangle(backBrush, 1, 1, 14, 14);
			}
			using (Brush foreBrush = new SolidBrush(foreColor))
			{
				g.FillRectangle(foreBrush, 3, 3, 10, 10);
			}
			using (Pen borderPen = new Pen(Color.FromArgb(140, 140, 140)))
			{
				g.DrawRectangle(borderPen, 2, 2, 11, 11);
			}
		}
		return bmp;
	}

	private FullyEnabledMenuItem MarkerMenu(int i)
	{
		return new FullyEnabledMenuItem($"&{i + 1}", null, MarkerMenu_Click)
		{
			Tag = i
		};
	}

	private void MarkerMenu_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		ToggleMarks((int)fullyEnabledMenuItem.Tag, CurrentDisplayLines);
	}

	public void ConfigureToolTipHelpers()
	{
		if (Settings.ShowFilterToolTip)
		{
			if (m_toolTip == null)
			{
				base.MouseMove += LineCollectionDisplay_MouseMove;
				base.MouseLeave += LineCollectionDisplay_MouseLeave;
				m_toolTip = new ToolTip
				{
					ToolTipTitle = "Matching Filters"
				};
			}
		}
		else if (m_toolTip != null)
		{
			base.MouseMove -= LineCollectionDisplay_MouseMove;
			base.MouseLeave -= LineCollectionDisplay_MouseLeave;
			m_toolTip = null;
		}
	}

	private void LineCollectionDisplay_MouseLeave(object sender, EventArgs e)
	{
		m_toolTip.Active = false;
		m_toolTipLineNumber = -1;
	}

	private void LineCollectionDisplay_MouseMove(object sender, MouseEventArgs e)
	{
		if (!(sender is ListBox listBox))
		{
			return;
		}
		Point position = Cursor.Position;
		position = PointToClient(position);
		int num = listBox.IndexFromPoint(position);
		if (-1 == num)
		{
			return;
		}
		int num2 = CurrentDisplayLines.LineNumbersInCollection[num];
		if (num2 != m_toolTipLineNumber)
		{
			List<string> lineFilterNames = GetLineFilterNames(num2);
			if (lineFilterNames.Count == 0)
			{
				m_toolTip.Active = false;
			}
			else
			{
				m_toolTip.SetToolTip(listBox, string.Join("\n", lineFilterNames));
				m_toolTip.Active = true;
			}
			m_toolTipLineNumber = num2;
		}
	}

	public void RefreshDisplay()
	{
		BackColor = Settings.Window;
		ForeColor = Settings.WindowText;
		DisplayLines(m_sourceLines, m_filteredLines, showOnlyFilteredLinesChanging: true);
		ConfigureToolTipHelpers();
	}

	public void DisplayLines(LineCollection sourceLines, LineCollection filteredLines, bool showOnlyFilteredLinesChanging)
	{
		bool flag = sourceLines == m_sourceLines;
		bool flag2 = flag && !m_showOnlyFilteredLines && !showOnlyFilteredLinesChanging;
		if (m_toolTip != null)
		{
			m_toolTip.Active = false;
		}
		m_sourceLines = sourceLines;
		m_filteredLines = filteredLines;
		LineCollection currentDisplayLines = CurrentDisplayLines;
		if (!flag)
		{
			m_maximumLineNumberDigits = CalculateMaximumLineNumberDigits();
			m_currentlySelectedLineNumber = -1;
			LineCollection.ResetMarkersInUse();
			MarkersChanged?.Invoke(this, EventArgs.Empty);
		}
		if (flag2)
		{
			Invalidate();
			return;
		}
		BeginUpdate();
		int num = (flag ? base.TopIndex : 0);
		int numberDigits = Settings.ShowLineNumbers ? m_maximumLineNumberDigits : 0;
		bool includeMarkers = Settings.ShowMarkersColumn == ShowMarkersState.Always || (Settings.ShowMarkersColumn == ShowMarkersState.WhenInUse && LineCollection.MarkersInUse);
		int metaWidth = GetMetaDataWidth(null, numberDigits, includeMarkers, Font) + 6;
		int maxSafeChars = Math.Max(100, (int)((32000 - metaWidth) / m_characterWidthInPixels));
		int maxChars = Math.Min(maxSafeChars, currentDisplayLines.MaxDisplayWidth);
		int textPixelWidth = (int)Math.Ceiling(maxChars * m_characterWidthInPixels);
		base.HorizontalExtent = Math.Min(32000, metaWidth + textPixelWidth);
		CallSendMessage(423u, currentDisplayLines.Count, 0);
		m_scrollBar.Maximum = currentDisplayLines.Count;
		base.TopIndex = num;
		m_scrollBar.Value = base.TopIndex;
		EndUpdate();
		if (0 < currentDisplayLines.Count)
		{
			int number = ((!flag || -1 == m_currentlySelectedLineNumber) ? currentDisplayLines.LineNumbersInCollection[0] : m_currentlySelectedLineNumber);
			SelectLineNumber(number);
		}
	}

	public string GetSelectedLineText()
	{
		string result = null;
		int selectedIndex = GetSelectedIndex();
		if (-1 != selectedIndex)
		{
			LineCollection currentDisplayLines = CurrentDisplayLines;
			int lineNumber = currentDisplayLines.LineNumbersInCollection[selectedIndex];
			result = currentDisplayLines.GetLineText(lineNumber);
		}
		return result;
	}

	public int GetSelectedLineNumber()
	{
		int selectedIndex = GetSelectedIndex();
		if (-1 != selectedIndex)
		{
			return CurrentDisplayLines.LineNumbersInCollection[selectedIndex];
		}
		return -1;
	}

	public string GetSelectedLinesAsString(bool formatAsHtml)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (formatAsHtml)
		{
			stringBuilder.AppendFormat("<pre style=\"background-color:#{0};font-family:monospace;font-size:{1}pt;white-space:pre\">", Settings.Window.ToCssString(), Font.SizeInPoints);
		}
		LineCollection currentDisplayLines = CurrentDisplayLines;
		int numberDigits = ((Settings.ShowLineNumbers && Settings.CopyLineNumbers == CopyLineNumbersState.WhenVisible) ? CalculateMaximumLineNumberDigits() : 0);
		int[] selectedLineNumbers = GetSelectedLineNumbers();
		foreach (int lineNumber in selectedLineNumbers)
		{
			if (formatAsHtml)
			{
				GetLineColors(lineNumber, selected: false, out var foreColor, out var backColor);
				stringBuilder.AppendFormat("<span style=\"background-color:#{1};color:#{2}\">{0}</span>\n", currentDisplayLines.GetLineDisplayString(lineNumber, numberDigits, includeMarkers: false).Escape(), backColor.ToCssString(), foreColor.ToCssString());
			}
			else
			{
				stringBuilder.AppendFormat("{0}\r\n", currentDisplayLines.GetLineDisplayString(lineNumber, numberDigits, includeMarkers: false));
			}
		}
		if (formatAsHtml)
		{
			stringBuilder.Append("</pre>\n");
		}
		return stringBuilder.ToString().Replace("\0", "\ufffd");
	}

	public void OutputLinesToStream(TextWriter writer)
	{
		int[] lineNumbersInCollection = CurrentDisplayLines.LineNumbersInCollection;
		foreach (int lineNumber in lineNumbersInCollection)
		{
			writer.WriteLine(m_sourceLines.GetLineDisplayString(lineNumber, 0, includeMarkers: false));
		}
	}

	private int CalculateMaximumLineNumberDigits()
	{
		int num = 0;
		int num2 = m_sourceLines.Count;
		while (0 < num2)
		{
			num++;
			num2 /= 10;
		}
		return num;
	}

	public void FindNextMatch(Filter find, bool reverse)
	{
		if (m_currentlySelectedLineNumber != -1)
		{
			int nextMatchingLineNumberInCollection = CurrentDisplayLines.GetNextMatchingLineNumberInCollection(m_currentlySelectedLineNumber, find, reverse);
			if (((nextMatchingLineNumberInCollection < m_currentlySelectedLineNumber && !reverse) || (nextMatchingLineNumberInCollection > m_currentlySelectedLineNumber && reverse) || nextMatchingLineNumberInCollection == m_currentlySelectedLineNumber) && Settings.BeepOnSearchEnd)
			{
				SystemSounds.Beep.Play();
			}
			if (-1 != nextMatchingLineNumberInCollection)
			{
				SelectLineNumber(nextMatchingLineNumberInCollection);
			}
		}
	}

	public void ShowGoToDialog()
	{
		using GoToDialog goToDialog = new GoToDialog(m_currentlySelectedLineNumber, m_sourceLines.Count);
		if (DialogResult.OK == goToDialog.ShowDialog(this))
		{
			SelectLineNumber(goToDialog.LineNumber);
		}
	}

	public void Reload(string fileName)
	{
		BeginUpdate();
		int num = m_currentlySelectedLineNumber;
		int num2 = base.TopIndex;
		m_textAnalysisTool.LoadLinesFromFile(fileName);
		if (-1 != num)
		{
			if (!CurrentDisplayLines.IsLineNumberValid(num))
			{
				int[] lineNumbersInCollection = m_sourceLines.LineNumbersInCollection;
				num = ((lineNumbersInCollection.Length != 0) ? lineNumbersInCollection[lineNumbersInCollection.Length - 1] : (-1));
			}
			m_currentlySelectedLineNumber = num;
			if (0 < CurrentDisplayLines.Count)
			{
				SelectLineNumber(num);
				base.TopIndex = num2;
				m_scrollBar.Value = base.TopIndex;
			}
		}
		EndUpdate();
	}

	public void MarkLineNumberRange(int firstLine, int lastLine)
	{
		LineCollection currentDisplayLines = CurrentDisplayLines;
		for (int i = firstLine; i <= lastLine; i++)
		{
			currentDisplayLines.LineToggleMark(i, 0);
		}
		MarkersChanged?.Invoke(this, EventArgs.Empty);
	}

	private int GetMetaDataWidth(string sampleString, int numberDigits, bool includeMarkers, Font font)
	{
		if (m_cachedMetaDataWidth >= 0 &&
			m_cachedMetaDataDigits == numberDigits &&
			m_cachedIncludeMarkers == includeMarkers &&
			m_cachedMetaDataFont == font)
		{
			return m_cachedMetaDataWidth;
		}

		m_cachedMetaDataDigits = numberDigits;
		m_cachedIncludeMarkers = includeMarkers;
		m_cachedMetaDataFont = font;
		if (string.IsNullOrEmpty(sampleString))
		{
			int totalChars = numberDigits + (includeMarkers ? 9 : 0);
			m_cachedMetaDataWidth = (totalChars > 0) ? (int)Math.Ceiling(totalChars * m_characterWidthInPixels) : 0;
		}
		else
		{
			m_cachedMetaDataWidth = TextRenderer.MeasureText(sampleString, font).Width;
		}
		return m_cachedMetaDataWidth;
	}

	private static int GetVisualColumns(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		int cols = 0;
		for (int i = 0; i < text.Length; i++)
		{
			cols += (text[i] > 127) ? 2 : 1;
		}
		return cols;
	}

	private static string TruncateTextToVisualColumns(string text, int maxVisualColumns)
	{
		const string notice = "[TextAnalysisTool.NET could not display the whole line. Filtering, copying, and searching are unaffected.]";
		int noticeCols = GetVisualColumns(notice) + 5;
		int targetContentCols = Math.Max(0, maxVisualColumns - noticeCols);

		int currentCols = 0;
		int cutIndex = 0;
		for (int i = 0; i < text.Length; i++)
		{
			int charCols = (text[i] > 127) ? 2 : 1;
			if (currentCols + charCols > targetContentCols)
			{
				break;
			}
			currentCols += charCols;
			cutIndex = i + 1;
		}
		return text.Substring(0, cutIndex) + " ... " + notice;
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		if (-1 != e.Index && e.Graphics.IsVisible(e.Bounds))
		{
			int lineNumber = CurrentDisplayLines.LineNumbersInCollection[e.Index];
			int numberDigits = Settings.ShowLineNumbers ? m_maximumLineNumberDigits : 0;
			bool includeMarkers = Settings.ShowMarkersColumn == ShowMarkersState.Always || (Settings.ShowMarkersColumn == ShowMarkersState.WhenInUse && LineCollection.MarkersInUse);
			string lineMetaDataString = m_sourceLines.GetLineMetaDataString(lineNumber, numberDigits, includeMarkers);
			string text = m_sourceLines.GetLineDisplayString(lineNumber, 0, includeMarkers: false);
			int num = 3;
			int metaDataWidth = GetMetaDataWidth(lineMetaDataString, numberDigits, includeMarkers, e.Font);
			Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, metaDataWidth + num * 2, e.Bounds.Height);
			Rectangle rectangle = new Rectangle(rect.Right, e.Bounds.Y, e.Bounds.Width - rect.Width, e.Bounds.Height);
			Rectangle rect2 = Rectangle.Intersect(rectangle, Rectangle.Ceiling(e.Graphics.VisibleClipBounds));
			bool selected = (DrawItemState.Selected & e.State) != 0;
			GetLineColors(lineNumber, selected, out var foreColor, out var backColor);
			m_brush.Color = backColor;
			e.Graphics.FillRectangle(m_brush, rect2);
			m_brush.Color = Settings.GrayBackground;
			e.Graphics.FillRectangle(m_brush, rect);
			m_brush.Color = Settings.GrayText;
			e.Graphics.DrawString(lineMetaDataString, e.Font, m_brush, rect.X + num, rect.Y, m_stringFormat);
			int maxSafeVisualColumns = Math.Max(100, (int)((32000 - metaDataWidth - num * 2) / m_characterWidthInPixels));
			if (GetVisualColumns(text) > maxSafeVisualColumns)
			{
				text = TruncateTextToVisualColumns(text, maxSafeVisualColumns);
			}
			TextRenderer.DrawText(e.Graphics, text, e.Font, rectangle, foreColor, backColor, TextFormatFlags.NoPrefix);
			new DrawItemEventArgs(e.Graphics, e.Font, rect2, e.Index, e.State, e.ForeColor, e.BackColor).DrawFocusRectangle();
		}
	}

	public void GetLineColors(int lineNumber, bool selected, out Color foreColor, out Color backColor)
	{
		if (selected)
		{
			foreColor = Settings.HighlightText;
			backColor = Settings.Highlight;
			return;
		}
		bool flag = m_filteredLines.IsLineNumberInCollection(lineNumber);
		foreColor = (flag ? Settings.WindowText : Settings.GrayText);
		backColor = Settings.Window;
		if (!flag)
		{
			return;
		}
		foreach (Filter filter in m_textAnalysisTool.FilterCollection.Filters)
		{
			if (filter.Enabled && !filter.Excluding && filter.FilteredLines != null && filter.FilteredLines.IsLineNumberInCollection(lineNumber))
			{
				if (Color.Empty != filter.ForegroundColor)
				{
					foreColor = filter.ForegroundColor;
				}
				if (Color.Empty != filter.BackgroundColor)
				{
					backColor = filter.BackgroundColor;
				}
				break;
			}
		}
	}

	private List<string> GetLineFilterNames(int lineNumber)
	{
		List<string> list = new List<string>();
		if (m_filteredLines.IsLineNumberInCollection(lineNumber))
		{
			int num = 0;
			foreach (Filter filter in m_textAnalysisTool.FilterCollection.Filters)
			{
				if (filter.Enabled && !filter.Excluding && filter.FilteredLines != null && filter.FilteredLines.IsLineNumberInCollection(lineNumber))
				{
					string text = ((num < 26) ? ((char)(97 + num)).ToString() : num.ToString());
					if (string.IsNullOrEmpty(filter.Description))
					{
						list.Add($"Unnamed filter ({text})");
					}
					else
					{
						list.Add($"{filter.Description} ({text})");
					}
				}
				num++;
			}
		}
		return list;
	}

	public void SelectAll()
	{
		CallSendMessage(389u, 1, -1);
		OnSelectedIndexChanged(EventArgs.Empty);
	}

	public void SelectLineNumber(int number)
	{
		if (CurrentDisplayLines.IsLineNumberValid(number))
		{
			CallSendMessage(389u, 0, -1);
			int indexOfLineNumber = CurrentDisplayLines.GetIndexOfLineNumber(CurrentDisplayLines.GetNearestLineNumberInCollection(number));
			int num = base.ClientSize.Height / ItemHeight;
			if (indexOfLineNumber < base.TopIndex || base.TopIndex + num <= indexOfLineNumber)
			{
				base.TopIndex = Math.Max(indexOfLineNumber - num / 2, 0);
				m_scrollBar.Value = base.TopIndex;
			}
			CallSendMessage(389u, 1, indexOfLineNumber);
			m_currentlySelectedLineNumber = number;
		}
	}

	public void SelectIndex(int number)
	{
		CallSendMessage(389u, 0, -1);
		CallSendMessage(389u, 1, number);
		OnSelectedIndexChanged(EventArgs.Empty);
	}

	private int GetSelectedIndex()
	{
		int result = -1;
		if (CallSendMessage(400u, 0, 0) != 0)
		{
			result = CallSendMessage(415u, 0, 0);
		}
		return result;
	}

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		int selectedIndex = GetSelectedIndex();
		if (-1 != selectedIndex)
		{
			m_currentlySelectedLineNumber = CurrentDisplayLines.LineNumbersInCollection[selectedIndex];
			m_scrollBar.Value = selectedIndex;
		}
		base.OnSelectedIndexChanged(e);
	}

	protected override void OnResize(EventArgs e)
	{
		m_scrollBar.LargeChange = base.ClientSize.Height / ItemHeight;
	}

	private int[] GetSelectedLineNumbers()
	{
		int[] selectedIndexes = GetSelectedIndexes();
		int[] array = new int[selectedIndexes.Length];
		int[] lineNumbersInCollection = CurrentDisplayLines.LineNumbersInCollection;
		for (int i = 0; i < selectedIndexes.Length; i++)
		{
			array[i] = lineNumbersInCollection[selectedIndexes[i]];
		}
		return array;
	}

	private int[] GetSelectedIndexes()
	{
		int num = CallSendMessage(400u, 0, 0);
		int[] array = new int[num];
		Utility.ThrowExceptionIf(num != CallSendMessage(401u, num, array), "LB_GETSELITEMS returned an unexpected value.");
		return array;
	}

	private void SelectNextMatchingLine(LineCollection currentDisplayLines, int selectedIndex, int matchMarker, Filter matchFilter, bool backward)
	{
		int[] lineNumbersInCollection = currentDisplayLines.LineNumbersInCollection;
		int num = lineNumbersInCollection.Length;
		int num2 = selectedIndex;
		int num3 = ((!backward) ? 1 : (-1));
		do
		{
			num2 = (num2 + num3 + num) % num;
			int num4 = lineNumbersInCollection[num2];
			if ((-1 != matchMarker && currentDisplayLines.IsLineMarkedBy(num4, matchMarker)) || (matchFilter != null && matchFilter.FilteredLines.IsLineNumberInCollection(num4)))
			{
				SelectLineNumber(num4);
				break;
			}
		}
		while (num2 != selectedIndex);
	}

	private void ToggleMarks(int marker, LineCollection currentDisplayLines)
	{
		int[] selectedLineNumbers = GetSelectedLineNumbers();
		foreach (int lineNumber in selectedLineNumbers)
		{
			currentDisplayLines.LineToggleMark(lineNumber, marker);
		}
		if (m_textAnalysisTool.FilterCollection.MarkersChanged(marker))
		{
			m_textAnalysisTool.FiltersChanged(updateFilterDisplay: false);
		}
		else
		{
			Invalidate();
		}
		MarkersChanged?.Invoke(this, EventArgs.Empty);
	}

	public void CallOnKeyDown(KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		int selectedIndex = GetSelectedIndex();
		LineCollection currentDisplayLines = CurrentDisplayLines;
		Keys keys = Keys.KeyCode & e.KeyData;
		switch (keys)
		{
		case Keys.Space:
			if (!e.Control && !e.Alt && -1 != selectedIndex && m_filteredLines.Count > 0)
			{
				m_filteredLines.GetNextLineNumberInCollection(currentDisplayLines.LineNumbersInCollection[selectedIndex], out var previousLineNumber, out var nextLineNumber);
				SelectLineNumber(e.Shift ? previousLineNumber : nextLineNumber);
			}
			break;
		case Keys.D1:
		case Keys.D2:
		case Keys.D3:
		case Keys.D4:
		case Keys.D5:
		case Keys.D6:
		case Keys.D7:
		case Keys.D8:
		{
			int num2 = (int)(keys - 49);
			if (!e.Control && !e.Alt && -1 != selectedIndex)
			{
				SelectNextMatchingLine(currentDisplayLines, selectedIndex, num2, null, e.Shift);
			}
			else if (!e.Shift && e.Control && !e.Alt)
			{
				ToggleMarks(num2, currentDisplayLines);
			}
			break;
		}
		case Keys.A:
		case Keys.B:
		case Keys.C:
		case Keys.D:
		case Keys.E:
		case Keys.F:
		case Keys.G:
		case Keys.H:
		case Keys.I:
		case Keys.J:
		case Keys.K:
		case Keys.L:
		case Keys.M:
		case Keys.N:
		case Keys.O:
		case Keys.P:
		case Keys.Q:
		case Keys.R:
		case Keys.S:
		case Keys.T:
		case Keys.U:
		case Keys.V:
		case Keys.W:
		case Keys.X:
		case Keys.Y:
		case Keys.Z:
		{
			int num = (int)(keys - 65);
			if (num < m_textAnalysisTool.FilterCollection.Filters.Count)
			{
				Filter filter = (Filter)m_textAnalysisTool.FilterCollection.Filters[num];
				if (!e.Control && !e.Alt && -1 != selectedIndex)
				{
					filter.FilterLines();
					SelectNextMatchingLine(currentDisplayLines, selectedIndex, -1, filter, e.Shift);
				}
				else if (e.Shift && e.Control && !e.Alt)
				{
					m_textAnalysisTool.FilterCollection.SetEnable(filter, !filter.Enabled);
				}
			}
			break;
		}
		case Keys.Add:
		case Keys.Oemplus:
		case Keys.OemCloseBrackets:
			if (e.Control)
			{
				ZoomIn();
			}
			break;
		case Keys.Subtract:
		case Keys.OemMinus:
		case Keys.OemOpenBrackets:
			if (e.Control)
			{
				ZoomOut();
			}
			break;
		case Keys.NumPad0:
		case Keys.Multiply:
			if (e.Control)
			{
				ResetZoom();
			}
			break;
		}
		base.OnKeyDown(e);
	}

	protected override bool ProcessKeyMessage(ref Message m)
	{
		if (258 != m.Msg)
		{
			return base.ProcessKeyMessage(ref m);
		}
		return true;
	}

	protected override void WndProc(ref Message m)
	{
		if (522 == m.Msg)
		{
			int num = (int)(long)m.WParam >> 16;
			m_accumulatedWheelDelta += num;
			if (120 > Math.Abs(m_accumulatedWheelDelta))
			{
				return;
			}
			int num2 = m_accumulatedWheelDelta / 120;
			m_accumulatedWheelDelta %= 120;
			if (Control.ModifierKeys == Keys.Control)
			{
				if (num2 > 0)
				{
					ZoomIn();
				}
				else if (num2 < 0)
				{
					ZoomOut();
				}
			}
			else if (Control.ModifierKeys == Keys.Shift)
			{
				int scrollPos = GetScrollPos(base.Handle, 0);
				int num3 = 25;
				int num4 = num2 * (int)(m_characterWidthInPixels * (float)num3);
				int val = Math.Max(0, scrollPos - num4);
				val = Math.Min(val, 32000);
				CallSendMessage(276u, 4 | ((val & 0xFFFF) << 16), 0);
			}
			else
			{
				int num5 = Math.Max(base.TopIndex - num2 * 3, 0);
				if (base.TopIndex != num5)
				{
					base.TopIndex = num5;
					m_scrollBar.Value = base.TopIndex;
				}
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.Items.Clear();
		base.OnHandleDestroyed(e);
	}

	public void ResetZoom()
	{
		ZoomPercent = 100;
	}

	public void ZoomIn()
	{
		ZoomPercent = Settings.ZoomPercent + 10;
	}

	public void ZoomOut()
	{
		ZoomPercent = Settings.ZoomPercent - 10;
	}
}
