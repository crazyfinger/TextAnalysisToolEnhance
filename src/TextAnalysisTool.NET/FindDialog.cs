using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("Form")]
internal sealed class FindDialog : FilterFindDialogBase
{
	private bool m_reverse;

	private IContainer components;

	private TableLayoutPanel m_mainTableLayoutPanel;

	private Label m_textLabel;

	private ComboBox m_textCombo;

	private FlowLayoutPanel m_checkboxFlowLayoutPanel;

	private CheckBox m_caseSensitiveCheck;

	private CheckBox m_regularExpressionCheck;

	private CheckBox m_reverseCheck;

	private TableLayoutPanel tableLayoutPanel2;

	private Button m_cancelButton;

	private Button m_okButton;

	private ToolTip m_toolTip;

	private ErrorProvider m_errorProvider;

	public bool Reverse => m_reverse;

	public FindDialog(string initialText, bool findReverse)
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
		Text = "Find";
		m_reverseCheck.Text = "Re&verse";
		m_minWidthControls.Add(m_textLabel);
		m_minWidthControls.Add(m_checkboxFlowLayoutPanel);
		m_minHeightControls.Add(m_mainTableLayoutPanel);
		RecalculateFormSizes(Settings.FindDialogWidth, 4000);
		base.Resize += FindDialog_Resize;
	}

	private void FindDialog_Resize(object sender, EventArgs e)
	{
		if (sender is Form)
		{
			Settings.FindDialogWidth = base.Width;
		}
	}

	protected override void OK_Click(object sender, EventArgs e)
	{
		base.OK_Click(sender, e);
		m_filter = new MatchesTextFilter(enabled: false, excluding: false, Color.Empty, Color.Empty, m_textCombo.Text, m_caseSensitiveCheck.Checked, m_regularExpressionCheck.Checked);
		m_reverse = m_reverseCheck.Checked;
		base.DialogResult = DialogResult.OK;
		Close();
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
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_textLabel = new System.Windows.Forms.Label();
		this.m_textCombo = new System.Windows.Forms.ComboBox();
		this.m_checkboxFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_caseSensitiveCheck = new System.Windows.Forms.CheckBox();
		this.m_regularExpressionCheck = new System.Windows.Forms.CheckBox();
		this.m_reverseCheck = new System.Windows.Forms.CheckBox();
		this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_mainTableLayoutPanel.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.m_checkboxFlowLayoutPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_mainTableLayoutPanel.ColumnCount = 2;
		this.m_mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.m_mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.m_mainTableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 0, 2);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_textLabel, 0, 0);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_textCombo, 1, 0);
		this.m_mainTableLayoutPanel.Controls.Add(this.m_checkboxFlowLayoutPanel, 1, 1);
		this.m_mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_mainTableLayoutPanel.Location = new System.Drawing.Point(6, 6);
		this.m_mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
		this.m_mainTableLayoutPanel.Name = "m_mainTableLayoutPanel";
		this.m_mainTableLayoutPanel.RowCount = 3;
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.m_mainTableLayoutPanel.Size = new System.Drawing.Size(591, 115);
		this.m_mainTableLayoutPanel.TabIndex = 0;
		this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.tableLayoutPanel2.AutoSize = true;
		this.tableLayoutPanel2.ColumnCount = 2;
		this.m_mainTableLayoutPanel.SetColumnSpan(this.tableLayoutPanel2, 2);
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.Controls.Add(this.m_cancelButton, 1, 0);
		this.tableLayoutPanel2.Controls.Add(this.m_okButton, 0, 0);
		this.tableLayoutPanel2.Location = new System.Drawing.Point(143, 69);
		this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 1;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.Size = new System.Drawing.Size(304, 44);
		this.tableLayoutPanel2.TabIndex = 2;
		this.m_cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_cancelButton.Location = new System.Drawing.Point(198, 6);
		this.m_cancelButton.Margin = new System.Windows.Forms.Padding(2);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(104, 32);
		this.m_cancelButton.TabIndex = 6;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_okButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_okButton.Location = new System.Drawing.Point(2, 6);
		this.m_okButton.Margin = new System.Windows.Forms.Padding(2, 2, 46, 2);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(104, 32);
		this.m_okButton.TabIndex = 5;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_textLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_textLabel.AutoSize = true;
		this.m_textLabel.Location = new System.Drawing.Point(3, 13);
		this.m_textLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 5);
		this.m_textLabel.Name = "m_textLabel";
		this.m_textLabel.Size = new System.Drawing.Size(40, 18);
		this.m_textLabel.TabIndex = 0;
		this.m_textLabel.Text = "Text:";
		this.m_textCombo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_textCombo.FormattingEnabled = true;
		this.m_textCombo.Location = new System.Drawing.Point(49, 10);
		this.m_textCombo.Margin = new System.Windows.Forms.Padding(3, 10, 20, 3);
		this.m_textCombo.Name = "m_textCombo";
		this.m_textCombo.Size = new System.Drawing.Size(522, 26);
		this.m_textCombo.TabIndex = 1;
		this.m_checkboxFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_checkboxFlowLayoutPanel.AutoSize = true;
		this.m_checkboxFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_checkboxFlowLayoutPanel.Controls.Add(this.m_caseSensitiveCheck);
		this.m_checkboxFlowLayoutPanel.Controls.Add(this.m_regularExpressionCheck);
		this.m_checkboxFlowLayoutPanel.Controls.Add(this.m_reverseCheck);
		this.m_checkboxFlowLayoutPanel.Location = new System.Drawing.Point(46, 39);
		this.m_checkboxFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
		this.m_checkboxFlowLayoutPanel.Name = "m_checkboxFlowLayoutPanel";
		this.m_checkboxFlowLayoutPanel.Size = new System.Drawing.Size(501, 28);
		this.m_checkboxFlowLayoutPanel.TabIndex = 1;
		this.m_checkboxFlowLayoutPanel.WrapContents = false;
		this.m_caseSensitiveCheck.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.m_caseSensitiveCheck.AutoSize = true;
		this.m_caseSensitiveCheck.Location = new System.Drawing.Point(3, 3);
		this.m_caseSensitiveCheck.Name = "m_caseSensitiveCheck";
		this.m_caseSensitiveCheck.Size = new System.Drawing.Size(156, 22);
		this.m_caseSensitiveCheck.TabIndex = 2;
		this.m_caseSensitiveCheck.Text = "Case-sensitive [Aa]";
		this.m_caseSensitiveCheck.UseVisualStyleBackColor = true;
		this.m_regularExpressionCheck.AutoSize = true;
		this.m_regularExpressionCheck.Location = new System.Drawing.Point(196, 3);
		this.m_regularExpressionCheck.Margin = new System.Windows.Forms.Padding(34, 3, 3, 3);
		this.m_regularExpressionCheck.Name = "m_regularExpressionCheck";
		this.m_regularExpressionCheck.Size = new System.Drawing.Size(180, 22);
		this.m_regularExpressionCheck.TabIndex = 3;
		this.m_regularExpressionCheck.Text = "Regular expression [R]";
		this.m_regularExpressionCheck.UseVisualStyleBackColor = true;
		this.m_reverseCheck.AutoSize = true;
		this.m_reverseCheck.Location = new System.Drawing.Point(413, 3);
		this.m_reverseCheck.Margin = new System.Windows.Forms.Padding(34, 3, 3, 3);
		this.m_reverseCheck.Name = "m_reverseCheck";
		this.m_reverseCheck.Size = new System.Drawing.Size(85, 22);
		this.m_reverseCheck.TabIndex = 4;
		this.m_reverseCheck.Text = "Reverse";
		this.m_reverseCheck.UseVisualStyleBackColor = true;
		this.m_errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
		this.m_errorProvider.ContainerControl = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(9f, 18f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(603, 127);
		base.Controls.Add(this.m_mainTableLayoutPanel);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(4498, 200);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(621, 174);
		base.Name = "FindDialog";
		base.Padding = new System.Windows.Forms.Padding(6);
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "FindDialog";
		this.m_mainTableLayoutPanel.ResumeLayout(false);
		this.m_mainTableLayoutPanel.PerformLayout();
		this.tableLayoutPanel2.ResumeLayout(false);
		this.m_checkboxFlowLayoutPanel.ResumeLayout(false);
		this.m_checkboxFlowLayoutPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
	}
}
