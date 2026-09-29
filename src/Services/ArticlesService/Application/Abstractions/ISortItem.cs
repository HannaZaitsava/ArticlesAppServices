namespace ArticlesService.Application.Abstractions
{  
    public interface ISortItem<TEnum> where TEnum : struct, Enum
    {
        TEnum Field { get; set; }
        bool IsDescending { get; set; }
    }
}
