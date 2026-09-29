using System.Text.Json.Serialization;
using ArticlesService.Domain.Errors;

namespace ArticlesService.Domain.Result
{
    public class Result<T>: IResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
               
        public List<Error> Errors { get; } = new();

        [JsonConstructor]
        private Result(bool isSuccess, T? value, List<Error> errors)
        {
            if (isSuccess && errors.Count is not 0 || !isSuccess && errors.Count is 0)
            {
                throw new ArgumentException("Invalid arguments");
            }

            IsSuccess = isSuccess;
            Value = value;
            Errors = errors;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, []);
        public static Result<T> Failure(List<Error> errors) => new Result<T>(false, default, errors);

        // Реализация интерфейса
        object? IResult.RawValue => Value;
        object IResult.ToFailureResult() => this; // Возвращает сам себя, если это ошибка
    }
}
