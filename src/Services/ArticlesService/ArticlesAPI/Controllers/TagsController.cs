using ArticlesService.Application.CQRS.Commands.TagCommands.DeleteTag;
using ArticlesService.Application.CQRS.Queries.TagQueries.GetTag;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using ArticlesService.Application.CQRS.Queries.TagQueries.GetAllTags;
using ArticlesService.Application.CQRS.Commands.TagCommands.UpdateTag;
using ArticlesService.Application.CQRS.Commands.TagCommands.CreateTag;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.ArticlesAPI.Models.Requests;
using ArticlesService.ArticlesAPI.Extensions;
using ArticlesService.ArticlesAPI.Models.Common;

namespace ArticlesService.ArticlesAPI.Controllers
{
    /// <summary>
    /// Controller responsible for tag lifecycle operations (CRUD).
    /// </summary>
    public class TagsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TagsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get tag by identifier.
        /// </summary>
        /// <param name="id">Tag identifier (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Tag found and returned.</response>
        /// <response code="404">Tag with specified id was not found.</response>
        // GET api/<TagsController>/5
        [HttpGet("{id:guid}", Name = "GetTag")]
        [ProducesResponseType(typeof(TagApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagApiResponse>> GetTag([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetTagQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<TagApiResponse>(result.Value!);
                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Get offset-paginated list of tags.
        /// </summary>
        /// <param name="request">Pagination, filtering and sorting parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Returns a paginated list of tags.</response>
        [HttpGet(Name = "GetAllTags")]
        [ProducesResponseType(typeof(OffsetPagedListApiResponse<TagShortInfoApiResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<OffsetPagedListApiResponse<TagShortInfoApiResponse>>> GetAllTags(
            [FromQuery] GetAllTagsPaginatedApiRequest request,
            CancellationToken cancellationToken)
        {
            var finalRequest = _mapper.Map<GetAllTagsQuery>(request);
            var result = await _mediator.Send(finalRequest, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<OffsetPagedListApiResponse<TagShortInfoApiResponse>>(result.Value!); 
                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }


        /// <summary>
        /// Create a new tag.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="requestBody">Tag payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="201">Tag was created successfully.</response>
        /// <response code="409">Conflict during creation (for example duplicate name).</response>
        // POST api/<TagsController>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TagApiResponse>> Addtag([FromBody] TagApiRequest requestBody, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<CreateTagCommand>(requestBody);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<TagApiResponse>(result.Value!);

                return CreatedAtRoute(
                    routeName: "GetTag",
                    routeValues: new { id = response.Id },
                    value: response
                    );
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Update existing tag.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="id">Tag identifier.</param>
        /// <param name="requestBody">Updated tag payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Update succeeded.</response>
        /// <response code="404">Tag was not found.</response>
        // PUT api/<TagsController>/5        
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateTag(Guid id, [FromBody] TagApiRequest requestBody, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<UpdateTagCommand>((id, requestBody));
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Delete tag by identifier.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>        
        /// <param name="id">Tag identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Tag deleted.</response>
        /// <response code="404">Tag was not found.</response>
        // DELETE api/<TagsController>/5       
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]        
        [ProducesResponseType(StatusCodes.Status404NotFound)]        
        public async Task<ActionResult> DeleteTag(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {            
            var result = await _mediator.Send(new DeleteTagCommand(id), cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }
    }
}
