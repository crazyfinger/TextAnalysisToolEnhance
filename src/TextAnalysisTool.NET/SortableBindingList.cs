using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TextAnalysisTool.NET;

public class SortableBindingList<T> : BindingList<T>
{
	private bool isSortedValue;

	private ListSortDirection sortDirectionValue;

	private PropertyDescriptor sortPropertyValue;

	protected override PropertyDescriptor SortPropertyCore => sortPropertyValue;

	protected override ListSortDirection SortDirectionCore => sortDirectionValue;

	protected override bool SupportsSortingCore => true;

	protected override bool IsSortedCore => isSortedValue;

	public SortableBindingList()
	{
	}

	public SortableBindingList(IList<T> list)
	{
		foreach (T item in list)
		{
			Add(item);
		}
	}

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		Type @interface = prop.PropertyType.GetInterface("IComparable");
		if (@interface == null && prop.PropertyType.IsValueType)
		{
			Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
			if (underlyingType != null)
			{
				@interface = underlyingType.GetInterface("IComparable");
			}
		}
		if (@interface != null)
		{
			sortPropertyValue = prop;
			sortDirectionValue = direction;
			IEnumerable<T> source = base.Items;
			source = ((direction != 0) ? source.OrderByDescending((T i) => prop.GetValue(i)) : source.OrderBy((T i) => prop.GetValue(i)));
			int num = 0;
			foreach (T item in source)
			{
				base.Items[num] = item;
				num++;
			}
			isSortedValue = true;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			return;
		}
		throw new NotSupportedException("Cannot sort by " + prop.Name + ". This" + prop.PropertyType.ToString() + " does not implement IComparable");
	}
}
