using System.Net.Http.Json;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Tests.IntegrationTests.Common;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ArticlesService.Tests.IntegrationTests.Controllers.Tags
{
    public class GetTagTests : BaseIntegrationTest
    {
        public GetTagTests(ArticlesAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetTag_ShouldReturnSuccess_WhenTagExists()
        {
            // Arrange
            // Fixture уже настроена в базовом классе со всеми кастомизациями
            var tag = Fixture.Create<Tag>();

            DbContext.Tags.Add(tag);
            await DbContext.SaveChangesAsync();
            ClearTracker();

            // Act
            var response = await Client.GetAsync($"/api/tags/{tag.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TagApiResponse>();
            
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(tag, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetTag_ShouldReturnNotFound_WhenTagDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await Client.GetAsync($"/api/tags/{nonExistentId}");
                        
            // Assert           
            // Опционально: проверить структуру ошибки ProblemDetails // TODO проверять это в unit-тесте для ProblemDetails 
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();

            var expectedError = TagErrors.TagNotFound(nonExistentId);
            problem.Should().NotBeNull();
            problem!.Title.Should().Contain(expectedError.Name);
            problem!.Detail.Should().Contain(expectedError.Message);
        }
    }
}
