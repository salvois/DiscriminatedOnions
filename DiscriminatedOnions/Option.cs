/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022 Salvatore ISAJA. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED THE COPYRIGHT HOLDER ``AS IS'' AND ANY EXPRESS
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;

namespace DiscriminatedOnions;

public abstract record Option<T>
{
    public record None : Option<T>
    {
        internal static readonly Option<T> Value = new None();
        private None() { }
    }

    public record Some : Option<T>
    {
        public T Value { get; }
        internal Some(T value) => Value = value;
        public void Deconstruct(out T value) => value = Value;
    }

    public U Match<U>(Func<U> onNone, Func<T, U> onSome) =>
        this switch
        {
            None => onNone(),
            Some(var v) => onSome(v),
            _ => throw new ArgumentOutOfRangeException()
        };
}

public static class Option
{
    public static Option<T> Some<T>(T value) => new Option<T>.Some(value);
    public static Option<T> None<T>() => Option<T>.None.Value;

    public static Option<U> Bind<T, U>(this Option<T> option, Func<T, Option<U>> binder) =>
        option.Match(None<U>, binder);

    public static bool Contains<T>(this Option<T> option, T value) =>
        option.Match(() => false, v => Equals(v, value));

    public static int Count<T>(this Option<T> option) =>
        option.Match(() => 0, _ => 1);

    public static T DefaultValue<T>(this Option<T> option, T value) =>
        option.Match(() => value, v => v);

    public static T DefaultWith<T>(this Option<T> option, Func<T> defThunk) =>
        option.Match(defThunk, v => v);

    public static bool Exists<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(() => false, predicate);

    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(() => option, v => predicate(v) ? option : None<T>());

    public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
        option.Match(None<T>, v => v);

    public static TState Fold<T, TState>(this Option<T> option, TState initialState, Func<TState, T, TState> folder) =>
        option.Match(() => initialState, v => folder(initialState, v));

    public static bool ForAll<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(() => true, predicate);

    public static T Get<T>(this Option<T> option) =>
        option is Option<T>.Some(var v) ? v : throw new InvalidOperationException();

    public static bool IsNone<T>(this Option<T> option) =>
        option is Option<T>.None;

    public static bool IsSome<T>(this Option<T> option) =>
        option is Option<T>.Some;

    public static void Iter<T>(this Option<T> option, Action<T> action)
    {
        if (option is Option<T>.Some(var v))
            action(v);
    }

    public static Option<U> Map<T, U>(this Option<T> option, Func<T, U> mapping) =>
        option.Match(None<U>, v => Some(mapping(v)));

    public static Option<U> Map2<T1, T2, U>(this (Option<T1>, Option<T2>) options, Func<T1, T2, U> mapping) =>
        options switch
        {
            (Option<T1>.Some(var v1), Option<T2>.Some(var v2)) => Some(mapping(v1, v2)),
            _ => None<U>()
        };

    public static Option<U> Map3<T1, T2, T3, U>(this (Option<T1>, Option<T2>, Option<T3>) options, Func<T1, T2, T3, U> mapping) =>
        options switch
        {
            (Option<T1>.Some(var v1), Option<T2>.Some(var v2), Option<T3>.Some(var v3)) => Some(mapping(v1, v2, v3)),
            _ => None<U>()
        };

    public static Option<T> OfNullable<T>(T? value) where T : struct =>
        value.HasValue ? Some(value.Value) : None<T>();

    public static Option<T> OfObj<T>(T? obj) where T : class =>
        obj != null ? Some(obj) : None<T>();

    public static Option<T> OrElse<T>(this Option<T> option, Option<T> ifNone) =>
        option.Match(() => ifNone, _ => option);

    public static Option<T> OrElseWith<T>(this Option<T> option, Func<Option<T>> ifNoneThunk) =>
        option.Match(ifNoneThunk, _ => option);

    public static T[] ToArray<T>(this Option<T> option) =>
        option.Match(Array.Empty<T>, v => new[] { v });

    public static IEnumerable<T> ToEnumerable<T>(this Option<T> option)
    {
        if (option is Option<T>.Some(var v))
            yield return v;
    }

    public static T? ToNullable<T>(this Option<T> option) where T : struct =>
        option.Match(() => (T?)null, v => v);

    public static T? ToObj<T>(this Option<T> option) where T : class =>
        option.Match(() => (T?)null, v => v);
}