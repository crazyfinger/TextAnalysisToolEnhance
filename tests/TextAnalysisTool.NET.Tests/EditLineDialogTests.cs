using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class EditLineDialogTests
{
	[TestMethod]
	public void EditLineDialog_InitialText_MatchesEditedText()
	{
		string original = "Hello Android LogCat Line\r\nSecond Line";
		string edited = null;
		bool isChanged = true;

		Thread thread = new Thread(() =>
		{
			using EditLineDialog dlg = new EditLineDialog(original);
			edited = dlg.EditedText;
			isChanged = dlg.IsTextChanged;
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.AreEqual(original, edited);
		Assert.IsFalse(isChanged);
	}

	[TestMethod]
	public void EditLineDialog_SearchAndHighlight_FindsMatches()
	{
		string text = "Alpha Beta Gamma Alpha Delta Alpha";
		int matchCount = -1;
		bool searchBarVisible = false;

		Thread thread = new Thread(() =>
		{
			using EditLineDialog dlg = new EditLineDialog(text);
			dlg.Show();

			// Show search bar
			dlg.ShowSearchBar();

			FieldInfo pnlField = typeof(EditLineDialog).GetField("m_searchPanel", BindingFlags.NonPublic | BindingFlags.Instance);
			Panel pnl = (Panel)pnlField.GetValue(dlg);
			searchBarVisible = pnl.Visible;

			FieldInfo txtSearchField = typeof(EditLineDialog).GetField("m_txtSearch", BindingFlags.NonPublic | BindingFlags.Instance);
			TextBox txtSearch = (TextBox)txtSearchField.GetValue(dlg);
			txtSearch.Text = "Alpha";

			// Perform search
			MethodInfo searchMethod = typeof(EditLineDialog).GetMethod("PerformSearch", BindingFlags.NonPublic | BindingFlags.Instance);
			searchMethod.Invoke(dlg, new object[] { true });

			FieldInfo matchesField = typeof(EditLineDialog).GetField("m_matchIndices", BindingFlags.NonPublic | BindingFlags.Instance);
			var matches = (System.Collections.Generic.List<int>)matchesField.GetValue(dlg);
			matchCount = matches.Count;

			// Hide search bar
			dlg.HideSearchBar();
			dlg.Close();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.IsTrue(searchBarVisible);
		Assert.AreEqual(3, matchCount);
	}

	[TestMethod]
	public void EditLineDialog_SearchCaseSensitive_MatchesStrictly()
	{
		string text = "TAG tag Tag tAG";
		int matchCount = -1;

		Thread thread = new Thread(() =>
		{
			using EditLineDialog dlg = new EditLineDialog(text);
			dlg.CreateControl();
			IntPtr handle = dlg.Handle;

			dlg.ShowSearchBar();

			FieldInfo txtSearchField = typeof(EditLineDialog).GetField("m_txtSearch", BindingFlags.NonPublic | BindingFlags.Instance);
			TextBox txtSearch = (TextBox)txtSearchField.GetValue(dlg);
			txtSearch.Text = "Tag";

			FieldInfo chkCaseField = typeof(EditLineDialog).GetField("m_chkMatchCase", BindingFlags.NonPublic | BindingFlags.Instance);
			CheckBox chkCase = (CheckBox)chkCaseField.GetValue(dlg);
			chkCase.Checked = true; // Case sensitive

			MethodInfo searchMethod = typeof(EditLineDialog).GetMethod("PerformSearch", BindingFlags.NonPublic | BindingFlags.Instance);
			searchMethod.Invoke(dlg, new object[] { true });

			FieldInfo matchesField = typeof(EditLineDialog).GetField("m_matchIndices", BindingFlags.NonPublic | BindingFlags.Instance);
			var matches = (System.Collections.Generic.List<int>)matchesField.GetValue(dlg);
			matchCount = matches.Count;
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.AreEqual(1, matchCount);
	}
}
