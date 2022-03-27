# DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

[![NuGet](https://img.shields.io/nuget/v/DiscriminatedOnions.svg)](https://www.nuget.org/packages/DiscriminatedOnions)

Welcome to this hacky, tiny and hopefully useful library that aims to bring some discriminated unions to C#, together with a bunch of techniques to roll your own ones.

This is heavily inspired (as in "shamelessly copied") from F# discriminated union types and standard library, so you will find types and utility functions for `Option`s, `Result`s and `Choice`s, as well as poor man's `unit` type and `|>` (pipe) operator.

I have written this library because I needed it and because it was fun, but it doesn't claim to be something as structured as the [OneOf](https://github.com/mcintyre321/OneOf/) library. In my view, it is just something temporary that may be useful until the C# language gets native support for those features (hopefully soon).

## Table of contents

  - [Rolling your own discriminated unions](#rolling-your-own-discriminated-unions)
  - [Option type](#option-type)
    - [Utility functions for the Option type](#utility-functions-for-the-option-type)
    - [Utility functions for IEnumerable involving the Option type](#utility-functions-for-ienumerable-involving-the-option-type)
  - [Result type](#result-type)
    - [Utility functions for the Result type](#utility-functions-for-the-result-type)
  - [Choice types](#choice-types)
  - [Single-case union types](#single-case-union-types)
  - [Unit type](#unit-type)
  - [Piping function calls](#piping-function-calls)
  - [Caveats](#caveats)
  - [License](#license)

## Rolling your own discriminated unions

If you are here, you probably know better than me what a discriminated union is :) \
In a nutshell, it is a way to represent a data type that may be one of multiple cases, so that you can better express your application domain and receive better help from the compiler when you try to shoot on your foot.

The idea this library is based on is [ab]using C# 9 records to emulate F#-like discriminated unions, like this:
```csharp
public abstract record Shape {
    public record Rectangle(double Width, double Height) : Shape;
    public record Circle(double Radius) : Shape;
}

var rectangle = new Shape.Rectangle(10.0, 1.3);
var circle = new Shape.Circle(1.0);
```

You can then match union cases with the C# 8 `switch` expression and pattern matching, like this:

```csharp
var area =
    shape switch {
        Shape.Rectangle r => r.Width * r.Height,
        Shape.Circle c => c.Radius * c.Radius * Math.PI,
        _ => throw new ArgumentOutOfRangeException() // This is one point where this approach is inferior to real discriminated unions
    };

// or using deconstruction:

var area =
    shape switch
    {
        Shape.Rectangle(var width, var height) => width * height,
        Shape.Circle(var radius) => radius * radius * Math.PI,
        _ => throw new ArgumentOutOfRangeException()
    };
```

This lets you use the full power of pattern matching, including matching tuples or including conditions, but unfortunately the compiler has no way to tell that your `switch` expression is exhaustive, that is, you have covered all possible cases. If you want this extra safety at the expenses of flexibility you can roll your own matching method, like the following inspired from the F# `match` expression:

```csharp
public abstract record Shape {
    public record Rectangle(double Width, double Height) : Shape;
    public record Circle(double Radius) : Shape;

    public U Match<U>(Func<double, double, U> onRectangle, Func<double, U> onCircle) =>
        this switch
        {
            Rectangle(var width, var height) => onRectangle(width, height),
            Circle(var radius) => onCircle(radius),
            _ => throw new ArgumentOutOfRangeException()
        };
}

var area =
    shape.Match(
        onRectangle: (w, h) => w * h,
        onCircle: r => r * r * Math.PI);
```

`Match` is implemented as a method to stay close to the declaration of union cases. You may want to use named arguments when invoking `Match`, so that it is clear what union cases you are handling. Being immutable, a discriminated union like this can be operated on by non-member functions, such as extension methods, built on top of `Match` or the plain `switch`.

The functionality offered by this library to enable the above is... nothing! It is just a technique to leverage what is built into the language, and you are encouraged to try and make your own for your application domain. This library, however, offers some ready-made discriminated unions that have a very common use, and they are listed below.

## Option type

`Option` is a union type that can represent either `Some` value, or `None` to indicate that no value is present. Think of `null` as it should have been. The key feature is that the compiler will force you to handle both the `Some` and the `None` cases explicitly. Think of it as if was defined as:

```csharp
public abstract record Option<T>
{
    public record None : Option<T>;
    public record Some(T Value) : Option<T>;

    public U Match<U>(Func<U> onNone, Func<T, U> onSome);
}
```
The actual definition is a little more complex, to force you to use one of the named constructors explained below, and access the value of the `Some` case only using deconstruction.

### Utility functions for the Option type

Together with the `Option<T>` type itself, a companion static class `Option` of utility functions is provided, again heavily inspired from the [F# Option module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-optionmodule.html). Besides functions such as `Bind`, `DefaultValue`, `Iter`, `Map`, `OfNullable`, `OfObj` and more, the `Option` static class also provides named constructors to create `Option<T>` values. The name constructor for `Some` helps the compiler deduce the generic argument for `T` reducing some bolilerplate, whereas the named constructor for `None` returns a singleton instance for `T`.

```csharp
public static class Option
{
    public static Option<T> Some<T>(T value);
    public static Option<T> None<T>();

    public static Option<U> Bind<T, U>(this Option<T> option, Func<T, Option<U>> binder);
    public static bool Contains<T>(this Option<T> option, T value);
    public static int Count<T>(this Option<T> option);
    public static T DefaultValue<T>(this Option<T> option, T value);
    public static T DefaultWith<T>(this Option<T> option, Func<T> defThunk);
    public static bool Exists<T>(this Option<T> option, Func<T, bool> predicate);
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate);
    public static Option<T> Flatten<T>(this Option<Option<T>> option);
    public static T Get<T>(this Option<T> option);
    public static bool IsNone<T>(this Option<T> option);
    public static bool IsSome<T>(this Option<T> option);
    public static void Iter<T>(this Option<T> option, Action<T> action);
    public static Option<U> Map<T, U>(this Option<T> option, Func<T, U> mapping);
    public static Option<T> OfNullable<T>(T? value) where T : struct;
    public static Option<T> OfObj<T>(T? obj) where T : class;
    public static Option<T> OrElse<T>(this Option<T> option, Option<T> ifNone);
    public static Option<T> OrElseWith<T>(this Option<T> option, Func<Option<T>> ifNoneThunk);
    public static T? ToNullable<T>(this Option<T> option) where T : struct;
    public static T? ToObj<T>(this Option<T> option) where T : class;
}

Option<string> someString = Option.Some("I have a value");
Option<string> noString = Option.None<string>();

someString.Bind(v => v + " today");
// returns Option.Some("I have a value today")

noString.DefaultValue("default value");
// returns "default value"
```

### Utility functions for IEnumerable involving the Option type

An `OptionEnumerableExtensions` static class of extension methods is provided to enrich LINQ functionality on `IEnumerable`s with the `Option` type. This similar to functionality of the [F# Seq module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-seqmodule.html) for functions involving `Option`s either as argument or return value, such as `Choose`, `Pick`, `TryFind`, `TryHead`, `Unfold` and more:

```csharp
public static class OptionEnumerableExtensions
{
    public static IEnumerable<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);
    public static U Pick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);
    public static Option<T> TryExactlyOne<T>(this IEnumerable<T> source);
    public static Option<T> TryFind<T>(this IEnumerable<T> source, Func<T, bool> predicate);
    public static Option<T> TryFindBack<T>(this IEnumerable<T> source, Func<T, bool> predicate);
    public static Option<int> TryFindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate);
    public static Option<int> TryFindIndexBack<T>(this IEnumerable<T> source, Func<T, bool> predicate);
    public static Option<T> TryHead<T>(this IEnumerable<T> source);
    public static Option<T> TryItem<T>(this IEnumerable<T> source, int index);
    public static Option<T> TryLast<T>(this IEnumerable<T> source);
    public static Option<U> TryPick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);
    public static IEnumerable<T> Unfold<T, TState>(this TState state, Func<TState, Option<(T, TState)>> generator);
}

new[] { 1, 2, 3, 4, 5 }.Choose(i => i % 2 == 0 ? Option.Some(i) : Option.None<int>());
// returns { 2, 4 }

new[] { 1, 2, 3, 4, 5 }.TryFind(i => i % 2 == 0);
// returns Option.Some(2)
```

## Result type

`Result` is intended to represent the result of an operation or validation, that can be either `Ok` or `Error`, both carrying a payload:

```csharp
public abstract record Result<T, TError>
{
    public record Error(TError ErrorValue) : Result<T, TError>;
    public record Ok(T ResultValue) : Result<T, TError>;

    public U Match<U>(Func<TError, U> onError, Func<T, U> onOk);
}
```

### Utility functions for the Result type

Together with the `Result<T>` type itself, a companion static class `Result` of utility functions is provided, including `Bind`, `Map` and `MapError` as defined in the [F# Result module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-resultmodule.html). Especially, `Bind` is very convenient when chaining functions when the result of the previous one becomes the input of the next one, something described as [Railway oriented programming](https://fsharpforfunandprofit.com/posts/recipe-part2/) in the famous [F# for Fun and Profit](https://fsharpforfunandprofit.com/) site.

```csharp
public static class Result
{
    public static Result<U, TError> Bind<T, TError, U>(this Result<T, TError> result, Func<T, Result<U, TError>> binder);
    public static Result<U, TError> Map<T, TError, U>(this Result<T, TError> result, Func<T, U> mapping);
    public static Result<T, U> MapError<T, TError, U>(this Result<T, TError> result, Func<TError, U> mapping);
}

var ok = new Result<string, int>.Ok("result value");
var error = new Result<string, int>.Error(42);

ok.Bind(v => "beautiful " + v);
// returns new Results<string, int>.Ok("beautiful result value")

error.Bind(v => "beautiful " + v);
// returns new Result<string, int>.Error(42), short-circuiting
```

## Choice types

`Choice` types are ready-made generic discriminated unions to represent one of multiple cases, with the downside of having non-mnemonic case names. They are defined like the following one, but versions with 3, 4, 5, 6 and 7 generic parameters are also defined:

```csharp
public abstract record Choice<T1, T2>
{
    public record Choice1(T1 Item) : Choice<T1, T2>;
    public record Choice2(T2 Item) : Choice<T1, T2>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2);
}
```

This library includes no helper functions for `Choice` types, and, to be honest, I have included `Choice` types themselves more as an exercise and a demonstration than as a useful tool. In my opinion rolling your own discriminated union, with names and properties meaningful for your domain, is a much better option (pun intended).

## Single-case union types

A common use of F# discriminated unions is to create a union with a single case, to create strong types for otherwise primitive types, such as `string`s or `int`s. This is very convenient when, for example, you don't want to pass a customer ID to a function expecting an order ID and both are integer values.

C# records can be [ab]used again to provide such strong types. In C# 10 you can even declare them as `readonly record struct`s to avoid the extra dereference and bookkeeping, making them zero-cost abstractions.

```csharp
// C# 9
public record CustomerId(int Value);
public record OrderId(int Value);

// C# 10
public readonly record struct CustomerId(int Value);
public readonly record struct OrderId(int Value);
```

This library provides no features to create such types, because the built-in feature of the language can be leveraged, but I encourage to try this technique to let the compiler stop you when you try to shoot yourself. A properly modeled domain can dramatically reduce nasty bugs caused by the so called "primitive obsession".

## Unit type

OK, this is not a discriminated union, but we need it when we work with generic functions such as the ones in the `Option` "module". If you ever had to provide two nearly identical implementations, one taking `Action` and one taking `Func`, just because you cannot write `Func<void>` you know what I mean. Think of the unit type as the `void` as it should have been, that is a type useable as generic type argument (looks that may be the case in future C# versions).

This library provides the `Unit` type defined simply as a `record` with no properties. You can use it in your function signatures where you have to return nothing (that is, functions useful only for their side effects). The `unit` type in F# can only have one value, that is `()`. To emulate this in C#, the `Unit` record provides a static value named `Value` (which happens to be `null` as in the F# implementation) and a private constructor to prevent creating other `Unit` values:

```csharp
public record Unit
{
    private Unit() { }
    public static readonly Unit Value = null!;
}

// As an example, Match accepts Func's, not Action's:
shape.Match(
    onRectangle: (w, h) =>
    {
        Console.WriteLine($"Rectangle with width {w} and height {h}.");
        return Unit.Value; // unfortunately you have to return it explicitly
    }
    onCircle: r =>
    {
        Console.WriteLine($"Circle with radius {r}.");
        return Unit.Value;
    }
);
```

## Piping function calls

A feature commonly used when working with functions is calling them in a so-called pipeline where the output of the previous function becomes the input of the next one. F# uses its signature `|>` (pipe) operator to facilitate this. C# uses extension methods to offer similar functionality.

This library provides the following extension methods to emulate the pipe operator, letting you chain multiple function calls without writing ad hoc extension methods:

```csharp
public static class PipeExtensions
{
    public static TOut Pipe<TIn, TOut>(this TIn previous, Func<TIn, TOut> next) =>
        next(previous);

    public static T PipeIf<T>(this T previous, Func<T, bool> predicate, Func<T, T> next) =>
        predicate(previous) ? next(previous) : previous;
}

21.Pipe(v => v + 1).Pipe(v => v * 2);
// returns 42

20.PipeIf(v => v < 10, v => v * 2);
// returns 20
```

`Pipe` is defined just like F#'s `|>`, that is the `previous` value is passed to the `next` lambda function, effectively inverting the order they are written. `PipeIf` calls the `next` lambda function only if the specified predicate returns true, allowing pipelines with optional steps.

Both `Pipe` and `PipeIf` provide four overloads (not shown here for conciseness), where either `previous` or `next` are a `Task` that must be awaited, allowing mixed pipelines of synchronous and asynchronous steps.


## Caveats

As said, the `switch` expression using run-time type information does work, but the compiler has no real knowledge that your union cases are part of the same discriminated union, so it will do nothing to protect you from your future self adding a union case and not handling them on all usage sites. A `Match` method may be used to mitigate this issue.

Union cases for discriminated unions implemented as proposed here are full-fledged types. This means that you can have a function accepting a parameter of type `Option<string>.Some` instead of `Option<string>`, which doesn't make any sense for us but is perfectly fine for the C# type system. There is no way to protect yourself from doing this with this approach, so you should simply not do it.

For the same reason, you should refrain extending the base abstract record outside the declaration of the discriminated union: just pretend it is `sealed` even if it can't be.

Discriminated unions implemented as proposed here are reference types, so they have a cost in terms of performance and memory. You may want to take this into account when developing performance-sensitive pieces of code. F# lets you create value-type discriminated unions (`Result` in F# actually is) by annotating them with `[<Struct>]`, for scenarios when a value type can provide a benefit. Hovever, even if C# 10 lets you define `record struct`s, they cannot be used as shown here because of the [ab]use of the inheritance to leverage run-time type information and C# pattern matching. You could roll your own discriminated union using structs and tag fields, and in some cases that may be a good idea, but this prevents you to use C# pattern matching in the same way.

## License

Permissive, [2-clause BSD style](https://opensource.org/licenses/BSD-2-Clause)

DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022 Salvatore ISAJA. All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.