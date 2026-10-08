using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace TextAnalysisTool.NET;

public class EncodingNameEditor : UITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		IWindowsFormsEditorService windowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
		if (windowsFormsEditorService != null)
		{
			using EncodingDialog encodingDialog = new EncodingDialog();
			if (value is string text)
			{
				encodingDialog.SetEncoding(string.Compare(text, "System Default") == 0, text);
			}
			if (DialogResult.OK == windowsFormsEditorService.ShowDialog(encodingDialog))
			{
				if (encodingDialog.IsSystemDefault)
				{
					return "System Default";
				}
				try
				{
					return Encoding.GetEncoding(encodingDialog.EncodingName).WebName;
				}
				catch
				{
					return "System Default";
				}
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}
}
