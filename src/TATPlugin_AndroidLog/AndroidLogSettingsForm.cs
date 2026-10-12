using System;
using System.Drawing;
using System.Windows.Forms;

namespace TATPlugin_AndroidLog;

internal sealed class AndroidLogSettingsForm : Form
{
	private readonly AndroidLogSettings m_originalSettings;

	private readonly AndroidLogSettings m_settings;

	// Tag controls
	private RadioButton m_rbTagDefault;

	private RadioButton m_rbTagRandom;

	private RadioButton m_rbTagSpecified;

	private Panel m_pnlTagForeColor;

	private Button m_btnTagForeColor;

	private Panel m_pnlTagBackColor;

	private Button m_btnTagBackColor;

	private Button m_btnTagClearBack;

	private Label m_lblTagPreview;

	// PID controls
	private RadioButton m_rbPidDefault;

	private RadioButton m_rbPidRandom;

	private RadioButton m_rbPidSpecified;

	private Panel m_pnlPidForeColor;

	private Button m_btnPidForeColor;

	private Panel m_pnlPidBackColor;

	private Button m_btnPidBackColor;

	private Button m_btnPidClearBack;

	private Label m_lblPidPreview;

	// Random options
	private CheckBox m_chkConsistent;

	private Panel m_pnlPalette;

	private bool m_updatingUi;

	public AndroidLogSettingsForm(AndroidLogSettings settings)
	{
		m_originalSettings = settings ?? throw new ArgumentNullException(nameof(settings));
		m_settings = m_originalSettings.Clone();

		Text = "Android Log Plug-in Settings";
		StartPosition = FormStartPosition.CenterParent;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		ClientSize = new Size(540, 560);
		Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		AutoScaleMode = AutoScaleMode.Dpi;

		BuildUi();
		UpdateUiFromSettings();
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 4,
			ColumnCount = 1,
			Padding = new Padding(12),
			Margin = Padding.Empty
		};
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

		// 1. Tag GroupBox
		GroupBox grpTag = BuildTagGroupBox();
		root.Controls.Add(grpTag, 0, 0);

		// 2. PID GroupBox
		GroupBox grpPid = BuildPidGroupBox();
		root.Controls.Add(grpPid, 0, 1);

		// 3. Random Options GroupBox
		GroupBox grpRandom = BuildRandomOptionsGroupBox();
		root.Controls.Add(grpRandom, 0, 2);

		// 4. Bottom Buttons Panel
		Panel pnlBottom = BuildBottomPanel();
		root.Controls.Add(pnlBottom, 0, 3);

		Controls.Add(root);
	}

	private GroupBox BuildTagGroupBox()
	{
		GroupBox grp = new GroupBox
		{
			Text = "Tag Filter Color (Tag 过滤器颜色)",
			Dock = DockStyle.Top,
			AutoSize = true,
			Padding = new Padding(10),
			Margin = new Padding(0, 0, 0, 10)
		};

		TableLayoutPanel panel = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			ColumnCount = 1,
			RowCount = 5
		};

		m_rbTagDefault = new RadioButton { Text = "&Default (No color / System text)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbTagRandom = new RadioButton { Text = "&Random Color (Auto-distribute distinguishable colors)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbTagSpecified = new RadioButton { Text = "&Specified Color:", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };

		m_rbTagDefault.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagDefault.Checked) { m_settings.TagColorMode = FilterColorMode.Default; UpdateUiFromSettings(); } };
		m_rbTagRandom.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagRandom.Checked) { m_settings.TagColorMode = FilterColorMode.Random; UpdateUiFromSettings(); } };
		m_rbTagSpecified.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagSpecified.Checked) { m_settings.TagColorMode = FilterColorMode.Specified; UpdateUiFromSettings(); } };

		FlowLayoutPanel pnlColors = new FlowLayoutPanel
		{
			AutoSize = true,
			WrapContents = false,
			Margin = new Padding(24, 2, 3, 4)
		};

		Label lblFore = new Label { Text = "Text:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 5, 4, 0) };
		m_pnlTagForeColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Margin = new Padding(0, 2, 4, 0) };
		m_pnlTagForeColor.Click += (s, e) => ChooseTagForeColor();
		m_btnTagForeColor = new Button { Text = "Choose...", AutoSize = true, Margin = new Padding(0, 1, 16, 0) };
		m_btnTagForeColor.Click += (s, e) => ChooseTagForeColor();

		Label lblBack = new Label { Text = "Background:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 5, 4, 0) };
		m_pnlTagBackColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Margin = new Padding(0, 2, 4, 0) };
		m_pnlTagBackColor.Click += (s, e) => ChooseTagBackColor();
		m_btnTagBackColor = new Button { Text = "Choose...", AutoSize = true, Margin = new Padding(0, 1, 4, 0) };
		m_btnTagBackColor.Click += (s, e) => ChooseTagBackColor();
		m_btnTagClearBack = new Button { Text = "Clear", AutoSize = true, Margin = new Padding(0, 1, 0, 0) };
		m_btnTagClearBack.Click += (s, e) => { m_settings.TagBackColor = Color.Empty; UpdateUiFromSettings(); };

		pnlColors.Controls.AddRange(new Control[] { lblFore, m_pnlTagForeColor, m_btnTagForeColor, lblBack, m_pnlTagBackColor, m_btnTagBackColor, m_btnTagClearBack });

		m_lblTagPreview = new Label
		{
			Text = "  Preview: 10-12 10:20:30.123 1234 1234 D ActivityManager: Start proc",
			Dock = DockStyle.Top,
			Height = 26,
			BorderStyle = BorderStyle.FixedSingle,
			TextAlign = ContentAlignment.MiddleLeft,
			Margin = new Padding(4, 4, 4, 4)
		};

		panel.Controls.Add(m_rbTagDefault, 0, 0);
		panel.Controls.Add(m_rbTagRandom, 0, 1);
		panel.Controls.Add(m_rbTagSpecified, 0, 2);
		panel.Controls.Add(pnlColors, 0, 3);
		panel.Controls.Add(m_lblTagPreview, 0, 4);

		grp.Controls.Add(panel);
		return grp;
	}

	private GroupBox BuildPidGroupBox()
	{
		GroupBox grp = new GroupBox
		{
			Text = "PID Filter Color (PID 过滤器颜色)",
			Dock = DockStyle.Top,
			AutoSize = true,
			Padding = new Padding(10),
			Margin = new Padding(0, 0, 0, 10)
		};

		TableLayoutPanel panel = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			ColumnCount = 1,
			RowCount = 5
		};

		m_rbPidDefault = new RadioButton { Text = "D&efault (No color / System text)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbPidRandom = new RadioButton { Text = "R&andom Color (Auto-distribute distinguishable colors)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbPidSpecified = new RadioButton { Text = "S&pecified Color:", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };

		m_rbPidDefault.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidDefault.Checked) { m_settings.PidColorMode = FilterColorMode.Default; UpdateUiFromSettings(); } };
		m_rbPidRandom.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidRandom.Checked) { m_settings.PidColorMode = FilterColorMode.Random; UpdateUiFromSettings(); } };
		m_rbPidSpecified.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidSpecified.Checked) { m_settings.PidColorMode = FilterColorMode.Specified; UpdateUiFromSettings(); } };

		FlowLayoutPanel pnlColors = new FlowLayoutPanel
		{
			AutoSize = true,
			WrapContents = false,
			Margin = new Padding(24, 2, 3, 4)
		};

		Label lblFore = new Label { Text = "Text:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 5, 4, 0) };
		m_pnlPidForeColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Margin = new Padding(0, 2, 4, 0) };
		m_pnlPidForeColor.Click += (s, e) => ChoosePidForeColor();
		m_btnPidForeColor = new Button { Text = "Choose...", AutoSize = true, Margin = new Padding(0, 1, 16, 0) };
		m_btnPidForeColor.Click += (s, e) => ChoosePidForeColor();

		Label lblBack = new Label { Text = "Background:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 5, 4, 0) };
		m_pnlPidBackColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Margin = new Padding(0, 2, 4, 0) };
		m_pnlPidBackColor.Click += (s, e) => ChoosePidBackColor();
		m_btnPidBackColor = new Button { Text = "Choose...", AutoSize = true, Margin = new Padding(0, 1, 4, 0) };
		m_btnPidBackColor.Click += (s, e) => ChoosePidBackColor();
		m_btnPidClearBack = new Button { Text = "Clear", AutoSize = true, Margin = new Padding(0, 1, 0, 0) };
		m_btnPidClearBack.Click += (s, e) => { m_settings.PidBackColor = Color.Empty; UpdateUiFromSettings(); };

		pnlColors.Controls.AddRange(new Control[] { lblFore, m_pnlPidForeColor, m_btnPidForeColor, lblBack, m_pnlPidBackColor, m_btnPidBackColor, m_btnPidClearBack });

		m_lblPidPreview = new Label
		{
			Text = "  Preview: 10-12 10:20:30.123 1234 1234 D ActivityManager: Start proc",
			Dock = DockStyle.Top,
			Height = 26,
			BorderStyle = BorderStyle.FixedSingle,
			TextAlign = ContentAlignment.MiddleLeft,
			Margin = new Padding(4, 4, 4, 4)
		};

		panel.Controls.Add(m_rbPidDefault, 0, 0);
		panel.Controls.Add(m_rbPidRandom, 0, 1);
		panel.Controls.Add(m_rbPidSpecified, 0, 2);
		panel.Controls.Add(pnlColors, 0, 3);
		panel.Controls.Add(m_lblPidPreview, 0, 4);

		grp.Controls.Add(panel);
		return grp;
	}

	private GroupBox BuildRandomOptionsGroupBox()
	{
		GroupBox grp = new GroupBox
		{
			Text = "Random Color Palette & Options (随机调色板与策略)",
			Dock = DockStyle.Top,
			AutoSize = true,
			Padding = new Padding(10),
			Margin = new Padding(0, 0, 0, 10)
		};

		TableLayoutPanel panel = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			ColumnCount = 1,
			RowCount = 2
		};

		m_chkConsistent = new CheckBox
		{
			Text = "&Consistent color for same Tag / PID (同一 Tag 或 PID 保持固定颜色)",
			AutoSize = true,
			Margin = new Padding(3, 2, 3, 6)
		};
		m_chkConsistent.CheckedChanged += (s, e) =>
		{
			if (!m_updatingUi)
			{
				m_settings.ConsistentRandomColor = m_chkConsistent.Checked;
			}
		};

		m_pnlPalette = new Panel
		{
			Dock = DockStyle.Top,
			Height = 22,
			Margin = new Padding(3, 0, 3, 2)
		};
		m_pnlPalette.Paint += (s, e) =>
		{
			Graphics g = e.Graphics;
			int x = 0;
			int size = 18;
			int gap = 3;
			foreach (Color c in AndroidLogSettings.HighContrastPalette)
			{
				using (Brush b = new SolidBrush(c))
				{
					g.FillRectangle(b, x, 2, size, size);
				}
				using (Pen p = new Pen(Color.FromArgb(120, 120, 120)))
				{
					g.DrawRectangle(p, x, 2, size - 1, size - 1);
				}
				x += size + gap;
			}
		};

		panel.Controls.Add(m_chkConsistent, 0, 0);
		panel.Controls.Add(m_pnlPalette, 0, 1);

		grp.Controls.Add(panel);
		return grp;
	}

	private Panel BuildBottomPanel()
	{
		Panel pnl = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 40,
			Margin = Padding.Empty
		};

		Button btnReset = new Button
		{
			Text = "&Reset to Defaults",
			AutoSize = true,
			Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
			Location = new Point(0, 8),
			Height = 28
		};
		btnReset.Click += (s, e) =>
		{
			m_settings.Reset();
			UpdateUiFromSettings();
		};

		Button btnCancel = new Button
		{
			Text = "Cancel",
			DialogResult = DialogResult.Cancel,
			Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
			Location = new Point(430, 8),
			Size = new Size(80, 28)
		};
		btnCancel.Click += (s, e) => Close();

		Button btnOK = new Button
		{
			Text = "OK",
			DialogResult = DialogResult.OK,
			Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
			Location = new Point(342, 8),
			Size = new Size(80, 28)
		};
		btnOK.Click += (s, e) =>
		{
			m_originalSettings.Apply(m_settings);
			m_originalSettings.Save();
			Close();
		};

		AcceptButton = btnOK;
		CancelButton = btnCancel;

		pnl.Controls.AddRange(new Control[] { btnReset, btnOK, btnCancel });
		return pnl;
	}

	private void UpdateUiFromSettings()
	{
		m_updatingUi = true;
		try
		{
			m_rbTagDefault.Checked = m_settings.TagColorMode == FilterColorMode.Default;
			m_rbTagRandom.Checked = m_settings.TagColorMode == FilterColorMode.Random;
			m_rbTagSpecified.Checked = m_settings.TagColorMode == FilterColorMode.Specified;

			bool tagSpecified = m_settings.TagColorMode == FilterColorMode.Specified;
			m_pnlTagForeColor.Enabled = tagSpecified;
			m_btnTagForeColor.Enabled = tagSpecified;
			m_pnlTagBackColor.Enabled = tagSpecified;
			m_btnTagBackColor.Enabled = tagSpecified;
			m_btnTagClearBack.Enabled = tagSpecified && m_settings.TagBackColor != Color.Empty;

			m_pnlTagForeColor.BackColor = m_settings.TagForeColor;
			m_pnlTagBackColor.BackColor = (m_settings.TagBackColor == Color.Empty) ? SystemColors.Window : m_settings.TagBackColor;

			UpdateTagPreview();

			m_rbPidDefault.Checked = m_settings.PidColorMode == FilterColorMode.Default;
			m_rbPidRandom.Checked = m_settings.PidColorMode == FilterColorMode.Random;
			m_rbPidSpecified.Checked = m_settings.PidColorMode == FilterColorMode.Specified;

			bool pidSpecified = m_settings.PidColorMode == FilterColorMode.Specified;
			m_pnlPidForeColor.Enabled = pidSpecified;
			m_btnPidForeColor.Enabled = pidSpecified;
			m_pnlPidBackColor.Enabled = pidSpecified;
			m_btnPidBackColor.Enabled = pidSpecified;
			m_btnPidClearBack.Enabled = pidSpecified && m_settings.PidBackColor != Color.Empty;

			m_pnlPidForeColor.BackColor = m_settings.PidForeColor;
			m_pnlPidBackColor.BackColor = (m_settings.PidBackColor == Color.Empty) ? SystemColors.Window : m_settings.PidBackColor;

			UpdatePidPreview();

			m_chkConsistent.Checked = m_settings.ConsistentRandomColor;
		}
		finally
		{
			m_updatingUi = false;
		}
	}

	private void UpdateTagPreview()
	{
		Color fore;
		Color back;
		switch (m_settings.TagColorMode)
		{
		case FilterColorMode.Default:
			fore = SystemColors.WindowText;
			back = SystemColors.Window;
			break;
		case FilterColorMode.Specified:
			fore = m_settings.TagForeColor;
			back = (m_settings.TagBackColor == Color.Empty) ? SystemColors.Window : m_settings.TagBackColor;
			break;
		case FilterColorMode.Random:
			fore = m_settings.GetColorForTag("ActivityManager");
			back = SystemColors.Window;
			break;
		default:
			fore = SystemColors.WindowText;
			back = SystemColors.Window;
			break;
		}
		m_lblTagPreview.ForeColor = fore;
		m_lblTagPreview.BackColor = back;
	}

	private void UpdatePidPreview()
	{
		Color fore;
		Color back;
		switch (m_settings.PidColorMode)
		{
		case FilterColorMode.Default:
			fore = SystemColors.WindowText;
			back = SystemColors.Window;
			break;
		case FilterColorMode.Specified:
			fore = m_settings.PidForeColor;
			back = (m_settings.PidBackColor == Color.Empty) ? SystemColors.Window : m_settings.PidBackColor;
			break;
		case FilterColorMode.Random:
			fore = m_settings.GetColorForPid(1234);
			back = SystemColors.Window;
			break;
		default:
			fore = SystemColors.WindowText;
			back = SystemColors.Window;
			break;
		}
		m_lblPidPreview.ForeColor = fore;
		m_lblPidPreview.BackColor = back;
	}

	private void ChooseTagForeColor()
	{
		using ColorDialog dlg = new ColorDialog();
		dlg.Color = m_settings.TagForeColor;
		dlg.FullOpen = true;
		if (dlg.ShowDialog(this) == DialogResult.OK)
		{
			m_settings.TagForeColor = dlg.Color;
			m_settings.TagColorMode = FilterColorMode.Specified;
			UpdateUiFromSettings();
		}
	}

	private void ChooseTagBackColor()
	{
		using ColorDialog dlg = new ColorDialog();
		dlg.Color = m_settings.TagBackColor != Color.Empty ? m_settings.TagBackColor : Color.White;
		dlg.FullOpen = true;
		if (dlg.ShowDialog(this) == DialogResult.OK)
		{
			m_settings.TagBackColor = dlg.Color;
			m_settings.TagColorMode = FilterColorMode.Specified;
			UpdateUiFromSettings();
		}
	}

	private void ChoosePidForeColor()
	{
		using ColorDialog dlg = new ColorDialog();
		dlg.Color = m_settings.PidForeColor;
		dlg.FullOpen = true;
		if (dlg.ShowDialog(this) == DialogResult.OK)
		{
			m_settings.PidForeColor = dlg.Color;
			m_settings.PidColorMode = FilterColorMode.Specified;
			UpdateUiFromSettings();
		}
	}

	private void ChoosePidBackColor()
	{
		using ColorDialog dlg = new ColorDialog();
		dlg.Color = m_settings.PidBackColor != Color.Empty ? m_settings.PidBackColor : Color.White;
		dlg.FullOpen = true;
		if (dlg.ShowDialog(this) == DialogResult.OK)
		{
			m_settings.PidBackColor = dlg.Color;
			m_settings.PidColorMode = FilterColorMode.Specified;
			UpdateUiFromSettings();
		}
	}
}
