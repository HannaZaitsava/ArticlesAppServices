namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record CommentApiResponse
    {
        public Guid Id { get; init; }
        public string Text { get; init; } = default!;
        
        public Guid ArticleId { get; init; }

        public Guid? CreatedBy { get; init; }
        public string CreatorName { get; init; } = default!;

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastModifiedOn { get; init; }

        public bool IsDeleted { get; init; }


        // Hierarchy (Self-reference)
        // ParentId = null if it is the first comment
        public Guid? ParentId { get; init; }

        public Guid? RootCommentId { get; init; }
        //public IReadOnlyCollection<Guid> ReplyIds { get; init; } = new HashSet<Guid>();
        public IReadOnlyCollection<CommentApiResponse> Replies { get; init; } = new HashSet<CommentApiResponse>();
    }
}
