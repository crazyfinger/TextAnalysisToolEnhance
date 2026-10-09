using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

internal sealed class SourceItem
{
	public string FilePath { get; set; }

	public string ArchivePath { get; set; }

	public string MemberName { get; set; }

	public bool IsGzip { get; set; }

	public string SourceTag { get; set; }

	public string DisplayName { get; set; }
}

internal sealed class MergeDialog : Form
{
	private readonly List<SourceItem> m_sources = new List<SourceItem>();

	private readonly List<string> m_tempFiles = new List<string>();

	private readonly string m_tempDir;

	private CheckedListBox m_sourceList;

	private TextBox m_fromTextBox;

	private TextBox m_toTextBox;

	private CheckBox m_timestampedCheck;

	private TextBox m_outputTextBox;

	private Button m_browseButton;

	private ProgressBar m_progressBar;

	private Label m_statusLabel;

	private Button m_mergeButton;

	private Button m_directButton;

	private Button m_cancelButton;

	private volatile bool m_cancelled;

	private volatile bool m_running;

	public string SingleSourcePath { get; private set; }

	public string MergedOutputPath { get; private set; }

	public MergeDialog(IEnumerable<string> selectedFiles)
	{
		m_tempDir = Path.Combine(Path.GetTempPath(), "TAT-Merge-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(m_tempDir);

		Text = "Open / Merge Logs";
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		ClientSize = new Size(560, 440);

		BuildSources(selectedFiles);
		BuildUi();
		SetDefaultOutputPath();
		SetDefaultTimeRange();
	}

	private void SetDefaultOutputPath()
	{
		if (m_sources.Count == 0)
		{
			return;
		}
		SourceItem first = m_sources[0];
		string dir = first.ArchivePath != null ? Path.GetDirectoryName(first.ArchivePath) : Path.GetDirectoryName(first.FilePath);
		if (!string.IsNullOrEmpty(dir))
		{
			m_outputTextBox.Text = Path.Combine(dir, "merged.log");
		}
	}

	private void SetDefaultTimeRange()
	{
		string detected = null;
		if (m_sources.Count > 0)
		{
			try
			{
				using (TextReader reader = OpenSource(m_sources[0]))
				{
					for (int i = 0; i < 200; i++)
					{
						string line = reader.ReadLine();
						if (line == null)
						{
							break;
						}
						if (LogTimestampParser.TryParsePrefix(line, out DateTime ts, out int prefix))
						{
							detected = ts.ToString("MM-dd HH:mm:ss.fff");
							break;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
		// Pre-fill 'From' with the first timestamp found in the first source so
		// the range starts where the log data begins. If none is found, leave it
		// empty (no time filtering). Never fall back to "today" -- that would
		// drop all lines from past-dated logs.
		m_fromTextBox.Text = detected ?? string.Empty;
		m_toTextBox.Text = string.Empty;
	}

	private void BuildSources(IEnumerable<string> selectedFiles)
	{
		foreach (string file in selectedFiles)
		{
			if (!ArchiveSupport.IsArchivePath(file))
			{
				m_sources.Add(new SourceItem
				{
					FilePath = file,
					SourceTag = ArchiveSupport.GetSourceName(file),
					DisplayName = file
				});
				continue;
			}

			if (ArchiveSupport.IsGzipPath(file))
			{
				m_sources.Add(new SourceItem
				{
					FilePath = file,
					IsGzip = true,
					SourceTag = ArchiveSupport.GetSourceName(file),
					DisplayName = file
				});
				continue;
			}

			// multi-member archive (zip / 7z)
			if (ArchiveSupport.Is7zPath(file) && !ArchiveSupport.Is7zAvailable())
			{
				MessageBox.Show(this, "7-Zip (7z.exe) is required to open 7z archives but was not found.\n\nThe archive will be skipped.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				continue;
			}

			List<string> members = ArchiveSupport.ListMembers(file, out string error);
			if (members == null || members.Count == 0)
			{
				if (!string.IsNullOrEmpty(error))
				{
					MessageBox.Show(this, "Could not read archive:\n" + error, "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				continue;
			}
			foreach (string member in members)
			{
				string tag = Path.GetFileName(member);
				if (string.IsNullOrEmpty(tag))
				{
					tag = member;
				}
				m_sources.Add(new SourceItem
				{
					ArchivePath = file,
					MemberName = member,
					SourceTag = tag,
					DisplayName = file + "  ::  " + member
				});
			}
		}
	}

	private void BuildUi()
	{
		int margin = 10;
		int rowH = 26;

		Label listLabel = new Label { Text = "Sources:", AutoSize = true, Location = new Point(margin, margin) };
		Controls.Add(listLabel);

		m_sourceList = new CheckedListBox
		{
			Location = new Point(margin, margin + 20),
			Size = new Size(ClientSize.Width - 2 * margin, 190),
			CheckOnClick = true,
			IntegralHeight = false
		};
		foreach (SourceItem s in m_sources)
		{
			m_sourceList.Items.Add(s.DisplayName, isChecked: true);
		}
		m_sourceList.ItemCheck += (s, e) => BeginInvoke((MethodInvoker)UpdateDirectButtonEnabled);
		Controls.Add(m_sourceList);

		int y = m_sourceList.Bottom + 10;

		m_fromTextBox = AddLabeledTextBox("From:", y, "MM-dd HH:mm:ss.SSS (optional)");
		m_toTextBox = AddLabeledTextBox("To:", y + rowH, "MM-dd HH:mm:ss.SSS (optional)");

		y += 2 * rowH + 6;
		m_timestampedCheck = new CheckBox { Text = "Only merge lines that have a timestamp", Checked = true, AutoSize = true, Location = new Point(margin, y) };
		Controls.Add(m_timestampedCheck);

		y += 26;
		Label outLabel = new Label { Text = "Output:", AutoSize = true, Location = new Point(margin, y + 3) };
		Controls.Add(outLabel);
		m_outputTextBox = new TextBox { Location = new Point(margin + 52, y), Width = 360 };
		Controls.Add(m_outputTextBox);
		m_browseButton = new Button { Text = "Browse...", Location = new Point(m_outputTextBox.Right + 6, y - 2), Width = 76 };
		m_browseButton.Click += BrowseButton_Click;
		Controls.Add(m_browseButton);

		y += rowH + 4;
		m_progressBar = new ProgressBar { Location = new Point(margin, y), Width = ClientSize.Width - 2 * margin, Height = 18, Minimum = 0, Maximum = 100 };
		Controls.Add(m_progressBar);
		m_statusLabel = new Label { Text = "", AutoSize = true, Location = new Point(margin, y + 21) };
		Controls.Add(m_statusLabel);

		int buttonY = ClientSize.Height - 40;
		m_cancelButton = new Button { Text = "Cancel", Location = new Point(ClientSize.Width - 92, buttonY), Width = 80 };
		m_cancelButton.Click += CancelButton_Click;
		Controls.Add(m_cancelButton);
		m_mergeButton = new Button { Text = "Merge && Open", Location = new Point(m_cancelButton.Left - 118, buttonY), Width = 110 };
		m_mergeButton.Click += MergeButton_Click;
		Controls.Add(m_mergeButton);
		m_directButton = new Button { Text = "Open Directly", Location = new Point(m_mergeButton.Left - 106, buttonY), Width = 100 };
		m_directButton.Click += DirectButton_Click;
		Controls.Add(m_directButton);

		UpdateDirectButtonEnabled();
	}

	private TextBox AddLabeledTextBox(string label, int y, string placeholder)
	{
		int margin = 10;
		Label l = new Label { Text = label, AutoSize = true, Location = new Point(margin, y + 3) };
		Controls.Add(l);
		TextBox tb = new TextBox { Location = new Point(margin + 52, y), Width = 220 };
		tb.TextChanged += (s, e) => { };
		Controls.Add(tb);
		return tb;
	}

	private List<SourceItem> GetCheckedSources()
	{
		List<SourceItem> result = new List<SourceItem>();
		for (int i = 0; i < m_sourceList.Items.Count; i++)
		{
			if (m_sourceList.GetItemChecked(i) && i < m_sources.Count)
			{
				result.Add(m_sources[i]);
			}
		}
		return result;
	}

	private void UpdateDirectButtonEnabled()
	{
		m_directButton.Enabled = GetCheckedSources().Count == 1;
	}

	private void BrowseButton_Click(object sender, EventArgs e)
	{
		using SaveFileDialog dlg = new SaveFileDialog();
		dlg.Filter = "Log Files (*.txt;*.log)|*.txt;*.log|All Files (*)|*";
		dlg.FileName = "merged.log";
		if (dlg.ShowDialog(this) == DialogResult.OK)
		{
			m_outputTextBox.Text = dlg.FileName;
		}
	}

	private void DirectButton_Click(object sender, EventArgs e)
	{
		List<SourceItem> checkedItems = GetCheckedSources();
		if (checkedItems.Count != 1)
		{
			return;
		}
		SourceItem item = checkedItems[0];
		string path = Materialize(item);
		if (path == null)
		{
			MessageBox.Show(this, "Could not open this source.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		SingleSourcePath = path;
		CleanupTempFiles();
		DialogResult = DialogResult.OK;
		Close();
	}

	private void MergeButton_Click(object sender, EventArgs e)
	{
		if (m_running)
		{
			m_cancelled = true;
			return;
		}
		List<SourceItem> checkedItems = GetCheckedSources();
		if (checkedItems.Count == 0)
		{
			MessageBox.Show(this, "Select at least one source.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		if (string.IsNullOrWhiteSpace(m_outputTextBox.Text))
		{
			MessageBox.Show(this, "Specify an output file.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		DateTime? from = ParseTime(m_fromTextBox.Text);
		DateTime? to = ParseTime(m_toTextBox.Text);
		if (m_fromTextBox.Text.Trim().Length > 0 && !from.HasValue)
		{
			MessageBox.Show(this, "Invalid 'from' time. Use format MM-dd HH:mm:ss.SSS", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		if (m_toTextBox.Text.Trim().Length > 0 && !to.HasValue)
		{
			MessageBox.Show(this, "Invalid 'to' time. Use format MM-dd HH:mm:ss.SSS", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		if (from.HasValue && to.HasValue && to.Value <= from.Value)
		{
			MessageBox.Show(this, "'to' must be later than 'from'.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		MergeOptions options = new MergeOptions
		{
			OutputPath = m_outputTextBox.Text,
			From = from,
			To = to,
			DropNonTimestampedLines = m_timestampedCheck.Checked
		};

		List<MergeSource> sources = new List<MergeSource>();
		foreach (SourceItem item in checkedItems)
		{
			string tag = item.SourceTag;
			MergeSource source = new MergeSource(tag, () => OpenSource(item));
			if (item.ArchivePath == null && !item.IsGzip && File.Exists(item.FilePath))
			{
				source.SizeBytes = new FileInfo(item.FilePath).Length;
			}
			sources.Add(source);
		}

		SetRunning(true);
		m_cancelled = false;
		Thread thread = new Thread(() => RunMerge(sources, options));
		thread.IsBackground = true;
		thread.Start();
	}

	private TextReader OpenSource(SourceItem item)
	{
		if (item.ArchivePath != null)
		{
			return ArchiveSupport.OpenMember(item.ArchivePath, item.MemberName, m_tempDir, m_tempFiles);
		}
		if (item.IsGzip)
		{
			return ArchiveSupport.OpenGzip(item.FilePath);
		}
		return new StreamReader(item.FilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
	}

	private string Materialize(SourceItem item)
	{
		if (item.ArchivePath != null)
		{
			string dest = ArchiveSupport.ExtractMemberToFile(item.ArchivePath, item.MemberName, m_tempDir);
			if (dest != null)
			{
				m_tempFiles.Add(dest);
			}
			return dest;
		}
		if (item.IsGzip)
		{
			string dest = Path.Combine(m_tempDir, Path.GetFileNameWithoutExtension(item.FilePath) + ".txt");
			try
			{
				using (TextReader input = ArchiveSupport.OpenGzip(item.FilePath))
				using (StreamWriter writer = new StreamWriter(dest, false, new UTF8Encoding(true)))
				{
					string line;
					while ((line = input.ReadLine()) != null)
					{
						writer.WriteLine(line);
					}
				}
				m_tempFiles.Add(dest);
				return dest;
			}
			catch (Exception)
			{
				return null;
			}
		}
		return item.FilePath;
	}

	private void RunMerge(List<MergeSource> sources, MergeOptions options)
	{
		try
		{
			LogMergeEngine.Merge(sources, options, ReportProgress, () => m_cancelled);
			BeginInvoke((MethodInvoker)delegate
			{
				m_statusLabel.Text = "Done.";
				m_running = false;
				MergedOutputPath = options.OutputPath;
				CleanupTempFiles();
				DialogResult = DialogResult.OK;
				Close();
			});
		}
		catch (OperationCanceledException)
		{
			BeginInvoke((MethodInvoker)delegate
			{
				m_statusLabel.Text = "Cancelled.";
				SetRunning(false);
			});
		}
		catch (Exception ex)
		{
			BeginInvoke((MethodInvoker)delegate
			{
				m_statusLabel.Text = "Error: " + ex.Message;
				MessageBox.Show(this, "Merge failed:\n" + ex.Message, "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetRunning(false);
			});
		}
	}

	private void ReportProgress(int percent)
	{
		if (IsDisposed)
		{
			return;
		}
		try
		{
			BeginInvoke((MethodInvoker)delegate
			{
				if (!IsDisposed)
				{
					m_progressBar.Value = Math.Min(100, Math.Max(0, percent));
				}
			});
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private void SetRunning(bool running)
	{
		m_running = running;
		m_sourceList.Enabled = !running;
		m_mergeButton.Enabled = true;
		m_mergeButton.Text = running ? "Cancel" : "Merge && Open";
		m_directButton.Enabled = !running;
		m_browseButton.Enabled = !running;
		m_fromTextBox.Enabled = !running;
		m_toTextBox.Enabled = !running;
		m_timestampedCheck.Enabled = !running;
		m_outputTextBox.Enabled = !running;
		// The merge button toggles to "Cancel" while running, so hide the
		// dedicated Cancel button to avoid two Cancel buttons.
		m_cancelButton.Visible = !running;
	}

	private void CancelButton_Click(object sender, EventArgs e)
	{
		if (m_running)
		{
			m_cancelled = true;
			return;
		}
		CleanupTempFiles();
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void CleanupTempFiles()
	{
		foreach (string file in m_tempFiles)
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
		m_tempFiles.Clear();
		try
		{
			if (Directory.Exists(m_tempDir))
			{
				Directory.Delete(m_tempDir, recursive: true);
			}
		}
		catch (IOException)
		{
		}
	}

	private static DateTime? ParseTime(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (DateTime.TryParseExact(text.Trim(), "MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
		{
			// The merge engine parses log timestamps as year-2000 dates (only MM-dd
			// is present in the logs), so normalize to the same year to keep the
			// From/To filter comparable with log timestamps.
			return new DateTime(2000, result.Month, result.Day, result.Hour, result.Minute, result.Second, result.Millisecond);
		}
		return null;
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (m_running && !m_cancelled)
		{
			e.Cancel = true;
			return;
		}
		base.OnFormClosing(e);
	}
}