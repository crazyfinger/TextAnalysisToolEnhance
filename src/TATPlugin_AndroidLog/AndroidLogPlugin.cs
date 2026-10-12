using System;
using System.Collections.Generic;
using System.Drawing;
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

	private readonly AndroidLogSettings m_settings = new AndroidLogSettings();

	public AndroidLogPlugin()
	{
		m_settings.Load();
	}

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
		return new AndroidLogSettingsForm(m_settings);
	}

	public IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText)
	{
		List<PluginContextMenuItem> result = new List<PluginContextMenuItem>();
		if (string.IsNullOrEmpty(lineText))
		{
			return result;
		}

		LineInfo info = ParseLineInfo(lineText);
		if (info.Pid <= 0 && string.IsNullOrEmpty(info.Tag))
		{
			return result;
		}

		string pidPattern = info.Pid > 0 ? GetFilterPatternForPid(info.Pid) : null;
		string tagPattern = !string.IsNullOrEmpty(info.Tag) ? GetFilterPatternForTag(info.Tag) : null;

		if (info.Pid > 0)
		{
			Color fore = m_settings.GetColorForPid(info.Pid);
			Color back = m_settings.PidBackColor;
			result.Add(new PluginContextMenuItem(
				"Filter by PID " + info.Pid,
				pidPattern,
				caseSensitive: true,
				foreColor: fore,
				backColor: back,
				description: "PID " + info.Pid));
		}

		if (!string.IsNullOrEmpty(info.Tag))
		{
			Color fore = m_settings.GetColorForTag(info.Tag);
			Color back = m_settings.TagBackColor;
			result.Add(new PluginContextMenuItem(
				"Filter by tag " + info.Tag,
				tagPattern,
				caseSensitive: true,
				foreColor: fore,
				backColor: back,
				description: "Tag " + info.Tag));
		}

		List<PluginContextMenuItem> subItems = new List<PluginContextMenuItem>();
		if (info.Pid > 0)
		{
			subItems.Add(new PluginContextMenuItem("Filter by PID " + info.Pid + " (Choose Color...)", pidPattern, caseSensitive: true, promptForColor: true, description: "PID " + info.Pid));
			subItems.Add(new PluginContextMenuItem("Filter by PID " + info.Pid + " (Random Color)", pidPattern, caseSensitive: true, foreColor: m_settings.GetRandomColorForPid(info.Pid, forceNewRandom: true), description: "PID " + info.Pid));
			subItems.Add(new PluginContextMenuItem("Filter by PID " + info.Pid + " (Default Black)", pidPattern, caseSensitive: true, foreColor: Color.Empty, description: "PID " + info.Pid));
		}

		if (info.Pid > 0 && !string.IsNullOrEmpty(info.Tag))
		{
			subItems.Add(PluginContextMenuItem.Separator());
		}

		if (!string.IsNullOrEmpty(info.Tag))
		{
			subItems.Add(new PluginContextMenuItem("Filter by tag " + info.Tag + " (Choose Color...)", tagPattern, caseSensitive: true, promptForColor: true, description: "Tag " + info.Tag));
			subItems.Add(new PluginContextMenuItem("Filter by tag " + info.Tag + " (Random Color)", tagPattern, caseSensitive: true, foreColor: m_settings.GetRandomColorForTag(info.Tag, forceNewRandom: true), description: "Tag " + info.Tag));
			subItems.Add(new PluginContextMenuItem("Filter by tag " + info.Tag + " (Default Black)", tagPattern, caseSensitive: true, foreColor: Color.Empty, description: "Tag " + info.Tag));
		}

		subItems.Add(PluginContextMenuItem.Separator());
		subItems.Add(new PluginContextMenuItem("Plug-in Settings...", () =>
		{
			using Form dlg = GetSettingsDialog();
			dlg?.ShowDialog();
		}));

		result.Add(new PluginContextMenuItem("More Android Filter Options", subItems));

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
