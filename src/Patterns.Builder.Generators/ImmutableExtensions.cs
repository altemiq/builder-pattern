// -----------------------------------------------------------------------
// <copyright file="ImmutableExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

using System.Collections.Immutable;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="System.Collections.Immutable"/> extensions.
/// </summary>
internal static class ImmutableExtensions
{
    /// <content>The <see cref="System.Collections.Immutable"/> extensions.</content>
    /// <param name="list">The list to wrap.</param>
    /// <typeparam name="T">The type in the list.</typeparam>
    extension<T>(IImmutableList<T> list)
    {
        /// <summary>
        /// Forces value semantics for the immutable list.
        /// </summary>
        /// <returns>The list with value semantics.</returns>
        public IImmutableList<T> WithValueSemantics() => new ImmutableListWithValueSemantics<T>(list);

        /// <summary>
        /// Forces value semantics for the immutable list.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <returns>The list with value semantics.</returns>
        public IImmutableList<T> WithValueSemantics(IEqualityComparer<T> comparer) => new ImmutableListWithValueSemantics<T>(list, comparer);
    }

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
                var actualComparer = comparer ?? new PassThroughComparer();

                return this.Aggregate(
                    (HashCode: 19, Comparer: actualComparer),
                    static (o, i) => ((o.HashCode * 19) + GetHashCodeCore(i, o.Comparer), o.Comparer),
                    static o => o.HashCode);

                static int GetHashCodeCore(T? value, IEqualityComparer<T> comparer)
                {
                    return value is null ? 0 : comparer.GetHashCode(value);
                }
            }
        }

        private sealed class PassThroughComparer : IEqualityComparer<T>
        {
            public bool Equals(T x, T y) => x!.Equals(y);

            public int GetHashCode(T obj) => obj!.GetHashCode();
        }
    }
}