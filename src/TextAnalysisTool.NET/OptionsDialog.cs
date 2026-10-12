using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal class OptionsDialog : Form
{
	private PropertyGrid m_propertyGrid;

	private Button m_okButton;

	private Button m_cancelButton;

	private Button m_applyButton;

	private ButtonDropDownMenu m_moreButton;

	private int m_importFilterIndex;

	private int m_exportFilterIndex;

	private OptionsBehaviorProperties m_lastAppliedOptions;

	private Action<OptionsBehaviorProperties> m_updateSettingsInUI;

	public OptionsBehaviorProperties ConfiguredOptions => (OptionsBehaviorProperties)m_propertyGrid.SelectedObject;

	public OptionsDialog(OptionsBehaviorProperties propertiesObject, int importFilterIndex, int exportFilterIndex, Action<OptionsBehaviorProperties> updateSettingsInUI)
	{
		base.Icon = Resource.ApplicationIcon;
		m_importFilterIndex = importFilterIndex;
		m_exportFilterIndex = exportFilterIndex;
		m_updateSettingsInUI = updateSettingsInUI;
		m_lastAppliedOptions = propertiesObject.Clone();
		InitializeComponent();
		FillPropertyGrid(propertiesObject);
	}

	private void InitializeComponent()
	{
		TextAnalysisTool.NET.FullyEnabledMenuItem fullyEnabledMenuItem = new TextAnalysisTool.NET.FullyEnabledMenuItem("&Import...", null, new System.EventHandler(Import_Click));
		TextAnalysisTool.NET.FullyEnabledMenuItem fullyEnabledMenuItem2 = new TextAnalysisTool.NET.FullyEnabledMenuItem("&Export...", null, new System.EventHandler(Export_Click));
		TextAnalysisTool.NET.FullyEnabledMenuItem fullyEnabledMenuItemSendTo = new TextAnalysisTool.NET.FullyEnabledMenuItem(
			ShellShortcut.IsInSendTo() ? "Remove from Windows \"Send To\" Menu" : "Add to Windows \"Send To\" Menu",
			null,
			new System.EventHandler(SendTo_Click));
		TextAnalysisTool.NET.FullyEnabledMenuItem fullyEnabledMenuItem3 = new TextAnalysisTool.NET.FullyEnabledMenuItem("Restore &Defaults", null, new System.EventHandler(Defaults_Click));
		System.Windows.Forms.ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
		contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[6]
		{
			fullyEnabledMenuItem,
			fullyEnabledMenuItem2,
			new System.Windows.Forms.ToolStripSeparator(),
			fullyEnabledMenuItemSendTo,
			new System.Windows.Forms.ToolStripSeparator(),
			fullyEnabledMenuItem3
		});
		contextMenuStrip.Opening += (s, e) =>
		{
			fullyEnabledMenuItemSendTo.Text = ShellShortcut.IsInSendTo() ? "Remove from Windows \"Send To\" Menu" : "Add to Windows \"Send To\" Menu";
		};
		this.m_propertyGrid = new System.Windows.Forms.PropertyGrid();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_applyButton = new System.Windows.Forms.Button();
		this.m_moreButton = new TextAnalysisTool.NET.ButtonDropDownMenu(contextMenuStrip);
		base.SuspendLayout();
		this.m_propertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_propertyGrid.Location = new System.Drawing.Point(12, 12);
		this.m_propertyGrid.Name = "m_propertyGrid";
		this.m_propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
		this.m_propertyGrid.Size = new System.Drawing.Size(360, 360);
		this.m_propertyGrid.TabIndex = 2;
		this.m_propertyGrid.ToolbarVisible = false;
		this.m_propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(PropertyGrid_PropertyValueChanged);
		this.m_okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_okButton.Location = new System.Drawing.Point(136, 388);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 6;
		this.m_okButton.Text = "&OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_moreButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.m_moreButton.Location = new System.Drawing.Point(12, 388);
		this.m_moreButton.Name = "m_moreButton";
		this.m_moreButton.Size = new System.Drawing.Size(75, 23);
		this.m_moreButton.TabIndex = 5;
		this.m_moreButton.Text = "More >>";
		this.m_moreButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(217, 388);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 0;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_applyButton.Enabled = false;
		this.m_applyButton.Location = new System.Drawing.Point(298, 388);
		this.m_applyButton.Name = "m_applyButton";
		this.m_applyButton.Size = new System.Drawing.Size(75, 23);
		this.m_applyButton.TabIndex = 1;
		this.m_applyButton.Text = "&Apply";
		this.m_applyButton.UseVisualStyleBackColor = true;
		this.m_applyButton.Click += new System.EventHandler(Apply_Click);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(386, 423);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_moreButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_applyButton);
		base.Controls.Add(this.m_propertyGrid);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(402, 441);
		base.Name = "OptionsDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Preferences";
		base.ResumeLayout(false);
	}

	private void FillPropertyGrid(object propertiesObject)
	{
		m_propertyGrid.SelectedObject = propertiesObject;
	}

	private void ApplyOptions()
	{
		m_updateSettingsInUI(ConfiguredOptions);
		m_lastAppliedOptions = ConfiguredOptions.Clone();
		m_applyButton.Enabled = false;
		Settings.Save();
	}

	private void Defaults_Click(object sender, EventArgs e)
	{
		FillPropertyGrid(Settings.ToDefaultOptions());
		if (!ConfiguredOptions.Equals(m_lastAppliedOptions))
		{
			m_applyButton.Enabled = true;
		}
	}

	private void Apply_Click(object sender, EventArgs e)
	{
		ApplyOptions();
	}

	private void Import_Click(object sender, EventArgs e)
	{
		if (!m_lastAppliedOptions.Equals(ConfiguredOptions) && CTextAnalysisTool.ShowMessageBox("The settings have been changed.\nDo you want to overwrite your changes?", MessageBoxIcon.Question, MessageBoxButtons.YesNo) == DialogResult.No)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "XML Files (*.xml)|*.xml;|All Files (*)|*";
		openFileDialog.FilterIndex = m_importFilterIndex;
		openFileDialog.InitialDirectory = Settings.LastFileDirectory;
		openFileDialog.Title = "Import";
		if (DialogResult.OK == openFileDialog.ShowDialog(this))
		{
			OptionsBehaviorProperties newOptions = new OptionsBehaviorProperties();
			m_importFilterIndex = openFileDialog.FilterIndex;
			Settings.LastFileDirectory = new FileInfo(openFileDialog.FileName).DirectoryName;
			if (Settings.ImportConfiguration(openFileDialog.FileName, out newOptions))
			{
				FillPropertyGrid(newOptions);
			}
		}
	}

	private void Export_Click(object sender, EventArgs e)
	{
		if (!m_lastAppliedOptions.Equals(ConfiguredOptions))
		{
			switch (CTextAnalysisTool.ShowMessageBox("The settings have been changed.\nDo you wish to apply the settings before exporting them?", MessageBoxIcon.Question, MessageBoxButtons.YesNoCancel))
			{
			case DialogResult.Yes:
				ApplyOptions();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "XML Files (*.xml)|*.xml;|All Files (*)|*";
		saveFileDialog.FilterIndex = m_exportFilterIndex;
		saveFileDialog.InitialDirectory = Settings.LastFileDirectory;
		saveFileDialog.Title = "Export";
		if (DialogResult.OK == saveFileDialog.ShowDialog(this))
		{
			m_exportFilterIndex = saveFileDialog.FilterIndex;
			Settings.LastFileDirectory = new FileInfo(saveFileDialog.FileName).DirectoryName;
			Settings.ExportConfiguration(saveFileDialog.FileName, ConfiguredOptions);
		}
	}

	private void SendTo_Click(object sender, EventArgs e)
	{
		string appName = Resource.ApplicationName;
		if (ShellShortcut.IsInSendTo())
		{
			if (ShellShortcut.RemoveFromSendTo())
			{
				CTextAnalysisTool.ShowMessageBox($"{appName} has been removed from your Windows 'Send to' menu.", MessageBoxIcon.Information);
			}
			else
			{
				CTextAnalysisTool.ShowMessageBox($"Failed to remove {appName} from the 'Send to' menu.", MessageBoxIcon.Hand);
			}
		}
		else
		{
			if (ShellShortcut.AddToSendTo())
			{
				CTextAnalysisTool.ShowMessageBox($"{appName} has been added to your Windows 'Send to' menu.\n\nYou can now select multiple log files or archives in Windows Explorer, right-click, and choose 'Send to -> {appName}' to open or merge them.", MessageBoxIcon.Information);
			}
			else
			{
				CTextAnalysisTool.ShowMessageBox($"Failed to add {appName} to the 'Send to' menu.", MessageBoxIcon.Hand);
			}
		}
	}

	private void PropertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
	{
		m_applyButton.Enabled = !ConfiguredOptions.Equals(m_lastAppliedOptions);
	}
}
