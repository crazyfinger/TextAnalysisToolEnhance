using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ToolBox;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class EditLineDialog : Form
{
	private readonly TextBox m_textBox;

	public string EditedText => m_textBox.Text;

	public bool IsTextChanged => m_textBox.Text != m_originalText;

	private readonly string m_originalText;

	public EditLineDialog(string originalText)
	{
		m_originalText = originalText;
		SuspendLayout();
		Text = "Edit Line";
		base.FormBorderStyle = FormBorderStyle.Sizable;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterParent;
		base.ClientSize = new Size(720, 460);
		base.MinimumSize = new Size(400, 300);
		base.MaximizeBox = true;

		m_textBox = new TextBox
		{
			Multiline = true,
			WordWrap = true,
			ScrollBars = ScrollBars.Both,
			AcceptsReturn = true,
			AcceptsTab = true,
			Dock = DockStyle.Fill,
			Text = originalText
		};
		m_textBox.Font = new Font(m_textBox.Font.FontFamily, m_textBox.Font.Size + 6f);

		Button copyButton = new Button
		{
			Text = "&Copy",
			AutoSize = true,
			MinimumSize = new Size(140, 40),
			Padding = new Padding(14, 5, 14, 5),
			Margin = new Padding(0, 0, 20, 0),
			UseVisualStyleBackColor = true,
			TabStop = false
		};
		copyButton.Click += CopyButton_Click;

		Button confirmButton = new Button
		{
			Text = "&OK",
			AutoSize = true,
			MinimumSize = new Size(140, 40),
			Padding = new Padding(14, 5, 14, 5),
			Margin = new Padding(20, 0, 0, 0),
			UseVisualStyleBackColor = true,
			DialogResult = DialogResult.OK
		};

		TableLayoutPanel buttonsPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Bottom,
			AutoSize = true,
			ColumnCount = 4,
			RowCount = 1,
			Padding = new Padding(6, 14, 6, 14)
		};
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		buttonsPanel.Controls.Add(copyButton, 1, 0);
		buttonsPanel.Controls.Add(confirmButton, 2, 0);

		base.Controls.Add(m_textBox);
		base.Controls.Add(buttonsPanel);
		base.CancelButton = confirmButton;
		Shown += (_, _) => BeginInvoke((Action)(() => m_textBox.Select(m_textBox.Text.Length, m_textBox.Text.Length)));
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void CopyButton_Click(object sender, EventArgs e)
	{
		Utility.SafeSetClipboardText(m_textBox.Text);
	}
}