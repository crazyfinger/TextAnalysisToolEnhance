using System;
using System.Collections;
using System.Threading;

namespace ToolBox;

internal sealed class WorkItemRunner
{
	private int m_pendingItems;

	public void RunWorkItemsToCompletion(IEnumerable waitCallbacks)
	{
		lock (this)
		{
			foreach (WaitCallback waitCallback in waitCallbacks)
			{
				if (ThreadPool.QueueUserWorkItem(RunWorkItem, waitCallback))
				{
					m_pendingItems++;
					continue;
				}
				throw new Exception();
			}
			if (0 < m_pendingItems)
			{
				Monitor.Wait(this);
			}
		}
	}

	private void RunWorkItem(object state)
	{
		((WaitCallback)state)(null);
		lock (this)
		{
			m_pendingItems--;
			if (m_pendingItems == 0)
			{
				Monitor.Pulse(this);
			}
		}
	}
}
