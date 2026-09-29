using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.CQRS.Commands.TagCommands.UpdateTag;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Tests.UnitTests.Attributes;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;

namespace ArticlesService.Tests.UnitTests.Features.Tags
{
    public class UpdateTagCommandHandlerTest
    {
        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagExists_ShouldUpdateDetailsAndReturnSuccessAndInvalidateCache(
            [Frozen] Mock<ITagRepository> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            UpdateTagCommand command,
            Tag tagEntity,
            UpdateTagCommandHandler handler)
        {
            // Arrange           
            //tagEntity.Id = command.Id; // не обязательно, так как мы не проверяем конкретные данные, а только что они были обновлены

            repositoryMock
                .Setup(r => r.GetTagWithFullInfoAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tagEntity);

            // ожидаемые ключи кэша
            var expectedTagsToInvalidate = new HashSet<string>
            {
                CacheTags.Tags,
                CacheTags.Tag(command.Id)
            };
            foreach (var article in tagEntity.Articles)
                expectedTagsToInvalidate.Add(CacheTags.Article(article.Id));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            // Проверка маппинга (Sociable Test): данные из команды должны быть в сущности
            tagEntity.Label.Should().Be(command.Label);
            tagEntity.Color.Should().Be(command.Color);

            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            cacheContext.Tags.Should().BeEquivalentTo(expectedTagsToInvalidate);
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagDoesNotExist_ShouldReturnFailureAndNotInvalidateCache(
            [Frozen] Mock<ITagRepository> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            UpdateTagCommand command,
            UpdateTagCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(r => r.GetTagWithFullInfoAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tag?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
                        
            result.Errors.Should().ContainSingle()
                .Which.Should().Be(TagErrors.TagNotFound(command.Id));

            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            cacheContext.Tags.Should().BeEmpty();
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagHasNoRelatedArticles_ShouldInvalidateOnlyTagCacheKeys(
            [Frozen] Mock<ITagRepository> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            UpdateTagCommand command,
            Tag tagEntity,
            UpdateTagCommandHandler handler)
        {
            // Arrange
            tagEntity.Articles = [];

            repositoryMock
                .Setup(r => r.GetTagWithFullInfoAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tagEntity);

            var expectedCacheTagsToInvalidate = new HashSet<string>
            {
                CacheTags.Tags,
                CacheTags.Tag(command.Id)
            };           

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            cacheContext.Tags.Count.Should().Be(expectedCacheTagsToInvalidate.Count);
            cacheContext.Tags.Should().BeEquivalentTo(expectedCacheTagsToInvalidate);
        }
    }
}

