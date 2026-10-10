using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
	private static bool s_staleOpenCleanupDone;

	private readonly List<SourceItem> m_sources = new List<SourceItem>();

	private readonly List<string> m_tempFiles = new List<string>();

	private readonly string m_tempDir;

	private CheckedListBox m_sourceList;

	private CheckBox m_selectAllCheck;

	private bool m_updatingChecks;

	private TextBox m_fromTextBox;

	private TextBox m_toTextBox;

	private Button m_detectButton;

	private CheckBox m_timestampedCheck;

	private CheckBox m_openAfterMergeCheck;

	private TextBox m_outputTextBox;

	private Button m_browseButton;

	private string m_defaultOutputDir;

	private bool m_outputManuallySet;

	private bool m_updatingOutput;

	private ProgressBar m_progressBar;

	private Label m_statusLabel;

	private Button m_mergeButton;

	private Button m_directButton;

	private volatile bool m_cancelled;

	private volatile bool m_running;

	public string SingleSourcePath { get; private set; }

	public string MergedOutputPath { get; private set; }

	public MergeDialog(IEnumerable<string> selectedFiles)
	{
		EnsureStaleOpenDirCleanup();
		m_tempDir = Path.Combine(Path.GetTempPath(), "TAT-Merge-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(m_tempDir);

		Text = "Open / Merge Logs";
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		ClientSize = new Size(660, 528);

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
		string sourcePath = first.ArchivePath ?? first.FilePath;
		m_defaultOutputDir = Path.GetDirectoryName(sourcePath) ?? "";
		ApplyDefaultOutputName(GetFirstCheckedSource() ?? first);
	}

	private SourceItem GetFirstCheckedSource()
	{
		for (int i = 0; i < m_sourceList.Items.Count; i++)
		{
			if (m_sourceList.GetItemChecked(i) && i < m_sources.Count)
			{
				return m_sources[i];
			}
		}
		return null;
	}

	// Re-applies the default output name from the first checked source. Does
	// nothing once the user has set the output manually (e.g. via Browse).
	private void RecomputeDefaultOutput()
	{
		if (m_outputManuallySet)
		{
			return;
		}
		SourceItem first = GetFirstCheckedSource();
		if (first == null)
		{
			return;
		}
		ApplyDefaultOutputName(first);
	}

	private void ApplyDefaultOutputName(SourceItem item)
	{
		// For archive members use the member name inside the archive; otherwise
		// the plain file name.
		string sourceName = item.ArchivePath != null
			? Path.GetFileNameWithoutExtension(item.MemberName)
			: Path.GetFileNameWithoutExtension(item.FilePath);
		string prefix = BuildOutputPrefix(sourceName);
		m_updatingOutput = true;
		m_outputTextBox.Text = Path.Combine(m_defaultOutputDir, prefix + "_merged.txt");
		m_updatingOutput = false;
	}

	// Builds the default output name prefix from a source file name: take the
	// text before the first '-' if present, then cap the width at 10 letters
	// (English) / 5 CJK characters.
	private static string BuildOutputPrefix(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return "merged";
		}
		string p = name;
		int dash = p.IndexOf('-');
		if (dash >= 0)
		{
			p = p.Substring(0, dash);
		}
		p = p.Trim();
		if (p.Length == 0)
		{
			return "merged";
		}
		const int maxWidth = 10; // in "English letter" units; CJK counts as 2
		int width = 0;
		int count = 0;
		foreach (char c in p)
		{
			int w = c > 127 ? 2 : 1;
			if (width + w > maxWidth)
			{
				break;
			}
			width += w;
			count++;
		}
		if (count == 0)
		{
			return "merged";
		}
		return p.Substring(0, count);
	}

	private void SetDefaultTimeRange()
	{
		// Leave the time range empty by default. Detection is done on demand by
		// the "Detect Time" button (see DetectButton_Click).
		m_fromTextBox.Text = string.Empty;
		m_toTextBox.Text = string.Empty;
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
		if (m_running)
		{
			return;
		}
		List<SourceItem> checkedItems = GetCheckedSources();
		if (checkedItems.Count == 0)
		{
			ShowToast("Please select at least one source to detect a timestamp.");
			return;
		}
		m_statusLabel.Text = "Detecting...";
		m_detectButton.Enabled = false;
		// Run on a background thread so extracting a large 7z member does not
		// freeze the dialog.
		Thread thread = new Thread(() =>
		{
			string detected = DetectFirstTimestamp(checkedItems);
			BeginInvoke((MethodInvoker)delegate
			{
				m_detectButton.Enabled = true;
				if (detected == null)
				{
					m_statusLabel.Text = "No timestamp found.";
					MessageBox.Show(this, "Could not detect a timestamp in the selected sources.", "Open / Merge Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				m_statusLabel.Text = "";
				m_fromTextBox.Text = detected;
				DateTime? from = ParseTime(detected);
				if (from.HasValue)
				{
					m_toTextBox.Text = from.Value.AddMinutes(10).ToString("MM-dd HH:mm:ss.fff");
				}
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private string DetectFirstTimestamp(List<SourceItem> sources)
	{
		foreach (SourceItem item in sources)
		{
			TextReader reader;
			try
			{
				reader = OpenSource(item);
			}
			catch (Exception)
			{
				continue;
			}
			using (reader)
			{
				if (reader == null)
				{
					continue;
				}
				try
				{
					for (int i = 0; i < 100; i++)
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
				catch (Exception)
				{
				}
			}
		}
		return null;
	}

	// A lightweight, auto-dismissing toast notification shown near the source of
	// the action.
	private void ShowToast(string message)
	{
		Form toast = new Form
		{
			FormBorderStyle = FormBorderStyle.None,
			StartPosition = FormStartPosition.Manual,
			ShowInTaskbar = false,
			TopMost = true,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Padding = new Padding(14, 9, 14, 9),
			BackColor = Color.FromArgb(255, 45, 45, 48)
		};
		Label lbl = new Label
		{
			Text = message,
			ForeColor = Color.White,
			AutoSize = true,
			BackColor = Color.Transparent
		};
		toast.Controls.Add(lbl);
		Point screen = m_detectButton.PointToScreen(new Point(m_detectButton.Width / 2 - 40, m_detectButton.Height + 6));
		toast.Location = new Point(screen.X, screen.Y);
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 1800 };
		timer.Tick += (s, e) =>
		{
			timer.Stop();
			toast.Close();
		};
		timer.Start();
		toast.Show(this);
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
			// Show top-level entries first, then nested ones.
			members.Sort((a, b) =>
			{
				bool aTop = IsTopLevelMember(a);
				bool bTop = IsTopLevelMember(b);
				if (aTop != bTop)
				{
					return aTop ? -1 : 1;
				}
				return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
			});
			foreach (string member in members)
			{
				if (!IsTextMember(member))
				{
					continue;
				}
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
					DisplayName = member
				});
			}
		}
	}

	private static bool IsTopLevelMember(string member)
	{
		return member.IndexOf('/') < 0 && member.IndexOf('\\') < 0;
	}

	// Only archive members that look like text/log files are listed. Extension-less
	// entries are kept too (many log files have no extension).
	private static readonly HashSet<string> s_textExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".txt", ".log", ".xml", ".json", ".csv", ".conf", ".config", ".cfg", ".properties", ".ini", ".trace", ".out", ".err"
	};

	private static bool IsTextMember(string member)
	{
		string ext = Path.GetExtension(member);
		return string.IsNullOrEmpty(ext) || s_textExtensions.Contains(ext);
	}

	private void BuildUi()
	{
		int pad = 12;

		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 5,
			RowCount = 10,
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
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		// Row 0: Sources label + select-all checkbox
		Label listLabel = new Label { Text = "Sources:", AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
		root.Controls.Add(listLabel, 0, 0);
		m_selectAllCheck = new CheckBox
		{
			Text = "Select all",
			AutoSize = true,
			Margin = new Padding(8, 0, 0, 6)
		};
		m_selectAllCheck.CheckedChanged += (s, e) => SelectAllChanged();
		root.Controls.Add(m_selectAllCheck, 1, 0);

		// Row 1: source list
		m_sourceList = new CheckedListBox
		{
			Dock = DockStyle.Fill,
			CheckOnClick = true,
			IntegralHeight = false,
			HorizontalScrollbar = true,
			Margin = new Padding(0, 4, 0, 10)
		};
		foreach (SourceItem s in m_sources)
		{
			// Plain files and gzip are selected by default; archive members are
			// left unchecked so a large archive isn't fully selected on open.
			bool isChecked = s.ArchivePath == null;
			m_sourceList.Items.Add(s.DisplayName, isChecked);
		}
		int maxItemWidth = 0;
		foreach (SourceItem s in m_sources)
		{
			int w = TextRenderer.MeasureText(s.DisplayName, m_sourceList.Font).Width;
			if (w > maxItemWidth)
			{
				maxItemWidth = w;
			}
		}
		m_sourceList.HorizontalExtent = maxItemWidth + 20;
		m_sourceList.ItemCheck += (s, e) => BeginInvoke((MethodInvoker)delegate
		{
			UpdateDirectButtonEnabled();
			UpdateSelectAllState();
			RecomputeDefaultOutput();
		});
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

		// Row 4: timestamp checkbox
		m_timestampedCheck = new CheckBox
		{
			Text = "Only merge lines that have a timestamp",
			Checked = true,
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 2)
		};
		root.Controls.Add(m_timestampedCheck, 0, 4);
		root.SetColumnSpan(m_timestampedCheck, 5);

		// Row 5: open-after-merge checkbox
		m_openAfterMergeCheck = new CheckBox
		{
			Text = "Open merged file when done",
			Checked = true,
			AutoSize = true,
			Margin = new Padding(0, 2, 0, 4)
		};
		root.Controls.Add(m_openAfterMergeCheck, 0, 5);
		root.SetColumnSpan(m_openAfterMergeCheck, 5);

		// Row 6: Output (textbox spans the time box + button + output columns)
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
		m_outputTextBox.TextChanged += (s, e) =>
		{
			// Any manual edit (typing or Browse) stops the default-name updates.
			if (!m_updatingOutput)
			{
				m_outputManuallySet = true;
			}
		};
		root.Controls.Add(MakeLabel("Output:"), 0, 6);
		root.Controls.Add(m_outputTextBox, 1, 6);
		root.SetColumnSpan(m_outputTextBox, 3);
		root.Controls.Add(m_browseButton, 4, 6);

		// Row 7: progress
		m_progressBar = new ProgressBar
		{
			Dock = DockStyle.Fill,
			MinimumSize = new Size(0, 18),
			Minimum = 0,
			Maximum = 100,
			Margin = new Padding(0, 12, 0, 2),
			Visible = false
		};
		root.Controls.Add(m_progressBar, 0, 7);
		root.SetColumnSpan(m_progressBar, 5);

		// Row 8: status
		m_statusLabel = new Label { Text = "", AutoSize = true, Margin = new Padding(0, 2, 0, 4) };
		root.Controls.Add(m_statusLabel, 0, 8);
		root.SetColumnSpan(m_statusLabel, 5);

		// Row 9: buttons
		TableLayoutPanel buttonsPanel = BuildButtonsPanel();
		root.Controls.Add(buttonsPanel, 0, 9);
		root.SetColumnSpan(buttonsPanel, 5);

		Controls.Add(root);

		UpdateDirectButtonEnabled();
		UpdateSelectAllState();
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
			ColumnCount = 3,
			RowCount = 1,
			Margin = new Padding(0, 10, 0, 0)
		};
		p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

		m_directButton = MakeButton("Open Directly", DirectButton_Click);
		m_mergeButton = MakeButton("Merge", MergeButton_Click);

		p.Controls.Add(m_directButton, 1, 0);
		p.Controls.Add(m_mergeButton, 2, 0);
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
		m_directButton.Enabled = GetCheckedSources().Count > 0;
	}

	private void SelectAllChanged()
	{
		if (m_updatingChecks)
		{
			return;
		}
		m_updatingChecks = true;
		bool check = m_selectAllCheck.Checked;
		for (int i = 0; i < m_sourceList.Items.Count; i++)
		{
			m_sourceList.SetItemChecked(i, check);
		}
		m_updatingChecks = false;
	}

	private void UpdateSelectAllState()
	{
		if (m_updatingChecks)
		{
			return;
		}
		m_updatingChecks = true;
		int total = m_sourceList.Items.Count;
		int checkedCount = 0;
		for (int i = 0; i < total; i++)
		{
			if (m_sourceList.GetItemChecked(i))
			{
				checkedCount++;
			}
		}
		CheckState state;
		if (checkedCount == 0)
		{
			state = CheckState.Unchecked;
		}
		else if (checkedCount == total)
		{
			state = CheckState.Checked;
		}
		else
		{
			state = CheckState.Indeterminate;
		}
		m_selectAllCheck.CheckState = state;
		m_updatingChecks = false;
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
		if (checkedItems.Count == 0)
		{
			return;
		}
		foreach (SourceItem item in checkedItems)
		{
			string path = GetOpenablePath(item);
			if (path == null)
			{
				continue;
			}
			try
			{
				// Open each checked source in a separate new program instance.
				ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath)
				{
					Arguments = "\"" + path + "\"",
					UseShellExecute = true
				};
				Process.Start(psi);
			}
			catch (Exception)
			{
			}
		}
		CleanupTempFiles();
		DialogResult = DialogResult.OK;
		Close();
	}

	// Returns a path that can be handed to a new program instance and will still
	// exist after this dialog closes (plain files use their own path; archives
	// and gzip are extracted/decompressed to a persistent temp location).
	private string GetOpenablePath(SourceItem item)
	{
		if (item.ArchivePath != null)
		{
			try
			{
				string dir = Path.Combine(Path.GetTempPath(), "TAT-Open-" + Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(dir);
				return ArchiveSupport.ExtractMemberToFile(item.ArchivePath, item.MemberName, dir);
			}
			catch (Exception)
			{
				return null;
			}
		}
		if (item.IsGzip)
		{
			try
			{
				string dest = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(item.FilePath) + ".txt");
				using (TextReader input = ArchiveSupport.OpenGzip(item.FilePath))
				using (StreamWriter writer = new StreamWriter(dest, false, new UTF8Encoding(true)))
				{
					string line;
					while ((line = input.ReadLine()) != null)
					{
						writer.WriteLine(line);
					}
				}
				return dest;
			}
			catch (Exception)
			{
				return null;
			}
		}
		return item.FilePath;
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
				CleanupTempFiles();
				if (m_openAfterMergeCheck.Checked)
				{
					MergedOutputPath = options.OutputPath;
					DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show(this, "Merge completed successfully.\n\n" + options.OutputPath, "Merge Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
					Application.Exit();
				}
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
		m_mergeButton.Text = running ? "Cancel" : "Merge";
		m_directButton.Enabled = !running;
		m_browseButton.Enabled = !running;
		m_fromTextBox.Enabled = !running;
		m_toTextBox.Enabled = !running;
		m_timestampedCheck.Enabled = !running;
		m_openAfterMergeCheck.Enabled = !running;
		m_outputTextBox.Enabled = !running;
		m_progressBar.Visible = running;
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

	// Removes leftover "Open Directly" temp directories (TAT-Open-*) from
	// previous runs. Runs once per process, before any new ones are created, so
	// it never deletes a file a just-launched new instance still needs.
	private static void EnsureStaleOpenDirCleanup()
	{
		if (s_staleOpenCleanupDone)
		{
			return;
		}
		s_staleOpenCleanupDone = true;
		try
		{
			foreach (string dir in Directory.GetDirectories(Path.GetTempPath(), "TAT-Open-*"))
			{
				try
				{
					Directory.Delete(dir, recursive: true);
				}
				catch (IOException)
				{
				}
			}
		}
		catch (Exception)
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