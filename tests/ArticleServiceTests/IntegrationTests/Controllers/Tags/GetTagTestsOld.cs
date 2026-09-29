using System.Net;
using System.Net.Http.Json;
using ArticlesAPI.Models.Responses;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using FluentAssertions;
using IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationTests.Controllers.Tags
{    
    public class GetTagTestsOld(CollectionFixtureSharedTestContext context) : BaseCollectionIntegrationTest(context)
    {
        [Fact]
        public async Task GetTag_WhenTagExists_ShouldReturnOkAndTagResponse()
        {
            // Arrange
            var tag = _fixture.Create<Tag>();//_fixture.Build<Tag>().Create();

            await ExecuteInDbContext(async db =>
            {
                await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
            });

            // Act
            var response = await _httpClient.GetAsync($"/api/Tags/{tag.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<TagResponse>();
            result.Should().NotBeNull();
            result!.Id.Should().Be((tag.Id));
            result.Label.Should().Be(tag.Label);
            result.Color.Should().Be(tag.Color);
        }

        [Fact]
        public async Task GetTag_WhenTagDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.GetAsync($"/api/Tags/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Опционально: проверить структуру ошибки ProblemDetails // TODO проверять это в unit-тесте для ProblemDetails 
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();

            var expectedError = TagErrors.TagNotFound(nonExistentId);
            problem!.Title.Should().Contain(expectedError.Name);
            problem!.Detail.Should().Contain(expectedError.Message);           
        }
    }
}
