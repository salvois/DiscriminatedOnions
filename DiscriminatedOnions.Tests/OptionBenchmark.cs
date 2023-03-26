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
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture, Explicit]
public static class OptionBenchmark
{
    [Test]
    public static void Benchmark()
    {
        DoBenchmark(1_000_000);
        DoBenchmark(1_000_000_000);
    }

    private static void DoBenchmark(int count)
    {
        var stopwatch = Stopwatch.StartNew();
        var sum = 0L;
        for (var i = 0; i < count; i++)
            if (i % 2 == 0)
                sum += i;
        Console.WriteLine($"Sum {sum} computed imperatively in {stopwatch.ElapsedMilliseconds} ms (g0={GC.CollectionCount(0)}, g1={GC.CollectionCount(1)}, g2={GC.CollectionCount(2)}, g3={GC.CollectionCount(3)}).");

        stopwatch.Restart();
        sum = Enumerable.Range(0, count)
            .Where(i => i % 2 == 0)
            .Aggregate(0L, (acc, i) => acc + i);
        Console.WriteLine($"Sum {sum} computed with Where and Aggregate in {stopwatch.ElapsedMilliseconds} ms (g0={GC.CollectionCount(0)}, g1={GC.CollectionCount(1)}, g2={GC.CollectionCount(2)}, g3={GC.CollectionCount(3)}).");

        stopwatch.Restart();
        sum = Enumerable.Range(0, count)
            .Choose(i => i % 2 == 0 ? Option.Some(i) : Option.None<int>())
            .Aggregate(0L, (acc, i) => acc + i);
        Console.WriteLine($"Sum {sum} computed with Choose and Aggregate in {stopwatch.ElapsedMilliseconds} ms (g0={GC.CollectionCount(0)}, g1={GC.CollectionCount(1)}, g2={GC.CollectionCount(2)}, g3={GC.CollectionCount(3)}).");
    }
}