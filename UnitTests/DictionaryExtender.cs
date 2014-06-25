using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace UnitTests
{
    public static class DictionaryExtender
    {
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>) expando;

            foreach (var kvp in dictionary)
            {
                var innerDictionary = kvp.Value as IDictionary<string, object>;
                if (innerDictionary != null)
                {
                    var expandoValue = innerDictionary.ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else
                {
                    var innerCollection = kvp.Value as ICollection;
                    if (innerCollection != null)
                    {
                        var itemList = new List<object>();
                        foreach (var item in innerCollection)
                        {
                            var nestedDictionary = item as IDictionary<string, object>;
                            if (nestedDictionary != null)
                            {
                                var expandoItem = nestedDictionary.ToExpando();
                                itemList.Add(expandoItem);
                            }
                            else
                            {
                                itemList.Add(item);
                            }
                        }

                        expandoDic.Add(kvp.Key, itemList);
                    }
                    else
                    {
                        expandoDic.Add(kvp);
                    }
                }
            }

            return expando;
        }
    }
}
