using System;
using System.Collections;
using System.Threading;

namespace ToolBox;

internal sealed class WorkItemRunner
{
	private int m_pendingItems;

	private Exception m_lastException;

	public void RunWorkItemsToCompletion(IEnumerable waitCallbacks)
	{
		lock (this)
		{
			m_lastException = null;
			foreach (WaitCallback waitCallback in waitCallbacks)
			{
				if (ThreadPool.QueueUserWorkItem(RunWorkItem, waitCallback))
				{
					m_pendingItems++;
					continue;
				}
				throw new InvalidOperationException("Failed to queue work item to ThreadPool.");
			}
			if (0 < m_pendingItems)
			{
				Monitor.Wait(this);
			}
			if (m_lastException != null)
			{
				throw new AggregateException("An exception occurred in a worker thread.", m_lastException);
			}
		}
	}

	private void RunWorkItem(object state)
	{
		try
		{
			((WaitCallback)state)(null);
		}
		catch (Exception ex)
		{
			lock (this)
			{
				m_lastException ??= ex;
			}
		}
		finally
		{
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
}
