using System;
using System.Collections.Generic;

namespace TokoMart.Utils
{
    public class SearchParamParser
    {
        public static Dictionary<string, string> ParseSearchParams(string search)
        {
            var dataMap = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
            {
                var pairs = search.Split('&');

                foreach (var pair in pairs)
                {
                    // Memisahkan setiap pasangan berdasarkan '='
                    var keyValue = pair.Split('=');

                    // Memasukkan pasangan key-value ke dalam dictionary jika valid
                    if (keyValue.Length == 2)
                    {
                        dataMap[keyValue[0]] = keyValue[1];
                    }
                }
            }

            return dataMap;
        }
    }
}
