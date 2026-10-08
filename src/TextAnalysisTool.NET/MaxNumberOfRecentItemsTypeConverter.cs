namespace TextAnalysisTool.NET;

public class MaxNumberOfRecentItemsTypeConverter : IntRangeTypeConverter
{
	public MaxNumberOfRecentItemsTypeConverter()
		: base(1, 20)
	{
	}
}
