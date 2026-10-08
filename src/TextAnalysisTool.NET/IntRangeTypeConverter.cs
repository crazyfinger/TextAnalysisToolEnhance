using System;
using System.ComponentModel;
using System.Globalization;

namespace TextAnalysisTool.NET;

public abstract class IntRangeTypeConverter : TypeConverter
{
	private int m_minimumValue;

	private int m_maximumValue;

	public IntRangeTypeConverter(int minimumValue, int maximumValue)
	{
		m_minimumValue = minimumValue;
		m_maximumValue = maximumValue;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(string)))
		{
			return base.CanConvertFrom(context, sourceType);
		}
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string message = $"The value should be an integer between {m_minimumValue} and {m_maximumValue}.";
		if (!(value is string))
		{
			return base.ConvertFrom(context, culture, value);
		}
		int num = 0;
		try
		{
			num = Convert.ToInt32(value);
		}
		catch (FormatException innerException)
		{
			throw new FormatException(message, innerException);
		}
		if (num < m_minimumValue || num > m_maximumValue)
		{
			throw new FormatException(message);
		}
		return num;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			return Convert.ToInt32(value).ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
