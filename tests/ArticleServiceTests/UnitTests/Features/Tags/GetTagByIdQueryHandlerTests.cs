using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ArticlesService.Tests.UnitTests.Attributes;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Application.CQRS.Queries.TagQueries.GetTag;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Application.Abstractions.DataAccess;


namespace ArticlesService.Tests.UnitTests.Features.Tags
{
    public class GetTagByIdQueryHandlerTests
    {
        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagFound_ShouldReturnSuccessfulResult(
        [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
        GetTagQuery query,
        TagResponseDTO tagDto,
        GetTagByIdQueryHandler sut)
        {
            // Arrange
            repositoryMock
                .Setup(x => x.GetByIdProjectedAsync<TagResponseDTO>(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tagDto);

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDto);
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagNotFound_ShouldReturnFailureWithTagNotFoundError(
            [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
            GetTagQuery query,
            GetTagByIdQueryHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(x => x.GetByIdProjectedAsync<TagResponseDTO>(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((TagResponseDTO?)null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            
            result.Errors.Should().ContainSingle()
                .Which.Should().Be(TagErrors.TagNotFound(query.Id));
        }

        /*
         * В своей книге по тестированию Хориков говорит, что тестировать взаимодействие со стабами не нужно (Verify излишен). 
         * Тест не должен интересоваться, как тестируемая система генерирует конечный результат (это деталь имплементации), при условии что это результат правилен.
         * Тестирование деталей имплементации делает тесты неустойчивыми к рефакторингу, что ведет к хрупкости тестов.
         */
        //[Theory, AutoMoqData]
        //internal async Task Handle_ShouldConsultRepositoryOnce(
        //    [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
        //    GetTagQuery query,
        //    GetTagByIdQueryHandler handler)
        //{
        //    // Act
        //    await handler.Handle(query, default);

        //    // Assert           
        //    repositoryMock.Verify(x => x.GetBySpecAsync<TagResponseDTO>(
        //        It.IsAny<ISpecification<Tag>>(),
        //        false,
        //        false,
        //        It.IsAny<CancellationToken>()),
        //        Times.Once); 
        //}
    }
}
