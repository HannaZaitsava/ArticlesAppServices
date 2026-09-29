using ArticlesService.Domain.Entities;

namespace ArticlesService.Infrastructure.DataAccess.DbContext
{
    public static class DataForSeeding
    {
        public static IList<Tag> SeedTags()
        {
            return new List<Tag>
            {
                new()
                {
                    Id = Guid.Parse("50dc578c-e857-4f2d-ae4f-d17c62b90671"),
                    Label = "C#",
                    Color = "#c0c0c0"
                },
                new()
                {
                    Id = Guid.Parse("50dc578c-e857-4f2d-ae4f-d17c62b90672"),
                    Label = "BIO",
                    Color = "#778899"
                },
                new()
                {
                    Id = Guid.Parse("50dc578c-e857-4f2d-ae4f-d17c62b90673"),
                    Label = "GEO",
                    Color = "#778899"
                },
                new()
                {
                    Id = Guid.Parse("50dc578c-e857-4f2d-ae4f-d17c62b90674"),
                    Label = "MATH",
                    Color = "#778899"
                },
                new()
                {
                    Id = Guid.Parse("50dc578c-e857-4f2d-ae4f-d17c62b90675"),
                    Label = "DIY",
                    Color = "#778899"
                },
            };          
        }

        public static IList<ArticleCategory> SeedArticleCategories()
        {
            return new List<ArticleCategory>
            {
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90670"),
                    Name = "Art",
                    IsDefault = true,
                },
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90671"),
                    Name = "Architecture",
                    IsDefault = true,
                },
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90672"),
                    Name = "Literature",
                    IsDefault = true,
                },
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90673"),
                    Name = "Music",
                    IsDefault = true,
                },
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90674"),
                    Name = "Science",
                    IsDefault = true,
                },
                new()
                {
                    Id = Guid.Parse("51dc578c-e857-4f2d-ae4f-d17c62b90675"),
                    Name = "Sport",
                    IsDefault = false,
                },
            };
        }
    }
}
