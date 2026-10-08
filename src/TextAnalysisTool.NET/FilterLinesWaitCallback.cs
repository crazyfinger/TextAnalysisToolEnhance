namespace TextAnalysisTool.NET;

internal sealed class FilterLinesWaitCallback
{
	private readonly Filter m_filter;

	public FilterLinesWaitCallback(Filter filter)
	{
		m_filter = filter;
	}

	public void Run(object state)
	{
		m_filter.FilterLines();
	}
}
