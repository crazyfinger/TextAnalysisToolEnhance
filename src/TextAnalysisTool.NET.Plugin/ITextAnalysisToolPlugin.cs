using System;
using System.Collections.Generic;
using System.Drawing;
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

	public Color ForeColor { get; }

	public Color BackColor { get; }

	public string Description { get; }

	public bool PromptForColor { get; }

	public Action ClickAction { get; }

	public IList<PluginContextMenuItem> SubItems { get; }

	public bool IsSeparator => Text == "-";

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive = false)
		: this(text, filterPattern, caseSensitive, Color.Empty, Color.Empty, null, false, null, null)
	{
	}

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive, Color foreColor)
		: this(text, filterPattern, caseSensitive, foreColor, Color.Empty, null, false, null, null)
	{
	}

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive, Color foreColor, Color backColor)
		: this(text, filterPattern, caseSensitive, foreColor, backColor, null, false, null, null)
	{
	}

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive, Color foreColor, Color backColor, string description)
		: this(text, filterPattern, caseSensitive, foreColor, backColor, description, false, null, null)
	{
	}

	public PluginContextMenuItem(string text, string filterPattern, bool caseSensitive, bool promptForColor, string description = null)
		: this(text, filterPattern, caseSensitive, Color.Empty, Color.Empty, description, promptForColor, null, null)
	{
	}

	public PluginContextMenuItem(string text, Action clickAction)
		: this(text, null, false, Color.Empty, Color.Empty, null, false, clickAction, null)
	{
	}

	public PluginContextMenuItem(string text, IList<PluginContextMenuItem> subItems)
		: this(text, null, false, Color.Empty, Color.Empty, null, false, null, subItems)
	{
	}

	public PluginContextMenuItem(
		string text,
		string filterPattern,
		bool caseSensitive = false,
		Color foreColor = default,
		Color backColor = default,
		string description = null,
		bool promptForColor = false,
		Action clickAction = null,
		IList<PluginContextMenuItem> subItems = null)
	{
		Text = text;
		FilterPattern = filterPattern;
		CaseSensitive = caseSensitive;
		ForeColor = foreColor;
		BackColor = backColor;
		Description = description;
		PromptForColor = promptForColor;
		ClickAction = clickAction;
		SubItems = subItems;
	}

	public static PluginContextMenuItem Separator() => new PluginContextMenuItem("-", string.Empty, false);
}
