using System;
using System.Collections.Generic;
using System.Text;

namespace Diogen.Analyzers.Common;

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
