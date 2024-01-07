/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022-2023 Salvatore ISAJA. All rights reserved.

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiscriminatedOnions;

public interface INonEmptyEnumerable<out T> : IEnumerable<T> { }

// Note: INonEmptyEnumerable<T> parameters must not be IEnumerable<T> to avoid recursive calls
public static class NonEmptyEnumerable
{
    public static INonEmptyEnumerable<T> Append<T>(this INonEmptyEnumerable<T> source, T element) =>
        new NonEmptyEnumerable<T>(source.AsEnumerable().Append(element));

    public static INonEmptyEnumerable<T> Concat<T>(this INonEmptyEnumerable<T> first, IEnumerable<T> second) =>
        new NonEmptyEnumerable<T>(first.AsEnumerable().Concat(second));

    public static INonEmptyEnumerable<T> Concat<T>(this IEnumerable<T> first, INonEmptyEnumerable<T> second) =>
        new NonEmptyEnumerable<T>(first.Concat(second.AsEnumerable()));

    public static INonEmptyEnumerable<T> Of<T>(T firstElement, params T[] otherElements) =>
        new NonEmptyEnumerable<T>(new[] { firstElement }.Concat(otherElements));

    public static INonEmptyEnumerable<TOut> Select<TIn, TOut>(this INonEmptyEnumerable<TIn> source, Func<TIn, TOut> mapper) =>
        new NonEmptyEnumerable<TOut>(source.AsEnumerable().Select(mapper));

    public static INonEmptyCollection<T> ToNonEmptyCollection<T>(this INonEmptyEnumerable<T> source) =>
        new NonEmptyCollection<T>(source.AsEnumerable().ToList());

    public static INonEmptyList<T> ToNonEmptyList<T>(this INonEmptyEnumerable<T> source) =>
        new NonEmptyList<T>(source.AsEnumerable().ToList());
}

internal readonly record struct NonEmptyEnumerable<T> : INonEmptyEnumerable<T>
{
    private readonly IEnumerable<T> _elements;

    internal NonEmptyEnumerable(IEnumerable<T> elements) => _elements = elements;

    public IEnumerator<T> GetEnumerator() => _elements.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}