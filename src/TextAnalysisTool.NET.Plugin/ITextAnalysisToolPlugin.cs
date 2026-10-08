using System.IO;
using System.Windows.Forms;

namespace TextAnalysisTool.NET.Plugin;

internal interface ITextAnalysisToolPlugin
{
	string GetFileTypeDescription();

	string GetFileTypePattern();

	bool IsFileTypeSupported(string fileName);

	TextReader GetReaderForFile(string fileName);

	Form GetSettingsDialog();
}
