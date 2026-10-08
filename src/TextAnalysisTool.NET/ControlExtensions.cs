using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

public static class ControlExtensions
{
	public enum DeviceCap
	{
		VERTRES = 10,
		LOGPIXELSY = 90,
		DESKTOPVERTRES = 117
	}

	private const int WM_SETREDRAW = 11;

	private const int FALSE = 0;

	private const int TRUE = 1;

	[DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

	[DllImport("gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

	public static void SuspendRedraw(this Control control)
	{
		if (control.Visible)
		{
			SendMessage(control.Handle, 11, 0, 0);
		}
	}

	public static void ResumeRedraw(this Control control)
	{
		if (control.Visible)
		{
			SendMessage(control.Handle, 11, 1, 0);
		}
	}

	public static float GetDPIScalingFactor()
	{
		Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
		IntPtr hdc = graphics.GetHdc();
		GetDeviceCaps(hdc, 10);
		GetDeviceCaps(hdc, 117);
		int deviceCaps = GetDeviceCaps(hdc, 90);
		graphics.ReleaseHdc(hdc);
		graphics.Dispose();
		return (float)deviceCaps / 96f;
	}

	public static void RescaleForDPI(this Form form)
	{
		float dPIScalingFactor = GetDPIScalingFactor();
		if (dPIScalingFactor != 1f)
		{
			form.Scale(new SizeF(dPIScalingFactor, dPIScalingFactor));
		}
	}

	public static void RescaleForDPI(this ErrorProvider errorProvider)
	{
		float dPIScalingFactor = GetDPIScalingFactor();
		if (dPIScalingFactor != 1f)
		{
			errorProvider.Icon = new Icon(errorProvider.Icon, (int)((float)errorProvider.Icon.Width * dPIScalingFactor), (int)((float)errorProvider.Icon.Height * dPIScalingFactor));
		}
	}
}
