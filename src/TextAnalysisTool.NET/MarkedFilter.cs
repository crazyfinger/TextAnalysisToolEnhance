using System.Drawing;

namespace TextAnalysisTool.NET;

internal sealed class MarkedFilter : Filter
{
	public int Marker { get; private set; }

	public override string FilterType => "[#]";

	public override string FilterPattern => (Marker + 1).ToString();

	public MarkedFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int marker)
		: this(enabled, excluding, foreColor, backColor, marker, string.Empty)
	{
	}

	public MarkedFilter(bool enabled, bool excluding, Color foreColor, Color backColor, int marker, string description)
		: base(enabled, excluding, foreColor, backColor, description)
	{
		Marker = marker;
	}

	public override bool Accepts(LineCollection lineCollection, int lineNumber)
	{
		return lineCollection.IsLineMarkedBy(lineNumber, Marker);
	}

	public override bool MarkersChanged(int marker)
	{
		bool result = false;
		if (marker == Marker)
		{
			m_filteredLines = null;
			result = base.Enabled;
		}
		return result;
	}
}
