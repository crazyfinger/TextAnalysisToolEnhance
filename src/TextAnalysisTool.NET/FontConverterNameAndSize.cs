using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace TextAnalysisTool.NET;

internal class FontConverterNameAndSize : FontConverter
{
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		PropertyDescriptorCollection properties = base.GetProperties(context, value, attributes);
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		foreach (PropertyDescriptor item in properties)
		{
			if (item.Name != "Name" && item.Name != "Size")
			{
				list.Add(item);
			}
		}
		foreach (PropertyDescriptor item2 in list)
		{
			properties.Remove(item2);
		}
		return properties;
	}
}
