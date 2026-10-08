using System.ComponentModel;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class LineCollectionDisplay : TableLayoutPanel
{
	private readonly CTextAnalysisTool m_textAnalysisTool;

	private readonly LineCollectionDisplayListBox m_listbox;

	private readonly VScrollBar m_customScrollBar;

	private bool m_disposed;

	public LineCollectionDisplayListBox ListBox => m_listbox;

	public LineCollectionDisplay(CTextAnalysisTool textAnalysisTool)
	{
		m_textAnalysisTool = textAnalysisTool;
		base.RowCount = 1;
		base.ColumnCount = 2;
		base.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		base.ColumnStyles.Add(new ColumnStyle());
		m_customScrollBar = new VScrollBar
		{
			Dock = DockStyle.Right,
			Minimum = 0
		};
		m_customScrollBar.Scroll += CustomScrollBar_Scroll;
		m_listbox = new LineCollectionDisplayListBox(m_textAnalysisTool, m_customScrollBar)
		{
			Dock = DockStyle.Fill
		};
		base.Controls.Add(m_listbox, 0, 0);
		base.Controls.Add(m_customScrollBar, 1, 0);
	}

	private void CustomScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		m_listbox.TopIndex = e.NewValue;
	}

	protected override void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (disposing)
			{
				m_customScrollBar.Dispose();
				m_listbox.Dispose();
			}
			m_disposed = true;
			base.Dispose(disposing);
		}
	}
}
