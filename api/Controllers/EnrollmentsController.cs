using System;
using api.DAL;
using api.DTOs;
using api.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/enrollments")]

    public class EnrollmentsController : ControllerBase
    {

        private readonly IDbService _dbService;

        public EnrollmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost()]
        public IActionResult EnrollStudentToStudies(StudentEnrollment enrollment)
        {
            var res = _dbService.EnrollStudentToStudies(enrollment);
            return Ok(new EnrollmentDTO(res));
        }

        [Route("promotions")]
        [HttpPost]
        public IActionResult PromoteStudents(StudentsPromotion promotion)
        {
            var res = _dbService.PromoteStudents(promotion);
            return Created("enrollments/promotions", new EnrollmentDTO(res));
        }
    }
}