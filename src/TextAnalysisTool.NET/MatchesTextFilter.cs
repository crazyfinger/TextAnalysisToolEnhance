using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TextAnalysisTool.NET;

internal sealed class MatchesTextFilter : Filter
{
	private readonly Regex m_regEx;

	private CompareInfo m_cultureInfo;

	public static RegexOptions DefaultRegexOptions => RegexOptions.Singleline;

	public string Text { get; private set; }

	public bool RegularExpression => m_regEx != null;

	public override string FilterType
	{
		get
		{
			if (m_regEx != null)
			{
				return "[R]";
			}
			return "";
		}
	}

	public override string FilterPattern => Text;

	public MatchesTextFilter(bool enabled, bool excluding, Color foreColor, Color backColor, string str, bool caseSensitive, bool usesRegularExpression)
		: this(enabled, excluding, foreColor, backColor, str, caseSensitive, usesRegularExpression, string.Empty)
	{
	}

	public MatchesTextFilter(bool enabled, bool excluding, Color foreColor, Color backColor, string str, bool caseSensitive, bool usesRegularExpression, string description)
		: base(enabled, excluding, foreColor, backColor, description)
	{
		Text = str;
		base.CaseSensitive = caseSensitive;
		m_regEx = (usesRegularExpression ? new Regex(str, (RegexOptions)((int)DefaultRegexOptions | ((!caseSensitive) ? 1 : 0))) : null);
	}

	public override void InitializeAccepts()
	{
		m_cultureInfo = CultureInfo.CurrentCulture.CompareInfo;
	}

	public override bool Accepts(LineCollection lineCollection, int lineNumber)
	{
		string lineText = lineCollection.GetLineText(lineNumber);
		if (m_regEx == null)
		{
			if (base.CaseSensitive)
			{
				return -1 != lineText.IndexOf(Text);
			}
			return -1 != m_cultureInfo.IndexOf(lineText, Text, CompareOptions.IgnoreCase);
		}
		return m_regEx.IsMatch(lineText);
	}
}
