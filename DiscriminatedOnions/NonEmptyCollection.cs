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

public interface INonEmptyCollection<out T> : INonEmptyEnumerable<T> { }

public static class NonEmptyCollection
{
    public static Option<INonEmptyCollection<T>> TryCreateNonEmptyCollection<T>(this IReadOnlyCollection<T> collection) =>
        collection.Any()
            ? Option.Some<INonEmptyCollection<T>>(new NonEmptyCollection<T>(collection))
            : Option.None<INonEmptyCollection<T>>();
}

internal readonly record struct NonEmptyCollection<T> : INonEmptyCollection<T>
{
    private readonly IReadOnlyCollection<T> _elements;

    internal NonEmptyCollection(IReadOnlyCollection<T> elements) =>
        _elements = elements.Any()
            ? elements
            : throw new ArgumentException("Empty collection used to construct a NonEmptyCollection. This should never happen.");

    public IEnumerator<T> GetEnumerator() => _elements.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _elements.Count;
}