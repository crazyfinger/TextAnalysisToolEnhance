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
		base.ClientSize = new Size(230, 214);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterParent;
		Text = string.Format("About {0}", Resource.ApplicationName);
		PictureBox pictureBox = new PictureBox
		{
			Bounds = new Rectangle(16, 16, 32, 32),
			TabIndex = 0,
			Image = Resource.ApplicationIcon.ToBitmap()
		};
		Label label = new Label
		{
			Bounds = new Rectangle(64, 16, 160, 16),
			TabIndex = 1,
			Font = new Font(Font, FontStyle.Bold),
			Text = Resource.ApplicationName
		};
		Label label2 = new Label
		{
			Bounds = new Rectangle(64, 38, 160, 16),
			TabIndex = 2,
			Text = "Version " + versionString
		};
		Label label3 = new Label
		{
			Bounds = new Rectangle(64, 58, 160, 100),
			TabIndex = 3,
			Text = Resource.AboutCopyright
		};
		Button button = new Button();
		button.Bounds = new Rectangle(76, 172, 77, 26);
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
