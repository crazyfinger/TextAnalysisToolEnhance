using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolBox;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class UtilityTests
{
	[TestMethod]
	public void SafeSetClipboardText_NullText_ReturnsFalse()
	{
		bool result = Utility.SafeSetClipboardText(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void MainWindow_ShortcutsConfiguredCorrectly()
	{
		Exception thrown = null;
		Thread thread = new Thread(() =>
		{
			try
			{
				CTextAnalysisTool tat = new CTextAnalysisTool();
				FieldInfo mainField = typeof(CTextAnalysisTool).GetField("m_mainWindow", BindingFlags.NonPublic | BindingFlags.Instance);
				MainWindow main = (MainWindow)mainField.GetValue(tat);
				main.CreateControl();
				IntPtr h = main.Handle;

				BindingFlags fieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;
				var saveFilters = (FullyEnabledMenuItem)typeof(MainWindow).GetField("m_menuFileSaveFilters", fieldFlags).GetValue(main);
				Assert.AreEqual(Keys.S | Keys.Control | Keys.Alt, saveFilters.ShortcutKeys);

				var editPrefs = (FullyEnabledMenuItem)typeof(MainWindow).GetField("m_menuEditPreferences", fieldFlags).GetValue(main);
				Assert.AreEqual("Ctrl+,", editPrefs.ShortcutKeyDisplayString);

				var menuFile = (FullyEnabledMenuItem)typeof(MainWindow).GetField("m_menuFile", fieldFlags).GetValue(main);
				bool foundSaveAs = false;
				foreach (ToolStripItem item in menuFile.DropDownItems)
				{
					if (item.Text.Replace("&", "").Contains("Save Filters As") && item is ToolStripMenuItem mi)
					{
						Assert.AreEqual(Keys.S | Keys.Control | Keys.Shift, mi.ShortcutKeys);
						foundSaveAs = true;
						break;
					}
				}
				Assert.IsTrue(foundSaveAs);

				main.Dispose();
			}
			catch (Exception ex)
			{
				thrown = ex;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.IsNull(thrown, $"Shortcuts verification failed: {thrown}");
	}
}
