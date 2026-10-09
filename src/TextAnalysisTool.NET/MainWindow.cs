using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TextAnalysisTool.NET.Properties;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class MainWindow : Form
{
	private readonly CTextAnalysisTool m_textAnalysisTool;

	private readonly FullyEnabledMenuItem m_menuFileReload;

	private readonly FullyEnabledMenuItem m_menuFileSaveCurrentLines;

	private readonly FullyEnabledMenuItem m_menuFile;

	private readonly FullyEnabledMenuItem m_menuFileRecentFiles;

	private readonly FullyEnabledMenuItem m_menuFileRecentFilters;

	private readonly FullyEnabledMenuItem m_menuFileSaveFilters;

	private readonly FullyEnabledMenuItem m_menuEditCopy;

	private readonly FullyEnabledMenuItem m_menuEditCopyLegacy;

	private readonly FullyEnabledMenuItem m_menuEditPaste;

	private readonly FullyEnabledMenuItem m_menuEditPasteLegacy;

	private readonly FullyEnabledMenuItem m_menuEditSelectAll;

	private readonly FullyEnabledMenuItem m_menuEditFind;

	private readonly FullyEnabledMenuItem m_menuEditFindReverse;

	private readonly FullyEnabledMenuItem m_menuEditFindPrevious;

	private readonly FullyEnabledMenuItem m_menuEditFindNext;

	private readonly FullyEnabledMenuItem m_menuEditGoTo;

	private readonly FullyEnabledMenuItem m_menuEditFocusRefine;

	private readonly FullyEnabledMenuItem m_menuEditClearRefine;

	private Control m_lastFocusedBeforeRefine;

	private readonly FullyEnabledMenuItem m_menuEditPreferences;

	private readonly FullyEnabledMenuItem m_menuEdit;

	private readonly FullyEnabledMenuItem m_menuViewEncoding;

	private readonly FullyEnabledMenuItem m_menuViewFilterListLeft;

	private readonly FullyEnabledMenuItem m_menuViewFilterListRight;

	private readonly FullyEnabledMenuItem m_menuViewFilterListTop;

	private readonly FullyEnabledMenuItem m_menuViewFilterListBottom;

	private readonly FullyEnabledMenuItem m_menuViewFilterListLocation;

	private readonly FullyEnabledMenuItem m_menuViewShowOnlyFilteredLines;

	private readonly FullyEnabledMenuItem m_menuViewShowLineNumbers;

	private readonly FullyEnabledMenuItem m_menuViewShowFilterToolTip;

	private readonly FullyEnabledMenuItem m_menuViewMarkersAlways;

	private readonly FullyEnabledMenuItem m_menuViewMarkersNever;

	private readonly FullyEnabledMenuItem m_menuViewMarkersWhenInUse;

	private readonly FullyEnabledMenuItem m_menuViewShowMarkers;

	private readonly FullyEnabledMenuItem m_menuViewZoomIn;

	private readonly FullyEnabledMenuItem m_menuViewZoomOut;

	private readonly FullyEnabledMenuItem m_menuViewZoomReset;

	private readonly FullyEnabledMenuItem m_menuView;

	private readonly FullyEnabledMenuItem m_menuFilterEditFilter;

	private readonly FullyEnabledMenuItem m_menuFilterRemoveFilter;

	private readonly FullyEnabledMenuItem m_menuFilterMatchPrev;

	private readonly FullyEnabledMenuItem m_menuFilterMatchNext;

	private readonly FullyEnabledMenuItem m_menuEditCopyFilters;

	private readonly FullyEnabledMenuItem m_menuEditPasteFilters;

	private readonly FullyEnabledMenuItem m_menuFilterEnableAllFilters;

	private readonly FullyEnabledMenuItem m_menuFilterDisableAllFilters;

	private readonly FullyEnabledMenuItem m_menuFilterRemoveAllFilters;

	private readonly FullyEnabledMenuItem m_menuFilter;

	private readonly LineCollectionDisplay m_lineCollectionDisplay;

	private readonly Splitter m_splitter;

	private readonly RefineFilterControl m_refineFilterControl;

	private readonly FilterCollectionDisplay m_filterCollectionDisplay;

	private readonly MarkedLinesDisplay m_markedLinesDisplay;

	private readonly Panel m_filterPane;

	private readonly Splitter m_filterSplitter;

	private bool m_initialMarkedSplitDone;

	private readonly StatusStrip m_statusStrip;

	private readonly ToolStripStatusLabel m_statusMessage;

	private readonly ToolStripStatusLabel m_statusFiltering;

	private readonly ToolStripStatusLabel m_statusSelection;

	private readonly ToolStripStatusLabel m_statusLines;

	private readonly ToolStripStatusLabel m_statusTotal;

	private readonly ToolStripStatusLabel m_statusZoom;

	private int m_fileOpenFilterIndex;

	private int m_fileSaveCurrentLinesFilterIndex;

	private int m_fileLoadFiltersFilterIndex;

	private int m_fileSaveFiltersFilterIndex;

	private int m_preferencesExportFilterIndex;

	private int m_preferencesImportFilterIndex;

	public string StatusMessage
	{
		set
		{
			m_statusMessage.Text = value;
			m_statusMessage.ToolTipText = value;
		}
	}

	private int LineCollectionDisplayWidth => base.ClientRectangle.Width * Settings.SplitterPositionPercent / 100;

	private int DisplayAreaHeight => base.ClientRectangle.Height - m_statusStrip.Height - base.MainMenuStrip.Height;

	private int LineCollectionDisplayHeight => DisplayAreaHeight * Settings.SplitterPositionPercent / 100;

	public MainWindow(CTextAnalysisTool textAnalysisTool, LineCollectionDisplay lineCollectionDisplay, FilterCollectionDisplay filterCollectionDisplay)
	{
		m_textAnalysisTool = textAnalysisTool;
		MenuStrip menuStrip = new MenuStrip();
		menuStrip.SuspendLayout();
		SuspendLayout();
		base.StartPosition = Settings.WindowPositionStart;
		if (base.StartPosition == FormStartPosition.Manual)
		{
			base.WindowState = Settings.WindowPositionState;
			base.Bounds = Settings.WindowPositionRectangle;
		}
		MinimumSize = new Size(200, 200);
		Text = "TextAnalysisTool.NET";
		base.Icon = Resource.ApplicationIcon;
		AllowDrop = true;
		base.FormClosing += MainWindow_FormClosing;
		FullyEnabledMenuItem fullyEnabledMenuItem = new FullyEnabledMenuItem("&Open...", null, FileOpen_Click, Keys.O | Keys.Control);
		m_menuFileReload = new FullyEnabledMenuItem("&Reload", null, FileReload_Click, Keys.F5);
		m_menuFileSaveCurrentLines = new FullyEnabledMenuItem("&Save Current Lines...", null, FileSaveCurrentLines_Click, Keys.S | Keys.Control);
		FullyEnabledMenuItem fullyEnabledMenuItem2 = new FullyEnabledMenuItem("&Load Filters...", null, FileLoadFilters_Click, Keys.L | Keys.Control);
		m_menuFileSaveFilters = new FullyEnabledMenuItem("Sa&ve Filters", null, FileSaveFilters_Click);
		FullyEnabledMenuItem fullyEnabledMenuItem3 = new FullyEnabledMenuItem("S&ave Filters As...", null, FileSaveFiltersAs_Click);
		FullyEnabledMenuItem fullyEnabledMenuItem4 = new FullyEnabledMenuItem("A&ppend to Existing Filters...", null, FileAppendFilters_Click);
		m_menuFileRecentFiles = new FullyEnabledMenuItem("Recent &Files");
		m_menuFileRecentFilters = new FullyEnabledMenuItem("Recent F&ilter Files");
		FullyEnabledMenuItem fullyEnabledMenuItem5 = new FullyEnabledMenuItem("E&xit", null, FileExit_Click);
		m_menuFile = new FullyEnabledMenuItem("&File");
		m_menuFile.DropDownItems.AddRange(new ToolStripItem[13]
		{
			fullyEnabledMenuItem,
			m_menuFileReload,
			m_menuFileSaveCurrentLines,
			new ToolStripSeparator(),
			fullyEnabledMenuItem2,
			m_menuFileSaveFilters,
			fullyEnabledMenuItem3,
			fullyEnabledMenuItem4,
			new ToolStripSeparator(),
			m_menuFileRecentFiles,
			m_menuFileRecentFilters,
			new ToolStripSeparator(),
			fullyEnabledMenuItem5
		});
		m_menuFile.DropDownOpening += Menu_Opening;
		EventHandler onClick = EditCopy_Click;
		m_menuEditCopy = new FullyEnabledMenuItem("&Copy", null, onClick, Keys.C | Keys.Control);
		m_menuEditCopyLegacy = new FullyEnabledMenuItem("", null, onClick, Keys.Insert | Keys.Control);
		m_menuEditCopyLegacy.Visible = false;
		EventHandler onClick2 = EditPaste_Click;
		m_menuEditPaste = new FullyEnabledMenuItem("&Paste", null, onClick2, Keys.V | Keys.Control);
		m_menuEditPasteLegacy = new FullyEnabledMenuItem("", null, onClick2, Keys.Insert | Keys.Shift);
		m_menuEditPasteLegacy.Visible = false;
		m_menuEditCopyFilters = new FullyEnabledMenuItem("Copy F&ilters", null, onClick);
		m_menuEditPasteFilters = new FullyEnabledMenuItem("Paste Fi&lters", null, onClick2);
		m_menuEditSelectAll = new FullyEnabledMenuItem("Select &All", null, EditSelectAll_Click, Keys.A | Keys.Control);
		m_menuEditFind = new FullyEnabledMenuItem("&Find...", null, EditFind_Click, Keys.F | Keys.Control);
		m_menuEditFindReverse = new FullyEnabledMenuItem(null, null, EditFind_Click);
		m_menuEditFindReverse.Visible = false;
		m_menuEditFindPrevious = new FullyEnabledMenuItem("Find P&revious", null, EditFindPrevious_Click, Keys.F2);
		m_menuEditFindNext = new FullyEnabledMenuItem("Find &Next", null, EditFindNext_Click, Keys.F3);
		m_menuEditGoTo = new FullyEnabledMenuItem("&Go To...", null, EditGoTo_Click, Keys.G | Keys.Control);
		m_menuEditFocusRefine = new FullyEnabledMenuItem("Focus Re&fine Box", null, EditFocusRefine_Click, Keys.E | Keys.Control);
		m_menuEditClearRefine = new FullyEnabledMenuItem("Clea&r Refine Filter", null, EditClearRefine_Click, Keys.K | Keys.Control);
		m_menuEditPreferences = new FullyEnabledMenuItem("Pr&eferences...", null, EditPreferences_Click);
		m_menuEdit = new FullyEnabledMenuItem("&Edit");
		m_menuEdit.DropDownItems.AddRange(new ToolStripItem[20]
		{
			m_menuEditCopy,
			m_menuEditCopyLegacy,
			m_menuEditPaste,
			m_menuEditPasteLegacy,
			new ToolStripSeparator(),
			m_menuEditCopyFilters,
			m_menuEditPasteFilters,
			new ToolStripSeparator(),
			m_menuEditSelectAll,
			new ToolStripSeparator(),
			m_menuEditFind,
			m_menuEditFindReverse,
			m_menuEditFindPrevious,
			m_menuEditFindNext,
			new ToolStripSeparator(),
			m_menuEditGoTo,
			m_menuEditFocusRefine,
			m_menuEditClearRefine,
			new ToolStripSeparator(),
			m_menuEditPreferences
		});
		m_menuEdit.DropDownOpening += Menu_Opening;
		m_menuViewShowOnlyFilteredLines = new FullyEnabledMenuItem("S&how Only Filtered Lines", null, ViewShowOnlyFilteredLines_Click, Keys.H | Keys.Control);
		m_menuViewShowLineNumbers = new FullyEnabledMenuItem("Show &Line Numbers", null, ViewShowLineNumbers_Click);
		m_menuViewShowFilterToolTip = new FullyEnabledMenuItem("Show Filt&er Tool Tip", null, ViewShowFilterToolTip_Click);
		m_menuViewMarkersAlways = new FullyEnabledMenuItem("&Always", null, ViewShowMarkers_Click);
		m_menuViewMarkersAlways.Tag = ShowMarkersState.Always;
		m_menuViewMarkersNever = new FullyEnabledMenuItem("&Never", null, ViewShowMarkers_Click);
		m_menuViewMarkersNever.Tag = ShowMarkersState.Never;
		m_menuViewMarkersWhenInUse = new FullyEnabledMenuItem("&Only When In Use", null, ViewShowMarkers_Click);
		m_menuViewMarkersWhenInUse.Tag = ShowMarkersState.WhenInUse;
		m_menuViewShowMarkers = new FullyEnabledMenuItem("Show &Markers");
		m_menuViewShowMarkers.DropDownItems.AddRange(new ToolStripItem[3] { m_menuViewMarkersAlways, m_menuViewMarkersNever, m_menuViewMarkersWhenInUse });
		m_menuViewZoomIn = new FullyEnabledMenuItem("Zoom &In", null, ViewZoomIn_Click);
		m_menuViewZoomIn.ShortcutKeyDisplayString = "Ctrl++";
		m_menuViewZoomOut = new FullyEnabledMenuItem("Zoom &Out", null, ViewZoomOut_Click);
		m_menuViewZoomOut.ShortcutKeyDisplayString = "Ctrl+-";
		m_menuViewZoomReset = new FullyEnabledMenuItem("&Reset Zoom", null, ViewZoomReset_Click, Keys.D0 | Keys.Control);
		m_menuViewFilterListLeft = new FullyEnabledMenuItem("Left", null, ViewFilterList_Click, Keys.Left | Keys.Control);
		m_menuViewFilterListLeft.Tag = DockStyle.Left;
		m_menuViewFilterListLeft.ShortcutKeyDisplayString = "Ctrl+Left";
		m_menuViewFilterListRight = new FullyEnabledMenuItem("Right", null, ViewFilterList_Click, Keys.Right | Keys.Control);
		m_menuViewFilterListRight.Tag = DockStyle.Right;
		m_menuViewFilterListRight.ShortcutKeyDisplayString = "Ctrl+Right";
		m_menuViewFilterListTop = new FullyEnabledMenuItem("Top", null, ViewFilterList_Click, Keys.Up | Keys.Control);
		m_menuViewFilterListTop.Tag = DockStyle.Top;
		m_menuViewFilterListTop.ShortcutKeyDisplayString = "Ctrl+Up";
		m_menuViewFilterListBottom = new FullyEnabledMenuItem("Bottom", null, ViewFilterList_Click, Keys.Down | Keys.Control);
		m_menuViewFilterListBottom.Tag = DockStyle.Bottom;
		m_menuViewFilterListBottom.ShortcutKeyDisplayString = "Ctrl+Down";
		m_menuViewFilterListLocation = new FullyEnabledMenuItem("&Filter List Location");
		m_menuViewFilterListLocation.DropDownItems.AddRange(new ToolStripItem[4] { m_menuViewFilterListLeft, m_menuViewFilterListRight, m_menuViewFilterListTop, m_menuViewFilterListBottom });
		m_menuViewFilterListLocation.DropDownOpening += Menu_Opening;
		m_menuViewEncoding = new FullyEnabledMenuItem("E&ncoding");
		m_menuViewEncoding.DropDownOpening += Menu_Opening;
		m_menuViewEncoding.DropDownItems.AddRange(new ToolStripItem[7]
		{
			new FullyEnabledMenuItem("System &Default", null, ViewSetEncoding_Click)
			{
				Tag = ""
			},
			new ToolStripSeparator(),
			new FullyEnabledMenuItem("&Windows 1252", null, ViewSetEncoding_Click)
			{
				Tag = "Windows-1252"
			},
			new FullyEnabledMenuItem("&GB18030", null, ViewSetEncoding_Click)
			{
				Tag = "GB18030"
			},
			new FullyEnabledMenuItem("UTF-&8", null, ViewSetEncoding_Click)
			{
				Tag = "utf-8"
			},
			new ToolStripSeparator(),
			new FullyEnabledMenuItem("&More encodings...", null, ViewSetEncodingDialog_Click)
		});
		m_menuView = new FullyEnabledMenuItem("&View");
		m_menuView.DropDownItems.AddRange(new ToolStripItem[11]
		{
			m_menuViewShowOnlyFilteredLines,
			m_menuViewShowLineNumbers,
			m_menuViewShowFilterToolTip,
			m_menuViewShowMarkers,
			new ToolStripSeparator(),
			m_menuViewZoomIn,
			m_menuViewZoomOut,
			m_menuViewZoomReset,
			new ToolStripSeparator(),
			m_menuViewFilterListLocation,
			m_menuViewEncoding
		});
		m_menuView.DropDownOpening += Menu_Opening;
		FullyEnabledMenuItem fullyEnabledMenuItem6 = new FullyEnabledMenuItem("Add &New Filter...", null, FilterAddFilter_Click, Keys.N | Keys.Control);
		m_menuFilterEditFilter = new FullyEnabledMenuItem("&Edit Selected Filter(s)...", null, FilterEditFilter_Click);
		m_menuFilterRemoveFilter = new FullyEnabledMenuItem("&Remove Selected Filter", null, FilterRemoveFilter_Click);
		m_menuFilterMatchPrev = new FullyEnabledMenuItem("&Previous Match", null, FilterMatchPrev_Click, Keys.F8 | Keys.Shift);
		m_menuFilterMatchNext = new FullyEnabledMenuItem("Ne&xt Match", null, FilterMatchNext_Click, Keys.F8);
		m_menuFilterEnableAllFilters = new FullyEnabledMenuItem("E&nable All Filters", null, FilterEnableAllFilters_Click);
		m_menuFilterDisableAllFilters = new FullyEnabledMenuItem("&Disable All Filters", null, FilterDisableAllFilters_Click);
		m_menuFilterRemoveAllFilters = new FullyEnabledMenuItem("Remove A&ll Filters", null, FilterRemoveAllFilters_Click);
		m_menuFilter = new FullyEnabledMenuItem("Fi&lters");
		m_menuFilter.DropDownItems.AddRange(new ToolStripItem[11]
		{
			m_menuFilterMatchPrev,
			m_menuFilterMatchNext,
			new ToolStripSeparator(),
			fullyEnabledMenuItem6,
			new ToolStripSeparator(),
			m_menuFilterEditFilter,
			m_menuFilterRemoveFilter,
			new ToolStripSeparator(),
			m_menuFilterEnableAllFilters,
			m_menuFilterDisableAllFilters,
			m_menuFilterRemoveAllFilters
		});
		m_menuFilter.DropDownOpening += Menu_Opening;
		FullyEnabledMenuItem fullyEnabledMenuItem7 = new FullyEnabledMenuItem("&Documentation", null, HelpDocumentation_Click, Keys.F1);
		FullyEnabledMenuItem fullyEnabledMenuItem8 = new FullyEnabledMenuItem("Installed &plug-ins", null, HelpPlugins_Click);
		FullyEnabledMenuItem fullyEnabledMenuItem9 = new FullyEnabledMenuItem("&About", null, HelpAbout_Click);
		FullyEnabledMenuItem fullyEnabledMenuItem10 = new FullyEnabledMenuItem("&Help");
		fullyEnabledMenuItem10.DropDownItems.AddRange(new ToolStripItem[5]
		{
			fullyEnabledMenuItem7,
			new ToolStripSeparator(),
			fullyEnabledMenuItem8,
			new ToolStripSeparator(),
			fullyEnabledMenuItem9
		});
		menuStrip.Items.AddRange(new ToolStripItem[5] { m_menuFile, m_menuEdit, m_menuView, m_menuFilter, fullyEnabledMenuItem10 });
		m_lineCollectionDisplay = lineCollectionDisplay;
		m_lineCollectionDisplay.TabIndex = 0;
		m_lineCollectionDisplay.ListBox.ZoomLevelChanged += m_lineCollectionDisplay_ZoomLevelChanged;
		m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLinesChanged += m_lineCollectionDisplay_ShowOnlyFilteredLinesChanged;
		m_lineCollectionDisplay.ListBox.SelectedIndexChanged += m_lineCollectionDisplay_SelectedIndexChanged;
		m_lineCollectionDisplay.ListBox.DoubleClick += m_lineCollectionDisplay_DoubleClick;
		m_lineCollectionDisplay.ListBox.KeyDown += m_lineCollectionDisplay_KeyDown;
		m_splitter = new Splitter();
		m_splitter.SplitterMoving += OnSplitterMoving;
		m_refineFilterControl = new RefineFilterControl();
		m_refineFilterControl.CriteriaChanged += m_refineFilterControl_RefineTextChanged;
		m_refineFilterControl.Dock = DockStyle.Top;
		m_filterCollectionDisplay = filterCollectionDisplay;
		m_filterCollectionDisplay.Dock = DockStyle.Fill;
		m_filterCollectionDisplay.TabIndex = 1;
		float dPIScalingFactor = ControlExtensions.GetDPIScalingFactor();
		m_markedLinesDisplay = new MarkedLinesDisplay(m_lineCollectionDisplay.ListBox);
		m_markedLinesDisplay.Dock = DockStyle.Fill;
		m_filterPane = new Panel();
		m_filterPane.Dock = DockStyle.Fill;
		m_filterSplitter = new Splitter();
		m_filterSplitter.Width = (int)(4f * dPIScalingFactor);
		m_filterSplitter.Height = (int)(4f * dPIScalingFactor);
		m_filterSplitter.Cursor = Cursors.SizeWE;
		m_lineCollectionDisplay.ListBox.MarkersChanged += LineCollectionDisplayListBox_MarkersChanged;
		m_filterPane.Controls.Add(m_filterCollectionDisplay);
		m_filterPane.Controls.Add(m_filterSplitter);
		m_filterPane.Controls.Add(m_markedLinesDisplay);
		m_filterPane.Controls.Add(m_refineFilterControl);
		m_statusStrip = new StatusStrip();
		m_statusStrip.Dock = DockStyle.Bottom;
		m_statusStrip.ShowItemToolTips = true;
		m_statusMessage = new ToolStripStatusLabel();
		m_statusMessage.AutoSize = true;
		m_statusMessage.TextAlign = ContentAlignment.MiddleLeft;
		m_statusMessage.Spring = true;
		m_statusMessage.Text = "[No file loaded]";
		m_statusMessage.ToolTipText = "[No file loaded]";
		m_statusStrip.Items.Add(m_statusMessage);
		m_statusFiltering = new ToolStripStatusLabel();
		m_statusFiltering.BorderStyle = Border3DStyle.Etched;
		m_statusFiltering.BorderSides = ToolStripStatusLabelBorderSides.Left;
		m_statusFiltering.AutoSize = false;
		m_statusFiltering.ImageAlign = ContentAlignment.MiddleCenter;
		m_statusFiltering.ToolTipText = "S&how Only Filtered Lines".Replace("&", "");
		m_statusFiltering.Image = Resources.FilterImage;
		m_statusSelection = new ToolStripStatusLabel();
		m_statusSelection.BorderStyle = Border3DStyle.Etched;
		m_statusSelection.BorderSides = ToolStripStatusLabelBorderSides.Left;
		m_statusSelection.AutoSize = false;
		m_statusSelection.TextAlign = ContentAlignment.MiddleLeft;
		m_statusSelection.Alignment = ToolStripItemAlignment.Left;
		m_statusSelection.Size = new Size((int)(100f * dPIScalingFactor), m_statusSelection.Height);
		m_statusSelection.ToolTipText = "Selected Lines";
		m_statusStrip.Items.Add(m_statusSelection);
		m_statusLines = new ToolStripStatusLabel();
		m_statusLines.BorderStyle = Border3DStyle.Sunken;
		m_statusLines.AutoSize = false;
		m_statusLines.TextAlign = ContentAlignment.MiddleLeft;
		m_statusLines.Size = new Size((int)(100f * dPIScalingFactor), m_statusLines.Height);
		m_statusLines.ToolTipText = "Filtered Lines";
		m_statusStrip.Items.Add(m_statusLines);
		m_statusTotal = new ToolStripStatusLabel();
		m_statusTotal.AutoSize = false;
		m_statusTotal.TextAlign = ContentAlignment.MiddleLeft;
		m_statusTotal.Size = new Size((int)(100f * dPIScalingFactor), m_statusTotal.Height);
		m_statusTotal.ToolTipText = "Total Lines";
		m_statusStrip.Items.Add(m_statusTotal);
		m_statusZoom = new ToolStripStatusLabel();
		m_statusZoom.BorderStyle = Border3DStyle.Etched;
		m_statusZoom.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
		m_statusZoom.AutoSize = false;
		m_statusZoom.TextAlign = ContentAlignment.MiddleCenter;
		m_statusZoom.Size = new Size((int)(50f * dPIScalingFactor), m_statusZoom.Height);
		m_statusZoom.ToolTipText = "Zoom Level";
		m_statusZoom.Text = m_lineCollectionDisplay.ListBox.ZoomPercent.ToString("0 \\%");
		m_statusStrip.Items.Add(m_statusZoom);
		base.Controls.AddRange(new Control[4] { m_filterPane, m_splitter, m_lineCollectionDisplay, m_statusStrip });
		base.Controls.Add(menuStrip);
		base.MainMenuStrip = menuStrip;
		SetFilterListLocation(Settings.FilterListLocation);
		menuStrip.ResumeLayout(performLayout: false);
		menuStrip.PerformLayout();
		ResumeLayout(performLayout: false);
		PerformLayout();
		m_fileOpenFilterIndex = 1;
		m_fileSaveCurrentLinesFilterIndex = 1;
		m_fileLoadFiltersFilterIndex = 1;
		m_fileSaveFiltersFilterIndex = 1;
		m_preferencesExportFilterIndex = 1;
		m_preferencesImportFilterIndex = 1;
	}

	public void CreatePluginSettingsMenu(ArrayList plugins)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = new FullyEnabledMenuItem("Plug-in &Settings");
		foreach (PluginWrapper plugin in plugins)
		{
			if (plugin.GetSettingsDialog() != null)
			{
				FullyEnabledMenuItem fullyEnabledMenuItem2 = new FullyEnabledMenuItem(plugin.Name, null, PluginSettings_Click);
				fullyEnabledMenuItem2.Tag = plugin;
				fullyEnabledMenuItem.DropDownItems.Add(fullyEnabledMenuItem2);
			}
		}
		if (fullyEnabledMenuItem.DropDownItems.Count > 0)
		{
			m_menuEdit.DropDownItems.Add(fullyEnabledMenuItem);
		}
	}

	private bool ShouldPromptToSaveFilters()
	{
		if (!string.IsNullOrEmpty(m_textAnalysisTool.FilterFileName))
		{
			return m_textAnalysisTool.FilterCollection.IsDirty;
		}
		return false;
	}

	private DialogResult PromptToSaveFilters()
	{
		return CTextAnalysisTool.ShowMessageBox(string.Format("There are changes to the filter list that have not been saved.\n\nDo you want to save the filters?", "TextAnalysisTool.NET"), MessageBoxIcon.Exclamation, MessageBoxButtons.YesNoCancel);
	}

	private bool SaveFiltersIfNeeded()
	{
		bool result = false;
		if (ShouldPromptToSaveFilters())
		{
			switch (PromptToSaveFilters())
			{
			case DialogResult.Yes:
				FileSaveFilters(m_textAnalysisTool.FilterFileName);
				break;
			case DialogResult.Cancel:
				result = true;
				break;
			}
		}
		return result;
	}

	private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
	{
		if ((e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.WindowsShutDown) && ShouldPromptToSaveFilters())
		{
			switch (PromptToSaveFilters())
			{
			default:
				_ = 7;
				break;
			case DialogResult.Yes:
				FileSaveFilters(m_textAnalysisTool.FilterFileName);
				break;
			case DialogResult.Cancel:
				e.Cancel = true;
				break;
			}
		}
	}

	private void m_lineCollectionDisplay_DoubleClick(object sender, EventArgs e)
	{
		int lineNumber = m_lineCollectionDisplay.ListBox.GetSelectedLineNumber();
		if (lineNumber < 0)
		{
			return;
		}
		string text = m_lineCollectionDisplay.ListBox.SourceLines.GetLineText(lineNumber);
		using EditLineDialog editLineDialog = new EditLineDialog(text);
		editLineDialog.ClientSize = new Size(Math.Max(editLineDialog.MinimumSize.Width, base.ClientSize.Width / 2), Math.Max(editLineDialog.MinimumSize.Height, base.ClientSize.Height / 2));
		if (editLineDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		string newText = editLineDialog.EditedText;
		if (newText == text)
		{
			return;
		}
		using (PasteTextConfirmationDialog confirmationDialog = new PasteTextConfirmationDialog("Replace this line with the edited text?", "Confirm Line Edit"))
		{
			if (confirmationDialog.ShowDialog(this) == DialogResult.OK)
			{
				m_textAnalysisTool.ReplaceLine(lineNumber, newText);
			}
		}
	}

	private void m_lineCollectionDisplay_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt)
		{
			m_lineCollectionDisplay_DoubleClick(sender, EventArgs.Empty);
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
	}

	private void m_lineCollectionDisplay_ShowOnlyFilteredLinesChanged(object sender, LineCollectionDisplayListBox.ShowOnlyFilteredLinesEventArgs e)
	{
		if (e.ShowOnlyFilteredLines && !m_statusStrip.Items.Contains(m_statusFiltering))
		{
			m_statusStrip.Items.Insert(1, m_statusFiltering);
		}
		else if (!e.ShowOnlyFilteredLines && m_statusStrip.Items.Contains(m_statusFiltering))
		{
			m_statusStrip.Items.Remove(m_statusFiltering);
		}
		SetStatusSelection();
	}

	private void m_refineFilterControl_RefineTextChanged(object sender, EventArgs e)
	{
		m_textAnalysisTool.FilterCollection.SetRefineFilterText(m_refineFilterControl.RefineText, m_refineFilterControl.Excluding, m_refineFilterControl.CaseSensitive, m_refineFilterControl.UseRegex);
	}

	private void m_lineCollectionDisplay_ZoomLevelChanged(object sender, LineCollectionDisplayListBox.ZoomLevelEventArgs e)
	{
		m_statusZoom.Text = e.ZoomLevel.ToString("0 \\%");
	}

	private void m_lineCollectionDisplay_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetStatusSelection();
	}

	private void LineCollectionDisplayListBox_MarkersChanged(object sender, EventArgs e)
	{
		m_markedLinesDisplay.RefreshMarkedLines();
	}

	public void SetStatusSelection()
	{
		m_statusSelection.Text = string.Format(CultureInfo.CurrentCulture, " {0}: {1} ", "Sel", m_textAnalysisTool.NumberLinesSelected);
	}

	public void SetStatusFilteredLines()
	{
		m_statusLines.Text = string.Format(CultureInfo.CurrentCulture, " {0}: {1} ", "Fil", m_textAnalysisTool.NumberLinesFiltered);
	}

	public void SetStatusTotalLines()
	{
		m_statusTotal.Text = string.Format(CultureInfo.CurrentCulture, " {0}: {1} ", "Total", m_textAnalysisTool.NumberLinesLoaded);
	}

	private void FileOpen_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Text Files (*.txt;*.log)|*.txt;*.log|");
		num++;
		foreach (PluginWrapper plugin in m_textAnalysisTool.Plugins)
		{
			string fileTypeDescription = plugin.GetFileTypeDescription();
			string fileTypePattern = plugin.GetFileTypePattern();
			if (fileTypeDescription != null && fileTypePattern != null)
			{
				stringBuilder.AppendFormat("{0} ({1})|{1}|", fileTypeDescription, fileTypePattern);
				num++;
			}
		}
		stringBuilder.Append("All Files (*)|*");
		num++;
		openFileDialog.Filter = stringBuilder.ToString();
		openFileDialog.FilterIndex = Math.Min(m_fileOpenFilterIndex, num);
		openFileDialog.InitialDirectory = Settings.LastFileDirectory;
		if (DialogResult.OK == openFileDialog.ShowDialog(this))
		{
			m_fileOpenFilterIndex = openFileDialog.FilterIndex;
			LoadFile(openFileDialog.FileName);
			UpdateRecentFiles(openFileDialog.FileName);
		}
	}

	private void FileReload_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.ReloadLines();
	}

	private void FileSaveCurrentLines_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Text Files (*.txt;*.log)|*.txt;*.log|All Files (*)|*";
		saveFileDialog.FilterIndex = m_fileSaveCurrentLinesFilterIndex;
		saveFileDialog.InitialDirectory = Settings.LastFileDirectory;
		if (DialogResult.OK == saveFileDialog.ShowDialog(this))
		{
			m_fileSaveCurrentLinesFilterIndex = saveFileDialog.FilterIndex;
			Settings.LastFileDirectory = new FileInfo(saveFileDialog.FileName).DirectoryName;
			m_textAnalysisTool.SaveCurrentLinesToFile(saveFileDialog.FileName);
		}
	}

	private void FileSaveFilters(string fileName)
	{
		Settings.LastFilterDirectory = new FileInfo(fileName).DirectoryName;
		m_textAnalysisTool.SaveFiltersToFile(fileName);
	}

	private void FileSaveFiltersAs()
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Filter Files (*.tat)|*.tat|All Files (*)|*";
		saveFileDialog.FilterIndex = m_fileSaveFiltersFilterIndex;
		saveFileDialog.InitialDirectory = Settings.LastFilterDirectory;
		saveFileDialog.FileName = m_textAnalysisTool.FilterFileName;
		if (DialogResult.OK == saveFileDialog.ShowDialog(this))
		{
			m_fileSaveFiltersFilterIndex = saveFileDialog.FilterIndex;
			FileSaveFilters(saveFileDialog.FileName);
			UpdateRecentFilterFiles(saveFileDialog.FileName);
		}
	}

	private void FileSaveFilters_Click(object sender, EventArgs e)
	{
		FileSaveFilters(m_textAnalysisTool.FilterFileName);
	}

	private void FileSaveFiltersAs_Click(object sender, EventArgs e)
	{
		FileSaveFiltersAs();
	}

	private void FileLoadFilters_Click(object sender, EventArgs e)
	{
		FileLoadOrAddFilters(replaceFilters: true);
	}

	private void FileAppendFilters_Click(object sender, EventArgs e)
	{
		FileLoadOrAddFilters(replaceFilters: false);
	}

	private void FileLoadOrAddFilters(bool replaceFilters)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Filter Files (*.tat)|*.tat|All Files (*)|*";
		openFileDialog.FilterIndex = m_fileLoadFiltersFilterIndex;
		openFileDialog.InitialDirectory = Settings.LastFilterDirectory;
		if (DialogResult.OK == openFileDialog.ShowDialog(this))
		{
			m_fileLoadFiltersFilterIndex = openFileDialog.FilterIndex;
			LoadFilters(openFileDialog.FileName, replaceFilters);
			UpdateRecentFilterFiles(openFileDialog.FileName);
		}
	}

	private void FileExit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void EditCopy_Click(object sender, EventArgs e)
	{
		if (m_filterCollectionDisplay.Focused && m_filterCollectionDisplay.FiltersSelected)
		{
			m_filterCollectionDisplay.CopySelectedFiltersToClipboard();
		}
		else
		{
			m_textAnalysisTool.CopySelectedLinesToClipboard();
		}
	}

	private void EditPaste_Click(object sender, EventArgs e)
	{
		if (FilterCollectionDisplay.ClipboardContainsFilters)
		{
			m_filterCollectionDisplay.PasteFiltersFromClipboard();
		}
		else if (CanReplaceExistingLines())
		{
			m_textAnalysisTool.LoadLinesFromClipboard();
		}
	}

	private void EditSelectAll_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.SelectAll();
	}

	private void EditFind_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.ShowFindDialog(sender == m_menuEditFindReverse);
	}

	private void EditFindNext_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.FindNextMatch(reverse: false);
	}

	private void EditFindPrevious_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.FindNextMatch(reverse: true);
	}

	private void EditGoTo_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.ShowGoToDialog();
	}

	private void EditFocusRefine_Click(object sender, EventArgs e)
	{
		if (m_refineFilterControl.RefineTextBoxFocused)
		{
			Control previous = m_lastFocusedBeforeRefine;
			m_lastFocusedBeforeRefine = null;
			if (previous != null && !previous.IsDisposed && previous.Visible)
			{
				previous.Focus();
			}
			else
			{
				m_lineCollectionDisplay.ListBox.Focus();
			}
		}
		else
		{
			m_lastFocusedBeforeRefine = ActiveControl;
			m_refineFilterControl.FocusRefineTextBox();
		}
	}

	private void EditClearRefine_Click(object sender, EventArgs e)
	{
		m_refineFilterControl.ClearRefine();
	}

	private void EditPreferences_Click(object sender, EventArgs e)
	{
		using OptionsDialog optionsDialog = new OptionsDialog(Settings.ToOptions(), m_preferencesImportFilterIndex, m_preferencesExportFilterIndex, UpdateSettingsChanged);
		if (DialogResult.OK == optionsDialog.ShowDialog(this))
		{
			UpdateSettingsChanged(optionsDialog.ConfiguredOptions);
		}
	}

	private void ViewZoomReset_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.ResetZoom();
	}

	private void ViewShowMarkers_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (!fullyEnabledMenuItem.Checked)
		{
			m_menuViewMarkersAlways.Checked = fullyEnabledMenuItem == m_menuViewMarkersAlways;
			m_menuViewMarkersNever.Checked = fullyEnabledMenuItem == m_menuViewMarkersNever;
			m_menuViewMarkersWhenInUse.Checked = fullyEnabledMenuItem == m_menuViewMarkersWhenInUse;
			UpdateMarkersState((ShowMarkersState)fullyEnabledMenuItem.Tag);
		}
	}

	private void UpdateMarkersState(ShowMarkersState showMarkersState)
	{
		Settings.ShowMarkersColumn = showMarkersState;
		m_lineCollectionDisplay.ListBox.RefreshDisplay();
	}

	private void SetFilterListLocation(DockStyle dockStyle)
	{
		switch (dockStyle)
		{
		case DockStyle.Left:
			m_splitter.Dock = DockStyle.Right;
			m_lineCollectionDisplay.Dock = DockStyle.Right;
			m_lineCollectionDisplay.Left = base.ClientRectangle.Width - LineCollectionDisplayWidth;
			m_lineCollectionDisplay.Width = LineCollectionDisplayWidth;
			m_splitter.Left = m_lineCollectionDisplay.Left - m_splitter.Width;
			break;
		case DockStyle.Right:
			m_splitter.Dock = DockStyle.Left;
			m_lineCollectionDisplay.Dock = DockStyle.Left;
			m_lineCollectionDisplay.Left = 0;
			m_lineCollectionDisplay.Width = LineCollectionDisplayWidth;
			m_splitter.Left = m_lineCollectionDisplay.Right;
			break;
		case DockStyle.Top:
			m_splitter.Dock = DockStyle.Bottom;
			m_lineCollectionDisplay.Dock = DockStyle.Bottom;
			m_lineCollectionDisplay.Top = DisplayAreaHeight - LineCollectionDisplayHeight;
			m_lineCollectionDisplay.Height = LineCollectionDisplayHeight;
			m_splitter.Top = m_lineCollectionDisplay.Top - m_splitter.Height;
			break;
		case DockStyle.Bottom:
			m_splitter.Dock = DockStyle.Top;
			m_lineCollectionDisplay.Dock = DockStyle.Top;
			m_lineCollectionDisplay.Top = 0;
			m_lineCollectionDisplay.Height = LineCollectionDisplayHeight;
			m_splitter.Top = m_lineCollectionDisplay.Bottom;
			break;
		}
		float dPIScalingFactor = ControlExtensions.GetDPIScalingFactor();
		if (dockStyle == DockStyle.Left || dockStyle == DockStyle.Right)
		{
			m_splitter.Width = (int)(4f * dPIScalingFactor);
			m_splitter.Cursor = Cursors.SizeWE;
		}
		else
		{
			m_splitter.Height = (int)(4f * dPIScalingFactor);
			m_splitter.Cursor = Cursors.SizeNS;
		}
		UpdateFilterPaneLayout(dockStyle);
	}

	private void UpdateFilterPaneLayout(DockStyle dockStyle)
	{
		m_filterPane.SuspendLayout();
		m_initialMarkedSplitDone = false;
		float dPIScalingFactor = ControlExtensions.GetDPIScalingFactor();
		if (dockStyle == DockStyle.Left || dockStyle == DockStyle.Right)
		{
			m_filterSplitter.Dock = DockStyle.Bottom;
			m_filterSplitter.Height = (int)(4f * dPIScalingFactor);
			m_filterSplitter.Cursor = Cursors.SizeNS;
			m_markedLinesDisplay.Dock = DockStyle.Bottom;
			m_markedLinesDisplay.Height = ((m_filterPane.ClientSize.Height > 0) ? (m_filterPane.ClientSize.Height / 2) : ((int)(180f * dPIScalingFactor)));
		}
		else
		{
			m_filterSplitter.Dock = DockStyle.Right;
			m_filterSplitter.Width = (int)(4f * dPIScalingFactor);
			m_filterSplitter.Cursor = Cursors.SizeWE;
			m_markedLinesDisplay.Dock = DockStyle.Right;
			m_markedLinesDisplay.Width = ((m_filterPane.ClientSize.Width > 0) ? (m_filterPane.ClientSize.Width / 2) : ((int)(240f * dPIScalingFactor)));
		}
		m_filterPane.ResumeLayout(performLayout: true);
	}

	private void ViewFilterList_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (!fullyEnabledMenuItem.Checked)
		{
			SuspendLayout();
			DockStyle filterListLocation = (DockStyle)fullyEnabledMenuItem.Tag;
			SetFilterListLocation(filterListLocation);
			Settings.FilterListLocation = filterListLocation;
			ResumeLayout(performLayout: true);
		}
	}

	private void PluginSettings_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (!fullyEnabledMenuItem.Checked)
		{
			((PluginWrapper)fullyEnabledMenuItem.Tag).GetSettingsDialog().ShowDialog();
		}
	}

	private void ViewZoomIn_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.ZoomIn();
	}

	private void ViewZoomOut_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.ZoomOut();
	}

	private void ViewShowOnlyFilteredLines_Click(object sender, EventArgs e)
	{
		m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines = !m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines;
	}

	private void ViewShowLineNumbers_Click(object sender, EventArgs e)
	{
		Settings.ShowLineNumbers = !Settings.ShowLineNumbers;
		m_lineCollectionDisplay.ListBox.RefreshDisplay();
	}

	private void ViewShowFilterToolTip_Click(object sender, EventArgs e)
	{
		Settings.ShowFilterToolTip = !Settings.ShowFilterToolTip;
		m_lineCollectionDisplay.ListBox.ConfigureToolTipHelpers();
	}

	private void ViewSetEncoding_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		m_textAnalysisTool.SetCurrentEncoding(fullyEnabledMenuItem.Tag as string);
	}

	private void ViewSetEncodingDialog_Click(object sender, EventArgs e)
	{
		using EncodingDialog encodingDialog = new EncodingDialog();
		encodingDialog.SetEncoding(m_textAnalysisTool.CurrentEncodingIsSystemDefault, m_textAnalysisTool.CurrentEncoding.WebName);
		if (DialogResult.OK == encodingDialog.ShowDialog(this))
		{
			m_textAnalysisTool.SetCurrentEncoding(encodingDialog.EncodingName);
		}
	}

	private void FilterAddFilter_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.ShowAddFilterDialog();
	}

	private void FilterEditFilter_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.ShowEditFiltersDialog();
	}

	private void FilterRemoveFilter_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.FilterCollection.Remove(m_filterCollectionDisplay.SelectedFilter);
	}

	private void FilterMatchPrev_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.DoFind(FilterFindAction.Previous);
	}

	private void FilterMatchNext_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.DoFind(FilterFindAction.Next);
	}

	private void FilterEnableAllFilters_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.FilterCollection.SetAllEnabledState(enabled: true);
	}

	private void FilterDisableAllFilters_Click(object sender, EventArgs e)
	{
		m_filterCollectionDisplay.FilterCollection.SetAllEnabledState(enabled: false);
	}

	private void FilterRemoveAllFilters_Click(object sender, EventArgs e)
	{
		if (!SaveFiltersIfNeeded())
		{
			m_filterCollectionDisplay.FilterCollection.RemoveAll();
		}
	}

	private void HelpDocumentation_Click(object sender, EventArgs e)
	{
		if ((!m_textAnalysisTool.LinesLoaded && !m_textAnalysisTool.FiltersPresent) || DialogResult.OK == CTextAnalysisTool.ShowMessageBox(string.Format("{0} is about to replace the current lines and filters with lines from its documentation file and a few sample filters.\n\n(To preserve your current data: cancel this operation, open a new {0} window, and view the documentation file in that window.)", "TextAnalysisTool.NET"), MessageBoxIcon.Question, MessageBoxButtons.OKCancel))
		{
			m_filterCollectionDisplay.FilterCollection.ReplaceAll(new Filter[0]);
			m_textAnalysisTool.LoadLinesFromResourceStream("TextAnalysisTool.NET.txt");
			m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines = false;
			m_lineCollectionDisplay.ListBox.MarkLineNumberRange(10, 27);
			m_filterCollectionDisplay.FilterCollection.ReplaceAll(new Filter[5]
			{
				new MatchesTextFilter(enabled: false, excluding: false, Color.White, Color.Green, "Section:", caseSensitive: false, usesRegularExpression: false),
				new MarkedFilter(enabled: false, excluding: false, Color.Empty, Color.Empty, 0),
				new MatchesTextFilter(enabled: false, excluding: false, Color.DarkGreen, Color.LightGray, "TextAnalysisTool.NET", caseSensitive: false, usesRegularExpression: false),
				new MatchesTextFilter(enabled: false, excluding: false, Color.DarkBlue, Color.Empty, "\\bplug.?ins?\\b", caseSensitive: true, usesRegularExpression: true),
				new MatchesTextFilter(enabled: false, excluding: true, Color.Empty, Color.Empty, "the", caseSensitive: true, usesRegularExpression: false)
			});
			m_textAnalysisTool.FilterCollection.ClearDirtyFlag();
			m_lineCollectionDisplay.ListBox.Focus();
		}
	}

	private void HelpPlugins_Click(object sender, EventArgs e)
	{
		string text;
		if (m_textAnalysisTool.Plugins.Count == 0)
		{
			text = "There are no plug-ins installed.\n\n(Please see the documentation for information about how to install plug-ins.)";
		}
		else
		{
			text = "The following plug-ins are installed:\n";
			int num = 1;
			foreach (PluginWrapper plugin in m_textAnalysisTool.Plugins)
			{
				string fileTypeDescription = plugin.GetFileTypeDescription();
				string fileTypePattern = plugin.GetFileTypePattern();
				text = ((fileTypeDescription == null || fileTypePattern == null) ? (text + $"{num}) {plugin.Name}: [Disabled]\n") : (text + $"{num}) {plugin.Name}: {fileTypeDescription} ({fileTypePattern})  [{plugin.AssemblyLocation}, {plugin.AssemblyVersion}]\n"));
				num++;
			}
		}
		CTextAnalysisTool.ShowMessageBox(text, MessageBoxIcon.Asterisk);
	}

	private void HelpAbout_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.ShowAboutDialog();
	}

	public void UpdateUIFonts(float fontSize, int zoomPercent)
	{
		Font font = CTextAnalysisTool.SafeCreateFont(Settings.FontFamily, fontSize, zoomPercent, m_refineFilterControl.Font);
		m_markedLinesDisplay.ApplyFont(font);
		Font font2 = CTextAnalysisTool.SafeCreateFont(Settings.FontFamily, fontSize - 2, zoomPercent, m_refineFilterControl.Font);
		m_refineFilterControl.ApplyFont(font2);
	}

	public void UpdateSettingsChanged(OptionsBehaviorProperties options)
	{
		int tabSize = Settings.TabSize;
		Encoding currentEncoding = m_textAnalysisTool.CurrentEncoding;
		Settings.FromOptions(options);
		if (m_lineCollectionDisplay.ListBox.Font.FontFamily.Name != Settings.FontFamily || m_lineCollectionDisplay.ListBox.Font.Size != Settings.FontSize * ((float)Settings.ZoomPercent / 100f))
		{
			m_lineCollectionDisplay.ListBox.Font = CTextAnalysisTool.SafeCreateFont(Settings.FontFamily, Settings.FontSize, Settings.ZoomPercent, m_lineCollectionDisplay.ListBox.Font);
		}
		if (string.Compare(currentEncoding.WebName, Settings.DefaultEncodingName, ignoreCase: true) != 0)
		{
			m_textAnalysisTool.SetCurrentEncoding(Settings.DefaultEncodingName);
		}
		else if (tabSize != Settings.TabSize)
		{
			m_textAnalysisTool.UpdateLinesLength();
		}
		m_textAnalysisTool.SetMainWindowTitle();
		m_lineCollectionDisplay.ListBox.RefreshDisplay();
		m_filterCollectionDisplay.UpdateDisplay();
		Settings.Save();
	}

	private void Menu_Opening(object sender, EventArgs e)
	{
		bool linesVisible = m_textAnalysisTool.LinesVisible;
		bool filtersSelected = m_filterCollectionDisplay.FiltersSelected;
		bool dataPresent = Clipboard.GetDataObject().GetDataPresent(DataFormats.UnicodeText);
		bool clipboardContainsFilters = FilterCollectionDisplay.ClipboardContainsFilters;
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (fullyEnabledMenuItem == m_menuFile)
		{
			m_menuFileReload.Enabled = m_textAnalysisTool.ReloadLinesPossible;
			m_menuFileSaveCurrentLines.Enabled = linesVisible;
			m_menuFileSaveFilters.Enabled = !string.IsNullOrEmpty(m_textAnalysisTool.FilterFileName);
			return;
		}
		if (fullyEnabledMenuItem == m_menuEdit)
		{
			bool focused = m_filterCollectionDisplay.Focused;
			bool flag = m_filterCollectionDisplay.SelectedFilter != null;
			m_menuEditCopy.Enabled = linesVisible || filtersSelected;
			m_menuEditCopyLegacy.Enabled = m_menuEditCopy.Enabled;
			m_menuEditPaste.Enabled = dataPresent || clipboardContainsFilters;
			m_menuEditPasteLegacy.Enabled = m_menuEditPaste.Enabled;
			m_menuEditCopyFilters.Enabled = focused && flag;
			m_menuEditPasteFilters.Enabled = FilterCollectionDisplay.ClipboardContainsFilters;
			m_menuEditSelectAll.Enabled = linesVisible;
			m_menuEditFind.Enabled = linesVisible;
			m_menuEditFindReverse.Enabled = linesVisible;
			bool enabled = linesVisible && m_textAnalysisTool.FindNextMatchPossible;
			m_menuEditFindPrevious.Enabled = enabled;
			m_menuEditFindNext.Enabled = enabled;
			m_menuEditGoTo.Enabled = linesVisible;
			m_menuEditFocusRefine.Checked = m_refineFilterControl.RefineTextBoxFocused;
			return;
		}
		if (fullyEnabledMenuItem == m_menuView)
		{
			m_menuViewShowOnlyFilteredLines.Enabled = m_textAnalysisTool.LinesLoaded;
			m_menuViewShowLineNumbers.Enabled = m_textAnalysisTool.LinesLoaded;
			m_menuViewShowFilterToolTip.Enabled = m_textAnalysisTool.LinesLoaded;
			m_menuViewShowMarkers.Enabled = m_textAnalysisTool.LinesLoaded;
			m_menuViewShowOnlyFilteredLines.Checked = m_lineCollectionDisplay.ListBox.ShowOnlyFilteredLines;
			m_menuViewShowLineNumbers.Checked = Settings.ShowLineNumbers;
			m_menuViewShowFilterToolTip.Checked = Settings.ShowFilterToolTip;
			m_menuViewMarkersAlways.Checked = Settings.ShowMarkersColumn == (ShowMarkersState)m_menuViewMarkersAlways.Tag;
			m_menuViewMarkersNever.Checked = Settings.ShowMarkersColumn == (ShowMarkersState)m_menuViewMarkersNever.Tag;
			m_menuViewMarkersWhenInUse.Checked = Settings.ShowMarkersColumn == (ShowMarkersState)m_menuViewMarkersWhenInUse.Tag;
			return;
		}
		if (fullyEnabledMenuItem == m_menuViewFilterListLocation)
		{
			foreach (object dropDownItem in m_menuViewFilterListLocation.DropDownItems)
			{
				if (dropDownItem is FullyEnabledMenuItem { Tag: var tag } fullyEnabledMenuItem2 && tag is DockStyle dockStyle)
				{
					fullyEnabledMenuItem2.Checked = dockStyle == Settings.FilterListLocation;
				}
			}
			return;
		}
		if (fullyEnabledMenuItem == m_menuFilter)
		{
			bool focused2 = m_filterCollectionDisplay.Focused;
			bool flag2 = m_filterCollectionDisplay.SelectedFilter != null;
			bool flag3 = m_textAnalysisTool.LinesLoaded && m_textAnalysisTool.LinesVisible;
			m_menuFilterEditFilter.Enabled = focused2 && flag2;
			m_menuFilterRemoveFilter.Enabled = focused2 && flag2;
			m_menuFilterMatchPrev.Enabled = flag2 && flag3;
			m_menuFilterMatchNext.Enabled = flag2 && flag3;
			bool filtersPresent = m_textAnalysisTool.FiltersPresent;
			m_menuFilterEnableAllFilters.Enabled = filtersPresent;
			m_menuFilterDisableAllFilters.Enabled = filtersPresent;
			m_menuFilterRemoveAllFilters.Enabled = filtersPresent;
		}
		else
		{
			if (fullyEnabledMenuItem != m_menuViewEncoding)
			{
				return;
			}
			foreach (object dropDownItem2 in m_menuViewEncoding.DropDownItems)
			{
				if (dropDownItem2 is FullyEnabledMenuItem { Tag: string tag2 } fullyEnabledMenuItem3)
				{
					fullyEnabledMenuItem3.Checked = (m_textAnalysisTool.CurrentEncodingIsSystemDefault && tag2 == "") || string.Compare(tag2, m_textAnalysisTool.CurrentEncoding.WebName, ignoreCase: true) == 0;
				}
			}
		}
	}

	private void ClientToDisplayArea(int clientX, int clientY, out int displayX, out int displayY)
	{
		displayX = clientX;
		displayY = clientY - base.MainMenuStrip.Height;
	}

	public void SuspendRedrawForm()
	{
		m_filterCollectionDisplay.SuspendRedraw();
		m_lineCollectionDisplay.SuspendRedraw();
	}

	public void ResumeRedrawForm()
	{
		m_filterCollectionDisplay.ResumeRedraw();
		m_lineCollectionDisplay.ResumeRedraw();
	}

	private void OnSplitterMoving(object sender, SplitterEventArgs e)
	{
		ClientToDisplayArea(e.X, e.Y, out var displayX, out var displayY);
		switch (m_lineCollectionDisplay.Dock)
		{
		case DockStyle.Top:
			Settings.SplitterPositionPercent = 100 * displayY / DisplayAreaHeight;
			break;
		case DockStyle.Bottom:
			Settings.SplitterPositionPercent = 100 * (DisplayAreaHeight - displayY) / DisplayAreaHeight;
			break;
		case DockStyle.Left:
			Settings.SplitterPositionPercent = 100 * displayX / base.ClientRectangle.Width;
			break;
		case DockStyle.Right:
			Settings.SplitterPositionPercent = 100 * (base.ClientRectangle.Width - displayX) / base.ClientRectangle.Width;
			break;
		}
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		if (m_splitter.Dock == DockStyle.Top || m_splitter.Dock == DockStyle.Bottom)
		{
			m_splitter.SplitPosition = LineCollectionDisplayHeight;
		}
		else
		{
			m_splitter.SplitPosition = LineCollectionDisplayWidth;
		}
		base.OnLayout(e);
		if (!m_initialMarkedSplitDone && m_filterPane.ClientSize.Width > 0 && m_filterPane.ClientSize.Height > 0)
		{
			if (m_filterSplitter.Dock == DockStyle.Right)
			{
				m_markedLinesDisplay.Width = m_filterPane.ClientSize.Width / 2;
			}
			else
			{
				m_markedLinesDisplay.Height = m_filterPane.ClientSize.Height / 2;
			}
			m_initialMarkedSplitDone = true;
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.UnicodeText))
		{
			e.Effect = DragDropEffects.Copy;
		}
		base.OnDragEnter(e);
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		if (CanReplaceExistingLines())
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (1 <= array.Length)
				{
					m_textAnalysisTool.LoadLinesFromFile(array[0]);
					UpdateRecentFiles(array[0]);
				}
			}
			else if (e.Data.GetDataPresent(DataFormats.UnicodeText))
			{
				m_textAnalysisTool.LoadLinesFromDragDropText((string)e.Data.GetData(DataFormats.UnicodeText));
			}
		}
		base.OnDragDrop(e);
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);
		OnBoundsChanged();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		OnBoundsChanged();
	}

	private void OnBoundsChanged()
	{
		if (base.WindowState == FormWindowState.Normal)
		{
			Settings.WindowPositionRectangle = base.Bounds;
		}
		if (FormWindowState.Minimized != base.WindowState)
		{
			Settings.WindowPositionState = base.WindowState;
		}
	}

	private void LoadFile(string fileName)
	{
		Settings.LastFileDirectory = new FileInfo(fileName).DirectoryName;
		m_textAnalysisTool.LoadLinesFromFile(fileName);
	}

	private void LoadFilters(string fileName, bool replaceFilters)
	{
		Settings.LastFilterDirectory = new FileInfo(fileName).DirectoryName;
		m_textAnalysisTool.LoadFiltersFromFile(fileName, replaceFilters);
	}

	private void UpdateRecentFiles(string fileName)
	{
		UpdateRecentList(fileName, Settings.RecentFiles);
		UpdateRecentFilesMenus();
		Settings.Save();
	}

	private void UpdateRecentFilterFiles(string fileName)
	{
		UpdateRecentList(fileName, Settings.RecentFilterFiles);
		UpdateRecentFilterFilesMenus();
		Settings.Save();
	}

	private void UpdateRecentList(string fileName, List<string> recentList)
	{
		for (int i = 0; i < recentList.Count; i++)
		{
			string text = recentList[i];
			if (string.Equals(text, fileName, StringComparison.OrdinalIgnoreCase))
			{
				recentList.RemoveAt(i);
				recentList.Insert(0, text);
				return;
			}
		}
		if (recentList.Count >= Settings.MaxNumberOfRecentItems)
		{
			recentList.RemoveAt(recentList.Count - 1);
		}
		recentList.Insert(0, fileName);
	}

	public void UpdateRecentFilesMenus()
	{
		UpdateRecentItemsMenus(m_menuFileRecentFiles, Settings.RecentFiles, FileMenuRecentFiles_Click, FileMenuRecentFilesClear_Click, "&Clear Recent Files List");
	}

	public void UpdateRecentFilterFilesMenus()
	{
		UpdateRecentItemsMenus(m_menuFileRecentFilters, Settings.RecentFilterFiles, FileMenuRecentFilters_Click, FileMenuRecentFiltersClear_Click, "&Clear Recent Filter Files List");
	}

	public void UpdateRecentItemsMenus(FullyEnabledMenuItem recentItemsMenu, List<string> recentItemsList, EventHandler recentItemsMenu_Click, EventHandler recentItemsMenuClear_Click, string clearMenuText)
	{
		recentItemsMenu.DropDownItems.Clear();
		recentItemsMenu.Enabled = recentItemsList.Count > 0;
		for (int i = 0; i < recentItemsList.Count; i++)
		{
			string arg = recentItemsList[i].ShortenPath(70);
			string arg2 = ((i + 1 < 10) ? "&" : string.Empty);
			string text = $"{arg2}{i + 1} {arg}";
			recentItemsMenu.DropDownItems.Add(new FullyEnabledMenuItem(text, null, recentItemsMenu_Click));
		}
		recentItemsMenu.DropDownItems.Add(new ToolStripSeparator());
		recentItemsMenu.DropDownItems.Add(new FullyEnabledMenuItem(clearMenuText, null, recentItemsMenuClear_Click));
	}

	private void FileMenuRecentFilesClear_Click(object sender, EventArgs e)
	{
		Settings.RecentFiles.Clear();
		UpdateRecentFilesMenus();
	}

	private void FileMenuRecentFiltersClear_Click(object sender, EventArgs e)
	{
		Settings.RecentFilterFiles.Clear();
		UpdateRecentFilterFilesMenus();
	}

	private void FileMenuRecentFiles_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (fullyEnabledMenuItem != null)
		{
			int index = fullyEnabledMenuItem.Index;
			if (index < Settings.RecentFiles.Count)
			{
				string fileName = Settings.RecentFiles[index];
				LoadFile(fileName);
				UpdateRecentFiles(fileName);
			}
		}
	}

	private void FileMenuRecentFilters_Click(object sender, EventArgs e)
	{
		FullyEnabledMenuItem fullyEnabledMenuItem = (FullyEnabledMenuItem)sender;
		if (fullyEnabledMenuItem != null)
		{
			int index = fullyEnabledMenuItem.Index;
			if (index < Settings.RecentFilterFiles.Count && !SaveFiltersIfNeeded())
			{
				string fileName = Settings.RecentFilterFiles[index];
				LoadFilters(fileName, replaceFilters: true);
				UpdateRecentFilterFiles(fileName);
			}
		}
	}

	private bool CanReplaceExistingLines()
	{
		if ((Clipboard.ContainsData(DataFormats.UnicodeText) || Clipboard.ContainsData(DataFormats.Text)) && m_textAnalysisTool.LinesLoaded)
		{
			bool flag = Settings.PasteTextOverwrite == PasteTextOverwriteEnum.OverwriteAlwaysDontAsk;
			if (Settings.PasteTextOverwrite == PasteTextOverwriteEnum.AlwaysAsk)
			{
				Focus();
				Activate();
				PasteTextConfirmationDialog pasteTextConfirmationDialog = new PasteTextConfirmationDialog();
				flag = pasteTextConfirmationDialog.ShowDialog() == DialogResult.OK;
				if (!pasteTextConfirmationDialog.DoNotAskAgainChecked)
				{
					if (flag)
					{
						Settings.PasteTextOverwrite = PasteTextOverwriteEnum.OverwriteAlwaysDontAsk;
					}
					else
					{
						Settings.PasteTextOverwrite = PasteTextOverwriteEnum.OverwriteNeverDontAsk;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
