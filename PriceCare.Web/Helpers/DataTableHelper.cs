using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace PriceCare.Web.Helpers
{
    public static class DataTableHelper
    {
        public static DataTable ToDataTableSingle<T>(this T data)
        {
            var dataList = new List<T> {data};
            return dataList.ToDataTable(new List<string>());
        }
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            return data.ToDataTable(new List<string>());
        }

        public static DataTable ToDataTable<T>(this IList<T> data, IList<string> excludedProperties)
        {
            var props = TypeDescriptor.GetProperties(typeof(T));
            var included = props.Cast<PropertyDescriptor>().Where(prop => !excludedProperties.Contains(prop.Name)).ToList();
            var table = new DataTable();
            foreach (var prop in included)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            var values = new object[included.Count];
            foreach (var item in data)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = included[i].GetValue(item);
                }
                table.Rows.Add(values);
            }

            return table;
        }
    }
}