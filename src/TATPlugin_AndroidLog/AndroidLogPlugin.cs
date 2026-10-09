using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TextAnalysisTool.NET.Plugin;

namespace TATPlugin_AndroidLog;

public sealed class AndroidLogPlugin : ITextAnalysisToolPlugin
{
	private static readonly Regex[] s_lineInfoPatterns =
	{
		new Regex(@"^\s*(?:\d{4}-)?\d{1,2}-\d{1,2}\s+\d{1,2}:\d{1,2}:\d{1,2}[.,]\d+\s+(?:(?:\d+\s+)?(\d+)\s+(\d+))\s+([VDIWEF])\s+([^:]*?)\s*:\s*(.*)$", RegexOptions.Singleline),
		new Regex(@"^\s*(?:\d{4}-)?\d{1,2}-\d{1,2}\s+\d{1,2}:\d{1,2}:\d{1,2}[.,]\d+\s+(\d+)-(\d+)(?:/\S*)?\s+([VDIWEF])/([^:]*?)\s*:\s*(.*)$", RegexOptions.Singleline),
		new Regex(@"^\s*(?:\d{4}-)?\d{1,2}-\d{1,2}\s+\d{1,2}:\d{1,2}:\d{1,2}[.,]\d+\s+(\d+)-(\d+)(?:/\S*)?\s+([VDIWEF])\s+([^:]*?)\s*:\s*(.*)$", RegexOptions.Singleline)
	};

	public string GetFileTypeDescription()
	{
		return "Android log files";
	}

	public string GetFileTypePattern()
	{
		return "*.log";
	}

	public bool IsFileTypeSupported(string fileName)
	{
		return string.Equals(Path.GetExtension(fileName), ".log", StringComparison.OrdinalIgnoreCase);
	}

	public TextReader GetReaderForFile(string fileName)
	{
		return new StreamReader(fileName);
	}

	public Form GetSettingsDialog()
	{
		return null;
	}

	public IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText)
	{
		List<PluginContextMenuItem> result = new List<PluginContextMenuItem>();
		if (string.IsNullOrEmpty(lineText))
		{
			return result;
		}
		LineInfo info = ParseLineInfo(lineText);
		if (info.Pid > 0)
		{
			result.Add(new PluginContextMenuItem("Filter by PID " + info.Pid, GetFilterPatternForPid(info.Pid), caseSensitive: true));
		}
		if (!string.IsNullOrEmpty(info.Tag))
		{
			result.Add(new PluginContextMenuItem("Filter by tag " + info.Tag, GetFilterPatternForTag(info.Tag), caseSensitive: true));
		}
		return result;
	}

	private static string GetFilterPatternForPid(int pid)
	{
		return @"^\s*\S+\s+\S+\s+(?:(?:\d+\s+)?" + pid + @"\s+\d+\s+|" + pid + "-)";
	}

	private static string GetFilterPatternForTag(string tag)
	{
		string quoted = Regex.Escape(tag);
		return @"^\s*\S+\s+\S+[^:]*?\s[VDIWEF](?:/" + quoted + @"|\s+" + quoted + @")(?:\s*\([^)]*\))?\s*:";
	}

	private static LineInfo ParseLineInfo(string line)
	{
		foreach (Regex pattern in s_lineInfoPatterns)
		{
			Match m = pattern.Match(line);
			if (!m.Success)
			{
				continue;
			}
			int pid = 0;
			if (int.TryParse(m.Groups[1].Value, out int parsedPid))
			{
				pid = parsedPid;
			}
			string tag = m.Groups[4].Value.Trim();
			return new LineInfo(pid, string.IsNullOrEmpty(tag) ? null : tag);
		}
		return new LineInfo(0, null);
	}

	private sealed class LineInfo
	{
		public int Pid { get; }

		public string Tag { get; }

		public LineInfo(int pid, string tag)
		{
			Pid = pid;
			Tag = tag;
		}
	}
}