using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace TableIT.Remote.ViewModels
{
    public static class NavigationExtensions
    {
        public static T? GetQueryParamter<T>(this IDictionary<string, object> query, string name)
        {
            if (query.TryGetQueryParamter(name, out T? value))
            {
                return value;
            }
            return default;
        }



        public static bool TryGetQueryParamter<T>(this IDictionary<string, object> query, string name,
            [NotNullWhen(true)] out T? value)
        {
            if (query.TryGetValue(name, out object? objValue))
            {
                if (objValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }

                if (HttpUtility.UrlDecode(objValue?.ToString()) is { } decodedValue)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        value = (T)converter.ConvertFromString(decodedValue)!;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }


    }
}
