using System.Text;
using System.Text.Json;

namespace ArticlesService.Infrastructure.DataAccess.Pagination;

/// <summary>
/// Base64 кодирование/декодирование курсоров
/// </summary>
public class Base64CursorCodec : ICursorCodec
{
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        /*
        System.Text.Json по умолчанию не умеет сериализовать свойства и поля кортежей => 
        Включить поддержку полей в настройках
        */
        IncludeFields = true
    };

    public string Encode<T>(T data) where T : struct
    {
        if (data.Equals(default(T)))
            return string.Empty;

        string json = JsonSerializer.Serialize(data, JsonOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public T Decode<T>(string cursor) where T : struct
    {
        if (string.IsNullOrWhiteSpace(cursor))
            throw new ArgumentNullException(nameof(cursor));

        try
        {
            byte[] bytes = Convert.FromBase64String(cursor);
            string json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cursor decoding error: {cursor}", ex);
        }
    }
}
