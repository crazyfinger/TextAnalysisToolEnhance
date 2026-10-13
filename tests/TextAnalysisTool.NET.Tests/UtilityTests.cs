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
}
