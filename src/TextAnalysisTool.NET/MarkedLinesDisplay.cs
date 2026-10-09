using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
			SelectionMode = SelectionMode.One,
			DrawMode = DrawMode.OwnerDrawFixed,
			ItemHeight = Font.Height + 6,
			HorizontalScrollbar = true,
			ScrollAlwaysVisible = false
		};
		m_linesListBox.DrawItem += LinesListBox_DrawItem;
		m_linesListBox.SelectedIndexChanged += LinesListBox_SelectedIndexChanged;

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
		int maxWidth = 0;
		foreach (object item in m_linesListBox.Items)
		{
			maxWidth = Math.Max(maxWidth, TextRenderer.MeasureText(item.ToString(), m_linesListBox.Font).Width);
		}
		m_linesListBox.HorizontalExtent = maxWidth;
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
			for (int lineNumber = 1; lineNumber <= sourceLines.Count; lineNumber++)
			{
				bool marked = (m_selectedMarker < 0) ? sourceLines.IsLineMarkedByAny(lineNumber) : sourceLines.IsLineMarkedBy(lineNumber, m_selectedMarker);
				if (marked)
				{
					m_linesListBox.Items.Add(new MarkedLineItem(lineNumber, sourceLines.GetLineDisplayString(lineNumber, 0, includeMarkers: false)));
				}
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