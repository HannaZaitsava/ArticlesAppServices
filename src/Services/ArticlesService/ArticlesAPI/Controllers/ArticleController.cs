using ArticlesService.Application.CQRS.Commands.ArticleCommands.DeleteArticle;
using ArticlesService.Application.CQRS.Commands.ArticleCommands.PublishArticleCommand;
using ArticlesService.Application.CQRS.Queries.ArticleQueries.GetArticleQuery;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsOffsetPagedQuery;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsCursorPagedQuery;
using ArticlesService.Application.CQRS.Queries.ArticleQueries.GetAllArticlesQuery;
using ArticlesService.Application.CQRS.Commands.CommentCommands.CreateComment;
using ArticlesService.Application.CQRS.Commands.ArticleCommands.UpdateArticle;
using ArticlesService.Application.CQRS.Commands.ArticleCommands.CreateArticle;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.ArticlesAPI.Models.Requests;
using ArticlesService.ArticlesAPI.Models.Common;
using ArticlesService.ArticlesAPI.Extensions;

namespace ArticlesService.ArticlesAPI.Controllers
{
    /// <summary>
    /// Controller responsible for article lifecycle operations (CRUD) and related comment endpoints.
    /// </summary>    
    public class ArticlesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ArticlesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get article by identifier.
        /// </summary>
        /// <param name="id">Article identifier (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Article found and returned.</response>
        /// <response code="404">Article with specified id was not found.</response>
        [HttpGet("{id:guid}", Name = "GetArticle")]
        [ProducesResponseType(typeof(ArticleApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleApiResponse>> GetArticle([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetArticleQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<ArticleApiResponse>(result.Value!);
                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Get paginated list of articles.
        /// </summary>
        /// <param name="newQuery">Pagination, filtering and sorting parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Returns a paginated list of articles.</response>
        [HttpGet(Name = "GetAllArticles")]
        [ProducesResponseType(typeof(OffsetPagedListApiResponse<ArticleShortInfoApiResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<OffsetPagedListApiResponse<ArticleShortInfoApiResponse>>> GetAllArticles(
            [FromQuery] GetArticlesPaginatedApiRequest newQuery,
            CancellationToken cancellationToken = default)
        {
            var query = _mapper.Map<GetAllArticlesQuery>(newQuery);

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<OffsetPagedListApiResponse<ArticleShortInfoApiResponse>>(result.Value!);

                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Create a new article.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="requestBody">Article payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="201">Article was created successfully.</response>
        /// <response code="409">Conflict during creation (for example duplicate slug).</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        // TODO: заменить [FromBody] на [FromForm], когда добавится подгрузка изображения
        public async Task<ActionResult<ArticleApiResponse>> AddArticle([FromBody] ArticleApiRequest requestBody, CancellationToken cancellationToken = default)
        {
            var command = _mapper.Map<CreateArticleCommand>(requestBody);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<ArticleApiResponse>(result.Value!);

                return CreatedAtRoute(
                    routeName: "GetArticle",
                    routeValues: new { id = response.Id },
                    value: response
                    );
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Update existing article.
        /// </summary>
        /// <remarks>Requires authentication.</remarks>
        /// <param name="id">Article identifier.</param>
        /// <param name="requestBody">Updated article payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Update succeeded.</response>
        /// <response code="404">Article was not found.</response>
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // TODO: заменить [FromBody] на [FromForm], когда добавится подгрузка изображения
        public async Task<ActionResult> UpdateArticle(Guid id, [FromBody] ArticleApiRequest requestBody, CancellationToken cancellationToken = default)
        {
            var command = _mapper.Map<UpdateArticleCommand>((id, requestBody));
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Delete article by identifier.
        /// </summary>
        /// <param name="id">Article identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Article deleted.</response>
        /// <response code="404">Article was not found.</response>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteArticle(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteArticleCommand(id), cancellationToken);

            return result.IsSuccess
                 ? NoContent()
                 : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Publish an article.
        /// </summary>
        /// <remarks>Requires authentication.</remarks>
        /// <param name="id">Article identifier.</param>
        [Authorize]
        [HttpPost("{id:guid}/publish")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Publish(Guid id)
        {
            var result = await _mediator.Send(new PublishArticleCommand(id));

            return result.IsSuccess
                ? Ok()
                : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Create a comment for a given article.
        /// </summary>
        /// <param name="id">Article identifier.</param>
        /// <param name="requestBody">Comment payload.</param>
        /// <response code="201">Comment created successfully.</response>
        /// <response code="404">Article was not found.</response>
        [Authorize]
        [HttpPost("{id:guid}/comments")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Guid>> CreateComment([FromRoute] Guid id, [FromBody] CreateCommentApiRequest requestBody)
        {
            var command = _mapper.Map<CreateCommentCommand>((id, requestBody));

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtRoute(
                    routeName: "GetComment",
                    routeValues: new { id = result.Value },
                    value: result.Value
                    );
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Get article comments using cursor-based pagination.
        /// </summary>
        /// <param name="id">Article identifier.</param>
        /// <param name="requestBody">Cursor pagination parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Returns a paginated list of comments.</response>
        [HttpGet("{id:guid}/comments", Name = "GetArticleCommentsCursorPaginated")]       
        [ProducesResponseType(typeof(CursorPagedListApiResponse<CommentApiResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPagedListApiResponse<CommentApiResponse>>> GetArticleCommentsCursorPaginated(
            [FromRoute] Guid id,
            [FromQuery] GetArticleCommentsCursorPaginatedApiRequest requestBody,
            CancellationToken cancellationToken = default)
        {
            var request = _mapper.Map<GetCommentsCursorQuery>((id, requestBody));

            var result = await _mediator.Send(request, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<CursorPagedListApiResponse<CommentApiResponse>>(result.Value!);

                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Get article comments using offset-based pagination (management view, admin only).
        /// </summary>
        /// <param name="id">Article identifier.</param>
        /// <param name="requestBody">Offset pagination parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Returns a paginated list of comments.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}/comments/management", Name = "GetArticleCommentsOffsetPaginated")]
        [ProducesResponseType(typeof(OffsetPagedListApiResponse<CommentApiResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<OffsetPagedListApiResponse<CommentApiResponse>>> GetArticleCommentsOffsetPaginated(
           [FromRoute] Guid id,
           [FromQuery] GetArticleCommentsOffsetPaginatedApiRequest requestBody,
           CancellationToken cancellationToken = default)
        {
            var request = _mapper.Map<GetCommentsOffsetPagedQuery>((id, requestBody));

            var result = await _mediator.Send(request, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<OffsetPagedListApiResponse<CommentApiResponse>>(result.Value!);

                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }
    }
}
