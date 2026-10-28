using System;
using System.Drawing;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class FilterDialogColorSafetyTests
{
	[TestMethod]
	public void FilterDialog_ConfigureFromFilter_EmptyBackgroundColor_DoesNotThrow()
	{
		Exception thrown = null;
		Thread thread = new Thread(() =>
		{
			try
			{
				Filter filter = new MatchesTextFilter(enabled: true, excluding: false, Color.Red, Color.Empty, "test", false, false, "Tag test");
				using FilterDialog dlg = new FilterDialog("test", filter, b => new int[0]);
			}
			catch (Exception ex)
			{
				thrown = ex;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.IsNull(thrown, $"FilterDialog threw exception on Color.Empty background: {thrown}");
	}

	[TestMethod]
	public void FilterDialog_ConfigureFromFilter_TransparentBackgroundColor_DoesNotThrow()
	{
		Exception thrown = null;
		Thread thread = new Thread(() =>
		{
			try
			{
				Filter filter = new MatchesTextFilter(enabled: true, excluding: false, Color.Blue, Color.Transparent, "test", false, false, "Tag test");
				using FilterDialog dlg = new FilterDialog("test", filter, b => new int[0]);
			}
			catch (Exception ex)
			{
				thrown = ex;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		Assert.IsNull(thrown, $"FilterDialog threw exception on Color.Transparent background: {thrown}");
	}
}
