using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
public class FullyEnabledMenuItem : ToolStripMenuItem
{
	public int Index
	{
		get
		{
			int result = -1;
			if (base.OwnerItem is ToolStripDropDownItem toolStripDropDownItem)
			{
				result = toolStripDropDownItem.DropDownItems.IndexOf(this);
			}
			return result;
		}
	}

	public FullyEnabledMenuItem()
	{
	}

	public FullyEnabledMenuItem(Image image)
		: base(image)
	{
	}

	public FullyEnabledMenuItem(string text)
		: base(text)
	{
	}

	public FullyEnabledMenuItem(string text, Image image)
		: base(text, image)
	{
	}

	public FullyEnabledMenuItem(string text, Image image, EventHandler onClick)
		: base(text, image, onClick)
	{
	}

	public FullyEnabledMenuItem(string text, Image image, ToolStripItem[] dropDownItems)
		: base(text, image, dropDownItems)
	{
	}

	public FullyEnabledMenuItem(string text, Image image, EventHandler onClick, string name)
		: base(text, image, onClick, name)
	{
	}

	public FullyEnabledMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys)
		: base(text, image, onClick, shortcutKeys)
	{
	}

	protected override bool ProcessCmdKey(ref Message m, Keys keyData)
	{
		OnParentsDropDownShow();
		if (!AreAllParentsEnabled())
		{
			bool enabled = Enabled;
			Enabled = false;
			bool result = base.ProcessCmdKey(ref m, keyData);
			Enabled = enabled;
			return result;
		}
		return base.ProcessCmdKey(ref m, keyData);
	}

	private void OnParentsDropDownShow()
	{
		EventArgs e = new EventArgs();
		for (FullyEnabledMenuItem fullyEnabledMenuItem = this; fullyEnabledMenuItem != null; fullyEnabledMenuItem = fullyEnabledMenuItem.OwnerItem as FullyEnabledMenuItem)
		{
			fullyEnabledMenuItem.OnDropDownShow(e);
		}
	}

	private bool AreAllParentsEnabled()
	{
		if (!Enabled)
		{
			return false;
		}
		for (ToolStripItem ownerItem = base.OwnerItem; ownerItem != null; ownerItem = ownerItem.OwnerItem)
		{
			if (!ownerItem.Enabled)
			{
				return false;
			}
		}
		return true;
	}
}
