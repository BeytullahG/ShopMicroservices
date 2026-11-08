using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Shop.Catalog.Api.Common;

public enum ErrorType {Validation, NotFound, Conflict}

public sealed record Error(
    ErrorType Type,
    string Code,
    string Message)
{
    public static Error NotFound(string message) =>
        new(ErrorType.NotFound, "not found", message);
    public static Error Conflict(string code, string message) =>
        new(ErrorType.Conflict, code, message);
    public static Error Validation(string code, string message) =>
        new(ErrorType.Validation, code, message);

}

public class Result
{
    protected Result(Error? error) => Error = error;
    public Error? Error{get;}
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;
    public static Result Success() => new(null);
    public static implicit operator Result(Error error) => new(error);
}

public sealed class Result<T> : Result
{
    private Result(T? value, Error? error) : base(error) => Value = value;
    public T? Value {get;}
    public static implicit operator Result<T>(T value) => new(value, null);
    public static implicit operator Result<T>(Error error) => new(default, error);
}