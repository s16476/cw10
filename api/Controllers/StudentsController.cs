using api.DAL;
using api.models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {

        private readonly IDbService _dbService;


        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("{index}")]
        public IActionResult GetStudents(string index)
        {
            return Ok(_dbService.GetStudent(index));
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            return Ok(_dbService.CreateStudent(student));
        }

        [HttpPut]
        public IActionResult UpdateStudent(Student student)
        {
            var res = _dbService.UpdateStudent(student);
            return Ok(res);
        }

        [HttpDelete("{index}")]
        public IActionResult DeleteStudent(string index)
        {
            _dbService.DeleteStudent(index);
            return Ok("Usuwanie studenta o id " + index + " zakończone");
        }



    }
}