using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class RefineFilterControl : Panel
{
	private const int MinTextBoxChars = 5;

	private const int MaxGap = 8;

	private const int ContentPaddingX = 2;

	private const int ContentPaddingY = 3;

	private const int RowGap = 4;

	private readonly Panel m_content;

	private readonly TextBox m_textBox;

	private readonly Label m_label;

	private readonly CheckBox m_excludingCheckBox;

	private readonly CheckBox m_caseSensitiveCheckBox;

	private readonly CheckBox m_regexCheckBox;

	private readonly Button m_clearButton;

	private readonly Timer m_debounceTimer;

	private bool m_twoLine;

	public event EventHandler CriteriaChanged;

	public string RefineText => m_textBox.Text;

	public bool Excluding => m_excludingCheckBox.Checked;

	public bool CaseSensitive => m_caseSensitiveCheckBox.Checked;

	public bool UseRegex => m_regexCheckBox.Checked;

	public bool RefineTextBoxFocused => m_textBox.Focused;

	public void FocusRefineTextBox()
	{
		m_textBox.Focus();
	}

	public void ClearRefine()
	{
		m_textBox.Clear();
	}

	public void ApplyFont(Font font)
	{
		Font = font;
		m_textBox.Font = font;
		UpdateLayout();
	}

	public RefineFilterControl()
	{
		SuspendLayout();
		Padding = new Padding(2);
		m_debounceTimer = new Timer
		{
			Interval = 250
		};
		m_debounceTimer.Tick += m_debounceTimer_Tick;
		m_label = new Label
		{
			Text = "&Refine:",
			AutoSize = true,
			TextAlign = ContentAlignment.MiddleLeft
		};
		m_textBox = new TextBox
		{
			Multiline = false
		};
		m_excludingCheckBox = new CheckBox
		{
			Text = "&Excluding",
			AutoSize = true
		};
		m_caseSensitiveCheckBox = new CheckBox
		{
			Text = "Match &case",
			AutoSize = true
		};
		m_regexCheckBox = new CheckBox
		{
			Text = "Re&gex",
			AutoSize = true
		};
		m_clearButton = new Button
		{
			Text = "&Clear",
			AutoSize = true,
			TabStop = false
		};
		m_content = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			BackColor = Color.Transparent
		};
		m_content.Controls.Add(m_label);
		m_content.Controls.Add(m_textBox);
		m_content.Controls.Add(m_excludingCheckBox);
		m_content.Controls.Add(m_caseSensitiveCheckBox);
		m_content.Controls.Add(m_regexCheckBox);
		m_content.Controls.Add(m_clearButton);
		m_textBox.TextChanged += m_textBox_TextChanged;
		m_excludingCheckBox.CheckedChanged += m_criteria_Changed;
		m_caseSensitiveCheckBox.CheckedChanged += m_criteria_Changed;
		m_regexCheckBox.CheckedChanged += m_criteria_Changed;
		m_clearButton.Click += ClearButton_Click;
		Controls.Add(m_content);
		UpdateLayout();
		ResumeLayout(performLayout: true);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		int cw = Math.Max(0, m_content.ClientSize.Width - 2 * ContentPaddingX);
		int labelW = m_label.PreferredSize.Width;
		int exclW = m_excludingCheckBox.PreferredSize.Width;
		int caseW = m_caseSensitiveCheckBox.PreferredSize.Width;
		int regexW = m_regexCheckBox.PreferredSize.Width;
		int clearW = m_clearButton.PreferredSize.Width;
		int fixedW = labelW + exclW + caseW + regexW + clearW;
		int minTextW = MinTextBoxChars * TextRenderer.MeasureText("0", Font).Width;
		int scrollbarHeight = SystemInformation.HorizontalScrollBarHeight;
		int textBoxHeight = m_textBox.Height;
		int labelH = m_label.PreferredSize.Height;
		int exclH = m_excludingCheckBox.PreferredSize.Height;
		int caseH = m_caseSensitiveCheckBox.PreferredSize.Height;
		int regexH = m_regexCheckBox.PreferredSize.Height;
		int clearH = m_clearButton.PreferredSize.Height;
		int contentHeight;

		m_twoLine = cw < fixedW + minTextW;

		if (m_twoLine)
		{
			int row0Height = Math.Max(textBoxHeight, labelH);
			int x = ContentPaddingX;
			int y0 = ContentPaddingY;
			m_label.SetBounds(x, CenterY(y0, row0Height, labelH), labelW, labelH);
			x += labelW;
			int textW = Math.Max(minTextW, cw - labelW);
			m_textBox.SetBounds(x, CenterY(y0, row0Height, textBoxHeight), textW, textBoxHeight);

			int row1Height = Math.Max(Math.Max(exclH, caseH), Math.Max(regexH, clearH));
			int y1 = y0 + row0Height + RowGap;
			int x1 = ContentPaddingX;
			x1 = PlaceRight(m_excludingCheckBox, x1, CenterY(y1, row1Height, exclH));
			x1 = PlaceRight(m_caseSensitiveCheckBox, x1, CenterY(y1, row1Height, caseH));
			x1 = PlaceRight(m_regexCheckBox, x1, CenterY(y1, row1Height, regexH));
			PlaceRight(m_clearButton, x1, CenterY(y1, row1Height, clearH));

			int row0Width = labelW + textW + 2 * ContentPaddingX;
			int row1Width = exclW + caseW + regexW + clearW + 3 * MaxGap + 2 * ContentPaddingX;
			contentHeight = y1 + row1Height + ContentPaddingY;
			m_content.AutoScrollMinSize = new Size(Math.Max(cw, Math.Max(row0Width, row1Width)), 0);
		}
		else
		{
			int gap = MaxGap;
			if (cw < fixedW + minTextW + MaxGap * 5)
			{
				gap = Math.Max(0, (cw - fixedW - minTextW) / 5);
			}
			int textW = Math.Max(minTextW, cw - fixedW - gap * 5);
			int rowHeight = Math.Max(textBoxHeight, Math.Max(Math.Max(exclH, caseH), Math.Max(regexH, clearH)));
			int y = CenterY(ContentPaddingY, rowHeight, textBoxHeight);
			int x = ContentPaddingX;
			PlaceRight(m_label, x, CenterY(ContentPaddingY, rowHeight, labelH));
			x += labelW + gap;
			m_textBox.SetBounds(x, y, textW, textBoxHeight);
			x += textW + gap;
			x = PlaceRight(m_excludingCheckBox, x, CenterY(ContentPaddingY, rowHeight, exclH));
			x = PlaceRight(m_caseSensitiveCheckBox, x, CenterY(ContentPaddingY, rowHeight, caseH));
			x = PlaceRight(m_regexCheckBox, x, CenterY(ContentPaddingY, rowHeight, regexH));
			PlaceRight(m_clearButton, x, CenterY(ContentPaddingY, rowHeight, clearH));

			contentHeight = rowHeight + 2 * ContentPaddingY;
			int contentWidth = labelW + textW + exclW + caseW + regexW + clearW + gap * 5 + 2 * ContentPaddingX;
			m_content.AutoScrollMinSize = new Size(Math.Max(cw, contentWidth), 0);
		}

		int newHeight = contentHeight + scrollbarHeight + Padding.Vertical;
		if (newHeight != m_lastHeight)
		{
			m_lastHeight = newHeight;
			Height = newHeight;
		}
	}

	private static int CenterY(int top, int rowHeight, int controlHeight)
	{
		return top + (rowHeight - controlHeight) / 2;
	}

	private int m_lastHeight = -1;

	private int PlaceRight(Control control, int x, int y)
	{
		control.SetBounds(x, y, control.PreferredSize.Width, control.PreferredSize.Height);
		return x + control.PreferredSize.Width + MaxGap;
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
		ClearRefine();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_debounceTimer != null)
			{
				m_debounceTimer.Stop();
				m_debounceTimer.Tick -= m_debounceTimer_Tick;
				m_debounceTimer.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}