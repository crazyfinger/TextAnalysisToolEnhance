using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class RefineFilterControl : Panel
{
	private readonly TextBox m_textBox;

	private readonly CheckBox m_caseSensitiveCheckBox;

	private readonly CheckBox m_regexCheckBox;

	private readonly Timer m_debounceTimer;

	public event EventHandler CriteriaChanged;

	public string RefineText => m_textBox.Text;

	public bool CaseSensitive => m_caseSensitiveCheckBox.Checked;

	public bool UseRegex => m_regexCheckBox.Checked;

	public bool RefineTextBoxFocused => m_textBox.Focused;

	public void FocusRefineTextBox()
	{
		m_textBox.Focus();
	}

	public RefineFilterControl()
	{
		SuspendLayout();
		Height = 30;
		Padding = new Padding(3, 2, 3, 2);
		m_debounceTimer = new Timer
		{
			Interval = 250
		};
		m_debounceTimer.Tick += m_debounceTimer_Tick;
		m_textBox = new TextBox
		{
			Dock = DockStyle.Fill,
			Multiline = false
		};
		Label label = new Label
		{
			Text = "&Refine:",
			Dock = DockStyle.Left,
			AutoSize = true,
			Margin = new Padding(0, 4, 4, 0),
			TextAlign = ContentAlignment.MiddleLeft
		};
		m_caseSensitiveCheckBox = new CheckBox
		{
			Text = "Match &case",
			Dock = DockStyle.Right,
			AutoSize = true,
			Margin = new Padding(0, 5, 4, 0)
		};
		m_regexCheckBox = new CheckBox
		{
			Text = "Re&gex",
			Dock = DockStyle.Right,
			AutoSize = true,
			Margin = new Padding(0, 5, 4, 0)
		};
		Button clearButton = new Button
		{
			Text = "&Clear",
			Dock = DockStyle.Right,
			AutoSize = true,
			Margin = new Padding(0, 2, 0, 2),
			TabStop = false
		};
		m_textBox.TextChanged += m_textBox_TextChanged;
		m_caseSensitiveCheckBox.CheckedChanged += m_criteria_Changed;
		m_regexCheckBox.CheckedChanged += m_criteria_Changed;
		clearButton.Click += ClearButton_Click;
		Controls.Add(m_textBox);
		Controls.Add(label);
		Controls.Add(m_regexCheckBox);
		Controls.Add(m_caseSensitiveCheckBox);
		Controls.Add(clearButton);
		ResumeLayout(performLayout: true);
	}

	private void m_textBox_TextChanged(object sender, EventArgs e)
	{
		m_debounceTimer.Stop();
		m_debounceTimer.Start();
	}

	private void m_debounceTimer_Tick(object sender, EventArgs e)
	{
		m_debounceTimer.Stop();
		CriteriaChanged?.Invoke(this, EventArgs.Empty);
	}

	private void m_criteria_Changed(object sender, EventArgs e)
	{
		m_debounceTimer.Stop();
		CriteriaChanged?.Invoke(this, EventArgs.Empty);
	}

	private void ClearButton_Click(object sender, EventArgs e)
	{
		m_textBox.Clear();
	}
}