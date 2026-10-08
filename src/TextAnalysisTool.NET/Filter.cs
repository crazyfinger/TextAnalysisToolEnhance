using System;
using System.Drawing;
using System.Globalization;
using System.Xml;
using ToolBox;

namespace TextAnalysisTool.NET;

internal abstract class Filter
{
	protected const string m_xmlString_filterNode = "filter";

	protected const string m_xmlString_enabled = "enabled";

	protected const string m_xmlString_excluding = "excluding";

	protected const string m_xmlString_foreColor = "foreColor";

	protected const string m_xmlString_backColor = "backColor";

	protected const string m_xmlString_backCompatibilityColor = "color";

	protected const string m_xmlString_description = "description";

	protected const string m_xmlString_type = "type";

	protected const string m_xmlString_case_sensitive = "case_sensitive";

	protected const string m_xmlString_regular_expression = "regex";

	protected const string m_xmlString_text = "text";

	protected const string m_xmlString_marker = "marker";

	protected const string m_xmlString_filterTypeMarked = "marked";

	protected const string m_xmlString_filterTypeMatchesText = "matches_text";

	protected const string m_xmlString_yes = "y";

	protected const string m_xmlString_no = "n";

	protected LineCollection m_sourceLines;

	protected LineCollection m_filteredLines;

	public bool Enabled { get; set; }

	public bool Excluding { get; protected set; }

	public Color ForegroundColor { get; protected set; }

	public Color BackgroundColor { get; protected set; }

	public LineCollection FilteredLines => m_filteredLines;

	public abstract string FilterType { get; }

	public abstract string FilterPattern { get; }

	public bool CaseSensitive { get; protected set; }

	public string Description { get; protected set; }

	public Filter(bool enabled, bool excluding, Color foreColor, Color backColor, string description)
	{
		Enabled = enabled;
		Excluding = excluding;
		ForegroundColor = foreColor;
		BackgroundColor = backColor;
		Description = description;
	}

	public virtual void InitializeAccepts()
	{
	}

	public virtual void UninitializeAccepts()
	{
	}

	public abstract bool Accepts(LineCollection lineCollection, int lineNumber);

	public bool FilterLinesNecessary(LineCollection sourceLines)
	{
		if (m_sourceLines != sourceLines)
		{
			m_sourceLines = sourceLines;
			m_filteredLines = null;
		}
		if (Enabled)
		{
			return m_filteredLines == null;
		}
		return false;
	}

	public void FilterLines()
	{
		if (m_filteredLines == null)
		{
			InitializeAccepts();
			m_filteredLines = new LineCollection(m_sourceLines, this);
			UninitializeAccepts();
		}
	}

	public virtual bool MarkersChanged(int marker)
	{
		return false;
	}

	public void RefreshLinesLength()
	{
		if (m_filteredLines != null)
		{
			m_filteredLines.RefreshLinesLength();
		}
	}

	public void ToXml(XmlTextWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("filter");
		xmlWriter.WriteAttributeString("enabled", Enabled ? "y" : "n");
		xmlWriter.WriteAttributeString("excluding", Excluding ? "y" : "n");
		xmlWriter.WriteAttributeString("description", (Description != null) ? Description : string.Empty);
		if (Color.Empty != ForegroundColor)
		{
			xmlWriter.WriteAttributeString("foreColor", ForegroundColor.ToRgbString());
		}
		if (Color.Empty != BackgroundColor)
		{
			xmlWriter.WriteAttributeString("backColor", BackgroundColor.ToRgbString());
		}
		if (this is MatchesTextFilter)
		{
			MatchesTextFilter matchesTextFilter = (MatchesTextFilter)this;
			xmlWriter.WriteAttributeString("type", "matches_text");
			xmlWriter.WriteAttributeString("case_sensitive", matchesTextFilter.CaseSensitive ? "y" : "n");
			xmlWriter.WriteAttributeString("regex", matchesTextFilter.RegularExpression ? "y" : "n");
			xmlWriter.WriteAttributeString("text", matchesTextFilter.Text);
		}
		else if (this is MarkedFilter)
		{
			MarkedFilter markedFilter = (MarkedFilter)this;
			xmlWriter.WriteAttributeString("type", "marked");
			xmlWriter.WriteAttributeString("marker", (markedFilter.Marker + 1).ToString(CultureInfo.CurrentCulture));
		}
		xmlWriter.WriteEndElement();
	}

	public static Filter FromXml(XmlNode filterNode)
	{
		XmlAttributeCollection attributes = filterNode.Attributes;
		bool enabled = attributes.GetNamedItem("enabled").Value.FromYNToBool();
		bool excluding = attributes.GetNamedItem("excluding").Value.FromYNToBool();
		Color foreColor = Color.Empty;
		Color backColor = Color.Empty;
		string description = string.Empty;
		XmlNode namedItem = attributes.GetNamedItem("foreColor");
		XmlNode namedItem2 = attributes.GetNamedItem("color");
		XmlNode xmlNode = null;
		xmlNode = ((namedItem != null) ? namedItem : namedItem2);
		if (xmlNode != null)
		{
			foreColor = Color.FromArgb(Convert.ToInt32(xmlNode.Value, 16) | -16777216);
		}
		XmlNode namedItem3 = attributes.GetNamedItem("backColor");
		if (namedItem3 != null)
		{
			backColor = Color.FromArgb(Convert.ToInt32(namedItem3.Value, 16) | -16777216);
		}
		XmlNode namedItem4 = attributes.GetNamedItem("description");
		if (namedItem4 != null)
		{
			description = namedItem4.Value;
		}
		string value = attributes.GetNamedItem("type").Value;
		if ("matches_text" == value)
		{
			XmlNode namedItem5 = attributes.GetNamedItem("case_sensitive");
			XmlNode namedItem6 = attributes.GetNamedItem("regex");
			XmlNode namedItem7 = attributes.GetNamedItem("text");
			Utility.ThrowExceptionIf(namedItem5 == null || namedItem6 == null || namedItem7 == null, null, typeof(ApplicationException));
			bool caseSensitive = namedItem5.Value.FromYNToBool();
			bool usesRegularExpression = namedItem6.Value.FromYNToBool();
			string value2 = namedItem7.Value;
			Utility.ThrowExceptionIf(value2.Length == 0, null, typeof(ApplicationException));
			return new MatchesTextFilter(enabled, excluding, foreColor, backColor, value2, caseSensitive, usesRegularExpression, description);
		}
		if ("marked" == value)
		{
			XmlNode namedItem8 = attributes.GetNamedItem("marker");
			Utility.ThrowExceptionIf(namedItem8 == null, null, typeof(ApplicationException));
			int num = int.Parse(namedItem8.Value, CultureInfo.CurrentCulture) - 1;
			Utility.ThrowExceptionIf(num < 0 || 8 <= num, null, typeof(ApplicationException));
			return new MarkedFilter(enabled, excluding, foreColor, backColor, num, description);
		}
		return null;
	}
}
