/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022-2024 Salvatore ISAJA. All rights reserved.

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
using System.Threading.Tasks;

namespace DiscriminatedOnions;

/// Union type that can represent either Ok(resultValue) or Error(errorValue)
public readonly record struct Result<T, TError>
{
    public bool IsOk { get; }
    public T ResultValue { get; }
    public TError ErrorValue { get; }

    public U Match<U>(Func<TError, U> onError, Func<T, U> onOk) =>
        this switch
        {
            { IsOk: false, ErrorValue: var v } => onError(v),
            { IsOk: true, ResultValue: var v } => onOk(v)
        };

    public void Match(Action<TError> onError, Action<T> onOk)
    {
        switch (this)
        {
            case { IsOk: false, ErrorValue: var v }: onError(v); break;
            case { IsOk: true, ResultValue: var v }: onOk(v); break;
        }
    }

    public override string ToString() =>
        IsOk ? $"Ok({ResultValue})" : $"Error({ErrorValue})";

    internal Result(bool isOk, T resultValue, TError errorValue)
    {
        IsOk = isOk;
        ResultValue = resultValue;
        ErrorValue = errorValue;
    }
}

/// Utility functions for the Result type
public static class Result
{
    /// Creates a new result containing Ok(resultValue)
    public static Result<T, TError> Ok<T, TError>(T resultValue) =>
        new(true, resultValue, default!);

    /// Creates a new result containing Error(errorValue)
    public static Result<T, TError> Error<T, TError>(TError errorValue) =>
        new(false, default!, errorValue);

    /// Returns binder(v) if result is Ok(v) or Error(e) if it is Error(e)
    public static Result<U, TError> Bind<T, TError, U>(this Result<T, TError> result, Func<T, Result<U, TError>> binder) =>
        result.Match(Error<U, TError>, binder);

    /// Returns binder(v) if result is Ok(v) or Error(e) if it is Error(e)
    public static Task<Result<U, TError>> BindAsync<T, TError, U>(this Result<T, TError> result, Func<T, Task<Result<U, TError>>> binder) =>
        result.Match(e => Task.FromResult(Error<U, TError>(e)), binder);

    /// Returns true if result is Ok(value) or false if it is not
    public static bool Contains<T, TError>(this Result<T, TError> result, T value) =>
        result.Match(_ => false, v => Equals(v, value));

    /// Returns 1 if result is Ok(v) or 0 if it is Error(e)
    public static int Count<T, TError>(this Result<T, TError> result) =>
        result.Match(_ => 0, _ => 1);

    /// Returns v if result is Ok(v) or value if it is Error(e)
    public static T DefaultValue<T, TError>(this Result<T, TError> result, T value) =>
        result.Match(_ => value, v => v);

    /// Returns v if result is Ok(v) or defThunk(e) if it is Error(e)
    public static T DefaultWith<T, TError>(this Result<T, TError> result, Func<TError, T> defThunk) =>
        result.Match(defThunk, v => v);

    /// Returns v if result is Ok(v) or defThunk(e) if it is Error(e)
    public static Task<T> DefaultWithAsync<T, TError>(this Result<T, TError> result, Func<TError, Task<T>> defThunk) =>
        result.Match(defThunk, Task.FromResult);

    /// Returns predicate(v) if result is Ok(v) or false if it is Error(e)
    public static bool Exists<T, TError>(this Result<T, TError> result, Func<T, bool> predicate) =>
        result.Match(_ => false, predicate);

    /// Returns folder(initialState, v) if result is Ok(v) or initialState if it is Error(e)
    public static TState Fold<T, TError, TState>(this Result<T, TError> result, TState initialState, Func<TState, T, TState> folder) =>
        result.Match(_ => initialState, v => folder(initialState, v));

    /// Returns predicate(v) if result is Ok(v) or true if it is Error(e)
    public static bool ForAll<T, TError>(this Result<T, TError> result, Func<T, bool> predicate) =>
        result.Match(_ => true, predicate);

    /// Returns v if result is Ok(v) or throws an InvalidOperationException if it is Error(e), discouraged
    public static T Get<T, TError>(this Result<T, TError> result) =>
        result is { IsOk: true, ResultValue: var v } ? v : throw new InvalidOperationException();

    /// Returns e if result is Error(e) or throws an InvalidOperationException if it is Ok(v), discouraged
    public static TError GetError<T, TError>(this Result<T, TError> result) =>
        result is { IsOk: false, ErrorValue: var e } ? e : throw new InvalidOperationException();

    /// Returns true if result is Error(e), discouraged
    public static bool IsError<T, TError>(this Result<T, TError> result) =>
        !result.IsOk;

    /// Returns true if result is Ok(v), discouraged
    public static bool IsOk<T, TError>(this Result<T, TError> result) =>
        result.IsOk;

    /// Executes action(v) if result is Ok(v)
    public static void Iter<T, TError>(this Result<T, TError> result, Action<T> action)
    {
        if (result is { IsOk: true, ResultValue: var v })
            action(v);
    }

    /// Executes action(v) if result is Ok(v)
    public static Task IterAsync<T, TError>(this Result<T, TError> result, Func<T, Task> action) =>
        result is { IsOk: true, ResultValue: var v } ? action(v) : Task.CompletedTask;

    /// Executes action(e) if result is Error(e)
    public static void IterError<T, TError>(this Result<T, TError> result, Action<TError> action)
    {
        if (result is { IsOk: false, ErrorValue: var e })
            action(e);
    }

    /// Executes action(e) if result is Error(e)
    public static Task IterErrorAsync<T, TError>(this Result<T, TError> result, Func<TError, Task> action) =>
        result is { IsOk: false, ErrorValue: var e } ? action(e) : Task.CompletedTask;

    /// Returns Ok(mapping(v)) is result is Ok(v) or Error(e) if it is Error(e)
    public static Result<U, TError> Map<T, TError, U>(this Result<T, TError> result, Func<T, U> mapping) =>
        result.Match(Error<U, TError>, v => Ok<U, TError>(mapping(v)));

    /// Returns Ok(mapping(v)) is result is Ok(v) or Error(e) if it is Error(e)
    public static Task<Result<U, TError>> MapAsync<T, TError, U>(this Result<T, TError> result, Func<T, Task<U>> mapping) =>
        result.Match(e => Task.FromResult(Error<U, TError>(e)), async v => Ok<U, TError>(await mapping(v)));

    /// Returns Error(mapping(e)) if result is Error(e) or Ok(v) if it is Ok(v)
    public static Result<T, U> MapError<T, TError, U>(this Result<T, TError> result, Func<TError, U> mapping) =>
        result.Match(e => Error<T, U>(mapping(e)), Ok<T, U>);

    /// Returns Error(mapping(e)) if result is Error(e) or Ok(v) if it is Ok(v)
    public static Task<Result<T, U>> MapErrorAsync<T, TError, U>(this Result<T, TError> result, Func<TError, Task<U>> mapping) =>
        result.Match(async e => Error<T, U>(await mapping(e)), v => Task.FromResult(Ok<T, U>(v)));

    /// Returns a single-element array containing v if result is Ok(v) or an empty array if it is Error(e)
    public static T[] ToArray<T, TError>(this Result<T, TError> result) =>
        result.Match(_ => Array.Empty<T>(), v => new[] { v });

    /// Returns a single-element enumerable containing v if option is Ok(v) or an empty enumerable if it is Error(e)
    public static IEnumerable<T> ToEnumerable<T, TError>(this Result<T, TError> result)
    {
        if (result is { IsOk: true, ResultValue: var v })
            yield return v;
    }

    /// Returns Some(v) if result is Ok(v) otherwise returns None
    public static Option<T> ToOption<T, TError>(this Result<T, TError> result) =>
        result.Match(_ => Option.None<T>(), Option.Some);
}