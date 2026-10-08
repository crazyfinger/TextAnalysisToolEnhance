using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;

namespace TextAnalysisTool.NET;

public class OptionsBehaviorProperties
{
	[Category("Behavior")]
	[DisplayName("Max Number of Recent Items")]
	[Description("Maximum number of recent files or recent filter files displayed in the corresponding File menus.")]
	[DefaultValue(5)]
	[TypeConverter(typeof(MaxNumberOfRecentItemsTypeConverter))]
	public int MaxNumberOfRecentItems { get; set; }

	[Category("Behavior")]
	[DisplayName("File Name Display Style")]
	[Description("How to display the file name in the title bar.")]
	[DefaultValue(FileNameDisplayStyle.FileNameOnly)]
	public FileNameDisplayStyle FileNameDisplayStyle { get; set; }

	[Category("Behavior")]
	[DisplayName("File Name Display Length")]
	[Description("Length of the file name to display in the title bar. Only used for styles RetainPathEnd and RetainPathRootAndEnd.")]
	[DefaultValue(0)]
	[TypeConverter(typeof(FileNameDisplayLengthTypeConverter))]
	public int FileNameDisplayLength { get; set; }

	[Category("Behavior")]
	[DisplayName("Beep on Search End")]
	[Description("If set to true, will play a beep when the search wraps around the document.")]
	[DefaultValue(true)]
	public bool BeepOnSearchEnd { get; set; }

	[Category("Behavior")]
	[DisplayName("Overwrite Text on Paste")]
	[Description("Indicates whether attempts to paste text into the tool will always replace the current text, leave it alone, or ask the user.")]
	[DefaultValue(PasteTextOverwriteEnum.AlwaysAsk)]
	public PasteTextOverwriteEnum PasteTextOverwrite { get; set; }

	[Category("Behavior")]
	[DisplayName("Show Line Numbers")]
	[Description("Indicates whether or not to show line numbers.")]
	[DefaultValue(true)]
	public bool ShowLineNumbers { get; set; }

	[Category("Behavior")]
	[DisplayName("Show Filter Tool Tip")]
	[Description("Show a tool tip with the filters that match the line the pointer is over.")]
	[DefaultValue(true)]
	public bool ShowFilterToolTip { get; set; }

	[Category("Behavior")]
	[DisplayName("Copy Line Numbers")]
	[Description("Indicates when to include line numbers when copying text to the clipboard.")]
	[DefaultValue(CopyLineNumbersState.WhenVisible)]
	public CopyLineNumbersState CopyLineNumbers { get; set; }

	[Category("Behavior")]
	[DisplayName("Show Markers Column")]
	[Description("Indicates when to show the markers column next to the lines of text.")]
	[DefaultValue(ShowMarkersState.WhenInUse)]
	public ShowMarkersState ShowMarkersColumn { get; set; }

	[Category("Behavior")]
	[DisplayName("Mark Filters Changed")]
	[Description("Selects when the filter list is considered changed. AddRemoveEdit excludes changes like enable, disable, and move.")]
	[DefaultValue(FiltersDirtiedState.Always)]
	public FiltersDirtiedState FiltersDirtied { get; set; }

	[Category("Behavior")]
	[DisplayName("Remember Filter Column Layout")]
	[Description("Remember the order and width of columns in the filter list.")]
	[DefaultValue(true)]
	public bool RememberFilterColumnLayout { get; set; }

	[Category("Behavior")]
	[DisplayName("Tab Size")]
	[Description("Character width of each Tab character ('\\t').")]
	[DefaultValue(4)]
	[TypeConverter(typeof(TabSizeTypeConverter))]
	public int TabSize { get; set; }

	[Category("Behavior")]
	[DisplayName("Default Encoding")]
	[Description("The character encoding used to read a file. This can be overridden by UTF byte order marks or via the View/Encoding menu.")]
	[DefaultValue("System Default")]
	[Editor(typeof(EncodingNameEditor), typeof(UITypeEditor))]
	public string DefaultEncodingName { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Window Text Color")]
	[Description("The text color in the line display area.")]
	[DefaultValue(typeof(Color), "WindowText")]
	[XmlElement(typeof(XmlColor))]
	public Color WindowTextColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Window Color")]
	[Description("The background color in the line display area.")]
	[DefaultValue(typeof(Color), "Window")]
	[XmlElement(typeof(XmlColor))]
	public Color WindowColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Gray Text Color")]
	[Description("The text color used both in the Markers Column and for the grayed out text in the line display area.")]
	[DefaultValue(typeof(Color), "GrayText")]
	[XmlElement(typeof(XmlColor))]
	public Color GrayTextColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Gray Background Color")]
	[Description("The background color used both in the Markers Column and for the grayed out text in the line display area.")]
	[DefaultValue(typeof(Color), "MenuBar")]
	[XmlElement(typeof(XmlColor))]
	public Color GrayBackgroundColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Highlight Text Color")]
	[Description("The text color used for the selected lines in the line display area.")]
	[DefaultValue(typeof(Color), "HighlightText")]
	[XmlElement(typeof(XmlColor))]
	public Color HighlightTextColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Highlight Color")]
	[Description("The background color used for the selected lines in the line display area.")]
	[DefaultValue(typeof(Color), "Highlight")]
	[XmlElement(typeof(XmlColor))]
	public Color HighlightColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Filter Highlight Color")]
	[Description("The background color used for the selected filters in the filter display area.")]
	[DefaultValue(typeof(Color), "ControlDark")]
	[XmlElement(typeof(XmlColor))]
	public Color FilterHighlightColor { get; set; }

	[Category("Colors and Fonts")]
	[DisplayName("Filter Highlight Box Color")]
	[Description("The focus box color drawn around the selected filters in the filter display area.")]
	[DefaultValue(typeof(Color), "ControlDarkDark")]
	[XmlElement(typeof(XmlColor))]
	public Color FilterHighlightBoxColor { get; set; }

	[XmlIgnore]
	[Category("Colors and Fonts")]
	[DisplayName("Line Font")]
	[Description("The font used for displaying lines.")]
	[TypeConverter(typeof(FontConverterNameAndSize))]
	public Font LineFont { get; set; }

	[Browsable(false)]
	public string Serializable_LineFont
	{
		get
		{
			return TypeDescriptor.GetConverter(typeof(Font)).ConvertToString(LineFont);
		}
		set
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
			LineFont = (Font)converter.ConvertFromString(value);
		}
	}

	[Browsable(false)]
	public string FilterColumnLayout { get; set; }

	public override bool Equals(object obj)
	{
		OptionsBehaviorProperties options = obj as OptionsBehaviorProperties;
		return Equals(options);
	}

	public bool Equals(OptionsBehaviorProperties options)
	{
		if (options == null)
		{
			return false;
		}
		if (MaxNumberOfRecentItems == options.MaxNumberOfRecentItems && FileNameDisplayStyle == options.FileNameDisplayStyle && FileNameDisplayLength == options.FileNameDisplayLength && TabSize == options.TabSize && BeepOnSearchEnd == options.BeepOnSearchEnd && PasteTextOverwrite == options.PasteTextOverwrite && ShowLineNumbers == options.ShowLineNumbers && ShowFilterToolTip == options.ShowFilterToolTip && CopyLineNumbers == options.CopyLineNumbers && ShowMarkersColumn == options.ShowMarkersColumn && FiltersDirtied == options.FiltersDirtied && RememberFilterColumnLayout == options.RememberFilterColumnLayout && FilterColumnLayout == options.FilterColumnLayout && WindowColor == options.WindowColor && WindowTextColor == options.WindowTextColor && GrayBackgroundColor == options.GrayBackgroundColor && GrayTextColor == options.GrayTextColor && HighlightColor == options.HighlightColor && FilterHighlightColor == options.FilterHighlightColor && FilterHighlightBoxColor == options.FilterHighlightBoxColor && HighlightTextColor == options.HighlightTextColor && string.Compare(LineFont.FontFamily.Name, options.LineFont.FontFamily.Name, ignoreCase: true) == 0 && LineFont.Size == options.LineFont.Size)
		{
			return string.Compare(DefaultEncodingName, options.DefaultEncodingName, ignoreCase: true) == 0;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public OptionsBehaviorProperties Clone()
	{
		return new OptionsBehaviorProperties
		{
			MaxNumberOfRecentItems = MaxNumberOfRecentItems,
			FileNameDisplayStyle = FileNameDisplayStyle,
			FileNameDisplayLength = FileNameDisplayLength,
			TabSize = TabSize,
			BeepOnSearchEnd = BeepOnSearchEnd,
			PasteTextOverwrite = PasteTextOverwrite,
			ShowLineNumbers = ShowLineNumbers,
			ShowFilterToolTip = ShowFilterToolTip,
			CopyLineNumbers = CopyLineNumbers,
			ShowMarkersColumn = ShowMarkersColumn,
			FiltersDirtied = FiltersDirtied,
			RememberFilterColumnLayout = RememberFilterColumnLayout,
			FilterColumnLayout = FilterColumnLayout,
			WindowColor = WindowColor,
			WindowTextColor = WindowTextColor,
			GrayBackgroundColor = GrayBackgroundColor,
			GrayTextColor = GrayTextColor,
			HighlightColor = HighlightColor,
			FilterHighlightColor = FilterHighlightColor,
			FilterHighlightBoxColor = FilterHighlightBoxColor,
			HighlightTextColor = HighlightTextColor,
			LineFont = CTextAnalysisTool.SafeCreateFont(Settings.FontFamily, Settings.FontSize),
			DefaultEncodingName = DefaultEncodingName
		};
	}

	public bool ShouldSerializeLineFont()
	{
		if (!(LineFont.FontFamily.Name != Settings.DefaultFontFamily))
		{
			return LineFont.Size != 10f;
		}
		return true;
	}
}
