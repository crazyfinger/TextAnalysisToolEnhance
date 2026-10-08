using System;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

internal sealed class AboutDialog : Form
{
	public AboutDialog(string versionString)
	{
		SuspendLayout();
		AutoScaleBaseSize = new Size(5, 13);
		base.ClientSize = new Size(210, 204);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterParent;
		Text = string.Format("About {0}", "TextAnalysisTool.NET");
		PictureBox pictureBox = new PictureBox
		{
			Bounds = new Rectangle(16, 16, 32, 32),
			TabIndex = 0,
			Image = Resource.ApplicationIcon.ToBitmap()
		};
		Label label = new Label
		{
			Bounds = new Rectangle(64, 16, 146, 16),
			TabIndex = 1,
			Text = "TextAnalysisTool.NET"
		};
		Label label2 = new Label
		{
			Bounds = new Rectangle(64, 40, 146, 16),
			TabIndex = 2,
			Text = versionString
		};
		Label label3 = new Label
		{
			Bounds = new Rectangle(64, 64, 146, 92),
			TabIndex = 3,
			Text = "Copyright © 2000-2025\r\nby David L. A. Anson\r\nDavidAns@Microsoft.com\r\n\r\nProtected by United States\r\npatents 6,963,878 and 7,146,379."
		};
		Button button = new Button();
		button.Bounds = new Rectangle(66, 166, 77, 23);
		button.TabIndex = 4;
		button.Text = "&OK";
		button.Click += OK_Click;
		base.AcceptButton = button;
		base.CancelButton = button;
		base.Controls.AddRange(FormUtility.SetFlatStyleSystem(new Control[5] { button, label3, label2, label, pictureBox }));
		ResumeLayout(performLayout: false);
	}

	private void OK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
