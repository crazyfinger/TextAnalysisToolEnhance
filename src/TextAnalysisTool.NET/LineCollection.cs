using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TextAnalysisTool.NET;

internal sealed class LineCollection
{
	internal struct Line
	{
		private static char[] stringBuffer = new char[0];

		private byte m_marks;

		public string Text { get; private set; }

		public int Length => Text.Length + NumberOfTabs * (Settings.TabSize - 1);

		private int NumberOfTabs { get; set; }

		public bool MarkedByAny => m_marks > 0;

		public int Initialize(string text)
		{
			Text = text;
			return RefreshLength();
		}

		public int RefreshLength()
		{
			NumberOfTabs = ((Settings.TabSize != 1) ? Text.Count('\t') : 0);
			return Length;
		}

		public bool MarkedBy(int i)
		{
			return ((1 << i) & m_marks) != 0;
		}

		public void ToggleMark(int i)
		{
			m_marks ^= (byte)(1 << i);
		}

		private int WritePaddedLineNumberToCharArray(int lineNumber, int numberDigits, char[] output)
		{
			int num = (int)Math.Floor(Math.Log10(lineNumber) + 1.0);
			int num2 = numberDigits - num;
			int num3 = 0;
			while (num3 < num2)
			{
				output[num3++] = ' ';
			}
			while (num3 < numberDigits)
			{
				int num4 = num2 + numberDigits - num3++ - 1;
				output[num4] = (char)(48 + lineNumber % 10);
				lineNumber /= 10;
			}
			return num3;
		}

		private int WriteMarkersToCharArray(char[] output, int startIndex)
		{
			byte b = 1;
			for (int i = 0; i < 8; i++)
			{
				output[startIndex] = (((b & m_marks) != 0) ? ((char)(49 + i)) : '.');
				startIndex++;
				b <<= 1;
			}
			return startIndex;
		}

		public string GetMetadata(int lineNumber, int numberDigits, bool includeMarkers)
		{
			char[] array = new char[numberDigits + (includeMarkers ? 9 : 0)];
			int startIndex = WritePaddedLineNumberToCharArray(lineNumber, numberDigits, array);
			if (includeMarkers)
			{
				array[startIndex++] = ' ';
				startIndex = WriteMarkersToCharArray(array, startIndex);
			}
			return new string(array);
		}

		public string ToString(int lineNumber, int numberDigits, bool includeMarkers)
		{
			bool flag = numberDigits > 0;
			int num = numberDigits + (flag ? 1 : 0) + (includeMarkers ? 9 : 0) + Length;
			if (stringBuffer.Length < num)
			{
				stringBuffer = new char[num];
			}
			int num2 = 0;
			if (flag)
			{
				num2 = WritePaddedLineNumberToCharArray(lineNumber, numberDigits, stringBuffer);
				stringBuffer[num2++] = ' ';
			}
			if (includeMarkers)
			{
				num2 = WriteMarkersToCharArray(stringBuffer, num2);
				stringBuffer[num2++] = ' ';
			}
			for (int i = 0; i < Text.Length; i++)
			{
				char c = Text[i];
				switch (c)
				{
				case '\t':
				{
					c = ' ';
					for (int j = 0; j < Settings.TabSize - 1; j++)
					{
						stringBuffer[num2] = c;
						num2++;
					}
					break;
				}
				case '\u0084':
					c = '\ufffd';
					break;
				}
				stringBuffer[num2] = c;
				num2++;
			}
			return new string(stringBuffer, 0, num2);
		}

		public static int DisplayWidthFromLineLength(int maxLineNumber, int maxLineLength)
		{
			int num = 0;
			do
			{
				num++;
				maxLineNumber /= 10;
			}
			while (0 < maxLineNumber);
			return num + 1 + 8 + 1 + maxLineLength;
		}
	}

	public const int maxMark = 8;

	private const int LINE_BITS_PER_CHUNK = 14;

	private readonly List<Line[]> m_lineChunks;

	private readonly int m_linesInChunks;

	private int m_linesInCollection;

	private readonly BitArray m_members;

	private int m_maxDisplayWidth;

	private int[] m_lineNumbersInCollection;

	private static long m_markedLines;

	public int Count => m_linesInCollection;

	public int MaxDisplayWidth => m_maxDisplayWidth;

	public static bool MarkersInUse => m_markedLines > 0;

	public int[] LineNumbersInCollection
	{
		get
		{
			if (m_lineNumbersInCollection == null)
			{
				m_lineNumbersInCollection = new int[m_linesInCollection];
				int num = 0;
				int num2 = 1;
				while (num < m_linesInCollection)
				{
					if (m_members[num2])
					{
						m_lineNumbersInCollection[num] = num2;
						num++;
					}
					num2++;
				}
			}
			return m_lineNumbersInCollection;
		}
	}

	public LineCollection()
	{
		m_lineChunks = new List<Line[]>();
		m_members = new BitArray(0, defaultValue: false);
	}

	public LineCollection(TextReader reader)
	{
		m_lineChunks = new List<Line[]>();
		Line[] array = new Line[16384];
		m_lineChunks.Add(array);
		int num = 1;
		int num2 = 0;
		string text;
		while ((text = reader.ReadLine()) != null)
		{
			if (array.Length <= num)
			{
				array = new Line[16384];
				m_lineChunks.Add(array);
				num = 0;
			}
			num2 = Math.Max(num2, array[num].Initialize(text));
			num++;
		}
		int num3 = (m_lineChunks.Count - 1) * 16384 + num;
		m_members = new BitArray(num3, defaultValue: true);
		m_linesInChunks = num3 - 1;
		m_linesInCollection = m_linesInChunks;
		m_maxDisplayWidth = Line.DisplayWidthFromLineLength(m_linesInChunks, num2);
	}

	public LineCollection(LineCollection originalLines, Filter filter)
	{
		m_lineChunks = originalLines.m_lineChunks;
		m_linesInChunks = originalLines.m_linesInChunks;
		m_members = new BitArray(originalLines.m_members.Length, defaultValue: false);
		for (int i = 1; i <= m_linesInChunks; i++)
		{
			if (filter.Accepts(originalLines, i))
			{
				m_members[i] = true;
				m_linesInCollection++;
			}
		}
		m_maxDisplayWidth = -1;
	}

	public LineCollection(LineCollection originalLines, IEnumerable filters)
	{
		m_lineChunks = originalLines.m_lineChunks;
		m_linesInChunks = originalLines.m_linesInChunks;
		m_members = new BitArray(originalLines.m_members.Length, defaultValue: false);
		BitArray bitArray = new BitArray(m_members.Length, defaultValue: false);
		bool flag = false;
		bool flag2 = false;
		foreach (Filter filter in filters)
		{
			if (filter.Enabled)
			{
				if (filter.Excluding)
				{
					bitArray.Or(filter.FilteredLines.m_members);
					flag2 = true;
				}
				else
				{
					m_members.Or(filter.FilteredLines.m_members);
					flag = true;
				}
			}
		}
		if (flag || flag2)
		{
			if (!flag)
			{
				m_members.SetAll(value: true);
			}
			if (flag2)
			{
				m_members.And(bitArray.Not());
			}
			int num = 0;
			for (int i = 1; i <= m_linesInChunks; i++)
			{
				if (m_members[i])
				{
					m_linesInCollection++;
					num = Math.Max(num, m_lineChunks[i >> 14][i & 0x3FFF].Length);
				}
			}
			m_maxDisplayWidth = Line.DisplayWidthFromLineLength(m_linesInChunks, num);
		}
		else
		{
			m_members = originalLines.m_members;
			m_linesInCollection = originalLines.m_linesInCollection;
			m_maxDisplayWidth = originalLines.m_maxDisplayWidth;
		}
	}

	public LineCollection(LineCollection originalLines)
	{
		m_lineChunks = originalLines.m_lineChunks;
		m_linesInChunks = originalLines.m_linesInChunks;
		m_linesInCollection = originalLines.m_linesInCollection;
		m_members = new BitArray(originalLines.m_members);
		m_maxDisplayWidth = originalLines.m_maxDisplayWidth;
		m_lineNumbersInCollection = null;
	}

	public bool IsLineNumberValid(int number)
	{
		if (1 <= number)
		{
			return number <= m_linesInChunks;
		}
		return false;
	}

	public bool IsLineNumberInCollection(int number)
	{
		try
		{
			return IsLineNumberValid(number) && number < m_members.Count && m_members[number];
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
	}

	public string GetLineText(int lineNumber)
	{
		return m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].Text;
	}

	public string GetLineDisplayString(int lineNumber, int numberDigits, bool includeMarkers)
	{
		return m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].ToString(lineNumber, numberDigits, includeMarkers);
	}

	public string GetLineMetaDataString(int lineNumber, int numberDigits, bool includeMarkers)
	{
		return m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].GetMetadata(lineNumber, numberDigits, includeMarkers);
	}

	public bool IsLineMarkedBy(int lineNumber, int marker)
	{
		return m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].MarkedBy(marker);
	}

	public void LineToggleMark(int lineNumber, int marker)
	{
		bool markedByAny = m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].MarkedByAny;
		m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].ToggleMark(marker);
		bool markedByAny2 = m_lineChunks[lineNumber >> 14][lineNumber & 0x3FFF].MarkedByAny;
		if (!markedByAny && markedByAny2)
		{
			m_markedLines++;
		}
		else if (markedByAny && !markedByAny2)
		{
			m_markedLines--;
		}
	}

	public static void ResetMarkersInUse()
	{
		m_markedLines = 0L;
	}

	public void GetNextLineNumberInCollection(int number, out int previousLineNumber, out int nextLineNumber)
	{
		int num = 1;
		int num2 = Array.BinarySearch(LineNumbersInCollection, number);
		if (num2 < 0)
		{
			num2 = ~num2;
			num = 0;
		}
		int num3 = (num2 - 1 + m_linesInCollection) % m_linesInCollection;
		int num4 = (num2 + num) % m_linesInCollection;
		previousLineNumber = LineNumbersInCollection[num3];
		nextLineNumber = LineNumbersInCollection[num4];
	}

	public int GetNextMatchingLineNumberInCollection(int number, Filter find, bool reverse)
	{
		int result = -1;
		GetNextLineNumberInCollection(number, out var previousLineNumber, out var nextLineNumber);
		int indexOfLineNumber = GetIndexOfLineNumber(reverse ? previousLineNumber : nextLineNumber);
		int num = indexOfLineNumber;
		int num2 = ((!reverse) ? 1 : (-1));
		find.InitializeAccepts();
		do
		{
			if (find.Accepts(this, LineNumbersInCollection[num]) ^ find.Excluding)
			{
				result = LineNumbersInCollection[num];
				break;
			}
			num = (num + num2 + m_linesInCollection) % m_linesInCollection;
		}
		while (num != indexOfLineNumber);
		find.UninitializeAccepts();
		return result;
	}

	public int GetNearestLineNumberInCollection(int number)
	{
		int result = number;
		if (!IsLineNumberInCollection(number))
		{
			GetNextLineNumberInCollection(number, out var previousLineNumber, out var nextLineNumber);
			result = ((Math.Abs(previousLineNumber - number) <= Math.Abs(nextLineNumber - number)) ? previousLineNumber : nextLineNumber);
		}
		return result;
	}

	public int GetIndexOfLineNumber(int number)
	{
		return Array.BinarySearch(LineNumbersInCollection, number);
	}

	public void RefreshLinesLength()
	{
		int num = 0;
		for (int i = 1; i <= m_linesInChunks; i++)
		{
			num = Math.Max(num, m_lineChunks[i >> 14][i & 0x3FFF].RefreshLength());
		}
		m_maxDisplayWidth = Line.DisplayWidthFromLineLength(m_linesInChunks, num);
	}

	public void IntersectWith(LineCollection other)
	{
		m_members.And(other.m_members);
		m_linesInCollection = 0;
		int num = 0;
		for (int i = 1; i <= m_linesInChunks; i++)
		{
			if (m_members[i])
			{
				m_linesInCollection++;
				num = Math.Max(num, m_lineChunks[i >> 14][i & 0x3FFF].Length);
			}
		}
		m_maxDisplayWidth = Line.DisplayWidthFromLineLength(m_linesInChunks, num);
		m_lineNumbersInCollection = null;
	}

	public void RemoveLinesNotAcceptedBy(Filter filter)
	{
		if (m_linesInCollection == 0)
		{
			return;
		}
		filter.InitializeAccepts();
		int[] lineNumbers = LineNumbersInCollection;
		for (int i = 0; i < lineNumbers.Length; i++)
		{
			if (!filter.Accepts(this, lineNumbers[i]))
			{
				m_members[lineNumbers[i]] = false;
			}
		}
		filter.UninitializeAccepts();
		m_linesInCollection = 0;
		int num = 0;
		for (int i = 1; i <= m_linesInChunks; i++)
		{
			if (m_members[i])
			{
				m_linesInCollection++;
				num = Math.Max(num, m_lineChunks[i >> 14][i & 0x3FFF].Length);
			}
		}
		m_maxDisplayWidth = Line.DisplayWidthFromLineLength(m_linesInChunks, num);
		m_lineNumbersInCollection = null;
	}
}
