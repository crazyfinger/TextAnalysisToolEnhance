using System.Drawing;
using System.Xml.Serialization;

namespace TextAnalysisTool.NET;

public class XmlColor
{
	private Color m_value = Color.Empty;

	[XmlText]
	public string Default
	{
		get
		{
			return m_value.ToString();
		}
		set
		{
			StringColorConverter stringColorConverter = new StringColorConverter();
			m_value = (Color)stringColorConverter.ConvertFromString(value);
		}
	}

	public XmlColor()
	{
	}

	public XmlColor(Color source)
	{
		m_value = source;
	}

	public static implicit operator Color(XmlColor xc)
	{
		return xc?.m_value ?? default(Color);
	}

	public static implicit operator XmlColor(Color c)
	{
		if (!(c == default(Color)))
		{
			return new XmlColor(c);
		}
		return null;
	}
}
