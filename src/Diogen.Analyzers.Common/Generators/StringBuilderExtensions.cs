using System.Diagnostics;
using System.Text;

namespace Diogen.Analyzers.Common.Generators;

internal static unsafe class StringBuilderExtensions
{
    public static StringBuilder AppendJoin(this StringBuilder @this, string? separator, params object?[] values)
    {
        separator ??= string.Empty;
        fixed (char* pSeparator = separator)
        {
            return @this.AppendJoinCore(pSeparator, separator.Length, values);
        }
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder @this, string? separator, IEnumerable<T> values)
    {
        separator ??= string.Empty;
        fixed (char* pSeparator = separator)
        {
            return @this.AppendJoinCore(pSeparator, separator.Length, values);
        }
    }

    public static StringBuilder AppendJoin(this StringBuilder @this, string? separator, params string?[] values)
    {
        separator ??= string.Empty;
        fixed (char* pSeparator = separator)
        {
            return @this.AppendJoinCore(pSeparator, separator.Length, values);
        }
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder @this, string? separator, ReadOnlySpan<T> values)
    {
        separator ??= string.Empty;
        fixed (char* pSeparator = separator)
        {
            return @this.AppendJoinCore(pSeparator, separator.Length, values);
        }
    }

    public static StringBuilder AppendJoin(this StringBuilder @this, char separator, params object?[] values)
    {
        return @this.AppendJoinCore(&separator, 1, values);
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder @this, char separator, IEnumerable<T> values)
    {
        return @this.AppendJoinCore(&separator, 1, values);
    }

    public static StringBuilder AppendJoin(this StringBuilder @this, char separator, params string?[] values)
    {
        return @this.AppendJoinCore(&separator, 1, values);
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder @this, char separator, ReadOnlySpan<T> values)
    {
        return @this.AppendJoinCore(&separator, 1, values);
    }

    private static StringBuilder AppendJoinCore<T>(this StringBuilder @this, char* separator, int separatorLength, IEnumerable<T> values)
    {
        Debug.Assert(separator != null);
        Debug.Assert(separatorLength >= 0);

        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        Debug.Assert(values != null);
        using (IEnumerator<T> en = values!.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                return @this;
            }

            T value = en.Current;
            if (value != null)
            {
                @this.Append(value.ToString());
            }

            while (en.MoveNext())
            {
                @this.Append(separator, separatorLength);
                value = en.Current;
                if (value != null)
                {
                    @this.Append(value.ToString());
                }
            }
        }
        return @this;
    }

    private static StringBuilder AppendJoinCore<T>(this StringBuilder @this, char* separator, int separatorLength, T[] values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        Debug.Assert(values != null);
        if (values!.Length == 0)
        {
            return @this;
        }

        if (values[0] != null)
        {
            @this.Append(values[0]!.ToString());
        }

        for (int i = 1; i < values.Length; i++)
        {
            @this.Append(separator, separatorLength);
            if (values[i] != null)
            {
                @this.Append(values[i]!.ToString());
            }
        }
        return @this;
    }

    private static StringBuilder AppendJoinCore<T>(this StringBuilder @this, char* separator, int separatorLength, ReadOnlySpan<T> values)
    {
        if (values.Length == 0)
        {
            return @this;
        }

        if (values[0] != null)
        {
            @this.Append(values[0]!.ToString());
        }

        for (int i = 1; i < values.Length; i++)
        {
            @this.Append(separator, separatorLength);
            if (values[i] != null)
            {
                @this.Append(values[i]!.ToString());
            }
        }
        return @this;
    }
}
