using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class RefineFilterControl : Panel
{
	private readonly TextBox m_textBox;

	public event EventHandler RefineTextChanged;

	public string RefineText => m_textBox.Text;

	public RefineFilterControl()
	{
		SuspendLayout();
		Height = 26;
		m_textBox = new TextBox
		{
			Dock = DockStyle.Fill,
			Multiline = false
		};
		Label label = new Label
		{
			Text = "&Refine:",
			Dock = DockStyle.Left,
			AutoSize = false,
			Width = 56,
			TextAlign = ContentAlignment.MiddleLeft
		};
		Button button = new Button
		{
			Text = "Clear",
			Dock = DockStyle.Right,
			Width = 60,
			TabStop = false
		};
		m_textBox.TextChanged += m_textBox_TextChanged;
		button.Click += ClearButton_Click;
		Controls.Add(m_textBox);
		Controls.Add(label);
		Controls.Add(button);
		ResumeLayout(performLayout: true);
	}

	private void m_textBox_TextChanged(object sender, EventArgs e)
	{
		RefineTextChanged?.Invoke(this, EventArgs.Empty);
	}

	private void ClearButton_Click(object sender, EventArgs e)
	{
		m_textBox.Clear();
	}
}