using System;
using System.Drawing;
using System.IO;

namespace TextAnalysisTool.NET;

public static class StringExtensions
{
	public static int Count(this string value, char c)
	{
		int num = 0;
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] == c)
			{
				num++;
			}
		}
		return num;
	}

	public static string ShortenPath(this string path, int maxLength)
	{
		string text = "...";
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		string text2 = directorySeparatorChar.ToString();
		if (path.Length <= maxLength)
		{
			return path;
		}
		int length = text.Length;
		if (maxLength <= length)
		{
			return text;
		}
		bool flag = true;
		string text3 = string.Empty;
		string text4 = string.Empty;
		int num = 0;
		int num2 = 0;
		string[] array = path.Split(directorySeparatorChar);
		for (int i = 0; i < array.Length; i++)
		{
			if (flag)
			{
				string text5 = array[num] + text2;
				if (text3.Length + text4.Length + text5.Length + length > maxLength)
				{
					break;
				}
				text3 += text5;
				if (!(text5 == text2))
				{
					flag = false;
				}
				num++;
				continue;
			}
			int num3 = array.Length - num2 - 1;
			string text6 = text2 + array[num3];
			if (text3.Length + text4.Length + text6.Length + length > maxLength)
			{
				break;
			}
			text4 = text6 + text4;
			if (!(text6 == text2))
			{
				flag = true;
			}
			num2++;
		}
		if (string.IsNullOrEmpty(text4))
		{
			text4 = array[array.Length - 1];
			text4 = text4.Substring(text4.Length + length + text3.Length - maxLength, maxLength - length - text3.Length);
		}
		return text3 + text + text4;
	}

	public static bool FromYNToBool(this string input)
	{
		string text = input.ToLowerInvariant();
		if ("y" == text || "yes" == text)
		{
			return true;
		}
		if ("n" == text || "no" == text)
		{
			return false;
		}
		throw new ApplicationException($"Cannot parse unrecognized input '{input}'");
	}

	public static string Escape(this string s)
	{
		return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
	}

	public static Color ToNamedOrArgbColor(this string input)
	{
		Color result = Color.FromName(input);
		if (!result.IsKnownColor)
		{
			result = Color.FromArgb(Convert.ToInt32(input, 16) | -16777216);
		}
		return result;
	}
}
