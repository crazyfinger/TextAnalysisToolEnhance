using System.Windows.Forms;

namespace TextAnalysisTool.NET;

internal static class FormUtility
{
	public static Control[] SetFlatStyleSystem(Control[] controls)
	{
		for (int i = 0; i < controls.Length; i++)
		{
			if (controls[i] is ButtonBase buttonBase)
			{
				buttonBase.FlatStyle = FlatStyle.System;
			}
		}
		return controls;
	}
}
