using System.Drawing;

namespace TextAnalysisTool.NET;

internal sealed class LineNumberFilter : Filter
{
	public int LineNumber { get; private set; }

	public bool HideBefore { get; private set; }

	public override string FilterType => HideBefore ? "[<n]" : "[>n]";

	public override string FilterPattern => (HideBefore ? "line < " : "line > ") + LineNumber;

	public LineNumberFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int lineNumber, bool hideBefore)
		: this(enabled, excluding, foreColor, backColor, lineNumber, hideBefore, string.Empty)
	{
	}

	public LineNumberFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int lineNumber, bool hideBefore, string description)
		: base(enabled, excluding, foreColor, backColor, description)
	{
		LineNumber = lineNumber;
		HideBefore = hideBefore;
	}

	public override bool Accepts(LineCollection lineCollection, int lineNumber)
	{
		return HideBefore ? lineNumber < LineNumber : lineNumber > LineNumber;
	}
}