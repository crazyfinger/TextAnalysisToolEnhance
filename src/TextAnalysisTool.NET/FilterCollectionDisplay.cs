using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class FilterCollectionDisplay : DragDropListView
{
	private enum FilterSubitemEnum
	{
		Modifiers,
		Pattern,
		Description,
		Hitcount
	}

	private const int WM_CHAR = 258;

	private const int WM_LBUTTONDBLCLK = 515;

	private const int LVM_SETCOLUMNORDERARRAY = 4154;

	private readonly CTextAnalysisTool m_textAnalysisTool;

	private readonly FilterCollection m_filterCollection;

	private readonly LineCollectionDisplay m_lineCollectionDisplay;

	private readonly FullyEnabledMenuItem m_menuFindDialog;

	private readonly FullyEnabledMenuItem m_menuFindPrev;

	private readonly FullyEnabledMenuItem m_menuFindNext;

	private readonly FullyEnabledMenuItem m_menuMoveUp;

	private readonly FullyEnabledMenuItem m_menuMoveDown;

	private readonly FullyEnabledMenuItem m_menuEdit;

	private readonly FullyEnabledMenuItem m_menuCopy;

	private readonly FullyEnabledMenuItem m_menuPaste;

	private readonly FullyEnabledMenuItem m_menuRemove;

	private bool m_updatingDisplay;

	private bool m_filterDragInProgress;

	private const string CLIPBOARD_DATA_FILTER_FORMAT = "TEXTANALYSISTOOL.NET_FILTERS";

	private SolidBrush m_backgroundBrush;

	private Pen m_selectionRectanglePen;

	private int m_columnWidthDifference = -1;

	private int m_patternColumn;

	private Point m_mouseDownPoint = new Point(int.MinValue, int.MinValue);

	public FilterCollection FilterCollection => m_filterCollection;

	public Filter SelectedFilter
	{
		get
		{
			if (base.SelectedItems.Count == 0)
			{
				return null;
			}
			return (Filter)m_filterCollection.Filters[base.SelectedItems[0].Index];
		}
	}

	public static bool ClipboardContainsFilters => Clipboard.ContainsData("TEXTANALYSISTOOL.NET_FILTERS");

	public bool FiltersSelected => base.SelectedItems.Count > 0;

	private void CreateColumns()
	{
		base.Columns.Add("", 37);
		base.Columns.Add("Modifiers", 60);
		base.Columns.Add("Pattern", 595);
		base.Columns.Add("Description", 200);
		base.Columns.Add("Hits", 55);
		if (Settings.RememberFilterColumnLayout)
		{
			int num = 0;
			foreach (Match item in Regex.Matches(Settings.FilterColumnLayout, "([\\w]*):([\\d]+)"))
			{
				if (!int.TryParse(item.Groups[2].Value, out var result))
				{
					continue;
				}
				for (int i = 0; i < base.Columns.Count; i++)
				{
					if (string.Compare(base.Columns[i].Text, item.Groups[1].Value, ignoreCase: true) == 0)
					{
						base.Columns[i].DisplayIndex = num;
						base.Columns[i].Width = result;
						num++;
						break;
					}
				}
			}
		}
		int num2 = 0;
		for (int j = 0; j < base.Columns.Count; j++)
		{
			ColumnHeader columnHeader = base.Columns[j];
			if (string.Compare(columnHeader.Text, "Pattern") == 0)
			{
				m_patternColumn = j;
			}
			else
			{
				num2 += columnHeader.Width;
			}
		}
		if (!Settings.RememberFilterColumnLayout)
		{
			base.Columns[m_patternColumn].Width = base.ClientRectangle.Width - num2 - 25;
			m_columnWidthDifference = base.ClientRectangle.Width - base.Columns[m_patternColumn].Width;
		}
		base.ColumnWidthChanged += FilterCollectionDisplay_ColumnWidthChanged;
	}

	public FilterCollectionDisplay(CTextAnalysisTool textAnalysisTool, FilterCollection filterCollection, LineCollectionDisplay lineCollectionDisplay)
	{
		m_textAnalysisTool = textAnalysisTool;
		m_filterCollection = filterCollection;
		m_lineCollectionDisplay = lineCollectionDisplay;
		base.CheckBoxes = true;
		base.View = View.Details;
		base.FullRowSelect = true;
		base.MultiSelect = true;
		AllowDrop = true;
		base.AllowColumnReorder = true;
		CreateColumns();
		base.ItemCheck += FilterCollectionDisplay_ItemCheck;
		base.ItemChecked += FilterCollectionDisplay_ItemChecked;
		ItemDropped = (EventHandler<ItemDroppedEventArgs>)Delegate.Combine(ItemDropped, new EventHandler<ItemDroppedEventArgs>(FilterCollectionDisplay_ItemDropped));
		base.MouseDown += FilterCollectionDisplay_MouseDown;
		base.MouseUp += FilterCollectionDisplay_MouseUp;
		base.OwnerDraw = true;
		base.DrawItem += FilterCollectionDisplay_DrawItem;
		base.DrawSubItem += FilterCollectionDisplay_DrawSubItem;
		base.DrawColumnHeader += FilterCollectionDisplay_DrawColumnHeader;
		base.HideSelection = true;
		m_menuFindDialog = new FullyEnabledMenuItem("&Find...", null, Find_Click)
		{
			Tag = FilterFindAction.Load
		};
		m_menuFindPrev = new FullyEnabledMenuItem("&Previous Match", null, Find_Click, Keys.F8 | Keys.Shift)
		{
			Tag = FilterFindAction.Previous
		};
		m_menuFindNext = new FullyEnabledMenuItem("Ne&xt Match", null, Find_Click, Keys.F8)
		{
			Tag = FilterFindAction.Next
		};
		m_menuMoveUp = new FullyEnabledMenuItem("Move &Up", null, Move_Click);
		m_menuMoveDown = new FullyEnabledMenuItem("Move &Down", null, Move_Click);
		m_menuEdit = new FullyEnabledMenuItem("&Edit...", null, Edit_Click);
		m_menuCopy = new FullyEnabledMenuItem("&Copy", null, Copy_Click);
		m_menuPaste = new FullyEnabledMenuItem("P&aste", null, Paste_Click);
		m_menuRemove = new FullyEnabledMenuItem("&Remove", null, Remove_Click);
		ContextMenuStrip = new ContextMenuStrip();
		ContextMenuStrip.Items.AddRange(new ToolStripItem[15]
		{
			m_menuFindPrev,
			m_menuFindNext,
			new ToolStripSeparator(),
			m_menuFindDialog,
			new ToolStripSeparator(),
			m_menuMoveUp,
			m_menuMoveDown,
			new ToolStripSeparator(),
			m_menuEdit,
			m_menuRemove,
			new ToolStripSeparator(),
			m_menuCopy,
			m_menuPaste,
			new ToolStripSeparator(),
			new FullyEnabledMenuItem("Add &New Filter...", null, Add_Click)
		});
		ContextMenuStrip.Opening += Menu_Opening;
	}

	private void FilterCollectionDisplay_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		Point mouseDownPoint = m_mouseDownPoint;
		if (mouseDownPoint.X != int.MinValue && mouseDownPoint.Y != int.MinValue && !m_filterDragInProgress)
		{
			Point point = PointToClient(Control.MousePosition);
			int num = Math.Abs(point.X - mouseDownPoint.X);
			int num2 = Math.Abs(point.Y - mouseDownPoint.Y);
			if ((double)num > 10.0 || (double)num2 > 10.0)
			{
				e.NewValue = e.CurrentValue;
			}
		}
	}

	private void FilterCollectionDisplay_MouseUp(object sender, MouseEventArgs e)
	{
		m_mouseDownPoint.X = int.MinValue;
		m_mouseDownPoint.Y = int.MinValue;
	}

	private void FilterCollectionDisplay_MouseDown(object sender, MouseEventArgs e)
	{
		m_mouseDownPoint = e.Location;
	}

	public void DoFind(FilterFindAction tag)
	{
		if (base.SelectedItems.Count <= 0 || m_filterCollection == null || base.SelectedItems[0].Index >= m_filterCollection.Filters.Count)
		{
			return;
		}
		Filter filter = (Filter)m_filterCollection.Filters[base.SelectedItems[0].Index];
		LineCollection filteredLines = filter.FilteredLines;
		if (filteredLines != null)
		{
			if (tag == FilterFindAction.Load)
			{
				m_textAnalysisTool.ShowFindDialog(findReverse: false, filter);
				return;
			}
			bool reverse = tag == FilterFindAction.Previous;
			m_lineCollectionDisplay.ListBox.FindNextMatch(filter, reverse);
		}
	}

	private void Find_Click(object sender, EventArgs e)
	{
		FilterFindAction tag = (FilterFindAction)((FullyEnabledMenuItem)sender).Tag;
		DoFind(tag);
	}

	public void CacheColumnLayout()
	{
		Settings.FilterColumnLayout = string.Join(",", from ColumnHeader c in base.Columns
			orderby c.DisplayIndex
			select $"{c.Text}:{c.Width}");
	}

	private void FilterCollectionDisplay_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		if (e.ColumnIndex != m_patternColumn || base.Columns[m_patternColumn].Width > 0)
		{
			m_columnWidthDifference = base.ClientRectangle.Width - base.Columns[m_patternColumn].Width;
			CacheColumnLayout();
		}
	}

	private void Menu_Opening(object sender, CancelEventArgs e)
	{
		int num = ((base.SelectedItems.Count > 0) ? base.SelectedItems[0].Index : (-1));
		bool flag = m_textAnalysisTool.LinesLoaded && m_textAnalysisTool.LinesVisible;
		m_menuFindDialog.Enabled = -1 != num && flag;
		m_menuFindPrev.Enabled = -1 != num && flag;
		m_menuFindNext.Enabled = -1 != num && flag;
		m_menuMoveUp.Enabled = 0 < num;
		m_menuMoveDown.Enabled = -1 != num && num + 1 < m_filterCollection.Filters.Count;
		m_menuEdit.Enabled = -1 != num;
		m_menuRemove.Enabled = -1 != num;
		m_menuCopy.Enabled = -1 != num;
		m_menuPaste.Enabled = Clipboard.ContainsData("TEXTANALYSISTOOL.NET_FILTERS");
	}

	private void Move_Click(object sender, EventArgs e)
	{
		int num = ((base.SelectedItems.Count > 0) ? base.SelectedItems[0].Index : (-1));
		int index = num + ((sender != m_menuMoveUp) ? 1 : (-1));
		m_filterCollection.Swap((Filter)m_filterCollection.Filters[num], (Filter)m_filterCollection.Filters[index]);
		base.Items[index].Selected = true;
		base.Items[index].Focused = true;
	}

	private void Edit_Click(object sender, EventArgs e)
	{
		ShowEditFiltersDialog();
	}

	public void ShowEditFiltersDialog()
	{
		bool flag = true;
		List<Filter> list = new List<Filter>();
		if (SelectedFilter == null)
		{
			return;
		}
		if (base.SelectedItems.Count > 1)
		{
			foreach (int selectedIndex in base.SelectedIndices)
			{
				Filter filter = (Filter)m_filterCollection.Filters[selectedIndex];
				if (SelectedFilter.GetType() != filter.GetType())
				{
					flag = false;
					break;
				}
				list.Add(filter);
			}
		}
		if (base.SelectedItems.Count == 1 || !flag)
		{
			m_textAnalysisTool.ShowEditFilterDialog(SelectedFilter);
		}
		else
		{
			m_textAnalysisTool.ShowEditFilterDialog(list);
		}
	}

	private void Copy_Click(object sender, EventArgs e)
	{
		if (base.SelectedItems.Count > 0)
		{
			CopySelectedFiltersToClipboard();
		}
	}

	private void Paste_Click(object sender, EventArgs e)
	{
		PasteFiltersFromClipboard();
	}

	private void Remove_Click(object sender, EventArgs e)
	{
		m_filterCollection.Remove(SelectedFilter);
	}

	private void Add_Click(object sender, EventArgs e)
	{
		m_textAnalysisTool.ShowAddFilterDialog();
	}

	public void UpdateDisplay()
	{
		BeginUpdate();
		BackColor = Settings.Window;
		ForeColor = Settings.WindowText;
		int val = ((base.SelectedItems.Count > 0) ? base.SelectedItems[0].Index : (-1));
		int num = ((base.TopItem != null) ? base.TopItem.Index : 0);
		m_updatingDisplay = true;
		if (!m_filterCollection.IsEnableDisableFilterChange)
		{
			base.Items.Clear();
			int num2 = 0;
			foreach (Filter filter2 in m_filterCollection.Filters)
			{
				ListViewItem value = CreateItemFromFilter(filter2, num2);
				base.Items.Add(value);
				num2++;
			}
		}
		else
		{
			foreach (ListViewItem item in base.Items)
			{
				Filter filter = (Filter)m_filterCollection.Filters[item.Index];
				item.Checked = filter.Enabled;
			}
		}
		if (base.Items.Count > 0)
		{
			int index = Math.Min(Math.Max(val, 0), base.Items.Count - 1);
			base.Items[index].Selected = true;
			base.Items[index].Focused = true;
		}
		EndUpdate();
		if (base.Items.Count > num)
		{
			base.TopItem = base.Items[num];
		}
		else if (base.Items.Count > 0)
		{
			base.TopItem = base.Items[0];
		}
		CacheColumnLayout();
		m_updatingDisplay = false;
	}

	private static ListViewItem CreateItemFromFilter(Filter f, int index)
	{
		ListViewItem listViewItem = new ListViewItem
		{
			Checked = f.Enabled,
			UseItemStyleForSubItems = false
		};
		if (index < 26)
		{
			listViewItem.Text = ((char)(97 + index)).ToString();
		}
		else
		{
			listViewItem.Text = index.ToString();
		}
		string text = (f.Excluding ? "[!]" : string.Empty) + (f.CaseSensitive ? " [Aa]" : string.Empty) + " " + f.FilterType;
		ListViewItem.ListViewSubItem listViewSubItem = new ListViewItem.ListViewSubItem();
		listViewSubItem.Text = text.Trim();
		listViewSubItem.Name = FilterSubitemEnum.Modifiers.ToString();
		listViewItem.SubItems.Add(listViewSubItem);
		ListViewItem.ListViewSubItem listViewSubItem2 = new ListViewItem.ListViewSubItem();
		listViewSubItem2.Text = f.FilterPattern;
		listViewSubItem2.Name = FilterSubitemEnum.Pattern.ToString();
		listViewSubItem2.ForeColor = f.ForegroundColor;
		listViewSubItem2.BackColor = f.BackgroundColor;
		listViewItem.SubItems.Add(listViewSubItem2);
		ListViewItem.ListViewSubItem listViewSubItem3 = new ListViewItem.ListViewSubItem();
		listViewSubItem3.Text = (string.IsNullOrEmpty(f.Description) ? string.Empty : f.Description);
		listViewSubItem3.Name = FilterSubitemEnum.Description.ToString();
		listViewItem.SubItems.Add(listViewSubItem3);
		ListViewItem.ListViewSubItem listViewSubItem4 = new ListViewItem.ListViewSubItem();
		listViewSubItem4.Text = (f.Enabled ? f.FilteredLines.Count.ToString() : string.Empty);
		listViewSubItem4.Name = FilterSubitemEnum.Hitcount.ToString();
		listViewItem.SubItems.Add(listViewSubItem4);
		return listViewItem;
	}

	private void FilterCollectionDisplay_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		if (e != null && e.Item != null)
		{
			Filter filter = (Filter)m_filterCollection.Filters[e.Item.Index];
			if (!m_updatingDisplay)
			{
				m_filterCollection.SetEnable(filter, e.Item.Checked);
			}
			base.Items[e.Item.Index].SubItems[FilterSubitemEnum.Hitcount.ToString()].Text = (filter.Enabled ? filter.FilteredLines.Count.ToString() : string.Empty);
		}
	}

	private void FilterCollectionDisplay_ItemDropped(object sender, ItemDroppedEventArgs e)
	{
		m_filterDragInProgress = true;
		Filter filter = m_filterCollection.Filters[e.OldIndex] as Filter;
		m_filterCollection.Move(filter, e.NewIndex);
		m_filterDragInProgress = false;
	}

	public void CopySelectedFiltersToClipboard()
	{
		StringWriter stringWriter = new StringWriter();
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
		try
		{
			xmlTextWriter.WriteStartElement("filters");
			foreach (int selectedIndex in base.SelectedIndices)
			{
				((Filter)m_filterCollection.Filters[selectedIndex]).ToXml(xmlTextWriter);
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Flush();
			Clipboard.SetData("TEXTANALYSISTOOL.NET_FILTERS", stringWriter.ToString());
		}
		catch (Exception)
		{
		}
	}

	public void PasteFiltersFromClipboard()
	{
		if (!Clipboard.ContainsData("TEXTANALYSISTOOL.NET_FILTERS"))
		{
			return;
		}
		List<Filter> list = new List<Filter>();
		try
		{
			XmlTextReader reader = new XmlTextReader(new StringReader((string)Clipboard.GetData("TEXTANALYSISTOOL.NET_FILTERS")));
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(reader);
			if (xmlDocument.DocumentElement.Name != "filters")
			{
				return;
			}
			foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
			{
				Filter filter = Filter.FromXml(childNode);
				if (filter != null)
				{
					list.Add(filter);
				}
			}
			m_filterCollection.AddRange(list);
		}
		catch (Exception)
		{
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (!Settings.RememberFilterColumnLayout)
		{
			int val = base.ClientSize.Width - m_columnWidthDifference;
			base.Columns[m_patternColumn].Width = Math.Max(0, val);
		}
		base.OnLayout(levent);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		Keys keys = Keys.KeyCode & e.KeyData;
		if (Keys.Delete == keys && !e.Shift && !e.Control && !e.Alt && SelectedFilter != null)
		{
			m_filterCollection.Remove(SelectedFilter);
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
		{
			CopySelectedFiltersToClipboard();
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
		{
			if (Clipboard.ContainsData("TEXTANALYSISTOOL.NET_FILTERS"))
			{
				PasteFiltersFromClipboard();
				e.Handled = true;
			}
		}
		else if ((Keys.D1 <= keys && keys <= Keys.D8) || (Keys.A <= keys && keys <= Keys.Z))
		{
			m_lineCollectionDisplay.ListBox.CallOnKeyDown(e);
			e.Handled = true;
		}
		else if (keys == Keys.Return && SelectedFilter != null)
		{
			m_textAnalysisTool.ShowEditFilterDialog(SelectedFilter);
		}
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 515)
		{
			if (SelectedFilter != null)
			{
				m_textAnalysisTool.ShowEditFilterDialog(SelectedFilter);
			}
			else
			{
				m_textAnalysisTool.ShowAddFilterDialog();
			}
		}
		else if (m.Msg == 4154)
		{
			CacheColumnLayout();
			base.WndProc(ref m);
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override bool ProcessKeyMessage(ref Message m)
	{
		if (258 != m.Msg || 32 == (ushort)(int)m.WParam)
		{
			return base.ProcessKeyMessage(ref m);
		}
		return true;
	}

	private void FilterCollectionDisplay_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
	{
		e.DrawDefault = true;
	}

	private void FilterCollectionDisplay_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
	{
		bool flag = base.SelectedItems.Contains(e.Item);
		if (m_backgroundBrush == null || m_backgroundBrush.Color != Settings.FilterHighlight)
		{
			m_backgroundBrush?.Dispose();
			m_backgroundBrush = new SolidBrush(Settings.FilterHighlight);
		}
		if (flag && e.ColumnIndex != m_patternColumn)
		{
			Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width, e.Bounds.Height - 2);
			if (e.ColumnIndex == 0)
			{
				rect.X++;
			}
			if (e.ColumnIndex == base.Columns.Count - 1)
			{
				rect.Width--;
			}
			e.Graphics.FillRectangle(m_backgroundBrush, rect);
		}
		if (e.ColumnIndex == 0)
		{
			Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.UncheckedNormal);
			int num = (e.Bounds.Height - glyphSize.Width) / 2;
			Rectangle rectangle = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + num, glyphSize.Width, glyphSize.Height);
			CheckBoxRenderer.DrawCheckBox(e.Graphics, rectangle.Location, (!e.Item.Checked) ? CheckBoxState.UncheckedNormal : CheckBoxState.CheckedNormal);
			Rectangle bounds = new Rectangle(rectangle.X + glyphSize.Width, e.Bounds.Y, e.Bounds.Width - glyphSize.Width, e.Bounds.Height);
			TextRenderer.DrawText(e.Graphics, e.Item.Text, e.Item.Font, bounds, e.Item.ForeColor, TextFormatFlags.VerticalCenter);
		}
		else if (e.ColumnIndex == m_patternColumn)
		{
			using (SolidBrush brush = new SolidBrush(e.SubItem.BackColor))
			{
				if (flag)
				{
					e.Graphics.FillRectangle(brush, e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width, e.Bounds.Height - 2);
				}
				else
				{
					e.Graphics.FillRectangle(brush, e.Bounds);
				}
			}
			TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.VerticalCenter);
		}
		else
		{
			TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.VerticalCenter);
		}
	}

	private void FilterCollectionDisplay_DrawItem(object sender, DrawListViewItemEventArgs e)
	{
		if (m_selectionRectanglePen == null || m_selectionRectanglePen.Color != Settings.FilterHighlightBox)
		{
			m_selectionRectanglePen?.Dispose();
			m_selectionRectanglePen = new Pen(Settings.FilterHighlightBox, 1f);
		}
		if (base.SelectedItems.Contains(e.Item))
		{
			e.Graphics.DrawRectangle(m_selectionRectanglePen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
		}
	}
}
