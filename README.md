# Discriminated Onions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

[![NuGet](https://img.shields.io/nuget/v/DiscriminatedOnions.svg)](https://www.nuget.org/packages/DiscriminatedOnions)

Welcome to this hacky, tiny and hopefully useful .NET 6 library that aims to bring some discriminated unions to C#, together with a bunch of techniques to roll your own ones.

This is heavily inspired (as in "shamelessly copied") from F# discriminated union types and standard library, so you will find types and utility functions for `Option`s, `Result`s and `Choice`s, as well as poor man's `unit` type and `|>` (pipe) operator.

I have written this library because I needed it and because it was fun, but it doesn't claim to be something as structured as the [OneOf](https://github.com/mcintyre321/OneOf/) library. In my view, it is just something temporary that may be useful until the C# language gets native support for those features (hopefully soon).

## Table of contents

  - [Changelog](#changelog)
  - [Rolling your own discriminated unions](#rolling-your-own-discriminated-unions)
  - [Reference type vs. value type discriminated unions](#reference-type-vs-value-type-discriminated-unions)
  - [Option type](#option-type)
    - [Utility functions for the Option type](#utility-functions-for-the-option-type)
    - [Utility functions for IEnumerable involving the Option type](#utility-functions-for-ienumerable-involving-the-option-type)
  - [Result type](#result-type)
    - [Utility functions for the Result type](#utility-functions-for-the-result-type)
  - [Choice types](#choice-types)
  - [Single-case union types](#single-case-union-types)
  - [Unit type](#unit-type)
  - [Piping function calls](#piping-function-calls)
  - [Non-empty enumerable and collections](#non-empty-enumerable-and-collections)
    - [Non-empty enumerables](#non-empty-enumerables)
    - [Non-empty collections](#non-empty-collections)
    - [Non-empty lists](#non-empty-lists)
  - [Helpers to work with read-only collections](#helpers-to-work-with-read-only-collections)
  - [License](#license)

## Changelog

* 1.4: `DefaultWithAsync` and `OrElseAsync` for `Option`
* 1.3: Fluent `ToOption` for `Option`; non-empty collections; helpers for read-only collections
* 1.2: Async versions of `bind`, `iter` and `map` for `Option` and `Result`
* 1.1: `Option`-based `TryGetValue` for dictionaries
* 1.0: Finalized API with `Option` and `Result` reimplemented as value types for better performance

## Rolling your own discriminated unions

If you are here, you probably know better than me what a discriminated union is :) \
In a nutshell, it is a way to represent a data type that may be one of multiple cases, so that you can better express your application domain and receive better help from the compiler when you try to shoot on your foot.

The idea this library is based on is [ab]using C# 9 records to emulate F#-like discriminated unions, like this:
```csharp
public abstract record Shape {
    public record Rectangle(double Width, double Height) : Shape;
    public record Circle(double Radius) : Shape;
    private Shape() { }
}

var rectangle = new Shape.Rectangle(10.0, 1.3);
var circle = new Shape.Circle(1.0);
```

Note that union cases are written inside the base abstract record, which should then be used as a discriminated union type, and that a private constructor has been implemented in the base abstract record to forbid creation of unexpected new union cases.

Once you have your discriminated union, you can then match union cases with the C# 8 `switch` expression and pattern matching, like this:

```csharp
var area = shape switch {
    Shape.Rectangle r => r.Width * r.Height,
    Shape.Circle c => c.Radius * c.Radius * Math.PI,
    // The need for a default arm is where this approach is inferior to real discriminated unions
    _ => throw new ArgumentOutOfRangeException()
};
```

This lets you use the full power of pattern matching, including matching tuples, deconstruction or additional conditions, but unfortunately the compiler has no way to tell that your `switch` expression is exhaustive, that is, you have covered all possible cases, which is *the* killer feature of real discriminated unions.

If you want this extra safety at the expenses of flexibility you can roll your own matching method, like the following inspired from the F# `match` expression. Thus, your complete `Shape` "discriminated onion" would look like this:

```csharp
public abstract record Shape {
    public record Rectangle(double Width, double Height) : Shape;
    public record Circle(double Radius) : Shape;

    public U Match<U>(Func<Rectangle, U> onRectangle, Func<Circle, U> onCircle) =>
        this switch
        {
            Rectangle r => onRectangle(r),
            Circle c => onCircle(c),
            _ => throw new ArgumentOutOfRangeException()
        };

    private Shape() { }
}

var area = shape.Match(
    onRectangle: r => r.Width * r.Height,
    onCircle: c => c.Radius * c.Radius * Math.PI);
```

`Match` is implemented as a method to stay close to the declaration of union cases. You may want to use named arguments when invoking `Match`, so that it is clear what union cases you are handling. Being immutable, a discriminated union like this can be operated on by non-member functions, such as extension methods, built on top of `Match` or the plain `switch`.

The functionality offered by this library to enable the above is... nothing! It is just a technique to leverage what is built into the language, and you are encouraged to try and make your own for your application domain. This library, however, offers some ready-made discriminated unions that have a very common use, and they are listed below.

## Reference type vs. value type discriminated unions

Discriminated unions implemented as proposed above are reference types, so they have a cost in terms of performance and memory. You may want to take this into account when developing performance-sensitive pieces of code. For example, F# lets you create value-type discriminated unions (`Result` in F# actually is) by annotating them with `[<Struct>]`, for scenarios where a value type can provide a benefit.

In some cases, rolling your own discriminated-union-like value types, for example using C# 10 `readonly record struct`s, may be desirable. The idea is embedding in your struct all possible cases as properties and use some tag (such as an enum) to identify which one is currently "active". For an example, see how `Option<T>` and `Result<T, TError>` are implemented in this library.

Avoiding inheritance to achieve value-type unions usually costs more in terms of boiler plate code, prevents to leverage run-time type information for union cases, and prevents you to use pattern matching in the way described above (rather, you would pattern match on the tag), but it may let you gain better performance and lower pressure on the garbage collector.

## Option type

`Option` is a union type that can represent either `Some` value, or `None` to indicate that no value is present. Think of `null` as it should have been. The key feature is that the compiler will force you to handle both the `Some` and the `None` cases explicitly.

Since `Option<T>` is intended to be used pervasively, it is implemented as a value type defined like:

```csharp
public readonly record struct Option<T>
{
    public bool IsSome { get; }
    public T Value { get; } // undefined if IsSome is false

    public U Match<U>(Func<U> onNone, Func<T, U> onSome);
    public void Match(Action onNone, Action<T> onSome);
    public override string ToString();

    internal static readonly Option<T> None = new(false, default!);
    internal Option(bool isSome, T value);
}
```
Properties are public for compatibility with test libraries, and to let to use C# pattern matching in those cases `Match` could not be used. **Accessing `IsSome` and `Value` directly (especially the latter) is generally discouraged**. The constructor is internal to force you to use one of the named constructors explained below.

A `ToString` override provides representations such as `"Some(Value)"` or `"None"` for debugging convenience.

### Utility functions for the Option type

Together with the `Option<T>` type itself, a companion static class `Option` of utility functions is provided. For a full description of those functions please refer to the official documentation of the [F# Option module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-optionmodule.html).

Moreover, some novel functions not included in the F# standard library are included, such as async versions that may be useful when working with I/O.

The `Option` static class also provides named constructors to create `Option<T>` values. The named constructor for `Some` helps the compiler deduce the generic argument for `T` reducing some bolilerplate, whereas the named constructor for `None` returns a singleton instance for `T`.

```csharp
public static class Option
{
    /// Creates a new option containing Some(value)
    public static Option<T> Some<T>(T value);

    /// Returns an option representing None
    public static Option<T> None<T>();

    /// Returns an option representing None wrapped in a Task
    public static Task<Option<T>> TaskNone<T>();


    /// Returns binder(v) if option is Some(v) or None if it is None
    public static Option<U>       Bind     <T, U>(this Option<T> option, Func<T, Option<U>>       binder);
    public static Task<Option<U>> BindAsync<T, U>(this Option<T> option, Func<T, Task<Option<U>>> binder);

    /// Returns true if option is Some(value) or false if it is None
    public static bool Contains<T>(this Option<T> option, T value);

    /// Returns 1 if option is Some(v) or 0 if it is None
    public static int Count<T>(this Option<T> option);

    /// Returns v if option is Some(v) or value if it is None
    public static T DefaultValue<T>(this Option<T> option, T value);

    /// Returns v if option is Some(v) or defThunk() if it is None
    public static T       DefaultWith     <T>(this Option<T> option, Func<T>       defThunk);
    public static Task<T> DefaultWithAsync<T>(this Option<T> option, Func<Task<T>> defThunk);

    /// Returns predicate(v) if option is Some(v) or false if it is None
    public static bool Exists<T>(this Option<T> option, Func<T, bool> predicate);

    /// Returns option if option is Some(v) and predicate(v) is true
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate);

    /// Returns Some(v) if option is Some(Some(v))
    public static Option<T> Flatten<T>(this Option<Option<T>> option);

    /// Returns folder(initialState, v) if option is Some(v) or initialState if it is None
    public static TState Fold<T, TState>(this Option<T> option, TState initialState, Func<TState, T, TState> folder);

    /// Returns predicate(v) if option is Some(v) or true if it is None
    public static bool ForAll<T>(this Option<T> option, Func<T, bool> predicate);

    /// Returns v if option is Some(v) or throws an InvalidOperationException if it is None, discouraged
    public static T Get<T>(this Option<T> option);

    /// Returns true if option is None, discouraged
    public static bool IsNone<T>(this Option<T> option);

    /// Returns true if option is Some(v), discouraged
    public static bool IsSome<T>(this Option<T> option);

    /// Executes action(v) if option is Some(v)
    public static void Iter     <T>(this Option<T> option, Action<T>     action);
    public static Task IterAsync<T>(this Option<T> option, Func<T, Task> action);

    /// Returns Some(mapping(v)) if option is Some(v) or None if it is None
    public static Option<U>       Map     <T, U>(this Option<T> option, Func<T, U>       mapping);
    public static Task<Option<U>> MapAsync<T, U>(this Option<T> option, Func<T, Task<U>> mapping);

    /// Returns Some(mapping(v1, v2)) if options are Some(v1) and Some(v2) or None if at least one is None
    public static Option<U> Map2<T1, T2, U>(this (Option<T1>, Option<T2>) options, Func<T1, T2, U> mapping);

    /// Returns Some(mapping(v1, v2, v3)) if options are Some(v1), Some(v2) and Some(v3) or None if at least one is None
    public static Option<U> Map3<T1, T2, T3, U>(this (Option<T1>, Option<T2>, Option<T3>) options, Func<T1, T2, T3, U> mapping);

    /// Creates a new option containing Some(value) if value.HasValue
    public static Option<T> OfNullable<T>(T? value) where T : struct;

    /// Creates a new option containing Some(obj) if obj is not null
    public static Option<T> OfObj<T>(T? obj) where T : class;

    /// Returns option if option is Some(v) or ifNone if it is None
    public static Option<T> OrElse<T>(this Option<T> option, Option<T> ifNone);

    /// Returns option if option is Some(v) or ifNoneThunk() if it is None
    public static Option<T>       OrElseWith     <T>(this Option<T> option, Func<Option<T>>       ifNoneThunk);
    public static Task<Option<T>> OrElseWithAsync<T>(this Option<T> option, Func<Task<Option<T>>> ifNoneThunk);

    /// Returns a single-element array containing v if option is Some(v) or an empty array if it is None
    public static T[] ToArray<T>(this Option<T> option);

    /// Returns a single-element enumerable containing v if option is Some(v) or an empty enumerable if it is None
    public static IEnumerable<T> ToEnumerable<T>(this Option<T> option);

    /// Returns a non-null value type v if option is Some(v) or null if it is None
    public static T? ToNullable<T>(this Option<T> option) where T : struct;

    /// Returns a non-null reference type v if option is Some(v) or null if it is None
    public static T? ToObj<T>(this Option<T> option) where T : class;

    /// Creates a new option containing Some(value) if value.HasValue, fluently
    public static Option<T> ToOption<T>(this T? value) where T : struct;

    /// Creates a new option containing Some(obj) if obj is not null, fluently
    public static Option<T> ToOption<T>(this T? obj) where T : class;
}

Option<string> someString = Option.Some("I have a value");
Option<string> mapped = someString.Map(v => v + " today");
// returns Option.Some("I have a value today")

Option<string> noString = Option.None<string>();
string defaulted = noString.DefaultValue("default value");
// returns "default value"

Option<string> asyncBound = await someString
    .BindAsync(v => Task.FromResult(Option.Some(v + " altered")))
    .Pipe(o => o.BindAsync(v => Task.FromResult(Option.Some(v + " two times"))));
// returns Option.Some("I have a value altered two times")
```
Note how [Pipe](#piping-function-calls) has been used in the last example to remove async impedance mismatch.

### Utility functions for IEnumerable involving the Option type

An `OptionEnumerableExtensions` static class of extension methods is provided to enrich LINQ functionality on `IEnumerable`s with the `Option` type. Please refer to the official documentation of the [F# Seq module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-seqmodule.html) for a full description of the following:

```csharp
public static class OptionEnumerableExtensions
{
    /// Returns chooser(v) for each element of source which is Some(v)
    public static IEnumerable<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);

    /// Returns chooser(v) for the first element of source which is Some(v), or throws KeyNotFoundException if not found
    public static U Pick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);

    /// Returns Some(v) if there is exactly one element of source which is Some(v)
    public static Option<T> TryExactlyOne<T>(this IEnumerable<T> source);

    /// Returns Some(v) for the first Some(v) satisfying predicate(v)
    public static Option<T> TryFind<T>(this IEnumerable<T> source, Func<T, bool> predicate);

    /// Returns Some(v) for the last Some(v) satisfying predicate(v)
    public static Option<T> TryFindBack<T>(this IEnumerable<T> source, Func<T, bool> predicate);

    /// Returns the index of the first Some(v) satisfying predicate(v)
    public static Option<int> TryFindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate);

    /// Returns the index of the last Some(v) satisfying predicate(v)
    public static Option<int> TryFindIndexBack<T>(this IEnumerable<T> source, Func<T, bool> predicate);

    /// Returns Some(v) if the first element is Some(v) or None if it is None
    public static Option<T> TryHead<T>(this IEnumerable<T> source);

    /// Returns Some(v) if the index-th element is Some(v) or None if it is None
    public static Option<T> TryItem<T>(this IEnumerable<T> source, int index);

    /// Returns Some(v) if the last element is Some(v) or None if it is None
    public static Option<T> TryLast<T>(this IEnumerable<T> source);

    /// Returns chooser(v) for the first element of source which is Some(v), or None if not found
    public static Option<U> TryPick<T, U>(this IEnumerable<T> source, Func<T, Option<U>> chooser);

    /// Returns an enumerable applying generator on an accumulated state until it returns None
    public static IEnumerable<T> Unfold<T, TState>(this TState state, Func<TState, Option<(T, TState)>> generator);
}

IEnumerable<int> chosen = new[] { 1, 2, 3, 4, 5 }.Choose(
    i => i % 2 == 0
        ? Option.Some(i)
        : Option.None<int>());
// returns { 2, 4 }

Option<int> found = new[] { 1, 2, 3, 4, 5 }.TryFind(i => i % 2 == 0);
// returns Option.Some(2)
```

### Utility functions augmenting common functionality with the Option type

An `OptionExtensions` static class provides extension methods that help use the Option type in scenarios where nullable types or booleans are otherwise to be checked, for example when trying to get values out of a dictionary:

```csharp
public static class OptionExtensions
{
    /// Returns Some(v) if dict contains key or None if it doesn't
    public static Option<TValue> TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull;
    public static Option<TValue> TryGetValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull;
    public static Option<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key);
    public static Option<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key);
}

var dict = new Dictionary<int, string> { [42] = "The answer" };
Option<string> value = dict.TryGetValue(42);
// returns Option.Some("The answer")
```

## Result type

`Result<T, TError>` is intended to represent the result of an operation or validation, that can be either `Ok` or `Error`, both carrying a payload.

Like `Option<T>`, it is implemented as a value type because its usage is expected to be pervasive:

```csharp
public readonly record struct Result<T, TError>
{
    public bool IsOk { get; }
    public T ResultValue { get; } // undefined if IsOk is false
    public TError ErrorValue { get; } // undefined if IsOk is true

    public U Match<U>(Func<TError, U> onError, Func<T, U> onOk);
    public void Match(Action<TError> onError, Action<T> onOk);
    public override string ToString();

    internal Result(bool isOk, T resultValue, TError errorValue);
}
```

Properties are public for compatibility with test libraries, and to let to use C# pattern matching in those cases `Match` could not be used. **Accessing `IsOk`, `ResultValue` and `ErrorValue` directly (especially the latter two) is generally discouraged**. The constructor is internal to force you to use one of the named constructors explained below.

A `ToString` override provides representations such as `"Ok(ResultValue)"` or `"Error(ErrorValue)"` for debugging convenience.

### Utility functions for the Result type

Together with the `Result<T, TError>` type itself, a companion static class `Result` of utility functions is provided. Please refer to the official documentation of the [F# Result module](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-resultmodule.html) for a full descrition of them.

Moreover, some novel functions not included in the F# standard library are included, such as async versions that may be useful when working with I/O.

The `Bind` function is especially very convenient when chaining functions when the result of the previous one becomes the input of the next one, something described as [Railway oriented programming](https://fsharpforfunandprofit.com/posts/recipe-part2/) in the famous [F# for Fun and Profit](https://fsharpforfunandprofit.com/) site.

```csharp
public static class Result
{
    /// Creates a new result containing Ok(resultValue)
    public static Result<T, TError> Ok<T, TError>(T resultValue);

    /// Creates a new result containing Error(errorValue)
    public static Result<T, TError> Error<T, TError>(TError errorValue);


    /// Returns binder(v) if result is Ok(v) or Error(e) if it is Error(e)
    public static Result<U, TError>       Bind     <T, TError, U>(this Result<T, TError> result, Func<T, Result<U, TError>>       binder);
    public static Task<Result<U, TError>> BindAsync<T, TError, U>(this Result<T, TError> result, Func<T, Task<Result<U, TError>>> binder);

    /// Returns true if result is Ok(value) or false if it is not
    public static bool Contains<T, TError>(this Result<T, TError> result, T value);

    /// Returns 1 if result is Ok(v) or 0 if it is Error(e)
    public static int Count<T, TError>(this Result<T, TError> result);

    /// Returns v if result is Ok(v) or value if it is Error(e)
    public static T DefaultValue<T, TError>(this Result<T, TError> result, T value);

    /// Returns v if result is Ok(v) or defThunk(e) if it is Error(e)
    public static T       DefaultWith     <T, TError>(this Result<T, TError> result, Func<TError, T>       defThunk);
    public static Task<T> DefaultWithAsync<T, TError>(this Result<T, TError> result, Func<TError, Task<T>> defThunk);

    /// Returns predicate(v) if result is Ok(v) or false if it is Error(e)
    public static bool Exists<T, TError>(this Result<T, TError> result, Func<T, bool> predicate);

    /// Returns predicate(v) if result is Ok(v) or true if it is Error(e)
    public static bool ForAll<T, TError>(this Result<T, TError> result, Func<T, bool> predicate);

    /// Returns v if result is Ok(v) or throws an InvalidOperationException if it is Error(e), discouraged
    public static T Get<T, TError>(this Result<T, TError> result);

    /// Returns e if result is Error(e) or throws an InvalidOperationException if it is Ok(v), discouraged
    public static TError GetError<T, TError>(this Result<T, TError> result);

    /// Returns true if result is Error(e), discouraged
    public static bool IsError<T, TError>(this Result<T, TError> result);

    /// Returns true if result is Ok(v), discouraged
    public static bool IsOk<T, TError>(this Result<T, TError> result);

    /// Executes action(v) if result is Ok(v)
    public static void Iter     <T, TError>(this Result<T, TError> result, Action<T>     action);
    public static Task IterAsync<T, TError>(this Result<T, TError> result, Func<T, Task> action);

    /// Executes action(e) if result is Error(e)
    public static void IterError     <T, TError>(this Result<T, TError> result, Action<TError>     action);
    public static Task IterErrorAsync<T, TError>(this Result<T, TError> result, Func<TError, Task> action);

    /// Returns Ok(mapping(v)) is result is Ok(v) or Error(e) if it is Error(e)
    public static Result<U, TError>       Map     <T, TError, U>(this Result<T, TError> result, Func<T, U>       mapping);
    public static Task<Result<U, TError>> MapAsync<T, TError, U>(this Result<T, TError> result, Func<T, Task<U>> mapping);

    /// Returns Error(mapping(e)) if result is Error(e) or Ok(v) if it is Ok(v)
    public static Result<T, U>       MapError     <T, TError, U>(this Result<T, TError> result, Func<TError, U>       mapping);
    public static Task<Result<T, U>> MapErrorAsync<T, TError, U>(this Result<T, TError> result, Func<TError, Task<U>> mapping);

    /// Returns Some(v) if result is Ok(v) otherwise returns None
    public static Option<T> ToOption<T, TError>(this Result<T, TError> result);
}

Result<string, int> ok = Result.Ok<string, int>("result value");
Result<string, int> boundOk = ok.Bind(v => Result.Ok<string, int>("beautiful " + v));
// returns Result.Ok<string, int>("beautiful result value")

Result<string, int> error = Result.Error<string, int>(42);
Result<string, int> boundError = error.Bind(v => Result.Ok<string, int>("beautiful " + v));
// returns Result.Error<string, int>(42), short-circuiting

Result<string, int> asyncBoundOk = await ok
    .BindAsync(v => Task.FromResult(Result.Ok<string, int>("beautiful " + v)))
    .Pipe(o => o.BindAsync(v => Task.FromResult(Result.Ok<string, int>("very " + v))));
// returns Result.Ok<string, int>("very beautiful result value")
```
Note how [Pipe](#piping-function-calls) has been used in the last example to remove async impedance mismatch.

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

C# records can be [ab]used again to provide such strong types. In C# 10 you can even declare them as `readonly record struct`s to avoid the extra dereference and bookkeeping, making them zero-cost abstractions:

```csharp
public readonly record struct CustomerId(int Value);
public readonly record struct OrderId(int Value);
```

This library provides no features to create such types, because the built-in feature of the language can be leveraged, but I encourage to try this technique to let the compiler stop you when you try to shoot yourself. A properly modeled domain can dramatically reduce nasty bugs caused by the so called "primitive obsession".

**Caveat:** Instances of record structs may be created using the parameterless constructor, such as `new CustomerId()`, in that case the wrapped value will be initialized to its default value. This is built into the language and cannot be prevented, but should generally be avoided with the approach proposed here, because you could, for example, create a wrapped Svalue even if the wrapped type is a non-nullable reference type.

## Unit type

OK, this is not a discriminated union, but we may need it when we work with generic functions. If you ever had to provide two nearly identical implementations, one taking `Action` and one taking `Func`, just because you cannot write `Func<void>` you know what I mean.

Think of the unit type as the `void` as it should have been, that is a type useable as generic type argument (looks that may be the case in future C# versions).

This library provides the `Unit` type defined simply as a `record` with no properties. You can use it in your function signatures where you have to return nothing (that is, functions useful only for their side effects). The `unit` type in F# can only have one value, that is `()`. To emulate this in C#, the `Unit` record provides a static value named `Value` (which happens to be `null` as in the F# implementation) and a private constructor to prevent creating other `Unit` values:

```csharp
public record Unit
{
    private Unit() { }
    public static readonly Unit Value = null!;
}

// Let's say you did not implement an overload of Shape.Match accepting Action's:
shape.Match(
    onRectangle: r =>
    {
        Console.WriteLine($"Rectangle with width {r.Width} and height {r.Height}.");
        return Unit.Value; // unfortunately you have to return it explicitly
    }
    onCircle: c =>
    {
        Console.WriteLine($"Circle with radius {c.Radius}.");
        return Unit.Value;
    }
);
```

In cases like the one above, you could just return `0`, `false`, `""`, `42` or anything else that would be discarded, but having an explicit unit type may help conveying the intent better.

## Piping function calls

A feature commonly used when working with functions is calling them in a so-called pipeline where the output of the previous function becomes the input of the next one. F# uses its signature `|>` (pipe) operator to facilitate this. In general C# uses extension methods to offer similar functionality.

This library provides the following extension methods to emulate the pipe operator, letting you chain multiple function calls without writing ad hoc extension methods:

```csharp
public static class PipeExtensions
{
    public static TOut Pipe<TIn, TOut>(this TIn previous, Func<TIn, TOut> next) =>
        next(previous);

    public static T PipeIf<T>(this T previous, Func<T, bool> predicate, Func<T, T> next) =>
        predicate(previous) ? next(previous) : previous;
}

const int twenty = 20;

int piped = twenty.Pipe(v => v + 1).Pipe(v => v * 2);
// returns 42

int maybePiped = twenty.PipeIf(v => v < 10, v => v * 2);
// returns 20
```

`Pipe` is defined just like F#'s `|>`, that is the `previous` value is passed to the `next` lambda function, effectively inverting the order they are written. `PipeIf` calls the `next` lambda function only if the specified predicate returns true, allowing pipelines with optional steps.

Both `Pipe` and `PipeIf` provide four overloads (not shown here for conciseness), where either `previous` or `next` are a `Task` that must be awaited, allowing mixed pipelines of synchronous and asynchronous steps.

The async versions of `Pipe` may also be used to chain synchronous and asynchronous function involving `Option`s and `Result`s (see their examples).

## Non-empty enumerable and collections

Sometimes it's useful knowing that a collection of elements is non-empty at compile time.

This library provides the `INonEmptyEnumerable<T>`, `INonEmptyCollection<T>` and `INonEmptyList<T>` to represent, respectively, `IEnumerable<T>`, `IReadOnlyCollection<T>` and `IReadOnlyList<T>` that are guaranteed to contain at least one element.

This, paired with the `Option` type, allows to take action only when a collection is not empty, with no need to check for `Any()` and get extra safety from the compiler. For example:

```csharp
var maybeElements = elements.TryCreateNonEmptyCollection();
// ...
await maybeElements.IterAsync(es => PostElements(es));
```

### Non-empty enumerables

The following functionality provides non-empty, lazy enumerables and utility functions to work on them:

```csharp
public interface INonEmptyEnumerable<out T> : IEnumerable<T> { }

public static class NonEmptyEnumerable
{
    /// Returns a new non-empty enumerable containing element after source
    public static INonEmptyEnumerable<T> Append<T>(this INonEmptyEnumerable<T> source, T element);

    /// Returns a new non-empty enumerable containing second after first
    public static INonEmptyEnumerable<T> Concat<T>(this INonEmptyEnumerable<T> first, IEnumerable<T> second);
    public static INonEmptyEnumerable<T> Concat<T>(this IEnumerable<T> first, INonEmptyEnumerable<T> second);

    /// Returns a new non-empty enumerable containing firstElements and any otherElements
    public static INonEmptyEnumerable<T> Of<T>(T firstElement, params T[] otherElements);

    /// Returns a new non-empty enumerable applying mapper to each element of source
    public static INonEmptyEnumerable<TOut> Select<TIn, TOut>(this INonEmptyEnumerable<TIn> source, Func<TIn, TOut> mapper);

    /// Creates a new non-empty collection from an INonEmptyEnumerable
    public static INonEmptyCollection<T> ToNonEmptyCollection<T>(this INonEmptyEnumerable<T> source);

    /// Creates a new non-empty list from an INonEmptyEnumerable
    public static INonEmptyList<T> ToNonEmptyList<T>(this INonEmptyEnumerable<T> source);
}
```

### Non-empty collections

The following functionality provides non-empty collections and utility functions to work on them:

```csharp
public interface INonEmptyCollection<out T> : INonEmptyEnumerable<T>, IReadOnlyCollection<T> { }

public static class NonEmptyCollection
{
    /// Returns Some INonEmptyCollection if collection is not empty, or None if collection is empty
    public static Option<INonEmptyCollection<T>> TryCreateNonEmptyCollection<T>(this IReadOnlyCollection<T> collection);
}
```

### Non-empty lists

The following functionality provides non-empty, indexed lists and utility functions to work on them:

```csharp
public interface INonEmptyList<out T> : INonEmptyCollection<T>, IReadOnlyList<T> { }

public static class NonEmptyList
{
    /// Returns Some INonEmptyList if list is not empty, or None if list is empty
    public static Option<INonEmptyList<T>> TryCreateNonEmptyList<T>(this IReadOnlyList<T> list);
}
```

## Helpers to work with read-only collections

To promote safety, using read-only variants of collection interfaces is advised. The standard `Enumerable` class provides LINQ methods such as `ToList`, `ToDictionary` and `ToHashSet` to materialize enumerables into collections, but the resulting type is that of the concrete, mutable implementation.

This can be uncomfortable to work with when generic invariancy is involved. For example, if you want an `IReadOnlyDictionary` of `IReadOnlyCollection`s, you will need casts if you have a `Dictionary` of `List`s to begin with.

The following helpers aim at reducing the amount of casts needed, as well as better convey the intent to work on read-only collections.

```csharp
public static class ReadOnlyCollection
{
    /// Like Enumerable.ToList() but casts the result to IReadOnlyCollection
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source);

    /// Returns a singleton empty IReadOnlyCollection
    public static IReadOnlyCollection<T> Empty<T>();
}

public static class ReadOnlyList
{
    /// Like Enumerable.ToList() but casts the result to IReadOnlyList
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source);

    /// Returns a singleton empty IReadOnlyList
    public static IReadOnlyList<T> Empty<T>();
}

public static class ReadOnlyDictionary
{
    /// Like Enumerable.ToDictionary() but casts the result to IReadOnlyDictionary
    public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<T, TKey, TValue>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull;
    public static IReadOnlyDictionary<TKey, T> ToReadOnlyDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : notnull;

    /// Returns a singleton empty IReadOnlyDictionary
    public static IReadOnlyDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull;
}

public static class ReadOnlySet
{
    /// Like Enumerable.ToHashSet() but casts the result to IReadOnlySet
    public static IReadOnlySet<T> ToReadOnlySet<T>(this IEnumerable<T> source);

    /// Returns a singleton empty IReadOnlySet
    public static IReadOnlyCollection<T> Empty<T>();
}
```

## License

Permissive, [2-clause BSD style](https://opensource.org/licenses/BSD-2-Clause)

DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022-2024 Salvatore ISAJA. All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.