using ArticlesService.Application.CQRS.Commands.CommentCommands.DeleteComment;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetComment;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArticlesService.Application.CQRS.Commands.CommentCommands.UpdateComment;
using ArticlesService.ArticlesAPI.Extensions;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.ArticlesAPI.Models.Requests;

namespace ArticlesService.ArticlesAPI.Controllers
{
    /// <summary>
    /// Controller responsible for comment operations (retrieval, update, and deletion).
    /// </summary>
    public class CommentsController: BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CommentsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /*
            Эндпоинт GET /api/comments/{id} используется редко как основной источник данных для пользователя.

            Когда метод GET по ID все-таки нужен?
            Этот метод становится «техническим» или вспомогательным для следующих юзкейсов:
             - Переход по уведомлению: «Вам ответили на комментарий». Пользователь кликает, и фронтенду нужно быстро показать этот конкретный ответ и ветку над ним.
             - Прямая ссылка (Deep Linking): Когда кто-то скидывает ссылку на конкретное высказывание.
             - Админка/Модерация: Модератор открывает жалобу на конкретный текст. Ему нужно видеть ТОЛЬКО ЭТОТ ОБЪЕКТ для вынесения вердикта.
         */


        // GET api/<CommentsController>/5
        /// <summary>
        /// Get comment by identifier.
        /// </summary>
        /// <remarks>
        /// Primarily used for auxiliary/technical scenarios: following notifications, deep linking to specific comments, and moderation tasks.
        /// </remarks>
        /// <param name="id">Comment identifier (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Comment found and returned.</response>
        /// <response code="404">Comment with specified id was not found.</response>
        [HttpGet("{id:guid}", Name = "GetComment")]
        [ProducesResponseType(typeof(CommentApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentApiResponse>> GetComment([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCommentQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                var response = _mapper.Map<CommentApiResponse>(result.Value!);
                return Ok(response);
            }

            return result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Update existing comment.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="id">Comment identifier.</param>
        /// <param name="requestBody">Updated comment payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Update succeeded.</response>
        /// <response code="404">Comment was not found.</response>
        // PUT api/<CommentsController>/5        
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentApiRequest requestBody, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<UpdateCommentCommand>((id, requestBody));
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }

        /// <summary>
        /// Delete comment by identifier.
        /// </summary>
        /// <remarks>Endpoint requires authentication.</remarks>
        /// <param name="id">Comment identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Comment deleted.</response>
        /// <response code="404">Comment was not found.</response>
        // DELETE api/<CommentsController>/5       
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteComment(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCommentCommand(id), cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : result.Errors.ToProblem(HttpContext);
        }
    }
}
