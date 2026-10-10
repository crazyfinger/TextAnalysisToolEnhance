using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
	private struct SortedItem
	{
		public DateTime Timestamp;

		public int Index;

		public string Line;
	}

	private struct SortedSource
	{
		public List<SortedItem> Memory;

		public string TempPath;

		public long LineCount;
	}

	private struct RunItem
	{
		public DateTime Timestamp;

		public int RunIndex;

		public string Line;
	}

	private struct HeapItem
	{
		public DateTime Timestamp;

		public bool HasTimestamp;

		public int SourceIndex;

		public string Line;
	}

	private const int RunLineCap = 100000;

	private const int StreamBufferSize = 1 << 20;

	private const int MergeParallelism = 2;

	private static readonly long MaxInMemorySortBytes = 300L * 1024 * 1024;

	private static readonly object s_tempFileLock = new object();

	private static string NewTempFile(List<string> tempFiles)
	{
		lock (s_tempFileLock)
		{
			string path = Path.GetTempFileName();
			tempFiles.Add(path);
			return path;
		}
	}

	// Tracks sort-phase progress by bytes read / total bytes across all sources.
	private sealed class SortProgress
	{
		private readonly object m_lock = new object();

		private readonly long m_totalBytes;

		private readonly Action<int> m_report;

		private long m_readBytes;

		private int m_lastReported = -1;

		public SortProgress(long totalBytes, Action<int> report)
		{
			m_totalBytes = totalBytes;
			m_report = report;
		}

		public void AddBytes(long delta)
		{
			if (delta <= 0)
			{
				return;
			}
			lock (m_lock)
			{
				m_readBytes += delta;
				int pct = m_totalBytes > 0 ? (int)(m_readBytes * 90 / (double)m_totalBytes) : 90;
				if (pct > 90)
				{
					pct = 90;
				}
				if (pct != m_lastReported)
				{
					m_lastReported = pct;
					m_report?.Invoke(pct);
				}
			}
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

		string outDir = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath));
		if (!string.IsNullOrEmpty(outDir))
		{
			Directory.CreateDirectory(outDir);
		}

		if (sources.Count == 1)
		{
			// Fast path: a single source can be sorted in memory and written
			// directly without any temp files or k-way merge.
			MergeSingleSource(sources[0], options, reportProgress, isCancelled);
			return;
		}

		List<string> tempFiles = new List<string>();
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

			SortedSource[] results = new SortedSource[sources.Count];
			long totalLines = 0;

			// Precompute source sizes so the sort phase can report real byte-based
			// progress (bytes read / total bytes across all sources).
			long totalSize = 0;
			long[] sizes = new long[sources.Count];
			for (int i = 0; i < sources.Count; i++)
			{
				sizes[i] = GetSourceSize(sources[i]);
				totalSize += sizes[i];
			}
			SortProgress sortProgress = new SortProgress(totalSize, reportProgress);

			// Unified monotonic progress: sorting occupies 0..90% (real byte
			// progress), the k-way merge occupies 90..100% (real line progress).
			// Nothing ever jumps backward.
			Parallel.For(0, sources.Count, new ParallelOptions { MaxDegreeOfParallelism = MergeParallelism }, i =>
			{
				results[i] = SortSource(sources[i], options, tempFiles, sortProgress, sizes[i]);
				Interlocked.Add(ref totalLines, results[i].LineCount);
			});

			throttled(90);
			GlobalMerge(results, options, totalLines, throttled, isCancelled);
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

	private static void MergeSingleSource(MergeSource source, MergeOptions options, Action<int> reportProgress, Func<bool> isCancelled)
	{
		long sourceSize = GetSourceSize(source);
		SortProgress progress = new SortProgress(sourceSize, reportProgress);
		List<SortedItem> items = LoadAndSort(source, options, out long lineCount, progress, sourceSize);
		int total = items.Count;
		using (StreamWriter writer = new StreamWriter(options.OutputPath, false, new UTF8Encoding(true), StreamBufferSize))
		{
			int written = 0;
			for (int i = 0; i < total; i++)
			{
				SortedItem it = items[i];
				bool hasTs = it.Timestamp != DateTime.MaxValue;
				if (hasTs && !PassesTimeFilter(it.Timestamp, options))
				{
					continue;
				}
				writer.WriteLine(it.Line);
				written++;
				if ((i & 0xFFF) == 0)
				{
					ThrowIfCancelled(isCancelled);
				}
				if (total > 0)
				{
					int pct = (int)(90 + written * 10 / (double)total);
					if (pct > 100)
					{
						pct = 100;
					}
					reportProgress?.Invoke(pct);
				}
			}
		}
		// Report completion even when the time filter dropped lines (written < total),
		// so the progress bar reaches 100 before the dialog closes.
		reportProgress?.Invoke(100);
	}

	private static SortedSource SortSource(MergeSource source, MergeOptions options, List<string> tempFiles, SortProgress progress, long sourceSize)
	{
		if (ShouldSortInMemory(source))
		{
			return new SortedSource { Memory = LoadAndSort(source, options, out long count, progress, sourceSize), LineCount = count };
		}
		return SortSourceToTemp(source, options, tempFiles, progress, sourceSize);
	}

	private static long GetSourceSize(MergeSource source)
	{
		if (source.SizeBytes.HasValue && source.SizeBytes.Value > 0)
		{
			return source.SizeBytes.Value;
		}
		try
		{
			using (TextReader reader = source.Open())
			{
				if (reader is StreamReader sr && sr.BaseStream.CanSeek)
				{
					return sr.BaseStream.Length;
				}
			}
		}
		catch (IOException)
		{
		}
		return 1;
	}

	private static bool ShouldSortInMemory(MergeSource source)
	{
		if (source.SizeBytes.HasValue && source.SizeBytes.Value > 0)
		{
			return source.SizeBytes.Value * 3 <= MaxInMemorySortBytes;
		}
		return true;
	}

	private static List<SortedItem> LoadAndSort(MergeSource source, MergeOptions options, out long lineCount, SortProgress progress, long sourceSize)
	{
		List<SortedItem> items = new List<SortedItem>();
		int index = 0;
		long lastPos = 0;
		bool seekable;
		using (TextReader reader = source.Open())
		{
			seekable = reader is StreamReader sr && sr.BaseStream.CanSeek;
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix))
				{
					items.Add(new SortedItem { Timestamp = ts, Index = index, Line = line });
				}
				else if (!options.DropNonTimestampedLines)
				{
					items.Add(new SortedItem { Timestamp = DateTime.MaxValue, Index = index, Line = line });
				}
				index++;
				if (seekable && (index & 0x1FF) == 0)
				{
					long pos = ((StreamReader)reader).BaseStream.Position;
					progress?.AddBytes(pos - lastPos);
					lastPos = pos;
				}
			}
		}
		if (seekable)
		{
			progress?.AddBytes(sourceSize - lastPos);
		}
		lineCount = items.Count;
		items.Sort((a, b) =>
		{
			int c = a.Timestamp.CompareTo(b.Timestamp);
			return c != 0 ? c : a.Index.CompareTo(b.Index);
		});
		return items;
	}

	private static SortedSource SortSourceToTemp(MergeSource source, MergeOptions options, List<string> tempFiles, SortProgress progress, long sourceSize)
	{
		List<SortedItem> run = new List<SortedItem>(RunLineCap);
		List<string> runFiles = new List<string>();
		int index = 0;
		long lineCount = 0;
		long lastPos = 0;
		bool seekable;

		using (TextReader reader = source.Open())
		{
			seekable = reader is StreamReader sr && sr.BaseStream.CanSeek;
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix))
				{
					run.Add(new SortedItem { Timestamp = ts, Index = index, Line = line });
					lineCount++;
				}
				else if (!options.DropNonTimestampedLines)
				{
					run.Add(new SortedItem { Timestamp = DateTime.MaxValue, Index = index, Line = line });
					lineCount++;
				}
				index++;
				if (seekable && (index & 0x1FF) == 0)
				{
					long pos = ((StreamReader)reader).BaseStream.Position;
					progress?.AddBytes(pos - lastPos);
					lastPos = pos;
				}
				if (run.Count >= RunLineCap)
				{
					FlushRun(run, runFiles, tempFiles);
				}
			}
		}
		if (seekable)
		{
			progress?.AddBytes(sourceSize - lastPos);
		}

		if (run.Count > 0)
		{
			FlushRun(run, runFiles, tempFiles);
		}
		if (runFiles.Count == 0)
		{
			string empty = NewTempFile(tempFiles);
			return new SortedSource { TempPath = empty, LineCount = 0 };
		}

		string sortedPath = NewTempFile(tempFiles);
		MergeRuns(runFiles, sortedPath);
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
		return new SortedSource { TempPath = sortedPath, LineCount = lineCount };
	}

	private static void FlushRun(List<SortedItem> run, List<string> runFiles, List<string> tempFiles)
	{
		run.Sort((a, b) =>
		{
			int c = a.Timestamp.CompareTo(b.Timestamp);
			return c != 0 ? c : a.Index.CompareTo(b.Index);
		});
		string runFile = NewTempFile(tempFiles);
		using (StreamWriter writer = new StreamWriter(runFile, false, new UTF8Encoding(false), StreamBufferSize))
		{
			foreach (SortedItem item in run)
			{
				writer.WriteLine(item.Line);
			}
		}
		runFiles.Add(runFile);
		run.Clear();
	}

	private static void MergeRuns(List<string> runFiles, string sortedPath)
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

	private sealed class SourceCursor
	{
		public bool InMemory;

		public List<SortedItem> List;

		public int Pos;

		public StreamReader Reader;

		public bool HasNext;

		public bool HasTimestamp;

		public DateTime Timestamp;

		public string Line;

		public void Advance()
		{
			if (InMemory)
			{
				if (Pos < List.Count)
				{
					SortedItem it = List[Pos++];
					Line = it.Line;
					HasTimestamp = it.Timestamp != DateTime.MaxValue;
					Timestamp = it.Timestamp;
					HasNext = true;
				}
				else
				{
					HasNext = false;
				}
			}
			else
			{
				string line = Reader.ReadLine();
				if (line == null)
				{
					HasNext = false;
				}
				else
				{
					Line = line;
					HasTimestamp = LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix);
					Timestamp = HasTimestamp ? ts : DateTime.MaxValue;
					HasNext = true;
				}
			}
		}
	}

	private static void GlobalMerge(SortedSource[] sorted, MergeOptions options, long totalLines, Action<int> reportProgress, Func<bool> isCancelled)
	{
		BinaryHeap<HeapItem> heap = new BinaryHeap<HeapItem>((x, y) =>
		{
			int c = x.Timestamp.CompareTo(y.Timestamp);
			return c != 0 ? c : x.SourceIndex.CompareTo(y.SourceIndex);
		});
		SourceCursor[] cursors = new SourceCursor[sorted.Length];
		try
		{
			for (int i = 0; i < sorted.Length; i++)
			{
				SourceCursor cursor;
				if (sorted[i].Memory != null)
				{
					cursor = new SourceCursor { InMemory = true, List = sorted[i].Memory };
				}
				else
				{
					cursor = new SourceCursor { InMemory = false, Reader = new StreamReader(sorted[i].TempPath, Encoding.UTF8, true, StreamBufferSize) };
				}
				cursors[i] = cursor;
				cursor.Advance();
				if (cursor.HasNext)
				{
					heap.Push(new HeapItem { Timestamp = cursor.Timestamp, HasTimestamp = cursor.HasTimestamp, SourceIndex = i, Line = cursor.Line });
				}
			}

			using (StreamWriter writer = new StreamWriter(options.OutputPath, false, new UTF8Encoding(true), StreamBufferSize))
			{
				long processed = 0;
				while (heap.Count > 0)
				{
					HeapItem item = heap.Pop();
					processed++;

					bool keep = !item.HasTimestamp || PassesTimeFilter(item.Timestamp, options);
					if (keep)
					{
						writer.WriteLine(item.Line);
					}

					SourceCursor cursor = cursors[item.SourceIndex];
					cursor.Advance();
					if (cursor.HasNext)
					{
						heap.Push(new HeapItem { Timestamp = cursor.Timestamp, HasTimestamp = cursor.HasTimestamp, SourceIndex = item.SourceIndex, Line = cursor.Line });
					}

					if ((processed & 0xFFF) == 0)
					{
						ThrowIfCancelled(isCancelled);
					}
					// Track lines processed (not lines kept/written) so progress
					// reaches 100 even when the time filter drops some lines.
					if (totalLines > 0)
					{
						int pct = (int)(90 + processed * 10 / (double)totalLines);
						if (pct > 100)
						{
							pct = 100;
						}
						reportProgress?.Invoke(pct);
					}
				}
			}
		}
		finally
		{
			foreach (SourceCursor c in cursors)
			{
				if (c != null && c.Reader != null)
				{
					c.Reader.Dispose();
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