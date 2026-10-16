using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Entities.MtoM;
using ArticlesService.Domain.Enums;
using Bogus;
using Bogus.Extensions;

namespace ArticlesService.Infrastructure.DataAccess.DbContext
{ 
    public static class DbDataSeeder
    {
        public static void Seed(AppDbContext context, string scale)
        {
            // Проверяем, есть ли статьи. Если база уже наполнена — выходим
            if (context.Set<Article>().Any()) return;

            bool isLarge = scale.Equals("Large", StringComparison.OrdinalIgnoreCase); // Large/Small

            // Объемы данных для генерации 
            int extraUserCount = isLarge ? 3_000 : 10;
            int extraCategoryCount = isLarge ? 100 : 5; 
            int extraTagCount = isLarge ? 1_000 : 20;     
            int articleCount = isLarge ? 100_000 : 40;

            Randomizer.Seed = new Random(45);
            var faker = new Faker("en");

            // --- ЭТАП 1: Подхватываем HasData и генерируем НОВЫЕ категории/теги ---

            // Сначала считываем то, что уже вставил EF Core через HasData в миграциях
            var allCategoryIds = context.Set<ArticleCategory>().Select(c => c.Id).ToList();
            var allTagIds = context.Set<Tag>().Select(t => t.Id).ToList();

            // Генерируем уникальные дополнительные Категории
            var categoryFaker = new Faker<ArticleCategory>("en")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + " " + f.UniqueIndex)
                .RuleFor(c => c.IsDefault, false);

            var extraCategories = categoryFaker.Generate(extraCategoryCount);
            InsertInBatches(context, extraCategories, 500);
            allCategoryIds.AddRange(extraCategories.Select(c => c.Id));

            // Генерируем уникальные дополнительные Теги с учетом HEX-валидатора
            var tagFaker = new Faker<Tag>("en")
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Label, f => f.Commerce.Department() + f.UniqueIndex) // Уникальный Label для уникального индекса
                .RuleFor(t => t.Color, f => f.Internet.Color()); // Bogus по умолчанию генерирует HEX вид вроде "#ffffff"

            var extraTags = tagFaker.Generate(extraTagCount);
            InsertInBatches(context, extraTags, 500);
            allTagIds.AddRange(extraTags.Select(t => t.Id));


            // --- ЭТАП 2: авторы ---
            var existingUserIds = context.Set<User>().Select(u => u.Id).ToList();

            var userFaker = new Faker<User>("en")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.UserName, (f, u) =>
                {
                    // чтобы на больших объемах данных не дублировались значения
                    var suffix = f.Random.Number(1000, 999999);
                    return f.Internet.UserName(u.FirstName, u.LastName) + suffix;
                })
                .RuleFor(u => u.NormalizedUserName, (f, u) => u.UserName!.ToUpperInvariant())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email!.ToUpperInvariant())
                .RuleFor(u => u.EmailConfirmed, f => true)
                .RuleFor(u => u.PhoneNumberConfirmed, f => true)
                .RuleFor(u => u.SecurityStamp, f => Guid.NewGuid().ToString("D"))
                .RuleFor(u => u.ConcurrencyStamp, f => Guid.NewGuid().ToString("D"))
                .RuleFor(u => u.IsAdmin, f => false)
                .RuleFor(u => u.CreatedOn, f => f.Date.PastOffset(2).ToUniversalTime())
                .RuleFor(u => u.CreatorId, (f, u) => u.Id);

            var extraUsers = userFaker.Generate(extraUserCount);
            InsertInBatches(context, extraUsers, 1000);

            var allAuthorIds = existingUserIds.Concat(extraUsers.Select(u => u.Id)).ToList();

            // --- ЭТАП 3: Статьи, Комментарии и связи Многие-ко-Многим ---
            int articleBatchSize = isLarge ? 5_000 : articleCount;
            int generatedArticlesCount = 0;

            while (generatedArticlesCount < articleCount)
            {
                var currentBatchSize = Math.Min(articleBatchSize, articleCount - generatedArticlesCount);

                var articleBatch = new List<Article>();
                var m2mCategories = new List<ArticleArticleCategory>();
                var m2mTags = new List<ArticleTag>();
                var commentsToInsert = new List<Comment>();

                for (int i = 0; i < currentBatchSize; i++)
                {
                    var articleId = Guid.NewGuid();
                    var authorId = faker.PickRandom(allAuthorIds);
                    var createdDate = faker.Date.PastOffset(1).ToUniversalTime(); 

                    var status = faker.Random.WeightedRandom(
                        new[] { ArticleStatus.Published, ArticleStatus.Draft, ArticleStatus.PendingReview, ArticleStatus.Archived },
                        new[] { 0.75f, 0.12f, 0.1f, 0.03f }
                    );

                    var generatedContent = faker.Lorem.Paragraphs(faker.Random.Number(3, 8)).ClampLength(max: ArticleConstraints.ContentMaxLength);

                    var article = new Article
                    {
                        Id = articleId,
                        AuthorId = authorId,
                        Title = faker.Lorem.Sentence(faker.Random.Number(3, 7)).ClampLength(max: ArticleConstraints.TitleMaxLength),
                        Content = generatedContent, 
                        PreviewText = generatedContent.ClampLength(max: ArticleConstraints.PreviewTextMaxLength), 
                        CreatedOn = createdDate,
                        CreatorId = authorId
                    };

                    if (status == ArticleStatus.Published)
                    {
                        var publishResult = article.Publish(createdDate.AddDays(faker.Random.Number(1, 2)).ToUniversalTime());
                        if (!publishResult.IsSuccess)
                        {
                            throw new InvalidOperationException($"Domain validation failed during seeding: {string.Join(", ", publishResult.Errors)}");
                        }
                    }
                    else
                    {
                        typeof(Article).GetProperty(nameof(Article.Status))?.SetValue(article, status);
                    }

                    articleBatch.Add(article);

                    // Привязка Категорий (Art, Architecture, Literature...)
                    var selectedCats = faker.PickRandom(allCategoryIds, faker.Random.Number(1, 2));
                    foreach (var catId in selectedCats)
                    {
                        m2mCategories.Add(new ArticleArticleCategory { ArticleId = articleId, CategoryId = catId });
                    }

                    // Привязка вТегов (C#, BIO, GEO, MATH...)
                    var selectedTags = faker.PickRandom(allTagIds, faker.Random.Number(1, 3));
                    foreach (var tId in selectedTags)
                    {
                        m2mTags.Add(new ArticleTag { ArticleId = articleId, TagId = tId });
                    }

                    if (status == ArticleStatus.Published)
                    {
                        int commentsCount = isLarge ? faker.Random.Number(0, 4) : faker.Random.Number(1, 2);
                        var articleComments = new List<Comment>();

                        for (int c = 0; c < commentsCount; c++)
                        {
                            var creatorId = faker.PickRandom(allAuthorIds);

                            var comment = new Comment
                            {
                                Id = Guid.NewGuid(),
                                ArticleId = articleId,
                                Text = faker.Lorem.Sentence().ClampLength(max: CommentConstraints.MaxTextLength),
                                CreatedOn = article.PublicationDate!.Value.AddHours(faker.Random.Number(1, 24)).ToUniversalTime(),
                                CreatorId = creatorId,
                                AuthorId = creatorId
                            };
                            articleComments.Add(comment);
                            commentsToInsert.Add(comment);
                        }

                        if (articleComments.Count >= 2 && faker.Random.Bool(0.25f))
                        {
                            var parent = articleComments[0];
                            var reply = articleComments[1];

                            reply.ParentId = parent.Id;
                            reply.RootCommentId = parent.Id;
                            reply.CreatedOn = parent.CreatedOn.AddMinutes(faker.Random.Number(10, 180));
                        }
                    }
                }

                // Пакетное сохранение
                context.Set<Article>().AddRange(articleBatch);
                context.SaveChanges();

                context.Set<ArticleArticleCategory>().AddRange(m2mCategories);
                context.Set<ArticleTag>().AddRange(m2mTags);
                context.Set<Comment>().AddRange(commentsToInsert);
                context.SaveChanges();

                context.ChangeTracker.Clear();

                generatedArticlesCount += currentBatchSize;
            }
        }

        private static void InsertInBatches<T>(AppDbContext context, List<T> entities, int batchSize) where T : class
        {
            for (int i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize);
                context.Set<T>().AddRange(batch);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }
    }
}