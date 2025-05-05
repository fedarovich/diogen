using System.Collections;
using System.Collections.Immutable;
using Diogen.Extensions.DependencyInjection.Analyzers.Generators;

namespace Diogen.Analyzers.Common.Generators;

public readonly struct EquatableImmutableArray<T>(ImmutableArray<T> items, IEqualityComparer<T> itemComparer)
    : IEquatable<EquatableImmutableArray<T>>, IReadOnlyList<T>
{
    private readonly IEqualityComparer<T>? _itemComparer = itemComparer;

    public EquatableImmutableArray(ImmutableArray<T> items) : this(items, EqualityComparer<T>.Default)
    {
    }

    public EquatableImmutableArray(IEnumerable<T> items) 
        : this(items?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(items)))
    {
    }

    public EquatableImmutableArray(IEnumerable<T> items, IEqualityComparer<T> itemComparer)
        : this(items?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(items)), itemComparer)
    {
    }

    public ImmutableArray<T> Items { get; } = items;
    
    public IEqualityComparer<T> ItemComparer => _itemComparer ?? EqualityComparer<T>.Default;

    public ImmutableArray<T>.Enumerator GetEnumerator() => Items.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

    public ReadOnlySpan<T> AsSpan() => Items.AsSpan();

    public ReadOnlyMemory<T> AsMemory() => Items.AsMemory();

    public T this[int index] => Items[index];

    public int Length => Items.Length;

    public int Count => Items.Length;

    public bool Equals(EquatableImmutableArray<T> other)
    {
        return Items.SequenceEqual(other.Items, ItemComparer);
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableImmutableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var item in Items)
        {
            hashCode.Add(item, _itemComparer);
        }

        return hashCode.ToHashCode();
    }

    public static bool operator ==(EquatableImmutableArray<T> left, EquatableImmutableArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableImmutableArray<T> left, EquatableImmutableArray<T> right)
    {
        return !left.Equals(right);
    }

}
