namespace ArticlesService.Domain.Enums
{
    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        AlreadyExist =3,
        Conflict = 4,
        Unauthorized = 5,
        Forbidden = 6,
        BadRequest = 7,
        Unexpected = 8
    }
}
