using System;
using System.Collections.Generic;

namespace Hybel.Monads
{
    /// <summary>
    /// An <see cref="Option{T}"/> helps with representing an non-value without creating errors with null values such as the dreaded <see cref="NullReferenceException"/>.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the <see cref="Option{T}"/>.</typeparam>
    internal readonly struct Option<T> : IEquatable<Option<T>>
    {
        /// <summary>
        /// Value which represents nothing. (kinda like null but without all the annoying errors)
        /// </summary>
        public static Option<T> None = new Option<T>();

        private readonly T _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Create a new Option which contains <paramref name="value"/>.
        /// </summary>
        public Option(T value)
        {
            _value = value;
            _hasValue = value != null;
        }

        /// <summary>
        /// The absolute value contained in the <see cref="Option{T}"/>. <b>This can be null!</b>
        /// </summary>
        internal T DangerousValue => _value;

        /// <summary>
        /// Whether or not the <see cref="Option{T}"/> has a value.
        /// </summary>
        internal bool HasValue => _hasValue;

        public bool Equals(Option<T> other) => EqualityComparer<T>.Default.Equals(_value, other._value) && _hasValue == other._hasValue;
        public override bool Equals(object obj) => obj is Option<T> option && Equals(option);

        public override int GetHashCode() => HashCode.Combine(_value, _hasValue);

        public override string ToString() => $"{(this == None ? $"{nameof(Option<T>)}<{typeof(T).Name}>.{nameof(None)}" : _value.ToString())}";

        public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
        public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

        public static implicit operator Option<T>(T value)
        {
            if (value is null)
                return None;

            return value.Some();
        }
    }

    internal static class OptionExtensions
    {
        /// <summary>
        /// Convert from any type to an option of that type.
        /// </summary>
        /// <returns>An <see cref="Option{T}"/> which has a <paramref name="value"/> within it OR if <paramref name="value"/> is null, an <see cref="Option{T}"/> which does not have a <paramref name="value"/> within it.</returns>
        public static Option<T> Some<T>(this T value) => value is null ? Option<T>.None : new Option<T>(value);

        /// <summary>
        /// Convert <paramref name="value"/> to an option with that value and <u>safely</u> run any <paramref name="function"/> on it.
        /// </summary>
        /// <typeparam name="T">Type of value being converted to an <see cref="Option{T}"/> and then having a <paramref name="function"/> run on it.</typeparam>
        /// <typeparam name="TReturn">Type of value the run returns wrapped in a new <see cref="Option{T}"/>.</typeparam>
        /// <param name="value">Value being converted to and <paramref name="value"/> and then having a <paramref name="function"/> run on it.</param>
        /// <param name="function">Function to run on the <paramref name="value"/>.</param>
        /// <returns>An <see cref="Option{T}"/> of the <paramref name="function"/>s returned value.</returns>
        public static Option<TReturn> Run<T, TReturn>(this T value, Func<T, Option<TReturn>> function) =>
            value.Some().Run(function);

        /// <summary>
        /// <u>Safely</u> run any <paramref name="function"/> on the value contained within <paramref name="option"/>.
        /// </summary>
        /// <typeparam name="T">Type of value having a <paramref name="function"/> run on it.</typeparam>
        /// <typeparam name="TResult">Type of value the run returns wrapped in a new <see cref="Option{T}"/>.</typeparam>
        /// <param name="option"><see cref="Option{T}"/> which contains a value to run the <paramref name="function"/> on.</param>
        /// <param name="function">Function to run on the value contained on <paramref name="option"/>.</param>
        /// <returns>An <see cref="Option{T}"/> of the <paramref name="function"/>s returned value.</returns>
        public static Option<TResult> Run<T, TResult>(this Option<T> option, Func<T, Option<TResult>> function)
        {
            if (function is null || option == Option<T>.None)
                return Option<TResult>.None;

            return function(option.DangerousValue);
        }

        /// <summary>
        /// Checks if the <paramref name="option"/> has a value.
        /// </summary>
        public static bool IsSome<T>(this Option<T> option) => option.HasValue;

        /// <summary>
        /// Checks if the <paramref name="option"/> doesn't have a value.
        /// </summary>
        public static bool IsNone<T>(this Option<T> option) => !option.IsSome();

        /// <summary>
        /// Match the <paramref name="option"/> to have a value and retrieve it.
        /// </summary>
        /// <typeparam name="T">Type of the value contained in <paramref name="option"/>.</typeparam>
        /// <param name="option"><see cref="Option{T}"/> which you expect to have a value.</param>
        /// <param name="value">
        /// The retrieved value from <paramref name="option"/>.
        /// <para><b>This can be null!</b> Use the returned bool to branch based on if <paramref name="value"/> is null or not.</para>
        /// </param>
        /// <returns>True if <paramref name="option"/> has a value. False if <paramref name="option"/> does not have a value.</returns>
        public static bool TryUnwrap<T>(this Option<T> option, out T value)
        {
            value = option.DangerousValue;
            return option.HasValue;
        }

        /// <summary>
        /// Get the value contained in <paramref name="option"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">This exception is thrown if the <paramref name="option"/> has no value.</exception>
        /// <returns>The value of <paramref name="option"/> or null.</returns>
        public static T Unwrap<T>(this Option<T> option)
        {
            if (!option.HasValue)
                throw new InvalidOperationException($"The Option of type {typeof(T)} did not have a value when trying to access it.");

            return option.DangerousValue;
        }

        /// <summary>
        /// Yields one value if <paramref name="option"/> has value, otherwise none.
        /// </summary>
        public static IEnumerable<T> Iterator<T>(this Option<T> option)
        {
            if (option.TryUnwrap(out var value))
                yield return value;
        }

        /// <summary>
        /// Yields only the <see cref="Option{T}.Some(T)"/> variants in the <paramref name="collectionOfOptions"/> after unwrapping them.
        /// </summary>
        /// <param name="collectionOfOptions">Collection containing <see cref="Option{T}"/>s possibly with <see cref="Option{T}.None"/> variants in it.</param>
        /// <returns>New collection where no values can be null.</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<Option<T>> collectionOfOptions)
        {
            foreach (var option in collectionOfOptions)
                if (option.TryUnwrap(out T value))
                    yield return value;
        }

        /// <summary>
        /// Map one <paramref name="option"/> to another using a <paramref name="mapper"/> function.
        /// </summary>
        /// <returns>Returns a new Option with type <typeparamref name="TResult"/> using the <paramref name="mapper"/> to convert if <paramref name="option"/> has a value. Otherwise it returns None.</returns>
        public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (!option.TryUnwrap(out T value))
                return Option<TResult>.None;

            return mapper(value).Some();
        }
    }
}
