namespace ArticlesService.ArticlesAPI.Extensions
{
    public static class ExceptionExtension
    {
        public static string GetAllMessages(this Exception? ex)
        {
            if (ex is null) return string.Empty;

            var messages = new List<string>();
            var current = ex;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" ---> ", messages);
        }
    }
}
