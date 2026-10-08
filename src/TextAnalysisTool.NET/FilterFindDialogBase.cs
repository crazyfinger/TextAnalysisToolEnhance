using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
internal class FilterFindDialogBase : Form
{
	protected Label m_textLabelBase;

	protected ComboBox m_textComboBase;

	protected CheckBox m_caseSensitiveCheckBase;

	protected CheckBox m_regularExpressionCheckBase;

	protected Button m_okButtonBase;

	protected Button m_cancelButtonBase;

	protected ToolTip m_toolTipBase;

	protected ErrorProvider m_errorProviderBase;

	protected bool m_validInput;

	internal Filter m_filter;

	internal List<Filter> m_filterList;

	protected List<Control> m_minWidthControls = new List<Control>();

	protected List<Control> m_minHeightControls = new List<Control>();

	private static ArrayList previousTextEntries = new ArrayList();

	public Filter FilterObject => m_filter;

	public List<Filter> FilterList => m_filterList;

	public FilterFindDialogBase()
	{
	}

	public FilterFindDialogBase(string initialText)
	{
	}

	protected void InitBaseUI(string initialText)
	{
		base.StartPosition = FormStartPosition.CenterParent;
		m_textLabelBase.Text = "&Text:";
		m_textComboBase.TextChanged += ValidateInput;
		m_textComboBase.Items.AddRange(previousTextEntries.ToArray());
		m_caseSensitiveCheckBase.Text = "C&ase-sensitive [Aa]";
		m_regularExpressionCheckBase.Text = "&Regular expression [R]";
		m_regularExpressionCheckBase.CheckedChanged += ValidateInput;
		m_regularExpressionCheckBase.MouseDown += RegularExpressionCheck_MouseDown;
		m_okButtonBase.Text = "&OK";
		m_okButtonBase.Click += OK_Click;
		m_cancelButtonBase.Text = "Cancel";
		m_cancelButtonBase.Click += Cancel_Click;
		base.AcceptButton = m_okButtonBase;
		base.CancelButton = m_cancelButtonBase;
		m_toolTipBase.SetToolTip(m_regularExpressionCheckBase, "Right-click this checkbox for help with regular expression syntax");
		m_textComboBase.Text = initialText;
		base.Shown += FilterFindDialogBase_Shown;
		base.SizeGripStyle = SizeGripStyle.Show;
		ValidateInput(null, null);
	}

	private void FilterFindDialogBase_Shown(object sender, EventArgs e)
	{
		m_textComboBase.Select();
	}

	protected virtual void RecalculateFormSizes(int targetDialogWidth, int maxDialogWidth)
	{
		this.RescaleForDPI();
		m_errorProviderBase.RescaleForDPI();
		int captionHeight = SystemInformation.CaptionHeight;
		float dPIScalingFactor = ControlExtensions.GetDPIScalingFactor();
		int num = (int)(17f * dPIScalingFactor);
		int num2 = base.Padding.Left + base.Padding.Right + num + SystemInformation.BorderSize.Width * 2;
		for (int i = 0; i < m_minWidthControls.Count; i++)
		{
			num2 += m_minWidthControls[i].Width + m_minWidthControls[i].Margin.Left + m_minWidthControls[i].Margin.Right;
		}
		int num3 = captionHeight + base.Padding.Top + base.Padding.Bottom + SystemInformation.BorderSize.Height * 2 + num;
		for (int j = 0; j < m_minHeightControls.Count; j++)
		{
			num3 += m_minHeightControls[j].Height + m_minHeightControls[j].Margin.Top + m_minHeightControls[j].Margin.Bottom;
		}
		AutoSize = false;
		MaximumSize = new Size(maxDialogWidth, num3);
		MinimumSize = new Size(num2, num3);
		base.Size = new Size(targetDialogWidth, num3);
	}

	public virtual void ConfigureFromFilter(Filter f)
	{
		if (f is MatchesTextFilter)
		{
			MatchesTextFilter matchesTextFilter = (MatchesTextFilter)f;
			m_textComboBase.Text = matchesTextFilter.Text;
			m_caseSensitiveCheckBase.Checked = matchesTextFilter.CaseSensitive;
			m_regularExpressionCheckBase.Checked = matchesTextFilter.RegularExpression;
		}
		ValidateInput(null, null);
	}

	protected void ValidateInput(object sender, EventArgs ea)
	{
		m_errorProviderBase.SetError(m_textComboBase, "");
		string text = m_textComboBase.Text;
		m_validInput = true;
		string text2 = "";
		if (!ValidateMarkerInput())
		{
			if (text.Length == 0)
			{
				m_validInput = false;
				text2 = "The text box must contain at least one character of text";
			}
			else if (m_regularExpressionCheckBase.Checked)
			{
				try
				{
					new Regex(text, MatchesTextFilter.DefaultRegexOptions);
				}
				catch (ArgumentException ex)
				{
					m_validInput = false;
					text2 = ex.Message;
					int num = text2.LastIndexOf(" - ");
					int num2 = text2.LastIndexOf("\r\n");
					if (-1 != num && -1 != num2)
					{
						num += 3;
						num2--;
						text2 = text2.Substring(num, num2 - num);
					}
				}
			}
		}
		m_errorProviderBase.SetError(m_textComboBase, text2);
		m_okButtonBase.Enabled = m_validInput;
	}

	protected virtual bool ValidateMarkerInput()
	{
		return false;
	}

	public void RegularExpressionCheck_MouseDown(object sender, MouseEventArgs e)
	{
		if (MouseButtons.Right == e.Button)
		{
			try
			{
				Process.Start("https://msdn.microsoft.com/en-us/library/hs600312.aspx");
			}
			catch (Exception e2)
			{
				CTextAnalysisTool.ShowMessageBox("Unable to display the web page: {0}.", e2, MessageBoxIcon.Hand);
			}
		}
	}

	protected virtual void OK_Click(object sender, EventArgs e)
	{
		string text = m_textComboBase.Text;
		previousTextEntries.Remove(text);
		previousTextEntries.Insert(0, text);
		int num = 16;
		if (num < previousTextEntries.Count)
		{
			previousTextEntries.RemoveAt(num);
		}
	}

	private void Cancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}
}
