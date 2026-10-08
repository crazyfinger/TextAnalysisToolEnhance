using System;

namespace TextAnalysisTool.NET;

public class DirtyStatusChangedEventArgs : EventArgs
{
	public bool IsDirty { get; private set; }

	public DirtyStatusChangedEventArgs(bool isDirty)
	{
		IsDirty = isDirty;
	}
}
