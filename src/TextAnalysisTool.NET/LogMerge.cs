using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextAnalysisTool.NET;

internal sealed class MergeSource
{
	public string Name { get; set; }

	public Func<TextReader> Open { get; set; }

	public long? SizeBytes { get; set; }

	public MergeSource(string name, Func<TextReader> open)
	{
		Name = name;
		Open = open;
	}
}

internal sealed class MergeOptions
{
	public string OutputPath { get; set; }

	public DateTime? From { get; set; }

	public DateTime? To { get; set; }

	public bool DropNonTimestampedLines { get; set; } = true;
}

internal static class LogTimestampParser
{
	public static bool TryParsePrefix(string line, out DateTime timestamp, out int prefixLength)
	{
		timestamp = default;
		prefixLength = 0;
		if (line == null || line.Length < 18)
		{
			return false;
		}
		string s = line;
		if (!IsDigit(s[0]) || !IsDigit(s[1]) || s[2] != '-' ||
			!IsDigit(s[3]) || !IsDigit(s[4]) || s[5] != ' ' ||
			!IsDigit(s[6]) || !IsDigit(s[7]) || s[8] != ':' ||
			!IsDigit(s[9]) || !IsDigit(s[10]) || s[11] != ':' ||
			!IsDigit(s[12]) || !IsDigit(s[13]) || s[14] != '.' ||
			!IsDigit(s[15]) || !IsDigit(s[16]) || !IsDigit(s[17]))
		{
			return false;
		}

		int month = (s[0] - '0') * 10 + (s[1] - '0');
		int day = (s[3] - '0') * 10 + (s[4] - '0');
		int hour = (s[6] - '0') * 10 + (s[7] - '0');
		int minute = (s[9] - '0') * 10 + (s[10] - '0');
		int second = (s[12] - '0') * 10 + (s[13] - '0');
		int ms = (s[15] - '0') * 100 + (s[16] - '0') * 10 + (s[17] - '0');

		if (month < 1 || month > 12 || hour > 23 || minute > 59 || second > 59 || day < 1 || day > DateTime.DaysInMonth(2000, month))
		{
			return false;
		}

		timestamp = new DateTime(2000, month, day, hour, minute, second, ms);
		prefixLength = 18;
		return true;
	}

	private static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}
}

internal sealed class BinaryHeap<T>
{
	private readonly List<T> m_items = new List<T>();

	private readonly Comparison<T> m_compare;

	public int Count => m_items.Count;

	public BinaryHeap(Comparison<T> compare)
	{
		m_compare = compare;
	}

	public void Push(T item)
	{
		m_items.Add(item);
		int i = m_items.Count - 1;
		while (i > 0)
		{
			int parent = (i - 1) / 2;
			if (m_compare(m_items[i], m_items[parent]) >= 0)
			{
				break;
			}
			Swap(i, parent);
			i = parent;
		}
	}

	public T Pop()
	{
		T top = m_items[0];
		int last = m_items.Count - 1;
		m_items[0] = m_items[last];
		m_items.RemoveAt(last);
		int i = 0;
		while (true)
		{
			int left = 2 * i + 1;
			int right = 2 * i + 2;
			int smallest = i;
			if (left < m_items.Count && m_compare(m_items[left], m_items[smallest]) < 0)
			{
				smallest = left;
			}
			if (right < m_items.Count && m_compare(m_items[right], m_items[smallest]) < 0)
			{
				smallest = right;
			}
			if (smallest == i)
			{
				break;
			}
			Swap(i, smallest);
			i = smallest;
		}
		return top;
	}

	private void Swap(int a, int b)
	{
		T t = m_items[a];
		m_items[a] = m_items[b];
		m_items[b] = t;
	}
}

internal static class LogMergeEngine
{
	public const string BuildStamp = "merge-ext-chunk-v1";

	public static void WriteRunMarker(string dir)
	{
		try
		{
			File.AppendAllText(Path.Combine(dir, "TAT-run.log"),
				$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {BuildStamp}\n");
		}
		catch (IOException)
		{
		}
	}
	private struct RunItem
	{
		public DateTime Timestamp;

		public int RunIndex;

		public string Line;
	}

	private struct MergedItem
	{
		public DateTime Timestamp;

		public bool HasTimestamp;

		public int SourceIndex;

		public string Line;
	}

	private const int RunLineCap = 100000;

	private const int StreamBufferSize = 1 << 20;

	private static readonly object s_tempFileLock = new object();

	private static string NewTempFile()
	{
		lock (s_tempFileLock)
		{
			return Path.GetTempFileName();
		}
	}

	public static void Merge(IReadOnlyList<MergeSource> sources, MergeOptions options, Action<int> reportProgress, Func<bool> isCancelled)
	{
		if (sources == null || sources.Count == 0)
		{
			throw new ArgumentException("No sources to merge.");
		}
		if (string.IsNullOrEmpty(options.OutputPath))
		{
			throw new ArgumentException("No output path.");
		}

		List<string> tempFiles = new List<string>();
		string outDir = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath));
		if (!string.IsNullOrEmpty(outDir))
		{
			Directory.CreateDirectory(outDir);
		}

		try
		{
			int lastReported = -1;
			Action<int> throttled = pct =>
			{
				if (pct != lastReported)
				{
					lastReported = pct;
					reportProgress?.Invoke(pct);
				}
			};

			// Phase 1: sort each source to a temporary file using a bounded-memory external sort.
			List<(string SortedPath, int LineCount)> sorted = new List<(string, int)>();
			double sortShare = 100.0 / sources.Count;
			long totalLines = 0;

			for (int s = 0; s < sources.Count; s++)
			{
				ThrowIfCancelled(isCancelled);
				throttled((int)(s * sortShare));
				(string sortedPath, int lineCount) = SortSourceToTemp(sources[s], options, tempFiles, s * sortShare, sortShare, throttled, isCancelled);
				totalLines += lineCount;
				sorted.Add((sortedPath, lineCount));
			}

			throttled((int)(sources.Count * sortShare));
			GlobalMerge(sorted, options, totalLines, throttled, isCancelled);

			throttled(100);
		}
		finally
		{
			foreach (string file in tempFiles)
			{
				try
				{
					if (File.Exists(file))
					{
						File.Delete(file);
					}
				}
				catch (IOException)
				{
				}
			}
		}
	}

	private static (string SortedPath, int LineCount) SortSourceToTemp(MergeSource source, MergeOptions options, List<string> tempFiles, double progressBase, double progressSpan, Action<int> reportProgress, Func<bool> isCancelled)
	{
		List<(DateTime Timestamp, int Index, string Line)> run = new List<(DateTime, int, string)>(RunLineCap);
		List<string> runFiles = new List<string>();
		Stream seekableStream = null;
		long length = 0;
		int index = 0;
		int lineCount = 0;

		try
		{
			using (TextReader reader = source.Open())
			{
				if (reader is StreamReader streamReader && streamReader.BaseStream is Stream baseStream && baseStream.CanSeek && baseStream.Length > 0)
				{
					seekableStream = baseStream;
					length = baseStream.Length;
				}

				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix))
					{
						run.Add((ts, index, line));
						lineCount++;
					}
					else if (!options.DropNonTimestampedLines)
					{
						run.Add((DateTime.MaxValue, index, line));
						lineCount++;
					}
					index++;

					if (run.Count >= RunLineCap)
					{
						FlushRun(run, runFiles, tempFiles);
						if (seekableStream != null)
						{
							reportProgress?.Invoke(Math.Min(99, (int)(progressBase + progressSpan * seekableStream.Position / length)));
						}
						ThrowIfCancelled(isCancelled);
					}
				}
			}
		}
		finally
		{
			reportProgress?.Invoke(Math.Min(99, (int)(progressBase + progressSpan)));
		}

		if (run.Count > 0)
		{
			FlushRun(run, runFiles, tempFiles);
		}
		if (runFiles.Count == 0)
		{
			string empty = NewTempFile();
			tempFiles.Add(empty);
			return (empty, 0);
		}

		string sortedPath = NewTempFile();
		tempFiles.Add(sortedPath);
		MergeRuns(runFiles, sortedPath, isCancelled);
		foreach (string runFile in runFiles)
		{
			try
			{
				if (File.Exists(runFile))
				{
					File.Delete(runFile);
				}
			}
			catch (IOException)
			{
			}
		}
		return (sortedPath, lineCount);
	}

	private static void FlushRun(List<(DateTime Timestamp, int Index, string Line)> run, List<string> runFiles, List<string> tempFiles)
	{
		run.Sort((a, b) =>
		{
			int c = a.Timestamp.CompareTo(b.Timestamp);
			return c != 0 ? c : a.Index.CompareTo(b.Index);
		});
		string runFile = NewTempFile();
		tempFiles.Add(runFile);
		using (StreamWriter writer = new StreamWriter(runFile, false, new UTF8Encoding(false), StreamBufferSize))
		{
			foreach ((DateTime ts, int idx, string line) in run)
			{
				writer.WriteLine(line);
			}
		}
		runFiles.Add(runFile);
		run.Clear();
	}

	private static void MergeRuns(List<string> runFiles, string sortedPath, Func<bool> isCancelled)
	{
		BinaryHeap<RunItem> heap = new BinaryHeap<RunItem>((x, y) =>
		{
			int c = x.Timestamp.CompareTo(y.Timestamp);
			return c != 0 ? c : x.RunIndex.CompareTo(y.RunIndex);
		});
		StreamReader[] readers = new StreamReader[runFiles.Count];
		try
		{
			for (int i = 0; i < runFiles.Count; i++)
			{
				StreamReader reader = new StreamReader(runFiles[i], Encoding.UTF8, true, StreamBufferSize);
				readers[i] = reader;
				string line = reader.ReadLine();
				if (line != null)
				{
					DateTime key = LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix) ? ts : DateTime.MaxValue;
					heap.Push(new RunItem { Timestamp = key, RunIndex = i, Line = line });
				}
			}

			using (StreamWriter writer = new StreamWriter(sortedPath, false, new UTF8Encoding(false), StreamBufferSize))
			{
				while (heap.Count > 0)
				{
					RunItem item = heap.Pop();
					writer.WriteLine(item.Line);
					string next = readers[item.RunIndex].ReadLine();
					if (next != null)
					{
						DateTime key = LogTimestampParser.TryParsePrefix(next, out DateTime ts2, out int prefix2) ? ts2 : DateTime.MaxValue;
						heap.Push(new RunItem { Timestamp = key, RunIndex = item.RunIndex, Line = next });
					}
				}
			}
		}
		finally
		{
			foreach (StreamReader r in readers)
			{
				if (r != null)
				{
					r.Dispose();
				}
			}
		}
	}

	private static void GlobalMerge(List<(string SortedPath, int LineCount)> sorted, MergeOptions options, long totalLines, Action<int> reportProgress, Func<bool> isCancelled)
	{
		BinaryHeap<MergedItem> heap = new BinaryHeap<MergedItem>((x, y) =>
		{
			int c = x.Timestamp.CompareTo(y.Timestamp);
			return c != 0 ? c : x.SourceIndex.CompareTo(y.SourceIndex);
		});
		StreamReader[] readers = new StreamReader[sorted.Count];
		try
		{
			for (int i = 0; i < sorted.Count; i++)
			{
				StreamReader reader = new StreamReader(sorted[i].SortedPath, Encoding.UTF8, true, StreamBufferSize);
				readers[i] = reader;
				string line = reader.ReadLine();
				if (line != null)
				{
					bool hasTs = LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix);
					heap.Push(new MergedItem { Timestamp = hasTs ? ts : DateTime.MaxValue, HasTimestamp = hasTs, SourceIndex = i, Line = line });
				}
			}

			using (StreamWriter writer = new StreamWriter(options.OutputPath, false, new UTF8Encoding(true), StreamBufferSize))
			{
				long written = 0;
				while (heap.Count > 0)
				{
					MergedItem item = heap.Pop();

					bool keep = !item.HasTimestamp || PassesTimeFilter(item.Timestamp, options);
					if (keep)
					{
						writer.WriteLine(item.Line);
						written++;
					}

					if (written % 4096 == 0)
					{
						ThrowIfCancelled(isCancelled);
					}

					string next = readers[item.SourceIndex].ReadLine();
					if (next != null)
					{
						bool hasTs2 = LogTimestampParser.TryParsePrefix(next, out DateTime ts2, out int prefix2);
						heap.Push(new MergedItem { Timestamp = hasTs2 ? ts2 : DateTime.MaxValue, HasTimestamp = hasTs2, SourceIndex = item.SourceIndex, Line = next });
					}

					if (totalLines > 0)
					{
						int pct = (int)(50 + written * 50 / totalLines);
						reportProgress?.Invoke(Math.Min(99, pct));
					}
				}
			}
		}
		finally
		{
			foreach (StreamReader r in readers)
			{
				if (r != null)
				{
					r.Dispose();
				}
			}
		}
	}

	private static bool PassesTimeFilter(DateTime ts, MergeOptions options)
	{
		if (options.From.HasValue && ts <= options.From.Value)
		{
			return false;
		}
		if (options.To.HasValue && ts >= options.To.Value)
		{
			return false;
		}
		return true;
	}

	private static void ThrowIfCancelled(Func<bool> isCancelled)
	{
		if (isCancelled != null && isCancelled())
		{
			throw new OperationCanceledException();
		}
	}
}