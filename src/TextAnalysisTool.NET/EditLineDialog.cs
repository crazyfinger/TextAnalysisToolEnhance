using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
		m_textBox.Font = new Font(m_textBox.Font.FontFamily, m_textBox.Font.Size + 4f);

		Button copyButton = new Button
		{
			Text = "&Copy",
			AutoSize = true,
			Padding = new Padding(20, 4, 20, 4),
			UseVisualStyleBackColor = true,
			TabStop = false
		};
		copyButton.Click += CopyButton_Click;

		Button confirmButton = new Button
		{
			Text = "&Confirm",
			AutoSize = true,
			Padding = new Padding(20, 4, 20, 4),
			UseVisualStyleBackColor = true,
			DialogResult = DialogResult.OK
		};

		TableLayoutPanel buttonsPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Bottom,
			AutoSize = true,
			ColumnCount = 4,
			RowCount = 1,
			Padding = new Padding(6, 4, 6, 4)
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
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void CopyButton_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetText(m_textBox.Text);
		}
		catch
		{
		}
	}
}