namespace ArticlesService.Domain.Enums
{   
    public enum ArticleStatus : byte
    {
        /// <summary>
        /// Статья находится в режиме черновика (редактируется автором)
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Статья отправлена на проверку модераторам
        /// </summary>
        PendingReview = 2,

        /// <summary>
        /// Статья успешно опубликована и видна всем пользователям
        /// </summary>
        Published = 3,

        /// <summary>
        /// Статья заблокирована, удалена или отправлена в архив
        /// </summary>
        Archived = 4,

        /// <summary>
        /// Заблокирована. Скрыта администрацией за нарушение правил платформы.
        /// </summary>
        Banned = 5,

        /// <summary>
        /// Удалена. Мягкое удаление (Soft Delete) для исключения из общих списков.
        /// </summary>
        Deleted = 6
    }
}
