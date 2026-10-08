namespace TextAnalysisTool.NET;

public class FileNameDisplayLengthTypeConverter : IntRangeTypeConverter
{
	public FileNameDisplayLengthTypeConverter()
		: base(0, int.MaxValue)
	{
	}
}
