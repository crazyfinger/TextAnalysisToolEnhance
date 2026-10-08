using System.Drawing;
using System.IO;
using System.Reflection;

namespace TextAnalysisTool.NET;

internal sealed class Resource
{
	public static readonly Icon ApplicationIcon;

	public const string AboutCopyright = "Copyright © 2000-2025\r\nby David L. A. Anson\r\nDavidAns@Microsoft.com\r\n\r\nProtected by United States\r\npatents 6,963,878 and 7,146,379.";

	public const string AboutTitle = "About {0}";

	public const string ApplicationName = "TextAnalysisTool.NET";

	public const string ArgumentUnrecognized = "The following command line argument is not recognized and will be ignored: {0}.";

	public const string ArgumentErrorMultipleConfigs = "A /Config flag has already been supplied on the command line. Ignoring file: {0}.";

	public const string ArgumentErrorInvalidLineNumber = "The value of the /Line command line argument could not be parsed into a positive integer. Ignoring.";

	public const string Clipboard = "Clipboard";

	public const string ClipboardDataUnsupportedFormat = "The data on the clipboard is in an unsupported format.";

	public const string ClipboardExchangeError = "An error occurred while attempting to exchange data with the clipboard: {0}.";

	public const string CloseConfirmation = "There are changes to the filter list that have not been saved.\n\nDo you want to save the filters?";

	public const string ColorCustom = "[Custom]";

	public const string ColorDefault = "[Default]";

	public const string ColumnHeaderShortcut = "";

	public const string ColumnHeaderModifiers = "Modifiers";

	public const string ColumnHeaderFilterPattern = "Pattern";

	public const string ColumnHeaderHits = "Hits";

	public const string ColumnHeaderDescription = "Description";

	public const string DescriptionText = "&Description:";

	public const string ContextMenuFilterAdd = "Add &New Filter...";

	public const string ContextMenuFilterEdit = "&Edit...";

	public const string ContextMenuFilterCopy = "&Copy";

	public const string ContextMenuFilterPaste = "P&aste";

	public const string ContextMenuFilterFindDialog = "&Find...";

	public const string ContextMenuFilterFindPrevious = "&Previous Match";

	public const string ContextMenuFilterFindNext = "Ne&xt Match";

	public const string ContextMenuFilterMoveDown = "Move &Down";

	public const string ContextMenuFilterMoveUp = "Move &Up";

	public const string ContextMenuFilterRemove = "&Remove";

	public const string DialogApply = "&Apply";

	public const string DialogCancel = "Cancel";

	public const string DialogOK = "&OK";

	public const string DialogMore = "More >>";

	public const string DragDropText = "Drag-and-dropped text";

	public const string DragDropError = "An error occurred while attempting to access the dropped data: {0}.";

	public const string EncodingDialogTitle = "Select character encoding";

	public const string EncodingDialogOther = "Other encoding:";

	public const string EncodingDialogSystemDefault = "System Default ({0})";

	public const string FileOpenError = "An error occurred while attempting to read the file: {0}.";

	public const string FileOpenSaveFilterBegin = "Text Files (*.txt;*.log)|*.txt;*.log|";

	public const string FileOpenSaveFilterEnd = "All Files (*)|*";

	public const string FileSaveError = "An error occurred while attempting to write the file: {0}.";

	public const string FilterCaseSensitive = "C&ase-sensitive";

	public const string FilterForegroundColor = "Text Co&lor:";

	public const string FilterBackgroundColor = "&Background:";

	public const string FilterExcluding = "E&xcluding";

	public const string FilterFileParseError = "The file is not a valid {0} filter file.";

	public const string FilterInvalidText = "The text box must contain at least one character of text";

	public const string FilterLoadSaveFilter = "Filter Files (*.tat)|*.tat|All Files (*)|*";

	public const string FilterMarker = "&Marker:";

	public const string FilterMatchesMarker = "Marked by marker";

	public const string FilterMatchesText = "Matches text";

	public const string FilterRegularExpression = "&Regular expression";

	public const string FilterRegularExpressionToolTip = "Right-click this checkbox for help with regular expression syntax";

	public const string FilterRegularExpressionURL = "https://msdn.microsoft.com/en-us/library/hs600312.aspx";

	public const string FilterRegularExpressionURLError = "Unable to display the web page: {0}.";

	public const string FilterText = "&Text:";

	public const string FilterTitleAdd = "Add Filter";

	public const string FilterTitleEdit = "Edit Filter";

	public const string MultiFilterTitleEdit = "Edit Multiple Filters";

	public const string MultipleValuesTextboxText = "[multiple values]";

	public const string MultipleValuesComboboxText = "[multiple]";

	public const string FilterToolTipHeader = "Matching Filters";

	public const string FilterToStringCyclesFormatString = "{1}. {0}";

	public const string FilterToStringMatches = "";

	public const string FilterToStringMarkedFormatString = "[#]";

	public const string FilterToStringRegularExpression = "[R]";

	public const string FilterToStringExcluding = "[!]";

	public const string FilterToStringSensitive = "[Aa]";

	public const string FilterToStringInsensitive = "";

	public const string FilterType = "&Filter:";

	public const string UnnamedFilterLineTooltip = "Unnamed filter ({0})";

	public const string NamedFilterLineTooltip = "{0} ({1})";

	public const string FilterUnrecognized = "One or more filter types were unrecognized and have not been loaded.";

	public const string FindReverse = "Re&verse";

	public const string FindTitle = "Find";

	public const string GoToInvalidLineNumber = "Must specify a line number from 1 to {0}";

	public const string GoToLine = "&Line number:";

	public const string GoToTitle = "Go To";

	public const string HelpDocumentationConfirm = "{0} is about to replace the current lines and filters with lines from its documentation file and a few sample filters.\n\n(To preserve your current data: cancel this operation, open a new {0} window, and view the documentation file in that window.)";

	public const string LineSource = "Source: ";

	public const string MainWindowTitleFormat = "{0} - {1}{2} - {3}";

	public const string MenuEdit = "&Edit";

	public const string MenuEditCopy = "&Copy";

	public const string MenuEditFind = "&Find...";

	public const string MenuEditFindNext = "Find &Next";

	public const string MenuEditFindPrevious = "Find P&revious";

	public const string MenuEditGoTo = "&Go To...";

	public const string MenuEditPaste = "&Paste";

	public const string MenuEditCopyFilters = "Copy F&ilters";

	public const string MenuEditPasteFilters = "Paste Fi&lters";

	public const string MenuEditPluginSettings = "Plug-in &Settings";

	public const string MenuEditSelectAll = "Select &All";

	public const string MenuEditPreferences = "Pr&eferences...";

	public const string MenuFile = "&File";

	public const string MenuFileExit = "E&xit";

	public const string MenuFileLoadFilters = "&Load Filters...";

	public const string MenuFileOpen = "&Open...";

	public const string MenuFileReload = "&Reload";

	public const string MenuFileSaveCurrentLines = "&Save Current Lines...";

	public const string MenuFileSaveFilters = "Sa&ve Filters";

	public const string MenuFileSaveFiltersAs = "S&ave Filters As...";

	public const string MenuFileAppendFilters = "A&ppend to Existing Filters...";

	public const string MenuFileRecentFiles = "Recent &Files";

	public const string MenuFileRecentFilters = "Recent F&ilter Files";

	public const string MenuFileRecentFilesClear = "&Clear Recent Files List";

	public const string MenuFileRecentFiltersClear = "&Clear Recent Filter Files List";

	public const string MenuFilter = "Fi&lters";

	public const string MenuFilterAddFilter = "Add &New Filter...";

	public const string MenuFilterEnableAllFilters = "E&nable All Filters";

	public const string MenuFilterDisableAllFilters = "&Disable All Filters";

	public const string MenuFilterEditFilter = "&Edit Selected Filter(s)...";

	public const string MenuFilterMatchPrev = "&Previous Match";

	public const string MenuFilterMatchNext = "Ne&xt Match";

	public const string MenuFilterRemoveFilter = "&Remove Selected Filter";

	public const string MenuFilterRemoveAllFilters = "Remove A&ll Filters";

	public const string MenuHelp = "&Help";

	public const string MenuHelpAbout = "&About";

	public const string MenuHelpDocumentation = "&Documentation";

	public const string MenuView = "&View";

	public const string MenuViewEncoding = "E&ncoding";

	public const string MenuViewEncoding1252 = "&Windows 1252";

	public const string MenuViewEncodingGB18030 = "&GB18030";

	public const string MenuViewEncodingMoreEncodings = "&More encodings...";

	public const string MenuViewEncodingSystemDefault = "System &Default";

	public const string MenuViewEncodingUtf8 = "UTF-&8";

	public const string MenuViewFilterListLocation = "&Filter List Location";

	public const string MenuViewFilterListLocationBottom = "&Bottom";

	public const string MenuViewFilterListLocationLeft = "&Left";

	public const string MenuViewFilterListLocationRight = "&Right";

	public const string MenuViewFilterListLocationTop = "&Top";

	public const string MenuViewShowOnlyFilteredLines = "S&how Only Filtered Lines";

	public const string MenuViewShowFilterToolTip = "Show Filt&er Tool Tip";

	public const string MenuViewShowLineNumbers = "Show &Line Numbers";

	public const string MenuViewShowMarkers = "Show &Markers";

	public const string MenuViewShowMarkersAlways = "&Always";

	public const string MenuViewShowMarkersNever = "&Never";

	public const string MenuViewShowMarkersWhenInUse = "&Only When In Use";

	public const string MenuViewZoomIn = "Zoom &In";

	public const string MenuViewZoomOut = "Zoom &Out";

	public const string MenuViewZoomReset = "&Reset Zoom";

	public const string PreferencesDialogDefaults = "Restore &Defaults";

	public const string PreferencesDialogImport = "&Import...";

	public const string PreferencesDialogExport = "&Export...";

	public const string PreferencesDialogEncodingValidationError = "The character encoding \"{0}\" is not present on this operating system. Some common encodings are \"UTF-8\", \"GB18030\", and \"Windows-1252\".";

	public const string PreferencesDialogValidationError = "The value should be an integer between {0} and {1}.";

	public const string PreferencesImportTitle = "Import";

	public const string PreferencesExportTitle = "Export";

	public const string PreferencesExportError = "An error occurred while attempting to write the file: {0}.";

	public const string PreferencesImportError = "An error occurred while attempting to read the file: {0}.";

	public const string PreferencesSaveError = "An error occurred while attempting to save preferences: {0}.";

	public const string PreferencesImportExportFilter = "XML Files (*.xml)|*.xml;|All Files (*)|*";

	public const string PreferencesExportChangedMessage = "The settings have been changed.\nDo you wish to apply the settings before exporting them?";

	public const string PreferencesImportChangedMessage = "The settings have been changed.\nDo you want to overwrite your changes?";

	public const string PreferencesImportParseError = "The file is not a valid {0} configuration file.";

	public const string PreferencesTitle = "Preferences";

	public const string ReplaceLinesConfirmation = "{0} is about to replace the current text.";

	public const string ReplacementCharacter = "\ufffd";

	public const string StatusFilteredLinesAbbreviation = "Fil";

	public const string StatusFilteredLinesToolTip = "Filtered Lines";

	public const string StatusFilterListDirtyIndicator = "*";

	public const string StatusNoFileLoaded = "[No file loaded]";

	public const string StatusNoFilterFile = "[No filter file]";

	public const string StatusSelectedLinesAbbreviation = "Sel";

	public const string StatusSelectedLinesToolTip = "Selected Lines";

	public const string StatusTotalLinesAbbreviation = "Total";

	public const string StatusTotalLinesToolTip = "Total Lines";

	public const string StatusZoomLevelNumberFormat = "0 \\%";

	public const string StatusZoomLevelToolTip = "Zoom Level";

	public const string ToggleMarker = "Toggle Marker";

	public const string TruncatedLineFormat = "{0} ... {1}";

	public const string TruncatedLineMessage = "[TextAnalysisTool.NET could not display the whole line. Filtering, copying, and searching are unaffected.]";

	public const string UnexpectedError = "An unexpected error occurred: {0}.";

	public const string UnhandledSecurityException = "Sorry, a system security exception has occurred and the application is about to terminate!\n\nThe most common cause of this problem is attempting to run the application from a network share (by default the .NET Framework does not allow this).  Please copy the executable to your machine and try again.  If the error persists, please report this problem.";

	public const string UnhandledException = "Sorry, an unhandled exception has occurred and the application is about to terminate!\n\nPlease report this problem and include the following details:\n\n{0} - {1}\n{2}";

	public const string PasteTextConfirmationDialogTitle = "Confirm Paste";

	public const string PasteTextConfirmationMessage = "Replace the current text with the contents of the clipboard?";

	public const string PasteTextConfirmationNeverAskAgainMessage = "&Don't show this message again.";

	public const string FileOpenErrorPlugin = "An error occurred while attempting to read the file (via plug-in {0}): {1}.";

	public const string MenuHelpPlugins = "Installed &plug-ins";

	public const string PluginsInstalledNone = "There are no plug-ins installed.\n\n(Please see the documentation for information about how to install plug-ins.)";

	public const string PluginsInstalledHeader = "The following plug-ins are installed:\n";

	public const string PluginsInstalledEntry = "{0}) {1}: {2} ({3})  [{4}, {5}]\n";

	public const string PluginsInstalledEntryDisabled = "{0}) {1}: [Disabled]\n";

	public const string PluginLoadError = "An error occurred while attempting to load plug-in file {0}: {{0}}.";

	public const string PluginUseError = "An error occurred while attempting to use the {0} plug-in (from {1}).  The plug-in has been disabled for this instance of {2}.";

	public const string OptionsBehaviorCategory = "Behavior";

	public const string OptionsBehaviorMaxNumberOfRecentItemsName = "Max Number of Recent Items";

	public const string OptionsBehaviorMaxNumberOfRecentItemsDescription = "Maximum number of recent files or recent filter files displayed in the corresponding File menus.";

	public const string OptionsBehaviorFileNameDisplayStyleName = "File Name Display Style";

	public const string OptionsBehaviorFileNameDisplayStyleDescription = "How to display the file name in the title bar.";

	public const string OptionsBehaviorFileNameDisplayLengthName = "File Name Display Length";

	public const string OptionsBehaviorFileNameDisplayLengthDescription = "Length of the file name to display in the title bar. Only used for styles RetainPathEnd and RetainPathRootAndEnd.";

	public const string OptionsBehaviorBeepOnSearchEndName = "Beep on Search End";

	public const string OptionsBehaviorBeepOnSearchEndDescription = "If set to true, will play a beep when the search wraps around the document.";

	public const string OptionsBehaviorPasteTextOverwriteName = "Overwrite Text on Paste";

	public const string OptionsBehaviorPasteTextOverwriteDescription = "Indicates whether attempts to paste text into the tool will always replace the current text, leave it alone, or ask the user.";

	public const string OptionsBehaviorCopyLineNumbersName = "Copy Line Numbers";

	public const string OptionsBehaviorCopyLineNumbersDescription = "Indicates when to include line numbers when copying text to the clipboard.";

	public const string OptionsBehaviorDefaultEncodingName = "Default Encoding";

	public const string OptionsBehaviorDefaultEncodingDescription = "The character encoding used to read a file. This can be overridden by UTF byte order marks or via the View/Encoding menu.";

	public const string OptionsBehaviorFiltersDirtiedName = "Mark Filters Changed";

	public const string OptionsBehaviorFiltersDirtiedDescription = "Selects when the filter list is considered changed. AddRemoveEdit excludes changes like enable, disable, and move.";

	public const string OptionsBehaviorFiltersColumnLayoutName = "Remember Filter Column Layout";

	public const string OptionsBehaviorFiltersColumnLayoutDescription = "Remember the order and width of columns in the filter list.";

	public const string OptionsBehaviorShowFilterToolTipName = "Show Filter Tool Tip";

	public const string OptionsBehaviorShowFilterToolTipDescription = "Show a tool tip with the filters that match the line the pointer is over.";

	public const string OptionsBehaviorShowLineNumbersName = "Show Line Numbers";

	public const string OptionsBehaviorShowLineNumbersDescription = "Indicates whether or not to show line numbers.";

	public const string OptionsBehaviorShowMarkersColumnName = "Show Markers Column";

	public const string OptionsBehaviorShowMarkersColumnDescription = "Indicates when to show the markers column next to the lines of text.";

	public const string OptionsBehaviorTabSizeName = "Tab Size";

	public const string OptionsBehaviorTabSizeDescription = "Character width of each Tab character ('\\t').";

	public const string OptionsColorsAndFontsCategory = "Colors and Fonts";

	public const string OptionsColorsAndFontsWindowTextColorName = "Window Text Color";

	public const string OptionsColorsAndFontsWindowTextColorDescription = "The text color in the line display area.";

	public const string OptionsColorsAndFontsWindowColorName = "Window Color";

	public const string OptionsColorsAndFontsWindowColorDescription = "The background color in the line display area.";

	public const string OptionsColorsAndFontsGrayTextColorName = "Gray Text Color";

	public const string OptionsColorsAndFontsGrayTextColorDescription = "The text color used both in the Markers Column and for the grayed out text in the line display area.";

	public const string OptionsColorsAndFontsGrayBackgroundColorName = "Gray Background Color";

	public const string OptionsColorsAndFontsGrayBackgroundColorDescription = "The background color used both in the Markers Column and for the grayed out text in the line display area.";

	public const string OptionsColorsAndFontsHighlightTextColorName = "Highlight Text Color";

	public const string OptionsColorsAndFontsHighlightTextColorDescription = "The text color used for the selected lines in the line display area.";

	public const string OptionsColorsAndFontsHighlightColorName = "Highlight Color";

	public const string OptionsColorsAndFontsHighlightColorDescription = "The background color used for the selected lines in the line display area.";

	public const string OptionsColorsAndFontsFilterHighlightColorName = "Filter Highlight Color";

	public const string OptionsColorsAndFontsFilterHighlightColorDescription = "The background color used for the selected filters in the filter display area.";

	public const string OptionsColorsAndFontsFilterHighlightBoxColorName = "Filter Highlight Box Color";

	public const string OptionsColorsAndFontsFilterHighlightBoxColorDescription = "The focus box color drawn around the selected filters in the filter display area.";

	public const string OptionsColorsAndFontsLineFontName = "Line Font";

	public const string OptionsColorsAndFontsLineFontDescription = "The font used for displaying lines.";

	public const string Bottom = "Bottom";

	public const string Left = "Left";

	public const string Right = "Right";

	public const string Top = "Top";

	public const string CtrlDown = "Ctrl+Down";

	public const string CtrlLeft = "Ctrl+Left";

	public const string CtrlRight = "Ctrl+Right";

	public const string CtrlUp = "Ctrl+Up";

	static Resource()
	{
		ApplicationIcon = new Icon(GetStream("TextAnalysisTool.ico"));
	}

	private Resource()
	{
	}

	public static Stream GetStream(string name)
	{
		return Assembly.GetExecutingAssembly().GetManifestResourceStream("TextAnalysisTool.NET." + name);
	}
}
