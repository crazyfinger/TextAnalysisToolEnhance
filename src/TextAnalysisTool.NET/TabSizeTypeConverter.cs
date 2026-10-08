namespace TextAnalysisTool.NET;

public class TabSizeTypeConverter : IntRangeTypeConverter
{
	public TabSizeTypeConverter()
		: base(1, 100)
	{
	}
}
