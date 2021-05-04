using System.Collections.Generic;
using System.Linq;

using Dapper.Contrib.Extensions;

namespace EasyKeys.Extensions.Data.Dapper
{
    public static class FilterHelper
    {
        public static string GetSqlPairs(IEnumerable<string> keys, string separator = ", ")
        {
            var pairs = keys.Select(key => string.Format("{0}=@{0}", key)).ToList();
            return string.Join(separator, pairs);
        }

        /// <summary>
        /// Retrieves a Dictionary with name and value
        /// for all object properties matching the given criteria.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="obj"></param>
        /// <param name="includeNonPublicSetters"></param>
        public static PropertyContainer ParseProperties<TObject>(TObject obj, bool includeNonPublicSetters = true)
        {
            var propertyContainer = new PropertyContainer();
            var t = obj?.GetType();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var property in t.GetProperties())
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            {
                // Skip reference types (but still include string!)
                if (property.PropertyType.IsClass
                    && property.PropertyType != typeof(string))
                {
                    continue;
                }

                // Skip methods without a public setter
                if (!includeNonPublicSetters)
                {
                    if (property.GetSetMethod() == null)
                    {
                        continue;
                    }
                }

                var name = property.Name;

                var value = t.GetProperty(property.Name)?.GetValue(obj, null);

                if (value != null)
                {
                    if (property.IsDefined(typeof(KeyAttribute), false))
                    {
                        propertyContainer.AddId(name, value);
                    }
                    else
                    {
                        propertyContainer.AddValue(name, value);
                    }
                }
            }

            return propertyContainer;
        }
    }
}
