using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.CQRS.Commands.TagCommands.DeleteTag;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Tests.UnitTests.Attributes;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;

namespace ArticlesService.Tests.UnitTests.Features.Tags
{
    public class DeleteTagCommandHandlerTests
    {
        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagExists_ShouldDeleteAndPublishInvalidation(
            [Frozen] Mock<ITagRepository> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            DeleteTagCommand command,
            Tag tagEntity, 
            DeleteTagCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(x => x.GetTagWithFullInfoAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tagEntity);
            
            // Собираем ожидаемые ключи для проверки события
            var expectedTagsToInvalidate = new HashSet<string> { CacheTags.Tags, CacheTags.Tag(command.Id) };
            foreach (var article in tagEntity.Articles)
                expectedTagsToInvalidate.Add(CacheTags.Article(article.Id));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            // Проверяем удаление и сохранение сущности
            repositoryMock.Verify(x => x.Remove(tagEntity), Times.Once);
            repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            cacheContext.Tags.Should().BeEquivalentTo(expectedTagsToInvalidate);
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagNotFound_ShouldReturnFailure(
            [Frozen] Mock<ITagRepository> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            DeleteTagCommand command,
            DeleteTagCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(x => x.GetTagWithFullInfoAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tag?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be(TagErrors.TagNotFound(command.Id));

            repositoryMock.Verify(x => x.Remove(It.IsAny<Tag>()), Times.Never);
            cacheContext.Tags.Should().BeEmpty();
        }
    }
}
