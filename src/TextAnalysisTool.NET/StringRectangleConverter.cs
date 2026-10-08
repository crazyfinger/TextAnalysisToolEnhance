using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace TextAnalysisTool.NET;

public class StringRectangleConverter : RectangleConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
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
			text = text.Replace("{", "");
			text = text.Replace("}", "");
			text = text.Replace("X=", "");
			text = text.Replace("Y=", "");
			text = text.Replace("Width=", "");
			text = text.Replace("Height=", "");
			return base.ConvertFrom(context, culture, (object)text);
		}
		return base.ConvertFrom(context, culture, value);
	}
}
