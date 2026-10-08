using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("Form")]
internal sealed class FilterDialog : FilterFindDialogBase
{
	private const int MatchesTextIndex = 0;

	private const int MarkedIndex = 1;

	private bool m_activatedYet;

	private bool m_enableNewFilter = true;

	private IContainer components;

	private TableLayoutPanel m_mainTableLayoutPanel;

	private Label m_filterTypeLabel;

	private Label m_descriptionLabel;

	private ToolTip m_toolTip;

	private ErrorProvider m_errorProvider;

	private Label m_textLabel;

	private TableLayoutPanel okCancelButtonLayoutPanel;

	private Button m_cancelButton;

	private Button m_okButton;

	private TextBox m_descriptionBox;

	private FlowLayoutPanel checkboxLayoutPanel;

	private CheckBox m_excludingCheck;

	private CheckBox m_caseSensitiveCheck;

	private CheckBox m_regularExpressionCheck;

	private FlowLayoutPanel m_topRowFlowLayoutPanel;

	private ComboBox m_filterTypeCombo;

	private Label m_foreColorLabel;

	private ColorComboBox m_backColorCombo;

	private Label m_backColorLabel;

	private ColorComboBox m_foreColorCombo;

	private TableLayoutPanel filterTextMarkerLayoutPanel;

	private ComboBox m_markerTypeCombo;

	private ComboBox m_textCombo;

	public FilterDialog(string initialText, Filter filter, Func<bool, int[]> getCustomColors)
		: this(initialText, getCustomColors)
	{
		ConfigureFromFilter(filter);
	}

	public FilterDialog(string initialText, List<Filter> filterList, Func<bool, int[]> getCustomColors)
		: this(initialText, getCustomColors)
	{
		m_filterList = filterList;
		ConfigureFromFilters(filterList);
	}

	public FilterDialog(string initialText, Func<bool, int[]> getCustomColors)
		: base(initialText)
	{
		InitializeComponent();
		m_textLabelBase = m_textLabel;
		m_textComboBase = m_textCombo;
		m_caseSensitiveCheckBase = m_caseSensitiveCheck;
		m_regularExpressionCheckBase = m_regularExpressionCheck;
		m_okButtonBase = m_okButton;
		m_cancelButtonBase = m_cancelButton;
		m_toolTipBase = m_toolTip;
		m_errorProviderBase = m_errorProvider;
		InitBaseUI(initialText);
		Text = "Add Filter";
		m_filterTypeLabel.Text = "&Filter:";
		m_filterTypeCombo.Items.AddRange(new object[2] { "Matches text", "Marked by marker" });
		m_backColorLabel.Text = "&Background:";
		m_foreColorCombo.GetCustomColors = getCustomColors;
		m_foreColorCombo.Initialized = true;
		m_foreColorLabel.Text = "Text Co&lor:";
		m_backColorCombo.GetCustomColors = getCustomColors;
		m_backColorCombo.Initialized = true;
		m_markerTypeCombo.Items.AddRange(Enumerable.Range(1, 8).Select((Func<int, object>)((int i) => i)).ToArray());
		m_descriptionLabel.Text = "&Description:";
		m_excludingCheck.Text = "E&xcluding [!]";
		m_minWidthControls.Add(m_descriptionLabel);
		m_minWidthControls.Add(m_topRowFlowLayoutPanel);
		m_minHeightControls.Add(m_mainTableLayoutPanel);
		RecalculateFormSizes(Settings.FilterDialogWidth, 4000);
		m_filterTypeCombo.SelectedIndex = 0;
		base.Resize += Form_Resize;
	}

	private void Form_Resize(object sender, EventArgs e)
	{
		if (sender is Form)
		{
			Settings.FilterDialogWidth = base.Width;
		}
	}

	public override void ConfigureFromFilter(Filter f)
	{
		if (f != null)
		{
			Text = "Edit Filter";
			m_enableNewFilter = f.Enabled;
			m_excludingCheck.Checked = f.Excluding;
			bool initialized = m_foreColorCombo.Initialized;
			bool initialized2 = m_backColorCombo.Initialized;
			m_foreColorCombo.Initialized = false;
			m_backColorCombo.Initialized = false;
			m_foreColorCombo.SelectedColor = f.ForegroundColor;
			m_backColorCombo.SelectedColor = f.BackgroundColor;
			m_textCombo.ForeColor = f.ForegroundColor;
			m_textCombo.BackColor = f.BackgroundColor;
			m_descriptionBox.Text = f.Description;
			if (f is MatchesTextFilter)
			{
				m_filterTypeCombo.SelectedIndex = 0;
			}
			else if (f is MarkedFilter)
			{
				m_filterTypeCombo.SelectedIndex = 1;
				MarkedFilter markedFilter = (MarkedFilter)f;
				m_markerTypeCombo.SelectedIndex = markedFilter.Marker;
			}
			base.ConfigureFromFilter(f);
			m_foreColorCombo.Initialized = initialized;
			m_backColorCombo.Initialized = initialized2;
		}
	}

	public void ConfigureFromFilters(List<Filter> filters)
	{
		ConfigureFromFilter(filters[0]);
		Text = "Edit Multiple Filters";
		m_filterTypeCombo.Enabled = false;
		if (AreFilterPropsDifferent(filters, "ForegroundColor"))
		{
			m_foreColorCombo.AddIndeterminateColor();
			m_foreColorCombo.SelectedIndex = 0;
		}
		if (AreFilterPropsDifferent(filters, "BackgroundColor"))
		{
			m_backColorCombo.AddIndeterminateColor();
			m_backColorCombo.SelectedIndex = 0;
		}
		m_textCombo.Text = "[multiple values]";
		m_textCombo.Items.Clear();
		m_textCombo.Enabled = false;
		m_markerTypeCombo.Text = "[multiple values]";
		m_markerTypeCombo.Items.Clear();
		m_markerTypeCombo.Enabled = false;
		if (AreFilterPropsDifferent(filters, "Description"))
		{
			m_descriptionBox.Text = "[multiple values]";
		}
		if (AreFilterPropsDifferent(filters, "Excluding"))
		{
			m_excludingCheck.ThreeState = true;
			m_excludingCheck.CheckState = CheckState.Indeterminate;
		}
		if (AreFilterPropsDifferent(filters, "CaseSensitive"))
		{
			m_caseSensitiveCheck.ThreeState = true;
			m_caseSensitiveCheck.CheckState = CheckState.Indeterminate;
		}
		if (AreFilterPropsDifferent(filters, "RegularExpression"))
		{
			m_regularExpressionCheck.ThreeState = true;
			m_regularExpressionCheck.CheckState = CheckState.Indeterminate;
		}
	}

	private bool AreFilterPropsDifferent(List<Filter> filters, string propName)
	{
		Type type = filters[0].GetType();
		PropertyInfo property = type.GetProperty(propName);
		if (property != null)
		{
			object value = property.GetValue(filters[0], null);
			for (int i = 1; i < filters.Count; i++)
			{
				Filter obj = filters[i];
				object value2 = type.GetProperty(propName).GetValue(obj, null);
				if (!value.Equals(value2))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		if (!m_activatedYet)
		{
			m_textCombo.Focus();
			m_markerTypeCombo.Focus();
			m_activatedYet = true;
		}
	}

	private void FilterType_SelectedIndexChanged(object sender, EventArgs e)
	{
		bool flag = true;
		bool visible = false;
		int selectedIndex = m_filterTypeCombo.SelectedIndex;
		if (selectedIndex != 0 && selectedIndex == 1)
		{
			flag = false;
			visible = true;
		}
		m_markerTypeCombo.Visible = visible;
		m_textCombo.Visible = flag;
		m_caseSensitiveCheck.Visible = flag;
		m_regularExpressionCheck.Visible = flag;
		if (flag)
		{
			m_textLabel.Text = "&Text:";
		}
		else
		{
			m_textLabel.Text = "&Marker:";
		}
		ValidateInput(null, null);
	}

	private void ForegroundColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_textCombo.ForeColor = m_foreColorCombo.SelectedColor;
	}

	private void BackgroundColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_textCombo.BackColor = m_backColorCombo.SelectedColor;
	}

	private void Excluding_CheckedChanged(object sender, EventArgs e)
	{
		m_foreColorCombo.Enabled = !m_excludingCheck.Checked;
		m_backColorCombo.Enabled = !m_excludingCheck.Checked;
	}

	protected override bool ValidateMarkerInput()
	{
		bool flag = false;
		if (m_filterTypeCombo != null && 1 == m_filterTypeCombo.SelectedIndex)
		{
			flag = true;
		}
		if (!flag)
		{
			flag = m_filterList != null;
		}
		return flag;
	}

	protected override void OK_Click(object sender, EventArgs e)
	{
		base.OK_Click(sender, e);
		bool flag = false;
		if (m_filterList == null)
		{
			switch (m_filterTypeCombo.SelectedIndex)
			{
			case 0:
				m_filter = new MatchesTextFilter(m_enableNewFilter, m_excludingCheck.Checked, m_foreColorCombo.SelectedColor, m_backColorCombo.SelectedColor, m_textCombo.Text, m_caseSensitiveCheck.Checked, m_regularExpressionCheck.Checked, m_descriptionBox.Text);
				flag = true;
				break;
			case 1:
				m_filter = new MarkedFilter(m_enableNewFilter, m_excludingCheck.Checked, m_foreColorCombo.SelectedColor, m_backColorCombo.SelectedColor, m_markerTypeCombo.SelectedIndex, m_descriptionBox.Text);
				flag = true;
				break;
			}
		}
		else
		{
			m_filterList = RegenerateFilterList();
			if (m_filterList != null)
			{
				flag = true;
			}
		}
		if (flag)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private List<Filter> RegenerateFilterList()
	{
		List<Filter> list = new List<Filter>();
		for (int i = 0; i < m_filterList.Count; i++)
		{
			Filter filter = null;
			if (m_filterList[i] is MatchesTextFilter)
			{
				MatchesTextFilter matchesTextFilter = (MatchesTextFilter)m_filterList[i];
				filter = new MatchesTextFilter(matchesTextFilter.Enabled, (m_excludingCheck.CheckState == CheckState.Indeterminate) ? matchesTextFilter.Excluding : m_excludingCheck.Checked, m_foreColorCombo.IsIndeterminateColor ? matchesTextFilter.ForegroundColor : m_foreColorCombo.SelectedColor, m_backColorCombo.IsIndeterminateColor ? matchesTextFilter.BackgroundColor : m_backColorCombo.SelectedColor, matchesTextFilter.FilterPattern, (m_caseSensitiveCheck.CheckState == CheckState.Indeterminate) ? matchesTextFilter.CaseSensitive : m_caseSensitiveCheck.Checked, (m_regularExpressionCheck.CheckState == CheckState.Indeterminate) ? matchesTextFilter.RegularExpression : m_regularExpressionCheck.Checked, (m_descriptionBox.Text == "[multiple values]") ? matchesTextFilter.Description : m_descriptionBox.Text);
			}
			else
			{
				if (!(m_filterList[i] is MarkedFilter))
				{
					return null;
				}
				MarkedFilter markedFilter = (MarkedFilter)m_filterList[i];
				filter = new MarkedFilter(markedFilter.Enabled, (m_excludingCheck.CheckState == CheckState.Indeterminate) ? markedFilter.Excluding : m_excludingCheck.Checked, m_foreColorCombo.IsIndeterminateColor ? markedFilter.ForegroundColor : m_foreColorCombo.SelectedColor, m_backColorCombo.IsIndeterminateColor ? markedFilter.BackgroundColor : m_backColorCombo.SelectedColor, markedFilter.Marker, (m_descriptionBox.Text == "[multiple values]") ? markedFilter.Description : m_descriptionBox.Text);
			}
			list.Add(filter);
		}
		return list;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.m_mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.m_descriptionBox = new System.Windows.Forms.TextBox();
		this.m_textLabel = new System.Windows.Forms.Label();
		this.m_descriptionLabel = new System.Windows.Forms.Label();
		this.okCancelButtonLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.checkboxLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_excludingCheck = new TextAnalysisTool.NET.CustomCheckbox();
		this.m_caseSensitiveCheck = new TextAnalysisTool.NET.CustomCheckbox();
		this.m_regularExpressionCheck = new TextAnalysisTool.NET.CustomCheckbox();
		this.m_topRowFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_filterTypeCombo = new System.Windows.Forms.ComboBox();
		this.m_foreColorLabel = new System.Windows.Forms.Label();
		this.m_foreColorCombo = new TextAnalysisTool.NET.ColorComboBox();
		this.m_backColorLabel = new System.Windows.Forms.Label();
		this.m_backColorCombo = new TextAnalysisTool.NET.ColorComboBox();
		this.m_filterTypeLabel = new System.Windows.Forms.Label();
		this.filterTextMarkerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.m_markerTypeCombo = new System.Windows.Forms.ComboBox();
		this.m_textCombo = new System.Windows.Forms.ComboBox();
		this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_mainTableLayoutPanel.SuspendLayout();
		this.okCancelButtonLayoutPanel.SuspendLayout();
		this.checkboxLayoutPanel.SuspendLayout();
		this.m_topRowFlowLayoutPanel.SuspendLayout();
		this.filterTextMarkerLayoutPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_mainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_mainTableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
		this.m_mainTableLayoutPanel.ColumnCount = 2;
		this.m_mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.m_mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.m_mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.m_mainTableLayoutPanel.Controls.Add(this.m_descriptionBox, 1, 2);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_textLabel, 0, 1);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_descriptionLabel, 0, 2);
		this.m_mainTableLayoutPanel.Controls.Add(this.okCancelButtonLayoutPanel, 0, 4);
		this.m_mainTableLayoutPanel.Controls.Add(this.checkboxLayoutPanel, 1, 3);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_topRowFlowLayoutPanel, 1, 0);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_filterTypeLabel, 0, 0);
		this.m_mainTableLayoutPanel.Controls.Add(this.filterTextMarkerLayoutPanel, 1, 1);
		this.m_mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_mainTableLayoutPanel.Location = new System.Drawing.Point(6, 6);
		this.m_mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
		this.m_mainTableLayoutPanel.Name = "m_mainTableLayoutPanel";
		this.m_mainTableLayoutPanel.RowCount = 5;
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.Size = new System.Drawing.Size(676, 178);
		this.m_mainTableLayoutPanel.TabIndex = 1000;
		this.m_descriptionBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_descriptionBox.Location = new System.Drawing.Point(147, 132);
		this.m_descriptionBox.Margin = new System.Windows.Forms.Padding(2, 2, 20, 2);
		this.m_descriptionBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_descriptionBox.Name = "m_descriptionBox";
		this.m_descriptionBox.Size = new System.Drawing.Size(509, 35);
		this.m_descriptionBox.TabIndex = 3;
		this.m_textLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_textLabel.AutoSize = true;
		this.m_textLabel.Location = new System.Drawing.Point(77, 94);
		this.m_textLabel.Margin = new System.Windows.Forms.Padding(2, 7, 2, 7);
		this.m_textLabel.Name = "m_textLabel";
		this.m_textLabel.Size = new System.Drawing.Size(66, 29);
		this.m_textLabel.TabIndex = 0;
		this.m_textLabel.Text = "&Text:";
		this.m_descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_descriptionLabel.AutoSize = true;
		this.m_descriptionLabel.Location = new System.Drawing.Point(2, 135);
		this.m_descriptionLabel.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
		this.m_descriptionLabel.Name = "m_descriptionLabel";
		this.m_descriptionLabel.Size = new System.Drawing.Size(141, 29);
		this.m_descriptionLabel.TabIndex = 2;
		this.m_descriptionLabel.Text = "&Description:";
		this.okCancelButtonLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.okCancelButtonLayoutPanel.AutoSize = true;
		this.okCancelButtonLayoutPanel.ColumnCount = 2;
		this.m_mainTableLayoutPanel.SetColumnSpan(this.okCancelButtonLayoutPanel, 2);
		this.okCancelButtonLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.okCancelButtonLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.okCancelButtonLayoutPanel.Controls.Add(this.m_cancelButton, 1, 0);
		this.okCancelButtonLayoutPanel.Controls.Add(this.m_okButton, 0, 0);
		this.okCancelButtonLayoutPanel.Location = new System.Drawing.Point(203, 257);
		this.okCancelButtonLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 10, 2, 5);
		this.okCancelButtonLayoutPanel.Name = "okCancelButtonLayoutPanel";
		this.okCancelButtonLayoutPanel.RowCount = 1;
		this.okCancelButtonLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.okCancelButtonLayoutPanel.Size = new System.Drawing.Size(270, 32);
		this.okCancelButtonLayoutPanel.TabIndex = 5;
		this.m_cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(176, 2);
		this.m_cancelButton.Margin = new System.Windows.Forms.Padding(2);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(92, 28);
		this.m_cancelButton.TabIndex = 8;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_okButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_okButton.Location = new System.Drawing.Point(2, 2);
		this.m_okButton.Margin = new System.Windows.Forms.Padding(2, 2, 41, 2);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(92, 28);
		this.m_okButton.TabIndex = 7;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.checkboxLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.checkboxLayoutPanel.AutoSize = true;
		this.checkboxLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.checkboxLayoutPanel.Controls.Add(this.m_excludingCheck);
		this.checkboxLayoutPanel.Controls.Add(this.m_caseSensitiveCheck);
		this.checkboxLayoutPanel.Controls.Add(this.m_regularExpressionCheck);
		this.checkboxLayoutPanel.Location = new System.Drawing.Point(147, 171);
		this.checkboxLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
		this.checkboxLayoutPanel.Name = "checkboxLayoutPanel";
		this.checkboxLayoutPanel.Size = new System.Drawing.Size(453, 74);
		this.checkboxLayoutPanel.TabIndex = 4;
		this.m_excludingCheck.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_excludingCheck.AutoSize = true;
		this.m_excludingCheck.Location = new System.Drawing.Point(2, 2);
		this.m_excludingCheck.Margin = new System.Windows.Forms.Padding(2);
		this.m_excludingCheck.Name = "m_excludingCheck";
		this.m_excludingCheck.Size = new System.Drawing.Size(176, 33);
		this.m_excludingCheck.TabIndex = 4;
		this.m_excludingCheck.Text = "E&xcluding [!]";
		this.m_excludingCheck.UseVisualStyleBackColor = true;
		this.m_excludingCheck.CheckedChanged += new System.EventHandler(Excluding_CheckedChanged);
		this.m_caseSensitiveCheck.AutoSize = true;
		this.m_caseSensitiveCheck.Location = new System.Drawing.Point(200, 2);
		this.m_caseSensitiveCheck.Margin = new System.Windows.Forms.Padding(20, 2, 2, 2);
		this.m_caseSensitiveCheck.Name = "m_caseSensitiveCheck";
		this.m_caseSensitiveCheck.Size = new System.Drawing.Size(251, 33);
		this.m_caseSensitiveCheck.TabIndex = 5;
		this.m_caseSensitiveCheck.Text = "C&ase-sensitive [Aa]";
		this.m_caseSensitiveCheck.UseVisualStyleBackColor = true;
		this.m_regularExpressionCheck.AutoSize = true;
		this.m_regularExpressionCheck.Location = new System.Drawing.Point(20, 39);
		this.m_regularExpressionCheck.Margin = new System.Windows.Forms.Padding(20, 2, 2, 2);
		this.m_regularExpressionCheck.Name = "m_regularExpressionCheck";
		this.m_regularExpressionCheck.Size = new System.Drawing.Size(297, 33);
		this.m_regularExpressionCheck.TabIndex = 6;
		this.m_regularExpressionCheck.Text = "&Reguilar expression [R]";
		this.m_regularExpressionCheck.UseVisualStyleBackColor = true;
		this.m_topRowFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_topRowFlowLayoutPanel.AutoSize = true;
		this.m_topRowFlowLayoutPanel.Controls.Add(this.m_filterTypeCombo);
		this.m_topRowFlowLayoutPanel.Controls.Add(this.m_foreColorLabel);
		this.m_topRowFlowLayoutPanel.Controls.Add(this.m_foreColorCombo);
		this.m_topRowFlowLayoutPanel.Controls.Add(this.m_backColorLabel);
		this.m_topRowFlowLayoutPanel.Controls.Add(this.m_backColorCombo);
		this.m_topRowFlowLayoutPanel.Location = new System.Drawing.Point(145, 5);
		this.m_topRowFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.m_topRowFlowLayoutPanel.Name = "m_topRowFlowLayoutPanel";
		this.m_topRowFlowLayoutPanel.Size = new System.Drawing.Size(390, 77);
		this.m_topRowFlowLayoutPanel.TabIndex = 1002;
		this.m_filterTypeCombo.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_filterTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_filterTypeCombo.FormattingEnabled = true;
		this.m_filterTypeCombo.Location = new System.Drawing.Point(2, 2);
		this.m_filterTypeCombo.Margin = new System.Windows.Forms.Padding(2);
		this.m_filterTypeCombo.Name = "m_filterTypeCombo";
		this.m_filterTypeCombo.Size = new System.Drawing.Size(126, 37);
		this.m_filterTypeCombo.TabIndex = 10;
		this.m_filterTypeCombo.SelectedIndexChanged += new System.EventHandler(FilterType_SelectedIndexChanged);
		this.m_foreColorLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_foreColorLabel.AutoSize = true;
		this.m_foreColorLabel.Location = new System.Drawing.Point(145, 7);
		this.m_foreColorLabel.Margin = new System.Windows.Forms.Padding(15, 2, 2, 0);
		this.m_foreColorLabel.Name = "m_foreColorLabel";
		this.m_foreColorLabel.Size = new System.Drawing.Size(131, 29);
		this.m_foreColorLabel.TabIndex = 11;
		this.m_foreColorLabel.Text = "Text Co&lor:";
		this.m_foreColorCombo.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_foreColorCombo.BackgroundColorComboBox = false;
		this.m_foreColorCombo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.m_foreColorCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_foreColorCombo.FormattingEnabled = true;
		this.m_foreColorCombo.Location = new System.Drawing.Point(278, 2);
		this.m_foreColorCombo.Margin = new System.Windows.Forms.Padding(0);
		this.m_foreColorCombo.Name = "m_foreColorCombo";
		this.m_foreColorCombo.Size = new System.Drawing.Size(112, 36);
		this.m_foreColorCombo.TabIndex = 12;
		this.m_foreColorCombo.SelectedIndexChanged += new System.EventHandler(ForegroundColor_SelectedIndexChanged);
		this.m_backColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_backColorLabel.AutoSize = true;
		this.m_backColorLabel.Location = new System.Drawing.Point(15, 45);
		this.m_backColorLabel.Margin = new System.Windows.Forms.Padding(15, 2, 2, 0);
		this.m_backColorLabel.Name = "m_backColorLabel";
		this.m_backColorLabel.Size = new System.Drawing.Size(148, 29);
		this.m_backColorLabel.TabIndex = 13;
		this.m_backColorLabel.Text = "&Background:";
		this.m_backColorCombo.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_backColorCombo.BackgroundColorComboBox = true;
		this.m_backColorCombo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.m_backColorCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_backColorCombo.FormattingEnabled = true;
		this.m_backColorCombo.Location = new System.Drawing.Point(165, 41);
		this.m_backColorCombo.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
		this.m_backColorCombo.Name = "m_backColorCombo";
		this.m_backColorCombo.Size = new System.Drawing.Size(112, 36);
		this.m_backColorCombo.TabIndex = 14;
		this.m_backColorCombo.SelectedIndexChanged += new System.EventHandler(BackgroundColor_SelectedIndexChanged);
		this.m_filterTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_filterTypeLabel.AutoSize = true;
		this.m_filterTypeLabel.Location = new System.Drawing.Point(69, 29);
		this.m_filterTypeLabel.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
		this.m_filterTypeLabel.Name = "m_filterTypeLabel";
		this.m_filterTypeLabel.Size = new System.Drawing.Size(74, 29);
		this.m_filterTypeLabel.TabIndex = 9;
		this.m_filterTypeLabel.Text = "&Filter:";
		this.filterTextMarkerLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.filterTextMarkerLayoutPanel.AutoSize = true;
		this.filterTextMarkerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.filterTextMarkerLayoutPanel.ColumnCount = 2;
		this.filterTextMarkerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.filterTextMarkerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.filterTextMarkerLayoutPanel.Controls.Add(this.m_markerTypeCombo, 0, 0);
		this.filterTextMarkerLayoutPanel.Controls.Add(this.m_textCombo, 1, 0);
		this.filterTextMarkerLayoutPanel.Location = new System.Drawing.Point(145, 88);
		this.filterTextMarkerLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
		this.filterTextMarkerLayoutPanel.Name = "filterTextMarkerLayoutPanel";
		this.filterTextMarkerLayoutPanel.RowCount = 1;
		this.filterTextMarkerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.filterTextMarkerLayoutPanel.Size = new System.Drawing.Size(531, 41);
		this.filterTextMarkerLayoutPanel.TabIndex = 1;
		this.m_markerTypeCombo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_markerTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_markerTypeCombo.FormattingEnabled = true;
		this.m_markerTypeCombo.Location = new System.Drawing.Point(2, 2);
		this.m_markerTypeCombo.Margin = new System.Windows.Forms.Padding(2);
		this.m_markerTypeCombo.Name = "m_markerTypeCombo";
		this.m_markerTypeCombo.Size = new System.Drawing.Size(48, 37);
		this.m_markerTypeCombo.TabIndex = 16;
		this.m_textCombo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_textCombo.FormattingEnabled = true;
		this.m_textCombo.Location = new System.Drawing.Point(54, 2);
		this.m_textCombo.Margin = new System.Windows.Forms.Padding(2, 2, 20, 2);
		this.m_textCombo.Name = "m_textCombo";
		this.m_textCombo.Size = new System.Drawing.Size(508, 37);
		this.m_textCombo.TabIndex = 1;
		this.m_errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
		this.m_errorProvider.ContainerControl = this;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(14f, 29f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(685, 190);
		base.Controls.Add(this.m_mainTableLayoutPanel);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Margin = new System.Windows.Forms.Padding(2);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(703, 237);
		base.Name = "FilterDialog";
		base.Padding = new System.Windows.Forms.Padding(6);
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Add Filter";
		this.m_mainTableLayoutPanel.ResumeLayout(false);
		this.m_mainTableLayoutPanel.PerformLayout();
		this.okCancelButtonLayoutPanel.ResumeLayout(false);
		this.checkboxLayoutPanel.ResumeLayout(false);
		this.checkboxLayoutPanel.PerformLayout();
		this.m_topRowFlowLayoutPanel.ResumeLayout(false);
		this.m_topRowFlowLayoutPanel.PerformLayout();
		this.filterTextMarkerLayoutPanel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
	}
}
