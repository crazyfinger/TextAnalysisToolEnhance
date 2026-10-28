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
		ClientSize = new Size(700, 660);
		Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		AutoScaleMode = AutoScaleMode.None;

		BuildUi();
		UpdateUiFromSettings();
	}

	private void BuildUi()
	{
		Panel pnlBottom = BuildBottomPanel();

		Panel pnlContent = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = false,
			Padding = new Padding(16, 12, 16, 12),
			Margin = Padding.Empty
		};

		TableLayoutPanel contentLayout = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			RowCount = 2,
			ColumnCount = 1,
			Margin = Padding.Empty,
			Padding = Padding.Empty
		};
		contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		// 1. Tag GroupBox
		GroupBox grpTag = BuildTagGroupBox();
		contentLayout.Controls.Add(grpTag, 0, 0);

		// 2. PID GroupBox
		GroupBox grpPid = BuildPidGroupBox();
		contentLayout.Controls.Add(grpPid, 0, 1);

		pnlContent.Controls.Add(contentLayout);

		Controls.Add(pnlContent);
		Controls.Add(pnlBottom);
		pnlBottom.SendToBack();
	}

	private GroupBox BuildTagGroupBox()
	{
		GroupBox grp = new GroupBox
		{
			Text = "Tag Filter Color",
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
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

		m_rbTagDefault = new RadioButton { Text = "&Default (No color / System text)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbTagRandom = new RadioButton { Text = "&Random Color (Auto-distribute distinguishable colors)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbTagSpecified = new RadioButton { Text = "&Specified Color:", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };

		m_rbTagDefault.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagDefault.Checked) { m_settings.TagColorMode = FilterColorMode.Default; UpdateUiFromSettings(); } };
		m_rbTagRandom.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagRandom.Checked) { m_settings.TagColorMode = FilterColorMode.Random; UpdateUiFromSettings(); } };
		m_rbTagSpecified.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbTagSpecified.Checked) { m_settings.TagColorMode = FilterColorMode.Specified; UpdateUiFromSettings(); } };

		TableLayoutPanel pnlColors = new TableLayoutPanel
		{
			AutoSize = true,
			ColumnCount = 4,
			RowCount = 2,
			Margin = new Padding(22, 2, 3, 6),
			Padding = Padding.Empty
		};
		pnlColors.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
		pnlColors.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

		Label lblFore = new Label { Text = "Text color:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 4, 0) };
		m_pnlTagForeColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_pnlTagForeColor.Click += (s, e) => ChooseTagForeColor();
		m_btnTagForeColor = new Button { Text = "Choose...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_btnTagForeColor.Click += (s, e) => ChooseTagForeColor();

		Label lblBack = new Label { Text = "Background:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 4, 0) };
		m_pnlTagBackColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_pnlTagBackColor.Click += (s, e) => ChooseTagBackColor();
		m_btnTagBackColor = new Button { Text = "Choose...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_btnTagBackColor.Click += (s, e) => ChooseTagBackColor();
		m_btnTagClearBack = new Button { Text = "Clear", AutoSize = true, Anchor = AnchorStyles.Left, Margin = Padding.Empty };
		m_btnTagClearBack.Click += (s, e) => { m_settings.TagBackColor = Color.Empty; UpdateUiFromSettings(); };

		pnlColors.Controls.Add(lblFore, 0, 0);
		pnlColors.Controls.Add(m_pnlTagForeColor, 1, 0);
		pnlColors.Controls.Add(m_btnTagForeColor, 2, 0);

		pnlColors.Controls.Add(lblBack, 0, 1);
		pnlColors.Controls.Add(m_pnlTagBackColor, 1, 1);
		pnlColors.Controls.Add(m_btnTagBackColor, 2, 1);
		pnlColors.Controls.Add(m_btnTagClearBack, 3, 1);

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
			Text = "PID Filter Color",
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
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

		m_rbPidDefault = new RadioButton { Text = "D&efault (No color / System text)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbPidRandom = new RadioButton { Text = "R&andom Color (Auto-distribute distinguishable colors)", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };
		m_rbPidSpecified = new RadioButton { Text = "S&pecified Color:", AutoSize = true, Margin = new Padding(3, 2, 3, 2) };

		m_rbPidDefault.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidDefault.Checked) { m_settings.PidColorMode = FilterColorMode.Default; UpdateUiFromSettings(); } };
		m_rbPidRandom.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidRandom.Checked) { m_settings.PidColorMode = FilterColorMode.Random; UpdateUiFromSettings(); } };
		m_rbPidSpecified.CheckedChanged += (s, e) => { if (!m_updatingUi && m_rbPidSpecified.Checked) { m_settings.PidColorMode = FilterColorMode.Specified; UpdateUiFromSettings(); } };

		TableLayoutPanel pnlColors = new TableLayoutPanel
		{
			AutoSize = true,
			ColumnCount = 4,
			RowCount = 2,
			Margin = new Padding(22, 2, 3, 6),
			Padding = Padding.Empty
		};
		pnlColors.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
		pnlColors.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		pnlColors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

		Label lblFore = new Label { Text = "Text color:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 4, 0) };
		m_pnlPidForeColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_pnlPidForeColor.Click += (s, e) => ChoosePidForeColor();
		m_btnPidForeColor = new Button { Text = "Choose...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_btnPidForeColor.Click += (s, e) => ChoosePidForeColor();

		Label lblBack = new Label { Text = "Background:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 4, 0) };
		m_pnlPidBackColor = new Panel { Size = new Size(32, 22), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_pnlPidBackColor.Click += (s, e) => ChoosePidBackColor();
		m_btnPidBackColor = new Button { Text = "Choose...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
		m_btnPidBackColor.Click += (s, e) => ChoosePidBackColor();
		m_btnPidClearBack = new Button { Text = "Clear", AutoSize = true, Anchor = AnchorStyles.Left, Margin = Padding.Empty };
		m_btnPidClearBack.Click += (s, e) => { m_settings.PidBackColor = Color.Empty; UpdateUiFromSettings(); };

		pnlColors.Controls.Add(lblFore, 0, 0);
		pnlColors.Controls.Add(m_pnlPidForeColor, 1, 0);
		pnlColors.Controls.Add(m_btnPidForeColor, 2, 0);

		pnlColors.Controls.Add(lblBack, 0, 1);
		pnlColors.Controls.Add(m_pnlPidBackColor, 1, 1);
		pnlColors.Controls.Add(m_btnPidBackColor, 2, 1);
		pnlColors.Controls.Add(m_btnPidClearBack, 3, 1);

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

	private Panel BuildBottomPanel()
	{
		Panel pnlBottom = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 64,
			BackColor = SystemColors.Control,
			Padding = new Padding(20, 12, 20, 14),
			Margin = Padding.Empty
		};

		Button btnReset = new Button
		{
			Text = "&Reset",
			Size = new Size(120, 36),
			Dock = DockStyle.Left
		};
		btnReset.Click += (s, e) =>
		{
			m_settings.Reset();
			UpdateUiFromSettings();
		};

		FlowLayoutPanel rightFlow = new FlowLayoutPanel
		{
			Dock = DockStyle.Right,
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = Padding.Empty,
			Padding = Padding.Empty
		};

		Button btnOK = new Button
		{
			Text = "OK",
			DialogResult = DialogResult.OK,
			Size = new Size(120, 36),
			Margin = new Padding(0, 0, 10, 0)
		};
		btnOK.Click += (s, e) =>
		{
			m_originalSettings.Apply(m_settings);
			m_originalSettings.Save();
			Close();
		};

		Button btnCancel = new Button
		{
			Text = "Cancel",
			DialogResult = DialogResult.Cancel,
			Size = new Size(120, 36),
			Margin = Padding.Empty
		};
		btnCancel.Click += (s, e) => Close();

		rightFlow.Controls.Add(btnOK);
		rightFlow.Controls.Add(btnCancel);

		AcceptButton = btnOK;
		CancelButton = btnCancel;

		pnlBottom.Controls.Add(btnReset);
		pnlBottom.Controls.Add(rightFlow);
		return pnlBottom;
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
