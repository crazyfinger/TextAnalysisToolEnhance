using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ToolBox;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class MarkedLinesDisplay : Panel
{
	private const int NumMarkers = 8;

	private readonly LineCollectionDisplayListBox m_listBox;

	private readonly ComboBox m_markerComboBox;

	private readonly ListBox m_linesListBox;

	private int m_selectedMarker = -1;

	public MarkedLinesDisplay(LineCollectionDisplayListBox listBox)
	{
		m_listBox = listBox;
		base.BorderStyle = BorderStyle.FixedSingle;
		base.Padding = new Padding(2);

		Label label = new Label
		{
			Text = "Marked Lines",
			Dock = DockStyle.Top,
			AutoSize = true,
			TextAlign = ContentAlignment.MiddleLeft
		};

		m_markerComboBox = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			Dock = DockStyle.Top,
			Height = Font.Height + 8
		};
		m_markerComboBox.Items.Add("All markers");
		for (int i = 1; i <= NumMarkers; i++)
		{
			m_markerComboBox.Items.Add("Marker " + i);
		}
		m_markerComboBox.SelectedIndex = 0;
		m_markerComboBox.SelectedIndexChanged += MarkerComboBox_SelectedIndexChanged;

		m_linesListBox = new ListBox
		{
			Dock = DockStyle.Fill,
			IntegralHeight = false,
			SelectionMode = SelectionMode.MultiExtended,
			DrawMode = DrawMode.OwnerDrawFixed,
			ItemHeight = Font.Height + 6,
			HorizontalScrollbar = true,
			ScrollAlwaysVisible = false
		};
		m_linesListBox.DrawItem += LinesListBox_DrawItem;
		m_linesListBox.SelectedIndexChanged += LinesListBox_SelectedIndexChanged;
		m_linesListBox.KeyDown += LinesListBox_KeyDown;

		ContextMenuStrip contextMenu = new ContextMenuStrip();
		FullyEnabledMenuItem copyMenuItem = new FullyEnabledMenuItem("&Copy", null, CopyMenuItem_Click, Keys.C | Keys.Control);
		FullyEnabledMenuItem selectAllMenuItem = new FullyEnabledMenuItem("Select &All", null, SelectAllMenuItem_Click, Keys.A | Keys.Control);
		contextMenu.Items.Add(copyMenuItem);
		contextMenu.Items.Add(new ToolStripSeparator());
		contextMenu.Items.Add(selectAllMenuItem);
		contextMenu.Opening += (s, e) => copyMenuItem.Enabled = m_linesListBox.SelectedItems.Count > 0;
		m_linesListBox.ContextMenuStrip = contextMenu;

		base.Controls.Add(m_linesListBox);
		base.Controls.Add(m_markerComboBox);
		base.Controls.Add(label);
	}

	private void MarkerComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_selectedMarker = m_markerComboBox.SelectedIndex - 1;
		RefreshMarkedLines();
	}

	private void LinesListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_linesListBox.SelectedIndex >= 0 && m_linesListBox.SelectedItem is MarkedLineItem markedLineItem)
		{
			m_listBox.SelectLineNumber(markedLineItem.LineNumber);
		}
	}

	private void LinesListBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && !e.Alt && !e.Shift)
		{
			if (e.KeyCode == Keys.C)
			{
				CopySelectedLines();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.A)
			{
				m_linesListBox.BeginUpdate();
				for (int i = 0; i < m_linesListBox.Items.Count; i++)
				{
					m_linesListBox.SetSelected(i, true);
				}
				m_linesListBox.EndUpdate();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}
	}

	private void CopyMenuItem_Click(object sender, EventArgs e)
	{
		CopySelectedLines();
	}

	private void SelectAllMenuItem_Click(object sender, EventArgs e)
	{
		m_linesListBox.BeginUpdate();
		for (int i = 0; i < m_linesListBox.Items.Count; i++)
		{
			m_linesListBox.SetSelected(i, true);
		}
		m_linesListBox.EndUpdate();
	}

	private void CopySelectedLines()
	{
		if (m_linesListBox.SelectedItems.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object item in m_linesListBox.SelectedItems)
		{
			if (item is MarkedLineItem markedLineItem)
			{
				stringBuilder.AppendLine(markedLineItem.LineNumber + ": " + markedLineItem.Text);
			}
		}
		Utility.SafeSetClipboardText(stringBuilder.ToString());
	}

	private void LinesListBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index < 0 || e.Index >= m_linesListBox.Items.Count)
		{
			return;
		}
		MarkedLineItem markedLineItem = (MarkedLineItem)m_linesListBox.Items[e.Index];
		bool selected = (e.State & DrawItemState.Selected) != 0;
		m_listBox.GetLineColors(markedLineItem.LineNumber, selected, out var foreColor, out var backColor);
		using (SolidBrush brush = new SolidBrush(backColor))
		{
			e.Graphics.FillRectangle(brush, e.Bounds);
		}
		Rectangle textBounds = e.Bounds;
		textBounds.Width = Math.Max(textBounds.Width, m_linesListBox.HorizontalExtent);
		TextRenderer.DrawText(e.Graphics, markedLineItem.ToString(), e.Font, textBounds, foreColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter);
		if (selected)
		{
			e.DrawFocusRectangle();
		}
	}

	public void ApplyFont(Font font)
	{
		Font = font;
		m_markerComboBox.Font = font;
		m_markerComboBox.Height = font.Height + 8;
		m_linesListBox.Font = font;
		m_linesListBox.ItemHeight = font.Height + 6;
		RefreshHorizontalExtent();
	}

	private void RefreshHorizontalExtent()
	{
		if (m_linesListBox.Items.Count == 0)
		{
			m_linesListBox.HorizontalExtent = 0;
			return;
		}
		MarkedLineItem longestItem = null;
		int maxLen = 0;
		foreach (object item in m_linesListBox.Items)
		{
			if (item is MarkedLineItem mi && mi.Text.Length > maxLen)
			{
				maxLen = mi.Text.Length;
				longestItem = mi;
			}
		}
		if (longestItem != null)
		{
			m_linesListBox.HorizontalExtent = TextRenderer.MeasureText(longestItem.ToString(), m_linesListBox.Font).Width;
		}
	}

	public void RefreshMarkedLines()
	{
		m_linesListBox.BeginUpdate();
		try
		{
			m_linesListBox.Items.Clear();
			LineCollection sourceLines = m_listBox.SourceLines;
			if (sourceLines == null || sourceLines.Count == 0)
			{
				m_linesListBox.HorizontalExtent = 0;
				return;
			}
			foreach (int lineNumber in sourceLines.GetMarkedLineNumbers(m_selectedMarker))
			{
				m_linesListBox.Items.Add(new MarkedLineItem(lineNumber, sourceLines.GetLineDisplayString(lineNumber, 0, includeMarkers: false)));
			}
			RefreshHorizontalExtent();
		}
		finally
		{
			m_linesListBox.EndUpdate();
		}
	}
}

internal sealed class MarkedLineItem
{
	public int LineNumber { get; }

	public string Text => m_text;

	private readonly string m_text;

	public MarkedLineItem(int lineNumber, string text)
	{
		LineNumber = lineNumber;
		m_text = text;
	}

	public override string ToString()
	{
		return LineNumber + ": " + m_text;
	}
}