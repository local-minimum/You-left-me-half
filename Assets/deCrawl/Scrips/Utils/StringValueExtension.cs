using System;
using System.Reflection;

namespace DeCrawl.Utils
{
    public static class StringValueExtension
    {
        /// <summary>
        /// Get the string associated with the enum
        /// </summary>
        /// <param name="value">The enum value</param>
        /// <returns>The associated string</returns>
        public static string GetStringAttribute(this Enum value)
        {
            Type type = value.GetType();

            FieldInfo fieldInfo = type.GetField(value.ToString());

            var attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            return (attributes.Length > 0) ? attributes[0].StringValue : null;
        }
    }
}