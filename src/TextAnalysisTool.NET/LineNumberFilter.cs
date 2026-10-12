using System;
using System.Drawing;

namespace TextAnalysisTool.NET;

internal sealed class LineNumberFilter : Filter
{
	public int StartLine { get; private set; }

	public int EndLine { get; private set; }

	public int LineNumber => StartLine > 0 ? StartLine : EndLine;

	public bool HideBefore => StartLine > 0 && EndLine <= 0;

	public override string FilterType
	{
		get
		{
			if (StartLine > 0 && EndLine > 0)
			{
				return "[n..m]";
			}
			return HideBefore ? "[<n]" : "[>n]";
		}
	}

	public override string FilterPattern
	{
		get
		{
			if (StartLine > 0 && EndLine > 0)
			{
				return $"lines {StartLine}..{EndLine}";
			}
			return (HideBefore ? "line < " : "line > ") + LineNumber;
		}
	}

	public static string FormatDefaultDescription(int startLine, int endLine, bool excluding)
	{
		if (startLine > 0 && endLine > 0)
		{
			return excluding ? $"Hide lines {startLine} to {endLine}" : $"Lines {startLine} to {endLine}";
		}
		if (startLine > 0)
		{
			return excluding ? $"Hide lines before line {startLine}" : $"Lines before line {startLine}";
		}
		if (endLine > 0)
		{
			return excluding ? $"Hide lines after line {endLine}" : $"Lines after line {endLine}";
		}
		return string.Empty;
	}

	public LineNumberFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int lineNumber, bool hideBefore)
		: this(enabled, excluding, foreColor, backColor, lineNumber, hideBefore, string.Empty)
	{
	}

	public LineNumberFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int lineNumber, bool hideBefore, string description)
		: this(enabled, excluding, foreColor, backColor, hideBefore ? lineNumber : 0, hideBefore ? 0 : lineNumber, description)
	{
	}

	public LineNumberFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int startLine, int endLine, string description = "")
		: base(enabled, excluding, foreColor, backColor, description)
	{
		StartLine = Math.Max(0, startLine);
		EndLine = Math.Max(0, endLine);
		if (string.IsNullOrEmpty(Description))
		{
			Description = FormatDefaultDescription(StartLine, EndLine, Excluding);
		}
	}

	public override bool Accepts(LineCollection lineCollection, int lineNumber)
	{
		if (StartLine > 0 && EndLine > 0)
		{
			return lineNumber >= StartLine && lineNumber <= EndLine;
		}
		if (StartLine > 0)
		{
			return lineNumber < StartLine;
		}
		if (EndLine > 0)
		{
			return lineNumber > EndLine;
		}
		return false;
	}
}
