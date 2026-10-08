using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace TextAnalysisTool.NET;

public class StringColorConverter : ColorConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string) || sourceType == typeof(int))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = (string)value;
			text = text.Replace("Color [", "");
			text = text.Replace("]", "");
			if (text.IndexOf("A=") == -1)
			{
				return Color.FromName(text);
			}
			text = text.Replace("A=", "");
			text = text.Replace("R=", "");
			text = text.Replace("G=", "");
			text = text.Replace("B=", "");
			return base.ConvertFrom(context, culture, (object)text);
		}
		if (value is int)
		{
			return Color.FromArgb((int)value);
		}
		return base.ConvertFrom(context, culture, value);
	}
}
