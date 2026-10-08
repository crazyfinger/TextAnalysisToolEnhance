using System.Drawing;
using System.Globalization;

namespace TextAnalysisTool.NET;

public static class ColorExtensions
{
	public static string ToRgbString(this Color color)
	{
		return (0xFFFFFF & color.ToArgb()).ToString("x6", CultureInfo.InvariantCulture);
	}

	public static string ToNameOrRgbString(this Color color)
	{
		if (!color.IsKnownColor)
		{
			return color.ToRgbString();
		}
		return color.ToKnownColor().ToString();
	}

	public static string ToCssString(this Color color)
	{
		return (color.ToArgb() & 0xFFFFFF).ToString("x6");
	}
}
