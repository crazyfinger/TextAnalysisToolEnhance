using System;

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
}
