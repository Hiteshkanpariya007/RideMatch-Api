using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace RideMatch_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly ILoggerManager _logger;

        public BaseController(ILoggerManager logger)
        {
            _logger = logger;
        }
    }
}
