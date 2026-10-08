using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal sealed class PasteTextConfirmationDialog : Form
{
	private Button OKButton;

	private Button cancelButton;

	private Label MessageLabel;

	private CheckBox AskAgainCheckbox;

	public bool DoNotAskAgainChecked => !AskAgainCheckbox.Checked;

	public PasteTextConfirmationDialog()
	{
		OKButton = new Button();
		cancelButton = new Button();
		MessageLabel = new Label();
		AskAgainCheckbox = new CheckBox();
		SuspendLayout();
		OKButton.Location = new Point(194, 142);
		OKButton.Size = new Size(131, 60);
		OKButton.TabIndex = 0;
		OKButton.Text = "&OK";
		OKButton.UseVisualStyleBackColor = true;
		OKButton.Click += OKButton_Click;
		cancelButton.Location = new Point(455, 142);
		cancelButton.Size = new Size(131, 60);
		cancelButton.TabIndex = 1;
		cancelButton.Text = "Cancel";
		cancelButton.UseVisualStyleBackColor = true;
		cancelButton.Click += CancelButton_Click;
		MessageLabel.AutoSize = true;
		MessageLabel.Location = new Point(15, 21);
		MessageLabel.Size = new Size(714, 37);
		MessageLabel.TabIndex = 2;
		MessageLabel.Text = "Replace the current text with the contents of the clipboard?";
		MessageLabel.Font = new Font(MessageLabel.Font.FontFamily, MessageLabel.Font.Size + 2f);
		AskAgainCheckbox.AutoSize = true;
		AskAgainCheckbox.Location = new Point(20, 75);
		AskAgainCheckbox.Size = new Size(95, 20);
		AskAgainCheckbox.TabIndex = 3;
		AskAgainCheckbox.Text = "&Don't show this message again.";
		AskAgainCheckbox.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new SizeF(12f, 30f);
		base.AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		base.AutoSizeMode = AutoSizeMode.GrowOnly;
		base.ClientSize = new Size(737, 176);
		base.Controls.Add(AskAgainCheckbox);
		base.Controls.Add(MessageLabel);
		base.Controls.Add(cancelButton);
		base.Controls.Add(OKButton);
		base.Name = "Confirm Paste";
		Text = "Confirm Paste";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterParent;
		base.Margin = new Padding(6, 8, 6, 8);
		base.Padding = new Padding(18, 21, 18, 21);
		base.CancelButton = cancelButton;
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void OKButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void CancelButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.No;
		Close();
	}
}
