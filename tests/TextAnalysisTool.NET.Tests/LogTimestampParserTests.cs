using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAnalysisTool.NET;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class LogTimestampParserTests
{
	[TestMethod]
	public void TryParsePrefix_StandardLogcatFormat_ParsesSuccessfully()
	{
		string line = "10-13 14:30:15.123  1000  2000 I ActivityManager: test";
		bool result = LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefixLength);

		Assert.IsTrue(result);
		Assert.AreEqual(10, ts.Month);
		Assert.AreEqual(13, ts.Day);
		Assert.AreEqual(14, ts.Hour);
		Assert.AreEqual(30, ts.Minute);
		Assert.AreEqual(15, ts.Second);
		Assert.AreEqual(123, ts.Millisecond);
		Assert.AreEqual(18, prefixLength);
	}

	[TestMethod]
	public void TryParsePrefix_BoundaryTimestamps_ParsesSuccessfully()
	{
		string startOfYear = "01-01 00:00:00.000 Start of year";
		Assert.IsTrue(LogTimestampParser.TryParsePrefix(startOfYear, out DateTime tsStart, out _));
		Assert.AreEqual(1, tsStart.Month);
		Assert.AreEqual(1, tsStart.Day);
		Assert.AreEqual(0, tsStart.Hour);

		string endOfYear = "12-31 23:59:59.999 End of year";
		Assert.IsTrue(LogTimestampParser.TryParsePrefix(endOfYear, out DateTime tsEnd, out _));
		Assert.AreEqual(12, tsEnd.Month);
		Assert.AreEqual(31, tsEnd.Day);
		Assert.AreEqual(23, tsEnd.Hour);
		Assert.AreEqual(59, tsEnd.Minute);
		Assert.AreEqual(59, tsEnd.Second);
		Assert.AreEqual(999, tsEnd.Millisecond);
	}

	[TestMethod]
	public void TryParsePrefix_InvalidDateOrFormat_ReturnsFalse()
	{
		// Invalid month 13
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("13-01 10:00:00.000 log", out _, out _));
		// Invalid month 00
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("00-01 10:00:00.000 log", out _, out _));
		// Invalid hour 24
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("10-13 24:00:00.000 log", out _, out _));
		// Invalid minute 60
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("10-13 10:60:00.000 log", out _, out _));
		// Plain text without timestamp
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("Hello world", out _, out _));
		// Null or empty
		Assert.IsFalse(LogTimestampParser.TryParsePrefix(null, out _, out _));
		Assert.IsFalse(LogTimestampParser.TryParsePrefix("", out _, out _));
	}

	[TestMethod]
	public void TryParseFlexible_SupportsVariousRelaxedFormatsAndPadsZeros()
	{
		// 1. Standard format
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("01-01 00:00:00.000", out DateTime ts1));
		Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, 0), ts1);

		// 2. Without milliseconds (auto pads 0ms)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("01-01 00:00:00", out DateTime ts2));
		Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, 0), ts2);

		// 3. Short single-digit hours/mins/secs/ms (e.g. 01-01 0:0:0.0)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("01-01 0:0:0.0", out DateTime ts3));
		Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, 0), ts3);

		// 4. Single-digit month and day (1-1 0:0:0)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("1-1 0:0:0", out DateTime ts4));
		Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, 0), ts4);

		// 5. 1-digit millisecond (.5 -> 500ms)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("10-13 14:30:15.5", out DateTime ts5));
		Assert.AreEqual(new DateTime(2000, 10, 13, 14, 30, 15, 500), ts5);

		// 6. 2-digit millisecond (.12 -> 120ms)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("10-13 14:30:15.12", out DateTime ts6));
		Assert.AreEqual(new DateTime(2000, 10, 13, 14, 30, 15, 120), ts6);

		// 7. Full year prefix (2026-10-13)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("2026-10-13 14:30:15", out DateTime ts7));
		Assert.AreEqual(new DateTime(2000, 10, 13, 14, 30, 15, 0), ts7);

		// 8. Without seconds (auto pads 0s 0ms)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("10-13 14:30", out DateTime ts8));
		Assert.AreEqual(new DateTime(2000, 10, 13, 14, 30, 0, 0), ts8);

		// 9. Slash date separator (10/13 14:30:15)
		Assert.IsTrue(LogTimestampParser.TryParseFlexible("10/13 14:30:15", out DateTime ts9));
		Assert.AreEqual(new DateTime(2000, 10, 13, 14, 30, 15, 0), ts9);
	}

	[TestMethod]
	public void TryParseFlexible_InvalidInputs_ReturnsFalse()
	{
		Assert.IsFalse(LogTimestampParser.TryParseFlexible(null, out _));
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("", out _));
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("invalid", out _));
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("13-01 10:00:00", out _)); // invalid month
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("02-30 10:00:00", out _)); // invalid day for Feb
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("10-13 25:00:00", out _)); // invalid hour
		Assert.IsFalse(LogTimestampParser.TryParseFlexible("10-13 10:65:00", out _)); // invalid min
	}
}
