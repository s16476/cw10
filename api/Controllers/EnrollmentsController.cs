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
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudentToStudies(StudentEnrollment enrollment)
        {
            try
            {
                return Ok(_dbService.EnrollStudentToStudies(enrollment));
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("promotions")]
        [HttpPost]
        public IActionResult PromoteStudents(StudentsPromotion promotion)
        {
            try
            {
                return Created("enrollments/promotions", _dbService.PromoteStudents(promotion));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }





    }
}