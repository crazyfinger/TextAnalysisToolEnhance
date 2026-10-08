using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
public class DragDropListView : ListView
{
	public class ItemDroppedEventArgs : EventArgs
	{
		public int OldIndex { get; set; }

		public int NewIndex { get; set; }
	}

	public EventHandler<ItemDroppedEventArgs> ItemDropped;

	private const int WM_PAINT = 15;

	private const int WM_LBUTTONDOWN = 513;

	private const int MK_LBUTTON = 1;

	private const int MK_SHIFT = 4;

	private const int MK_CTRL = 8;

	private const int MK_LBUTTON_SHIFT = 5;

	private const int MK_LBUTTON_CTRL = 9;

	private int m_insertionLineIndex = -1;

	public DragDropListView()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(SelectedListViewItemCollection)))
		{
			e.Effect = DragDropEffects.Move;
		}
		base.OnDragEnter(e);
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(SelectedListViewItemCollection)))
		{
			DrawInsertionLine(GetInsertionIndexFromPos(e.X, e.Y));
		}
		base.OnDragOver(e);
	}

	protected override void OnDragLeave(EventArgs e)
	{
		ClearInsertionLine();
		base.OnDragLeave(e);
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(SelectedListViewItemCollection)))
		{
			int index = (e.Data.GetData(typeof(SelectedListViewItemCollection)) as SelectedListViewItemCollection)[0].Index;
			int num = GetInsertionIndexFromPos(e.X, e.Y);
			if (num != index)
			{
				if (num > index)
				{
					num--;
				}
				ItemDropped?.Invoke(this, new ItemDroppedEventArgs
				{
					NewIndex = num,
					OldIndex = index
				});
			}
			ClearInsertionLine();
		}
		base.OnDragDrop(e);
	}

	protected override void OnItemDrag(ItemDragEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoDragDrop(base.SelectedItems, DragDropEffects.Move);
			base.OnItemDrag(e);
		}
	}

	private void SelectItemRange(int clickedItemIndex)
	{
		int num = -1;
		if (base.FocusedItem != null)
		{
			num = base.FocusedItem.Index;
		}
		if (num != -1 && num != clickedItemIndex)
		{
			int num2 = num;
			int num3 = clickedItemIndex;
			if (num2 > num3)
			{
				int num4 = num2;
				num2 = num3;
				num3 = num4;
			}
			for (int i = 0; i < base.Items.Count; i++)
			{
				if (i >= num2 && i <= num3)
				{
					base.Items[i].Selected = true;
				}
				else
				{
					base.Items[i].Selected = false;
				}
			}
		}
		else
		{
			base.Items[clickedItemIndex].Selected = !base.Items[clickedItemIndex].Selected;
		}
	}

	private bool HandleLMouseDown(IntPtr WParam, IntPtr LParam)
	{
		bool result = false;
		int num = (int)LParam;
		int num2 = num & 0xFFFF;
		int num3 = num >> 16;
		ListViewItem itemAt = GetItemAt(num2, num3);
		if (itemAt != null)
		{
			if (WParam == (IntPtr)9)
			{
				itemAt.Selected = !itemAt.Selected;
				result = true;
			}
			else if (WParam == (IntPtr)5)
			{
				SelectItemRange(itemAt.Index);
				result = true;
			}
		}
		return result;
	}

	protected override void WndProc(ref Message m)
	{
		bool flag = false;
		if (m.Msg == 513 && (m.WParam == (IntPtr)5 || m.WParam == (IntPtr)9))
		{
			flag = HandleLMouseDown(m.WParam, m.LParam);
		}
		if (!flag)
		{
			base.WndProc(ref m);
		}
		if (m.Msg == 15)
		{
			if (m_insertionLineIndex >= 0 && m_insertionLineIndex < base.Items.Count)
			{
				Rectangle bounds = base.Items[m_insertionLineIndex].GetBounds(ItemBoundsPortion.Entire);
				RenderInsertionLine(bounds.Right, bounds.Top);
			}
			else if (m_insertionLineIndex == base.Items.Count)
			{
				Rectangle bounds2 = base.Items[base.Items.Count - 1].GetBounds(ItemBoundsPortion.Entire);
				RenderInsertionLine(bounds2.Right, bounds2.Bottom);
			}
		}
	}

	private void DrawInsertionLine(int index)
	{
		if (m_insertionLineIndex != index)
		{
			m_insertionLineIndex = index;
			Invalidate();
		}
	}

	private void ClearInsertionLine()
	{
		if (m_insertionLineIndex != -1)
		{
			m_insertionLineIndex = -1;
			Invalidate();
		}
	}

	private int GetInsertionIndexFromPos(int x, int y)
	{
		Point point = PointToClient(new Point(x, y));
		ListViewItem listViewItem = GetItemAt(0, point.Y);
		if (listViewItem == null)
		{
			listViewItem = ((point.Y >= base.Items[0].GetBounds(ItemBoundsPortion.Entire).Bottom) ? base.Items[base.Items.Count - 1] : base.Items[0]);
		}
		Rectangle bounds = listViewItem.GetBounds(ItemBoundsPortion.Entire);
		if (point.Y < bounds.Top + bounds.Height / 2)
		{
			return listViewItem.Index;
		}
		return listViewItem.Index + 1;
	}

	private void RenderInsertionLine(int x, int y)
	{
		using Graphics graphics = CreateGraphics();
		Pen pen = new Pen(SystemColors.ControlDarkDark, 2f);
		graphics.DrawLine(pen, 0, y, x, y);
	}
}
