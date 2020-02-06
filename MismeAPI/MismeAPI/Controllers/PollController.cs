using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/poll")]
    public class PollController : Controller
    {
        public PollController()
        {
        }

        [HttpGet()]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllLogs(int? page, int? perPage, string sortOrder, int logType, int eventTypeLog)
        {
            return Ok();
        }
    }
}