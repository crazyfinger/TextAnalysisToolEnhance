using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAnalysisTool.NET;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class LineCollectionTests
{
	[TestMethod]
	public void Line_VisualColumns_CalculatesAsciiAndChineseCorrectly()
	{
		LineCollection.Line lineAscii = default;
		lineAscii.Initialize("Hello");
		// 5 ASCII characters = 5 visual columns
		Assert.AreEqual(5, lineAscii.Length);

		LineCollection.Line lineCjk = default;
		lineCjk.Initialize("你好世界");
		// 4 CJK characters * 2 = 8 visual columns
		Assert.AreEqual(8, lineCjk.Length);

		LineCollection.Line lineMixed = default;
		lineMixed.Initialize("Hello你好");
		// 5 ASCII (5) + 2 CJK (4) = 9 visual columns
		Assert.AreEqual(9, lineMixed.Length);
	}

	[TestMethod]
	public void LineCollection_MarkedLinesSet_MaintainsIndexedMarks()
	{
		string content = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5\n";
		using StringReader reader = new StringReader(content);
		LineCollection lines = new LineCollection(reader);

		Assert.AreEqual(5, lines.Count);
		Assert.AreEqual(0, lines.MarkedLinesCount);

		// Toggle marker 0 on lines 2 and 4
		lines.LineToggleMark(2, 0);
		lines.LineToggleMark(4, 0);

		Assert.AreEqual(2, lines.MarkedLinesCount);

		List<int> markedAll = lines.GetMarkedLineNumbers(-1).ToList();
		CollectionAssert.AreEqual(new int[] { 2, 4 }, markedAll);

		// Toggle off line 2
		lines.LineToggleMark(2, 0);
		Assert.AreEqual(1, lines.MarkedLinesCount);
		CollectionAssert.AreEqual(new int[] { 4 }, lines.GetMarkedLineNumbers(-1).ToList());
	}

	[TestMethod]
	public void LineCollection_SetLineAccepted_UpdatesMembershipIncrementally()
	{
		string content = "Row A\nRow B\nRow C\n";
		using StringReader reader = new StringReader(content);
		LineCollection lines = new LineCollection(reader);

		// Initially all 3 lines are accepted in sourceLines
		Assert.AreEqual(3, lines.Count);
		Assert.IsTrue(lines.IsLineNumberInCollection(2));

		// Incrementally exclude line 2
		lines.SetLineAccepted(2, false);
		Assert.AreEqual(2, lines.Count);
		Assert.IsFalse(lines.IsLineNumberInCollection(2));

		// Incrementally restore line 2
		lines.SetLineAccepted(2, true);
		Assert.AreEqual(3, lines.Count);
		Assert.IsTrue(lines.IsLineNumberInCollection(2));
	}
}
