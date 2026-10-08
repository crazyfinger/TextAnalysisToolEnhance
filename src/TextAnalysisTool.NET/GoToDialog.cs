using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ToolBox;

namespace TextAnalysisTool.NET;

internal class GoToDialog : Form
{
	private readonly string m_errorMessage;

	private bool m_validInput;

	private readonly int m_maximumLineNumber;

	private int m_lineNumber;

	private IContainer components;

	private TableLayoutPanel tableLayoutPanel1;

	private TableLayoutPanel tableLayoutPanel2;

	private Button m_cancelButton;

	private Button m_okButton;

	private Label m_lineLabel;

	private TextBox m_lineText;

	private ErrorProvider m_errorProvider;

	public int LineNumber => m_lineNumber;

	public GoToDialog(int currentLineNumber, int maximumLineNumber)
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterParent;
		Text = "Go To";
		m_lineLabel.Text = "&Line number:";
		m_lineText.TabIndex = 1;
		m_lineText.Text = currentLineNumber.ToString(CultureInfo.CurrentCulture);
		m_okButton.TabIndex = 2;
		m_okButton.Text = "&OK";
		m_cancelButton.TabIndex = 3;
		m_cancelButton.Text = "Cancel";
		m_maximumLineNumber = maximumLineNumber;
		m_errorMessage = $"Must specify a line number from 1 to {m_maximumLineNumber}";
		ValidateInput(null, null);
	}

	private void ValidateInput(object sender, EventArgs ea)
	{
		m_errorProvider.SetError(m_lineText, "");
		m_validInput = false;
		try
		{
			m_lineNumber = int.Parse(m_lineText.Text, CultureInfo.CurrentCulture);
			Utility.ThrowExceptionIf(m_lineNumber < 1 || m_maximumLineNumber < m_lineNumber, null, typeof(FormatException));
			m_validInput = true;
		}
		catch (Exception)
		{
			m_errorProvider.SetError(m_lineText, m_errorMessage);
		}
		m_okButton.Enabled = m_validInput;
	}

	private void OK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void Cancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
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
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_lineLabel = new System.Windows.Forms.Label();
		this.m_lineText = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.tableLayoutPanel1.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tableLayoutPanel1.ColumnCount = 2;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.m_lineLabel, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.m_lineText, 1, 0);
		this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 4);
		this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 2;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(242, 97);
		this.tableLayoutPanel1.TabIndex = 0;
		this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.tableLayoutPanel2.AutoSize = true;
		this.tableLayoutPanel2.ColumnCount = 2;
		this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.Controls.Add(this.m_cancelButton, 1, 0);
		this.tableLayoutPanel2.Controls.Add(this.m_okButton, 0, 0);
		this.tableLayoutPanel2.Location = new System.Drawing.Point(21, 50);
		this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 1;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 45);
		this.tableLayoutPanel2.TabIndex = 9;
		this.m_cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(112, 8);
		this.m_cancelButton.Margin = new System.Windows.Forms.Padding(2);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(86, 28);
		this.m_cancelButton.TabIndex = 3;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(Cancel_Click);
		this.m_okButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_okButton.Location = new System.Drawing.Point(2, 8);
		this.m_okButton.Margin = new System.Windows.Forms.Padding(2, 2, 12, 2);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(86, 28);
		this.m_okButton.TabIndex = 2;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OK_Click);
		this.m_lineLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.m_lineLabel.AutoSize = true;
		this.m_lineLabel.Location = new System.Drawing.Point(19, 15);
		this.m_lineLabel.Margin = new System.Windows.Forms.Padding(19, 0, 2, 0);
		this.m_lineLabel.Name = "m_lineLabel";
		this.m_lineLabel.Size = new System.Drawing.Size(93, 18);
		this.m_lineLabel.TabIndex = 0;
		this.m_lineLabel.Text = "&Line number:";
		this.m_lineText.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.m_lineText.Location = new System.Drawing.Point(116, 12);
		this.m_lineText.Margin = new System.Windows.Forms.Padding(2);
		this.m_lineText.Name = "m_lineText";
		this.m_lineText.Size = new System.Drawing.Size(82, 24);
		this.m_lineText.TabIndex = 1;
		this.m_lineText.TextChanged += new System.EventHandler(ValidateInput);
		this.m_errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
		this.m_errorProvider.ContainerControl = this;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(120f, 120f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(250, 106);
		base.Controls.Add(this.tableLayoutPanel1);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "GoToDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Go To";
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		this.tableLayoutPanel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
	}
}
