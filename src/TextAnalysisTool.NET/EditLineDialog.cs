using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ToolBox;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class EditLineDialog : Form
{
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
	private const int WM_SETREDRAW = 0x000B;

	private static readonly Color MatchHighlightColor = Color.FromArgb(255, 235, 59); // Chrome / Notepad 亮黄色高亮

	private readonly RichTextBox m_richTextBox;
	private readonly string m_originalText;

	// 搜索条控件
	private readonly Panel m_searchPanel;
	private readonly TextBox m_txtSearch;
	private readonly Button m_btnConfirm;
	private readonly Button m_btnPrev;
	private readonly Button m_btnNext;
	private readonly Label m_lblMatchCount;
	private readonly CheckBox m_chkMatchCase;
	private readonly Button m_btnClose;

	// 搜索状态
	private readonly List<int> m_matchIndices = new List<int>();
	private int m_currentMatchIndex = -1;
	private string m_lastSearchText = string.Empty;
	private bool m_lastMatchCase;
	private bool m_isHighlighting;

	public string EditedText => m_richTextBox.Text;

	public bool IsTextChanged => m_richTextBox.Text != m_originalText;

	public EditLineDialog(string originalText)
	{
		m_originalText = originalText ?? string.Empty;
		SuspendLayout();
		Text = "Edit Line";
		base.FormBorderStyle = FormBorderStyle.Sizable;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterParent;
		base.ClientSize = new Size(760, 520);
		base.MinimumSize = new Size(520, 360);
		base.MaximizeBox = true;
		base.KeyPreview = true;

		m_richTextBox = new RichTextBox
		{
			Multiline = true,
			WordWrap = true,
			ScrollBars = RichTextBoxScrollBars.Both,
			AcceptsTab = true,
			Dock = DockStyle.Fill,
			Text = m_originalText,
			DetectUrls = false,
			BorderStyle = BorderStyle.None,
			Font = new Font(Font.FontFamily, Font.Size + 6f)
		};
		m_richTextBox.TextChanged += RichTextBox_TextChanged;

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
			Padding = new Padding(6, 12, 6, 12)
		};
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		buttonsPanel.Controls.Add(copyButton, 1, 0);
		buttonsPanel.Controls.Add(confirmButton, 2, 0);

		// 构建搜索条面板 (类似 Windows 记事本)
		m_searchPanel = BuildSearchPanel(out m_txtSearch, out m_btnConfirm, out m_btnPrev, out m_btnNext, out m_lblMatchCount, out m_chkMatchCase, out m_btnClose);

		base.Controls.Add(m_richTextBox);
		base.Controls.Add(m_searchPanel);
		base.Controls.Add(buttonsPanel);

		// 保证 Z-order 层次：buttonsPanel 在最底，m_searchPanel 在其上，m_richTextBox 填满剩余空间
		m_searchPanel.SendToBack();
		buttonsPanel.SendToBack();

		base.CancelButton = confirmButton;
		Shown += (_, _) => BeginInvoke((Action)(() => m_richTextBox.Select(m_richTextBox.Text.Length, 0)));
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private Panel BuildSearchPanel(
		out TextBox txtSearch,
		out Button btnConfirm,
		out Button btnPrev,
		out Button btnNext,
		out Label lblMatchCount,
		out CheckBox chkMatchCase,
		out Button btnClose)
	{
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 60,
			MinimumSize = new Size(0, 60),
			BackColor = Color.FromArgb(245, 246, 248),
			Visible = false,
			Padding = new Padding(12, 0, 12, 0)
		};
		panel.Paint += (s, e) =>
		{
			e.Graphics.DrawLine(SystemPens.ControlDark, 0, 0, panel.Width, 0);
		};

		TableLayoutPanel table = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 1,
			ColumnCount = 9,
			Margin = Padding.Empty,
			Padding = Padding.Empty
		};
		table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 0: 图标
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 1: txtSearch
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 2: btnConfirm
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 3: btnPrev
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 4: btnNext
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 5: lblMatchCount
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 6: chkMatchCase
		table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // 7: btnClose
		table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // 8: 留白

		PictureBox picIcon = new PictureBox
		{
			Size = new Size(20, 20),
			SizeMode = PictureBoxSizeMode.CenterImage,
			Image = CreateSearchIcon(),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 8, 0)
		};

		txtSearch = new TextBox
		{
			Width = 260,
			Font = new Font("Segoe UI", 9.5f),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 8, 0)
		};
		txtSearch.KeyDown += (s, e) =>
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				if (e.Shift)
				{
					NavigatePrevious();
				}
				else
				{
					PerformSearch(navigateNext: true);
				}
			}
			else if (e.KeyCode == Keys.Escape)
			{
				e.SuppressKeyPress = true;
				HideSearchBar();
			}
		};
		txtSearch.TextChanged += (s, e) =>
		{
			if (string.IsNullOrEmpty(m_txtSearch.Text))
			{
				ClearHighlights();
				m_lblMatchCount.Text = string.Empty;
			}
		};

		btnConfirm = new Button
		{
			Text = "确定",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowOnly,
			MinimumSize = new Size(72, 32),
			Padding = new Padding(12, 3, 12, 3),
			Font = new Font("Segoe UI", 8f),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 8, 0)
		};
		btnConfirm.Click += (s, e) => PerformSearch(navigateNext: true);

		btnPrev = new Button
		{
			Text = "↑",
			Size = new Size(30, 30),
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 3, 0)
		};
		btnPrev.Click += (s, e) => NavigatePrevious();

		btnNext = new Button
		{
			Text = "↓",
			Size = new Size(30, 30),
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 10, 0)
		};
		btnNext.Click += (s, e) => NavigateNext();

		lblMatchCount = new Label
		{
			AutoSize = true,
			Font = new Font("Segoe UI", 9f),
			TextAlign = ContentAlignment.MiddleLeft,
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 10, 0)
		};

		chkMatchCase = new CheckBox
		{
			Text = "Match case",
			AutoSize = true,
			Font = new Font("Segoe UI", 9f),
			Anchor = AnchorStyles.Left,
			Margin = new Padding(0, 0, 10, 0)
		};
		chkMatchCase.CheckedChanged += (s, e) =>
		{
			if (!string.IsNullOrEmpty(m_txtSearch.Text))
			{
				m_lastSearchText = string.Empty;
				PerformSearch(navigateNext: false);
			}
		};

		btnClose = new Button
		{
			Text = "✕",
			Size = new Size(30, 30),
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 9f),
			Anchor = AnchorStyles.Left,
			Margin = Padding.Empty
		};
		btnClose.FlatAppearance.BorderSize = 0;
		btnClose.Click += (s, e) => HideSearchBar();

		ToolTip toolTip = new ToolTip();
		toolTip.SetToolTip(btnConfirm, "Search and highlight matches (Enter)");
		toolTip.SetToolTip(btnPrev, "Previous match (Shift+Enter / Shift+F3)");
		toolTip.SetToolTip(btnNext, "Next match (Enter / F3)");
		toolTip.SetToolTip(btnClose, "Close search bar (Esc)");

		table.Controls.Add(picIcon, 0, 0);
		table.Controls.Add(txtSearch, 1, 0);
		table.Controls.Add(btnConfirm, 2, 0);
		table.Controls.Add(btnPrev, 3, 0);
		table.Controls.Add(btnNext, 4, 0);
		table.Controls.Add(lblMatchCount, 5, 0);
		table.Controls.Add(chkMatchCase, 6, 0);
		table.Controls.Add(btnClose, 7, 0);

		panel.Controls.Add(table);
		return panel;
	}

	private static Image CreateSearchIcon()
	{
		Bitmap bmp = new Bitmap(16, 16);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			using Pen pen = new Pen(Color.FromArgb(90, 90, 90), 1.8f);
			g.DrawEllipse(pen, 2, 2, 8, 8);
			g.DrawLine(pen, 8.5f, 8.5f, 13.5f, 13.5f);
		}
		return bmp;
	}

	private void PerformSearch(bool navigateNext)
	{
		string query = m_txtSearch.Text;
		if (string.IsNullOrEmpty(query))
		{
			ClearHighlights();
			m_lblMatchCount.Text = string.Empty;
			return;
		}

		string fullText = m_richTextBox.Text;
		bool matchCase = m_chkMatchCase.Checked;
		StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

		if (m_lastSearchText != query || m_lastMatchCase != matchCase)
		{
			m_lastSearchText = query;
			m_lastMatchCase = matchCase;
			m_matchIndices.Clear();

			int start = 0;
			while (start < fullText.Length)
			{
				int found = fullText.IndexOf(query, start, comparison);
				if (found < 0)
				{
					break;
				}
				m_matchIndices.Add(found);
				start = found + query.Length;
			}

			HighlightAllMatches();

			if (m_matchIndices.Count > 0)
			{
				int caret = m_richTextBox.SelectionStart;
				m_currentMatchIndex = 0;
				for (int i = 0; i < m_matchIndices.Count; i++)
				{
					if (m_matchIndices[i] >= caret)
					{
						m_currentMatchIndex = i;
						break;
					}
				}
			}
			else
			{
				m_currentMatchIndex = -1;
			}
		}
		else if (navigateNext && m_matchIndices.Count > 0)
		{
			m_currentMatchIndex = (m_currentMatchIndex + 1) % m_matchIndices.Count;
		}

		UpdateMatchSelectionAndCount();
	}

	private void NavigatePrevious()
	{
		if (m_matchIndices.Count == 0)
		{
			return;
		}
		m_currentMatchIndex = (m_currentMatchIndex - 1 + m_matchIndices.Count) % m_matchIndices.Count;
		UpdateMatchSelectionAndCount();
	}

	private void NavigateNext()
	{
		if (m_matchIndices.Count == 0)
		{
			return;
		}
		m_currentMatchIndex = (m_currentMatchIndex + 1) % m_matchIndices.Count;
		UpdateMatchSelectionAndCount();
	}

	private void UpdateMatchSelectionAndCount()
	{
		if (m_matchIndices.Count == 0)
		{
			m_lblMatchCount.Text = "No matches";
			m_lblMatchCount.ForeColor = Color.Red;
			return;
		}

		m_lblMatchCount.Text = $"{m_currentMatchIndex + 1} of {m_matchIndices.Count}";
		m_lblMatchCount.ForeColor = SystemColors.ControlText;

		int matchPos = m_matchIndices[m_currentMatchIndex];
		int matchLen = m_lastSearchText.Length;

		m_richTextBox.Select(matchPos, matchLen);
		m_richTextBox.ScrollToCaret();
	}

	private void HighlightAllMatches()
	{
		int originalStart = m_richTextBox.SelectionStart;
		int originalLength = m_richTextBox.SelectionLength;

		m_isHighlighting = true;
		SendMessage(m_richTextBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
		try
		{
			m_richTextBox.SelectAll();
			m_richTextBox.SelectionBackColor = m_richTextBox.BackColor;
			m_richTextBox.SelectionColor = m_richTextBox.ForeColor;

			int queryLen = m_lastSearchText.Length;
			foreach (int index in m_matchIndices)
			{
				m_richTextBox.Select(index, queryLen);
				m_richTextBox.SelectionBackColor = MatchHighlightColor;
				m_richTextBox.SelectionColor = Color.Black;
			}
		}
		finally
		{
			m_richTextBox.Select(originalStart, originalLength);
			SendMessage(m_richTextBox.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
			m_isHighlighting = false;
			m_richTextBox.Invalidate();
		}
	}

	private void ClearHighlights()
	{
		if (m_matchIndices.Count == 0 && string.IsNullOrEmpty(m_lastSearchText))
		{
			return;
		}

		int originalStart = m_richTextBox.SelectionStart;
		int originalLength = m_richTextBox.SelectionLength;

		m_isHighlighting = true;
		SendMessage(m_richTextBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
		try
		{
			m_richTextBox.SelectAll();
			m_richTextBox.SelectionBackColor = m_richTextBox.BackColor;
			m_richTextBox.SelectionColor = m_richTextBox.ForeColor;
		}
		finally
		{
			m_richTextBox.Select(originalStart, originalLength);
			SendMessage(m_richTextBox.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
			m_isHighlighting = false;
			m_richTextBox.Invalidate();
		}

		m_matchIndices.Clear();
		m_currentMatchIndex = -1;
		m_lastSearchText = string.Empty;
	}

	public void ShowSearchBar()
	{
		m_searchPanel.Visible = true;
		if (!string.IsNullOrEmpty(m_richTextBox.SelectedText) && !m_richTextBox.SelectedText.Contains("\n"))
		{
			m_txtSearch.Text = m_richTextBox.SelectedText;
		}

		m_txtSearch.Focus();
		m_txtSearch.SelectAll();

		if (!string.IsNullOrEmpty(m_txtSearch.Text))
		{
			PerformSearch(navigateNext: false);
		}
	}

	public void HideSearchBar()
	{
		if (!m_searchPanel.Visible)
		{
			return;
		}
		m_searchPanel.Visible = false;
		ClearHighlights();
		m_richTextBox.Focus();
	}

	private void RichTextBox_TextChanged(object sender, EventArgs e)
	{
		if (m_isHighlighting)
		{
			return;
		}

		if (m_searchPanel.Visible && !string.IsNullOrEmpty(m_txtSearch.Text))
		{
			m_lastSearchText = string.Empty;
			PerformSearch(navigateNext: false);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Control | Keys.F))
		{
			ShowSearchBar();
			return true;
		}

		if (keyData == Keys.Escape && m_searchPanel.Visible)
		{
			HideSearchBar();
			return true;
		}

		if (keyData == Keys.F3)
		{
			if (!m_searchPanel.Visible)
			{
				ShowSearchBar();
			}
			else
			{
				NavigateNext();
			}
			return true;
		}

		if (keyData == (Keys.Shift | Keys.F3))
		{
			if (m_searchPanel.Visible)
			{
				NavigatePrevious();
				return true;
			}
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void CopyButton_Click(object sender, EventArgs e)
	{
		Utility.SafeSetClipboardText(m_richTextBox.Text);
	}
}
