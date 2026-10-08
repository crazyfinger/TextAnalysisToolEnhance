using System;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

public class ButtonDropDownMenu : Button
{
	private bool m_menuIsOpened;

	private ContextMenuStrip m_contextMenu;

	public ContextMenuStrip DropDownMenu => m_contextMenu;

	public ButtonDropDownMenu(ContextMenuStrip dropDownMenu)
	{
		m_contextMenu = dropDownMenu;
		m_contextMenu.Closing += ContextMenu_Closing;
	}

	protected override void OnClick(EventArgs e)
	{
		if (DropDownMenu != null)
		{
			if (m_menuIsOpened)
			{
				m_menuIsOpened = false;
			}
			else
			{
				m_menuIsOpened = true;
				DropDownMenu.Show(base.Parent, new Point(base.Left, base.Bottom));
			}
		}
		base.OnClick(e);
	}

	private void ContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (e.CloseReason != ToolStripDropDownCloseReason.AppClicked || !RectangleToScreen(base.ClientRectangle).Contains(Control.MousePosition))
		{
			m_menuIsOpened = false;
		}
	}
}
