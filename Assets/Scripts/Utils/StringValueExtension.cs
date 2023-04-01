using System;
using System.Collections.Generic;
using System.Reflection;

public static class StringValueExtension
{
    public static string GetStringValue(this Enum value)
    {
        Type type = value.GetType();

        FieldInfo fieldInfo = type.GetField(value.ToString());

        var attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

        return (attributes.Length > 0) ? attributes[0].StringValue : null;
    }
}
