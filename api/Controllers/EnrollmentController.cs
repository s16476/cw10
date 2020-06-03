using api.DAL;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/enrollment")]
    public class EnrollmentController : ControllerBase
    {

        private readonly IDbService _dbService;

        public EnrollmentController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public IActionResult GetEnrollmentsByStudentId(string id)
        {
            return Ok(); // _dbService.GetEnrollmentsByStudentId(id));
        }

    }
}