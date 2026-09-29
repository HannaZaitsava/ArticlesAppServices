using ArticlesService.Application.Common.Behaviors;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Events;
using ArticlesService.Application.CQRS.Commands.TagCommands.CreateTag;
using ArticlesService.Domain.Result;
using AutoFixture.Xunit2;
using MediatR;
using Moq;

namespace ArticlesService.Tests.UnitTests.Features.Behaviors
{
    public class CacheInvalidationBehaviorTests
    {
        [Theory]
        [InlineAutoData(true, 3, 1)]   // Успех, есть теги -> Событие публикуется 1 раз
        [InlineAutoData(false, 3, 0)]  // Ошибка, есть теги -> Событие НЕ публикуется
        [InlineAutoData(true, 0, 0)]   // Успех, тегов НЕТ -> Событие НЕ публикуется
        internal async Task Handle_CacheInvalidationPolicy_ShouldPublishEventOnlyOnSuccessAndWithTags(
        bool isSuccess,
        int tagCount,
        int expectedPublishTimes,
        CreateTagCommand command,
        Mock<ICacheInvalidationContext> cacheContextMock,
        Mock<IMediator> mediatorMock)
        {
            // Arrange
            var resultMock = new Mock<IResult>();
            resultMock.Setup(r => r.IsSuccess).Returns(isSuccess);

            var expectedTimes = expectedPublishTimes == 0 ? Times.Never() : Times.Exactly(expectedPublishTimes);

            var tags = Enumerable.Range(0, tagCount).Select(i => $"tag-{i}").ToHashSet();
            cacheContextMock.Setup(c => c.Tags).Returns(tags);

            // 3. Настраиваем делегат next (имитируем вызов хендлера, возвращающего наш результат)
            RequestHandlerDelegate<IResult> nextDelegate = (ct) => Task.FromResult(resultMock.Object);

            var behavior = new CacheInvalidationBehavior<CreateTagCommand, IResult>(
                cacheContextMock.Object,
                mediatorMock.Object);

            // Act
            await behavior.Handle(command, nextDelegate, CancellationToken.None);

            // Assert
            // Проверяем, вызвался ли mediator.Publish нужное количество раз
            mediatorMock.Verify(m => m.Publish(
                It.IsAny<CacheInvalidationEvent>(),
                It.IsAny<CancellationToken>()),
                expectedTimes);
        }
    }
}
