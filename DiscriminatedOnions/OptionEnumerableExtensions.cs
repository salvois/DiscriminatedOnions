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

public static class OptionEnumerableExtensions
{
    public static IEnumerable<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser)
    {
        foreach (var item in source)
            if (chooser(item) is Option<U>.Some(var v))
                yield return v;
    }

    public static U Pick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser)
    {
        foreach (var item in source)
            if (chooser(item) is Option<U>.Some(var v))
                return v;
        throw new KeyNotFoundException();
    }

    public static Option<T> TryExactlyOne<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var option = Option.Some(enumerator.Current);
            if (!enumerator.MoveNext())
                return option;
        }
        return Option.None<T>();
    }

    public static Option<T> TryFind<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
            if (predicate(item))
                return Option.Some(item);
        return Option.None<T>();
    }

    public static Option<T> TryFindBack<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var option = Option.None<T>();
        foreach (var item in source)
            if (predicate(item))
                option = Option.Some(item);
        return option;
    }

    public static Option<int> TryFindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var count = 0;
        foreach (var item in source)
        {
            if (predicate(item))
                return Option.Some(count);
            count++;
        }
        return Option.None<int>();
    }

    public static Option<int> TryFindIndexBack<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var option = Option.None<int>();
        var count = 0;
        foreach (var item in source)
        {
            if (predicate(item))
                option = Option.Some(count);
            count++;
        }
        return option;
    }

    public static Option<T> TryHead<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext()
            ? Option.Some(enumerator.Current)
            : Option.None<T>();
    }

    public static Option<T> TryItem<T>(this IEnumerable<T> source, int index)
    {
        var count = 0;
        foreach (var item in source)
        {
            if (count == index)
                return Option.Some(item);
            if (count > index)
                break;
            count++;
        }
        return Option.None<T>();
    }

    public static Option<T> TryLast<T>(this IEnumerable<T> source)
    {
        var option = Option.None<T>();
        foreach (var item in source)
            option = Option.Some(item);
        return option;
    }

    public static Option<U> TryPick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser)
    {
        foreach (var item in source)
            if (chooser(item) is Option<U>.Some some)
                return some;
        return Option.None<U>();
    }

    public static IEnumerable<T> Unfold<T, TState>(this TState state, Func<TState, Option<(T, TState)>> generator)
    {
        var currentState = state;
        while (true)
        {
            var option = generator(currentState);
            if (option is Option<(T, TState)>.Some(var v))
            {
                var (el, newState) = v;
                currentState = newState;
                yield return el;
            }
            else break;
        }
    }
}