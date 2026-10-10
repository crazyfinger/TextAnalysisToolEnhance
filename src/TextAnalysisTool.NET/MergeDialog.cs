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

	private Button m_detectButton;

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
		ClientSize = new Size(660, 560);

		Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		AutoScaleDimensions = new SizeF(120f, 120f);
		AutoScaleMode = AutoScaleMode.Dpi;

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
		// Leave the time range empty by default. Detection is done on demand by
		// the "Detect Time" button (see DetectButton_Click).
		m_fromTextBox.Text = string.Empty;
		m_toTextBox.Text = string.Empty;
	}

	private string DetectFirstTimestamp()
	{
		if (m_sources.Count == 0)
		{
			return null;
		}
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
						return ts.ToString("MM-dd HH:mm:ss.fff");
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

private Button MakeDetectButton()
	{
		Button b = new Button
		{
			Text = "Detect Time",
			AutoSize = true,
			Padding = new Padding(8, 5, 8, 5),
			UseVisualStyleBackColor = true,
			Margin = new Padding(10, 0, 0, 0),
			Anchor = AnchorStyles.None
		};
		b.Click += DetectButton_Click;
		return b;
	}

	private void DetectButton_Click(object sender, EventArgs e)
	{
		string detected = DetectFirstTimestamp();
		if (detected == null)
		{
			return;
		}
		DateTime? from = ParseTime(detected);
		if (from.HasValue)
		{
			m_fromTextBox.Text = detected;
			m_toTextBox.Text = from.Value.AddMinutes(10).ToString("MM-dd HH:mm:ss.fff");
		}
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
		int pad = 12;

		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 5,
			RowCount = 9,
			Padding = new Padding(pad),
			Margin = Padding.Empty
		};
		// col0 = labels, col1 = time textboxes, col2 = "+10m" button,
		// col3 = output textbox, col4 = browse button
		root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		// Row 0: Sources label
		Label listLabel = new Label { Text = "Sources:", AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
		root.Controls.Add(listLabel, 0, 0);
		root.SetColumnSpan(listLabel, 5);

		// Row 1: source list
		m_sourceList = new CheckedListBox
		{
			Dock = DockStyle.Fill,
			CheckOnClick = true,
			IntegralHeight = false,
			Margin = new Padding(0, 4, 0, 10)
		};
		foreach (SourceItem s in m_sources)
		{
			m_sourceList.Items.Add(s.DisplayName, isChecked: true);
		}
		m_sourceList.ItemCheck += (s, e) => BeginInvoke((MethodInvoker)UpdateDirectButtonEnabled);
		root.Controls.Add(m_sourceList, 0, 1);
		root.SetColumnSpan(m_sourceList, 5);

		// Rows 2 & 3: From / To
		m_fromTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = 28, Margin = new Padding(10, 0, 0, 0) };
		m_toTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = 28, Margin = new Padding(10, 0, 0, 0) };
		m_detectButton = MakeDetectButton();
		root.Controls.Add(MakeLabel("From:"), 0, 2);
		root.Controls.Add(m_fromTextBox, 1, 2);
		root.Controls.Add(m_detectButton, 2, 2);
		root.SetRowSpan(m_detectButton, 2);
		root.Controls.Add(MakeLabel("To:"), 0, 3);
		root.Controls.Add(m_toTextBox, 1, 3);

		// Row 4: checkbox
		m_timestampedCheck = new CheckBox
		{
			Text = "Only merge lines that have a timestamp",
			Checked = true,
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 2)
		};
		root.Controls.Add(m_timestampedCheck, 0, 4);
		root.SetColumnSpan(m_timestampedCheck, 5);

		// Row 5: Output (textbox spans the time box + button + output columns)
		m_outputTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = 28, Margin = new Padding(10, 0, 10, 0) };
		m_browseButton = new Button
		{
			Text = "Browse",
			AutoSize = true,
			MinimumSize = new Size(88, 32),
			Padding = new Padding(14, 2, 14, 2),
			Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
			UseVisualStyleBackColor = true,
			Margin = new Padding(6, 0, 0, 0)
		};
		m_browseButton.Click += BrowseButton_Click;
		root.Controls.Add(MakeLabel("Output:"), 0, 5);
		root.Controls.Add(m_outputTextBox, 1, 5);
		root.SetColumnSpan(m_outputTextBox, 3);
		root.Controls.Add(m_browseButton, 4, 5);

		// Row 6: progress
		m_progressBar = new ProgressBar
		{
			Dock = DockStyle.Fill,
			MinimumSize = new Size(0, 18),
			Minimum = 0,
			Maximum = 100,
			Margin = new Padding(0, 12, 0, 2),
			Visible = false
		};
		root.Controls.Add(m_progressBar, 0, 6);
		root.SetColumnSpan(m_progressBar, 5);

		// Row 7: status
		m_statusLabel = new Label { Text = "", AutoSize = true, Margin = new Padding(0, 2, 0, 4) };
		root.Controls.Add(m_statusLabel, 0, 7);
		root.SetColumnSpan(m_statusLabel, 5);

		// Row 8: buttons
		TableLayoutPanel buttonsPanel = BuildButtonsPanel();
		root.Controls.Add(buttonsPanel, 0, 8);
		root.SetColumnSpan(buttonsPanel, 5);

		Controls.Add(root);

		UpdateDirectButtonEnabled();
	}

	private static Label MakeLabel(string text)
	{
		return new Label
		{
			Text = text,
			AutoSize = false,
			Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
			TextAlign = ContentAlignment.MiddleLeft,
			Margin = Padding.Empty
		};
	}

	private static Button MakeButton(string text, EventHandler onClick)
	{
		Button b = new Button
		{
			Text = text,
			AutoSize = true,
			MinimumSize = new Size(88, 32),
			Padding = new Padding(14, 5, 14, 5),
			UseVisualStyleBackColor = true,
			Margin = new Padding(6, 0, 6, 0)
		};
		b.Click += onClick;
		return b;
	}

	private TableLayoutPanel BuildButtonsPanel()
	{
		TableLayoutPanel p = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			AutoSize = true,
			ColumnCount = 4,
			RowCount = 1,
			Margin = new Padding(0, 10, 0, 0)
		};
		p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

		m_directButton = MakeButton("Open Directly", DirectButton_Click);
		m_mergeButton = MakeButton("Merge && Open", MergeButton_Click);
		m_cancelButton = MakeButton("Cancel", CancelButton_Click);

		p.Controls.Add(m_directButton, 1, 0);
		p.Controls.Add(m_mergeButton, 2, 0);
		p.Controls.Add(m_cancelButton, 3, 0);
		return p;
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
				m_progressBar.Value = 100;
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
		m_progressBar.Visible = running;
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