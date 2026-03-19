// -----------------------------------------------------------------------
// <copyright file="ImmutableExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// <see cref="System.Collections.Immutable"/> extensions.
/// </summary>
internal static class ImmutableExtensions
{
    /// <summary>
    /// Forces value semantics for the immutable list.
    /// </summary>
    /// <typeparam name="T">The type in the list.</typeparam>
    /// <param name="list">The list to wrap.</param>
    /// <returns>The list with value semantics.</returns>
    public static IImmutableList<T> WithValueSemantics<T>(this IImmutableList<T> list) => new ImmutableListWithValueSemantics<T>(list);

    /// <summary>
    /// Forces value semantics for the immutable list.
    /// </summary>
    /// <typeparam name="T">The type in the list.</typeparam>
    /// <param name="list">The list to wrap.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>The list with value semantics.</returns>
    public static IImmutableList<T> WithValueSemantics<T>(this IImmutableList<T> list, IEqualityComparer<T> comparer) => new ImmutableListWithValueSemantics<T>(list, comparer);

    private readonly struct ImmutableListWithValueSemantics<T>(IImmutableList<T> list, IEqualityComparer<T>? comparer = default) : IImmutableList<T>, IEquatable<IImmutableList<T>?>
    {
        public int Count => list.Count;

        public T this[int index] => list[index];

        public IImmutableList<T> Add(T value) => list.Add(value).WithValueSemantics();

        public IImmutableList<T> AddRange(IEnumerable<T> items) => list.AddRange(items).WithValueSemantics();

        public IImmutableList<T> Clear() => list.Clear().WithValueSemantics();

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => list.IndexOf(item, index, count, equalityComparer);

        public IImmutableList<T> Insert(int index, T element) => list.Insert(index, element).WithValueSemantics();

        public IImmutableList<T> InsertRange(int index, IEnumerable<T> items) => list.InsertRange(index, items).WithValueSemantics();

        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => list.LastIndexOf(item, index, count, equalityComparer);

        public IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer) => list.Remove(value, equalityComparer).WithValueSemantics();

        public IImmutableList<T> RemoveAll(Predicate<T> match) => list.RemoveAll(match).WithValueSemantics();

        public IImmutableList<T> RemoveAt(int index) => list.RemoveAt(index).WithValueSemantics();

        public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => list.RemoveRange(items, equalityComparer).WithValueSemantics();

        public IImmutableList<T> RemoveRange(int index, int count) => list.RemoveRange(index, count).WithValueSemantics();

        public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => list.Replace(oldValue, newValue, equalityComparer).WithValueSemantics();

        public IImmutableList<T> SetItem(int index, T value) => list.SetItem(index, value);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => list.GetEnumerator();

        public override bool Equals(object? obj) => obj is IImmutableList<T> immutableList && this.Equals(immutableList);

        public bool Equals(IImmutableList<T>? other) => this.SequenceEqual(other ?? ImmutableList<T>.Empty, comparer);

        public override int GetHashCode()
        {
            unchecked
            {
                var copiedComparer = comparer;
                return this.Aggregate(19, (h, i) => (h * 19) + GetHashCodeCore(i, copiedComparer));

                static int GetHashCodeCore(T? value, IEqualityComparer<T>? comparer)
                {
                    if (value is null)
                    {
                        return 0;
                    }

                    if (comparer is { } c)
                    {
                        return c.GetHashCode(value);
                    }

                    return value.GetHashCode();
                }
            }
        }
    }
}