using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.Builder.Extensions
{
    /// <summary>
    /// All kinds of short hand extensions
    /// </summary>
    internal static class HelperExtensions
    {
        /// <summary>
        /// Merges two dictionaries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="source"></param>
        public static void Merge<T>(this IDictionary<string, T> target, IDictionary<string, T> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x =>
                {
                    if (target.ContainsKey(x.Key))
                    {
                        target[x.Key] = x.Value;
                    }
                    else
                    {
                        target.Add(x.Key, x.Value);
                    }
                });
            }
        }
        /// <summary>
        /// Lowers the first letter so the name becomes camel case i.e helloWorld
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        /// <summary>
        /// Checks if type is IEnumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static Dictionary<string, object> ToDictionary<T>(this T obj, ref int id, IEnumerable<string> columns) where T : new()
        {
            int count = id;
            columns = columns.OrderBy(col => col);
            var dictionary = obj.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(prop => prop.Name)
            .Where(prop => columns.Any(col => col.ToLower() == prop.Name.ToLower()))
            .ToDictionary(prop =>
                (count++).ToString(),
            prop => prop.GetValue(obj, null)
            );
            id = count;
            return dictionary;
        }
    }
}