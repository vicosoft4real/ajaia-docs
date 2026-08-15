namespace AjaiaDocs.Core.Common;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly AjaiaError? _error;

    private Result(T? value, AjaiaError? error) => (_value, _error) = (value, error);

    public bool IsSuccess => _error is null;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result has no value.");

    public AjaiaError Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("A successful result has no error.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(AjaiaError error) => new(default, error);
}
