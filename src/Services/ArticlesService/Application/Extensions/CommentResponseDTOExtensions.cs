using ArticlesService.Application.DTOs.Comments;

namespace ArticlesService.Application.Extensions
{
    public static class CommentResponseDTOExtensions
    {        
        public static List<CommentResponseDTO> ConvertToTree(this IEnumerable<CommentResponseDTO> allComments)
        {
            var lookup = allComments.ToDictionary(x => x.Id);
            var rootNodes = new List<CommentResponseDTO>();

            foreach (var comment in allComments)
            {
                if (comment.ParentId is null || !lookup.ContainsKey(comment.ParentId.Value))
                {
                    // Если родителя нет или родитель не попал в текущую выборку
                    rootNodes.Add(comment);
                }
                else
                {
                    // Находим родителя в словаре и добавляем текущий DTO в его список Replies
                    lookup[comment.ParentId.Value].Replies.Add(comment);
                }
            }

            return rootNodes; 
        }
    }
}
