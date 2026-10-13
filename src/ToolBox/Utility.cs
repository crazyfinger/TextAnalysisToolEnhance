using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ToolBox;

internal static class Utility
{
	public static void ThrowExceptionIf(bool condition, string exceptionMessage)
	{
		ThrowExceptionIf(condition, exceptionMessage, typeof(Exception));
	}

	public static void ThrowExceptionIf(bool condition, string exceptionMessage, Type exceptionType)
	{
		if (condition)
		{
			Type[] types = new Type[1] { typeof(string) };
			object[] parameters = new object[1] { exceptionMessage };
			throw (Exception)exceptionType.GetConstructor(types).Invoke(parameters);
		}
	}

	public static bool SafeSetClipboardText(string text, int retryTimes = 5, int retryDelayMs = 50)
	{
		if (text == null)
		{
			return false;
		}
		for (int i = 0; i < retryTimes; i++)
		{
			try
			{
				Clipboard.SetText(text);
				return true;
			}
			catch (ExternalException)
			{
				if (i == retryTimes - 1)
				{
					return false;
				}
				Thread.Sleep(retryDelayMs);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}

	public static bool SafeSetClipboardDataObject(object dataObject, bool copy = true, int retryTimes = 5, int retryDelayMs = 100)
	{
		if (dataObject == null)
		{
			return false;
		}
		try
		{
			Clipboard.SetDataObject(dataObject, copy, retryTimes, retryDelayMs);
			return true;
		}
		catch (ExternalException)
		{
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
