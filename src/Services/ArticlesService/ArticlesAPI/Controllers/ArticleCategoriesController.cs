using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.DeleteArticleCategory;
using ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetArticleCategory;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.UpdateArticleCategory;
using ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetAllArticleCategories;
using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.CreateArticleCategory;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.ArticlesAPI.Extensions;
using ArticlesService.ArticlesAPI.Models.Common;
using ArticlesService.ArticlesAPI.Models.Requests;

namespace ArticlesService.ArticlesAPI.Controllers
{
    /// <summary>
    /// Controller responsible for article category lifecycle operations (CRUD).
    /// </summary>    
    public class ArticleCategoriesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ArticleCategoriesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get article category by identifier.
        /// </summary>
        /// <param name="id">Category identifier (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Category found and returned.</response>
        /// <response code="404">Category with specified id was not found.</response>
        [HttpGet("{id:guid}", Name = "GetArticleCategory")]
        [ProducesResponseType(typeof(ArticleCategoryApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleCategoryApiResponse>> GetArticleCategory([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetArticleCategoryQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<ArticleCategoryApiResponse>(result.Value!);
                return Ok(response);
            }
            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Get paginated list of article categories.
        /// </summary>
        /// <param name="request">Pagination, filtering and sorting parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Returns a paginated list of categories.</response>
        [HttpGet(Name = "GetAllArticleCategories")]
        [ProducesResponseType(typeof(OffsetPagedListApiResponse<ArticleCategoryShortInfoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<OffsetPagedListApiResponse<ArticleCategoryShortInfoResponse>>> GetAllArticleCategories(
            [FromQuery] GetAllCategoriesPaginatedApiRequest request,
            CancellationToken cancellationToken)
        {
            var finalRequest = _mapper.Map<GetAllArticleCategoriesQuery>(request);
            var result = await _mediator.Send(finalRequest, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<OffsetPagedListApiResponse<ArticleCategoryShortInfoResponse>>(result.Value!);
                return Ok(response);
            }
            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Create a new article category.
        /// </summary>
        /// <remarks>
        /// Endpoint requires authentication. Returns 201 with location header pointing to the created resource.
        /// </remarks>
        /// <param name="requestBody">Category payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="201">Category was created successfully.</response>
        /// <response code="409">Conflict during creation (for example duplicate slug).</response>
        // POST api/<ArticleCategoriesController>
        [Authorize]
        [HttpPost]       
        [ProducesResponseType(typeof(ArticleCategoryApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> AddArticleCategory([FromBody] ArticleCategoryApiRequest requestBody, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<CreateArticleCategoryCommand>(requestBody);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<ArticleCategoryApiResponse>(result.Value!);

                return CreatedAtRoute(
                    routeName: "GetArticleCategory",
                    routeValues: new { id = response.Id },
                    value: response
                    );
            }
            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Update existing article category.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="id">Category identifier.</param>
        /// <param name="requestBody">Updated category payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Update succeeded.</response>
        /// <response code="404">Category was not found.</response>
        // PUT api/<ArticleCategoriesController>/5        
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateArticleCategory(Guid id, [FromBody] ArticleCategoryApiRequest requestBody, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<UpdateArticleCategoryCommand>((id, requestBody));
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Delete article category by identifier.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="id">Category identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Category deleted.</response>
        /// <response code="404">Category was not found.</response>
        // DELETE api/<ArticlesCategoryController>/5       
        [Authorize]
        [HttpDelete("{id:guid}")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteArticleCategory(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteArticleCategoryCommand(id), cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }
    }
}
