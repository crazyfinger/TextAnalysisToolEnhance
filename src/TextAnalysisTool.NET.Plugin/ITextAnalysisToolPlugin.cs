using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TextAnalysisTool.NET.Plugin;

public interface ITextAnalysisToolPlugin
{
	string GetFileTypeDescription();

	string GetFileTypePattern();

	bool IsFileTypeSupported(string fileName);

	TextReader GetReaderForFile(string fileName);

	Form GetSettingsDialog();

	IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText);
}

public sealed class PluginContextMenuItem
{
	public string Text { get; }

	public string FilterPattern { get; }

	public bool CaseSensitive { get; }

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive = false)
	{
		Text = text;
		FilterPattern = filterPattern;
		CaseSensitive = caseSensitive;
	}
}