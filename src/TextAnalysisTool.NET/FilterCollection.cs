using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ToolBox;

namespace TextAnalysisTool.NET;

internal sealed class FilterCollection
{
	private readonly CTextAnalysisTool m_textAnalysisTool;

	private readonly List<Filter> m_filters;

	private LineCollection m_filteredLines;

	private bool m_isDirty;

	private MatchesTextFilter m_refineFilter;

	private int m_refineGeneration;

	private readonly object m_refineLock = new object();

	public IReadOnlyList<Filter> Filters => m_filters;

	public bool IsDirty
	{
		get
		{
			return m_isDirty;
		}
		private set
		{
			m_isDirty = value;
			this.DirtyStatusChanged?.Invoke(this, new DirtyStatusChangedEventArgs(m_isDirty));
		}
	}

	public bool IsEnableDisableFilterChange { get; set; }

	public LineCollection FilteredLines => m_filteredLines;

	public string RefineFilterText => m_refineFilter?.Text;

	public event EventHandler<DirtyStatusChangedEventArgs> DirtyStatusChanged;

	public FilterCollection(CTextAnalysisTool textAnalysisTool)
	{
		m_textAnalysisTool = textAnalysisTool;
		m_filters = new List<Filter>();
		m_isDirty = false;
	}

	public void ClearDirtyFlag()
	{
		IsDirty = false;
	}

	public void SetRefineFilterText(string text, bool excluding, bool caseSensitive, bool usesRegularExpression)
	{
		lock (m_refineLock)
		{
			m_refineGeneration++;
		}
		if (string.IsNullOrEmpty(text))
		{
			m_refineFilter = null;
		}
		else
		{
			m_refineFilter = new MatchesTextFilter(enabled: true, excluding, System.Drawing.Color.Empty, System.Drawing.Color.Empty, text, caseSensitive, usesRegularExpression, "Refine");
		}
		m_textAnalysisTool.FiltersChanged(updateFilterDisplay: true);
	}

	public void SetFilteredLines(LineCollection lines)
	{
		m_filteredLines = lines;
	}

	public bool IsCurrentRefineGeneration(int generation)
	{
		lock (m_refineLock)
		{
			return generation == m_refineGeneration;
		}
	}

	private void FiltersChanged(bool destructive)
	{
		if ((Settings.FiltersDirtied == FiltersDirtiedState.AddRemoveEdit && destructive) || Settings.FiltersDirtied == FiltersDirtiedState.Always)
		{
			IsDirty = true;
		}
		m_textAnalysisTool.FiltersChanged(updateFilterDisplay: true);
	}

	public void SetEnable(Filter f, bool enabled)
	{
		f.Enabled = enabled;
		IsEnableDisableFilterChange = true;
		FiltersChanged(destructive: false);
		IsEnableDisableFilterChange = false;
	}

	public void Add(Filter f)
	{
		m_filters.Add(f);
		FiltersChanged(destructive: true);
	}

	public void AddRange(IEnumerable<Filter> filters)
	{
		foreach (Filter filter in filters)
		{
			m_filters.Add(filter);
		}
		FiltersChanged(destructive: true);
	}

	public void Replace(Filter fOld, Filter fNew)
	{
		m_filters[m_filters.IndexOf(fOld)] = fNew;
		FiltersChanged(destructive: true);
	}

	public void Replace(List<Filter> fOldList, List<Filter> fNewList)
	{
		for (int i = 0; i < fOldList.Count; i++)
		{
			m_filters[m_filters.IndexOf(fOldList[i])] = fNewList[i];
		}
		FiltersChanged(destructive: true);
	}

	public void ReplaceAll(IEnumerable<Filter> filters)
	{
		m_filters.Clear();
		foreach (Filter filter in filters)
		{
			m_filters.Add(filter);
		}
		FiltersChanged(destructive: true);
	}

	public void Swap(Filter a, Filter b)
	{
		int index = m_filters.IndexOf(a);
		int index2 = m_filters.IndexOf(b);
		m_filters[index] = b;
		m_filters[index2] = a;
		FiltersChanged(destructive: false);
	}

	public void Move(Filter filter, int newIndex)
	{
		m_filters.Remove(filter);
		m_filters.Insert(newIndex, filter);
		FiltersChanged(destructive: false);
	}

	public void Remove(Filter f)
	{
		m_filters.Remove(f);
		FiltersChanged(destructive: true);
	}

	public void SetAllEnabledState(bool enabled)
	{
		foreach (Filter filter in m_filters)
		{
			filter.Enabled = enabled;
		}
		FiltersChanged(destructive: false);
	}

	public void RemoveAll()
	{
		m_filters.Clear();
		FiltersChanged(destructive: true);
	}

	public bool MarkersChanged(int marker)
	{
		bool flag = false;
		foreach (Filter filter in m_filters)
		{
			flag |= filter.MarkersChanged(marker);
		}
		return flag;
	}

	public void RefreshLinesLength()
	{
		m_filteredLines.RefreshLinesLength();
		foreach (Filter filter in m_filters)
		{
			filter.RefreshLinesLength();
		}
	}

	public void FilterLines(LineCollection sourceLines)
	{
		WorkItemRunner workItemRunner = new WorkItemRunner();
		List<WaitCallback> callbacks = new List<WaitCallback>(m_filters.Count);
		foreach (Filter filter in m_filters)
		{
			if (filter.FilterLinesNecessary(sourceLines))
			{
				callbacks.Add(new WaitCallback(new FilterLinesWaitCallback(filter).Run));
			}
		}
		workItemRunner.RunWorkItemsToCompletion(callbacks);
		m_filteredLines = new LineCollection(sourceLines, m_filters);
		if (m_refineFilter != null)
		{
			BeginAsyncRefine(m_filteredLines, m_refineFilter);
		}
	}

	public void RefreshLine(LineCollection sourceLines, int lineNumber)
	{
		if (m_filteredLines == null)
		{
			FilterLines(sourceLines);
			return;
		}

		bool hasIncluding = false;
		bool isIncluded = false;
		bool isExcluded = false;

		foreach (Filter filter in m_filters)
		{
			if (filter.Enabled)
			{
				if (filter.FilteredLines == null)
				{
					filter.FilterLinesNecessary(sourceLines);
					filter.FilterLines();
				}
				else
				{
					filter.RefreshLine(lineNumber);
				}

				bool accepted = filter.FilteredLines != null && filter.FilteredLines.IsLineNumberInCollection(lineNumber);
				if (filter.Excluding)
				{
					if (accepted)
					{
						isExcluded = true;
					}
				}
				else
				{
					hasIncluding = true;
					if (accepted)
					{
						isIncluded = true;
					}
				}
			}
		}

		bool finalAccepted = !isExcluded && (!hasIncluding || isIncluded);
		m_filteredLines.SetLineAccepted(lineNumber, finalAccepted);

		if (m_refineFilter != null)
		{
			BeginAsyncRefine(m_filteredLines, m_refineFilter);
		}
	}

	private void BeginAsyncRefine(LineCollection baseLines, Filter refineFilter)
	{
		LineCollection clone = new LineCollection(baseLines);
		int generation;
		lock (m_refineLock)
		{
			generation = ++m_refineGeneration;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				clone.RemoveLinesNotAcceptedBy(refineFilter);
			}
			catch (Exception)
			{
				return;
			}
			if (IsCurrentRefineGeneration(generation))
			{
				m_textAnalysisTool.OnRefineComputed(generation, clone);
			}
		});
	}
}
