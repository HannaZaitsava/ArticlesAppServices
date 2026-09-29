using ArticlesService.Domain.Entities.Base;

namespace ArticlesService.Domain.Entities
{
    /* СТРАТЕГИИ УДАЛЕНИЯ КОММЕНТАРИЕВ (выбрана вторая):
     *1. Подход «Удаленный узел» (YouTube, Reddit, Habr)
        При удалении родительского комментария:
        - Сам текст родительского комментария заменяется на системную заглушку: «Комментарий удален пользователем» или «[deleted]».
        - Все ответы сохраняются и остаются привязанными к этой заглушке.
        Зачем это нужно: Чтобы не разрушать контекст обсуждения. Если под комментарием развернулась ветка на 100 сообщений, 
        удаление корня сделает все остальные ответы бессмысленными и «повисшими в воздухе».

     * 2. Подход «Каскадное скрытие» (Facebook, Instagram)
        Здесь логика более жесткая, ориентированная на визуальную чистоту:
        - Если родитель удален, вся ветка ответов скрывается из публичного доступа.
        - Физически в базе данных ответы могут остаться (со статусом is_deleted), но пользователи их больше не видят.
        Зачем это нужно: Чтобы избежать «мусора» и веток, которые начинаются ни с чего.
     */

    public class Comment: BaseAuditableEntity
    {
        public string Text { get; set; } = default!;  

        public Article Article { get; set; } = null!;
        public Guid ArticleId { get; set; }        


        // Hierarchy (Self-reference)
        // ParentId = null if it is the first comment
        public Guid? ParentId { get; set; }        
        public Comment? Parent { get; set; }

        // Ссылка на самый первый комментарий в ветке
        public Guid? RootCommentId { get; set; } 
        public Comment? RootComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();        
    }
}
