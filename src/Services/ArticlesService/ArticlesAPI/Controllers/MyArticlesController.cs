using ArticlesService.Application.CQRS.Queries.My.GetAllMyArticlesQuery;
using ArticlesService.ArticlesAPI.Extensions;
using ArticlesService.ArticlesAPI.Models.Common;
using ArticlesService.ArticlesAPI.Models.Requests;
using ArticlesService.ArticlesAPI.Models.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArticlesService.ArticlesAPI.Controllers
{
    [Authorize]
    [Route("api/my/articles")]
    public class MyArticlesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public MyArticlesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of articles belonging exclusively to the currently authenticated user, filtered by status.
        /// </summary>
        /// <remarks>
        /// <b>Access Control and Security Rules:</b>
        /// <ul>
        /// <li>This endpoint is strictly user-scoped; it only returns articles where the current authenticated user is the author.</li>
        /// <li>Users can view their own articles in any standard lifecycle status: <c>Published</c> (used be default), <c>Draft</c>, <c>PendingReview</c>, <c>Archived</c>, or <c>Deleted</c>.</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Parameters for pagination, sorting, and status filtering for the user's articles.</param>
        /// <param name="cancellationToken">A token to monitor for operation cancellation requests.</param>
        /// <response code="200">Returns the paginated list of the user's articles matching the criteria.</response>
        /// <response code="400">If the request parameters are invalid or validation failed.</response>
        /// <response code="401">If the request is missing a valid authentication token.</response>
        [HttpGet(Name = "GetArticles")]
        [ProducesResponseType(typeof(OffsetPagedListApiResponse<ArticleShortInfoApiResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<OffsetPagedListApiResponse<ArticleShortInfoApiResponse>>> GetArticles(
            [FromQuery] GetMyArticlesPaginatedApiRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _mapper.Map<GetAllMyArticlesQuery>(request);

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<OffsetPagedListApiResponse<ArticleShortInfoApiResponse>>(result.Value!);

                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }       
    }
}
