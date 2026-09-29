using Microsoft.AspNetCore.Mvc;

namespace ArticlesService.ArticlesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Для корректной работы Swagger, который по умолчанию ставит media type: text/plain.
    // Это ломало поведение дефолтного problemDetailsService.TryWriteAsync(problemDetailsContext),
    // который работает только с media type: application/json. 
    [Produces("application/json")] 
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
