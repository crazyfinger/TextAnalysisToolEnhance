using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

internal static class ShellShortcut
{
	private const string ShortcutFileName = "TextAnalysisTool.NET.lnk";

	public static string GetSendToShortcutPath()
	{
		string sendTo = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
		return Path.Combine(sendTo, ShortcutFileName);
	}

	public static bool IsInSendTo()
	{
		try
		{
			return File.Exists(GetSendToShortcutPath());
		}
		catch
		{
			return false;
		}
	}

	public static bool AddToSendTo()
	{
		try
		{
			string shortcutPath = GetSendToShortcutPath();
			Type shellType = Type.GetTypeFromProgID("WScript.Shell");
			if (shellType == null)
			{
				return false;
			}
			object shell = Activator.CreateInstance(shellType);
			object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
			Type shortcutType = shortcut.GetType();
			string exePath = Assembly.GetExecutingAssembly().Location;
			if (string.IsNullOrEmpty(exePath) || !exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				exePath = Application.ExecutablePath;
			}
			shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { exePath });
			shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(exePath) });
			shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { "Open or merge text files and archives in TextAnalysisTool.NET" });
			shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool RemoveFromSendTo()
	{
		try
		{
			string shortcutPath = GetSendToShortcutPath();
			if (File.Exists(shortcutPath))
			{
				File.Delete(shortcutPath);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
