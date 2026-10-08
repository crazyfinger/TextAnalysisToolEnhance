using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class EncodingDialog : Form
{
	private RadioButton m_systemDefaultRadio;

	private RadioButton m_encodingRadio;

	private Button m_okButton;

	private Button m_cancelButton;

	private DataGridView m_encodingsGrid;

	private TextBox m_encodingNameTextBox;

	private bool m_validateInput = true;

	private bool m_validInput;

	private bool m_isSystemDefault;

	private string m_originalEncodingName;

	private ErrorProvider m_errorProvider;

	public bool IsSystemDefault => m_systemDefaultRadio.Checked;

	public string EncodingName => m_encodingNameTextBox.Text;

	public EncodingDialog()
	{
		InitializeComponent();
		base.Load += EncodingDialog_Load;
		m_systemDefaultRadio.Text = $"System Default ({Encoding.Default.WebName})";
		m_encodingNameTextBox.TextChanged += EncodingNameTextBoxChanged;
		m_errorProvider = new ErrorProvider();
		m_errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
		base.AcceptButton = m_okButton;
		base.CancelButton = m_cancelButton;
	}

	private void EncodingDialog_Load(object sender, EventArgs e)
	{
		m_systemDefaultRadio.Checked = m_isSystemDefault;
		m_encodingRadio.Checked = !m_isSystemDefault;
		if (m_isSystemDefault)
		{
			return;
		}
		foreach (DataGridViewRow item in (IEnumerable)m_encodingsGrid.Rows)
		{
			if (item.DataBoundItem is EncodingInfo encodingInfo && string.Compare(encodingInfo.Name, m_originalEncodingName, ignoreCase: true) == 0)
			{
				item.Selected = true;
				m_encodingsGrid.CurrentCell = item.Cells[0];
				m_encodingNameTextBox.Text = encodingInfo.Name;
				break;
			}
		}
	}

	public void SetEncoding(bool isSystemDefault, string originalEncodingName)
	{
		m_isSystemDefault = isSystemDefault;
		m_originalEncodingName = originalEncodingName;
	}

	private void EncodingNameTextBoxChanged(object sender, EventArgs ea)
	{
		m_validInput = true;
		if (!m_validateInput)
		{
			m_errorProvider.Clear();
		}
		else
		{
			m_systemDefaultRadio.Checked = false;
			m_encodingRadio.Checked = true;
			try
			{
				Encoding.GetEncoding(m_encodingNameTextBox.Text);
				m_errorProvider.Clear();
			}
			catch (Exception)
			{
				m_validInput = false;
				m_errorProvider.SetError(m_encodingNameTextBox, $"The character encoding \"{m_encodingNameTextBox.Text}\" is not present on this operating system. Some common encodings are \"UTF-8\", \"GB18030\", and \"Windows-1252\".");
				m_errorProvider.SetIconAlignment(m_encodingNameTextBox, ErrorIconAlignment.MiddleRight);
				m_errorProvider.SetIconPadding(m_encodingNameTextBox, 8);
			}
		}
		m_okButton.Enabled = m_validInput;
	}

	private void OK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void Cancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void InitializeComponent()
	{
		this.m_systemDefaultRadio = new System.Windows.Forms.RadioButton();
		this.m_encodingRadio = new System.Windows.Forms.RadioButton();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_encodingsGrid = new System.Windows.Forms.DataGridView();
		this.m_encodingNameTextBox = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_encodingsGrid).BeginInit();
		base.SuspendLayout();
		this.m_systemDefaultRadio.AutoSize = true;
		this.m_systemDefaultRadio.Location = new System.Drawing.Point(12, 12);
		this.m_systemDefaultRadio.Name = "m_systemDefaultButton";
		this.m_systemDefaultRadio.Size = new System.Drawing.Size(96, 17);
		this.m_systemDefaultRadio.TabIndex = 0;
		this.m_systemDefaultRadio.TabStop = true;
		this.m_systemDefaultRadio.UseVisualStyleBackColor = true;
		this.m_systemDefaultRadio.Click += new System.EventHandler(m_systemDefaultButton_Click);
		this.m_encodingRadio.AutoSize = true;
		this.m_encodingRadio.Location = new System.Drawing.Point(12, 36);
		this.m_encodingRadio.Name = "m_encodingButton";
		this.m_encodingRadio.Size = new System.Drawing.Size(101, 17);
		this.m_encodingRadio.TabIndex = 1;
		this.m_encodingRadio.TabStop = true;
		this.m_encodingRadio.Text = "Other encoding:";
		this.m_encodingRadio.UseVisualStyleBackColor = true;
		this.m_encodingsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_encodingsGrid.Location = new System.Drawing.Point(12, 60);
		this.m_encodingsGrid.Name = "m_encodingsGrid";
		this.m_encodingsGrid.Size = new System.Drawing.Size(478, 260);
		this.m_encodingsGrid.TabIndex = 3;
		this.m_encodingsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_encodingsGrid.ShowEditingIcon = false;
		this.m_encodingsGrid.MultiSelect = false;
		this.m_encodingsGrid.DataSource = new TextAnalysisTool.NET.SortableBindingList<System.Text.EncodingInfo>(System.Text.Encoding.GetEncodings().ToList());
		this.m_encodingsGrid.Click += new System.EventHandler(m_encodingsGrid_Click);
		this.m_encodingsGrid.DoubleClick += new System.EventHandler(m_encodingsGrid_DoubleClick);
		this.m_encodingsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.m_encodingNameTextBox.Location = new System.Drawing.Point(13, 330);
		this.m_encodingNameTextBox.Name = "m_encodingNameTextBox";
		this.m_encodingNameTextBox.Size = new System.Drawing.Size(450, 20);
		this.m_encodingNameTextBox.TabIndex = 6;
		this.m_encodingNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_okButton.Location = new System.Drawing.Point(173, 365);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 7;
		this.m_okButton.Text = "&OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(254, 365);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 8;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		base.ClientSize = new System.Drawing.Size(500, 400);
		base.Controls.Add(this.m_encodingNameTextBox);
		base.Controls.Add(this.m_encodingsGrid);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_encodingRadio);
		base.Controls.Add(this.m_systemDefaultRadio);
		base.Name = "EncodingDialog";
		this.Text = "Select character encoding";
		((System.ComponentModel.ISupportInitialize)this.m_encodingsGrid).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void m_systemDefaultButton_Click(object sender, EventArgs e)
	{
		m_validateInput = false;
		m_encodingNameTextBox.Text = Encoding.Default.WebName;
		m_validateInput = true;
	}

	private void m_encodingsGrid_Click(object sender, EventArgs e)
	{
		m_encodingRadio.Checked = true;
		if (sender is DataGridView dataGridView && dataGridView.SelectedRows.Count == 1 && dataGridView.SelectedRows[0].DataBoundItem is EncodingInfo encodingInfo)
		{
			m_encodingNameTextBox.Text = encodingInfo.Name;
		}
	}

	private void m_encodingsGrid_DoubleClick(object sender, EventArgs e)
	{
		m_encodingRadio.Checked = true;
		if (sender is DataGridView dataGridView && dataGridView.SelectedRows.Count == 1 && dataGridView.SelectedRows[0].DataBoundItem is EncodingInfo encodingInfo)
		{
			m_encodingNameTextBox.Text = encodingInfo.Name;
			if (m_validInput)
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
