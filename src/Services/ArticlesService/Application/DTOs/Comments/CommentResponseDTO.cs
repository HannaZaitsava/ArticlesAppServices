namespace ArticlesService.Application.DTOs.Comments
{
    public sealed record CommentResponseDTO
    {
        public Guid Id { get; init; }
        public string Text { get; init; } = default!;                
        public Guid ArticleId { get; init; }

        public Guid? CreatedBy { get; init; }
        public string CreatorName { get; init; } = default!;

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.Now;
        public DateTimeOffset? LastModifiedOn { get; init; }

        public bool IsDeleted { get; init; }

        // Hierarchy (Self-reference)
        // ParentId = null if it is the first comment
        public Guid? ParentId { get; init; }

        public Guid? RootCommentId { get; init; }
       
        public ICollection<CommentResponseDTO> Replies { get; set; } = new HashSet<CommentResponseDTO>();
    }
}
