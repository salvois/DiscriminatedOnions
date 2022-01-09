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
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture, Explicit]
public static class SingleCaseUnionTest
{

    private record CustomerId(int Value);

    private readonly record struct StructCustomerId(int Value);

    private static void BenchmarkPrimitive(int idsPerRound, StreamWriter streamWriter)
    {
        for (var id = 0; id < idsPerRound; id++)
            streamWriter.Write(id);
    }

    private static void BenchmarkRecord(int idsPerRound, StreamWriter streamWriter)
    {
        for (var id = 0; id < idsPerRound; id++)
            streamWriter.Write(new CustomerId(id).Value);
    }

    private static void BenchmarkRecordStruct(int idsPerRound, StreamWriter streamWriter)
    {
        for (var id = 0; id < idsPerRound; id++)
            streamWriter.Write(new StructCustomerId(id).Value);
    }

    [TestCase(10000, 100_000, 1000)] // try to fit everything in L1 cache
    [TestCase(10, 10, 10_000_000)] // cache-unfriendly test
    public static void Benchmark(int warmupRoundCount, int roundCount, int idsPerRound)
    {
        using var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);

        for (var i = 0; i < warmupRoundCount; i++)
        {
            stream.Position = 0;
            BenchmarkPrimitive(idsPerRound, streamWriter);
            BenchmarkRecord(idsPerRound, streamWriter);
            BenchmarkRecordStruct(idsPerRound, streamWriter);
        }

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < roundCount; i++)
        {
            stream.Position = 0;
            BenchmarkPrimitive(idsPerRound, streamWriter);
        }
        Console.WriteLine($"Plain int completed in {stopwatch.ElapsedMilliseconds} ms.");

        stopwatch.Restart();
        for (var i = 0; i < roundCount; i++)
        {
            stream.Position = 0;
            BenchmarkRecord(idsPerRound, streamWriter);
        }
        Console.WriteLine($"Record completed in {stopwatch.ElapsedMilliseconds} ms.");

        stopwatch.Restart();
        for (var i = 0; i < roundCount; i++)
        {
            stream.Position = 0;
            BenchmarkRecordStruct(idsPerRound, streamWriter);
        }
        Console.WriteLine($"Record struct completed in {stopwatch.ElapsedMilliseconds} ms.");
    }
}