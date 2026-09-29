using System.Linq.Expressions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.CreateArticleCategory;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Tests.UnitTests.Attributes;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;

namespace ArticlesService.Tests.UnitTests.Features.ArticleCategories
{
    public class CreateArticleCategoryCommandHandlerTests
    {
        [Theory, AutoMoqData]
        internal async Task Handle_WhenArticleCategoryDoesNotExist_ShouldCreateArticleCategoryAndReturnSuccess(
            [Frozen] Mock<IBaseRepository<ArticleCategory>> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            CreateArticleCategoryCommand command,
            CreateArticleCategoryCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(r => r.IsExistingAsync(It.IsAny<Expression<Func<ArticleCategory, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ArticleCategory>(), It.IsAny<CancellationToken>()))
                .Callback<ArticleCategory, CancellationToken>((category, _) => category.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.Id.Should().NotBeEmpty();
            result.Value.Name.Should().Be(command.Name);

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<ArticleCategory>(), It.IsAny<CancellationToken>()), Times.Once);
            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var expectedCacheTagsToInvalidate = new HashSet<string> { CacheTags.ArticleCategories };
            cacheContext.Tags.Should().BeEquivalentTo(expectedCacheTagsToInvalidate);            
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenArticleCategoryAlreadyExists_ShouldReturnFailureWithProperError(
            [Frozen] Mock<IBaseRepository<ArticleCategory>> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            CreateArticleCategoryCommand command,
            CreateArticleCategoryCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(r => r.IsExistingAsync(It.IsAny<Expression<Func<ArticleCategory, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be(ArticleCategoryErrors.ArticleCategoryAlreadyExists(command.Name));

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<ArticleCategory>(), It.IsAny<CancellationToken>()), Times.Never);
            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            cacheContext.Tags.Should().BeEmpty();
        }
    }
}
