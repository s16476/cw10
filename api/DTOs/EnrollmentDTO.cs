using api.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class EnrollmentDTO
    {
        public EnrollmentDTO(Enrollment input)
        {
            IdEnrollment = input.IdEnrollment;
            Semester = input.Semester;
            IdStudy = input.IdStudy;
            StartDate = input.StartDate;
        }

        public int IdEnrollment { get; set; }
        public int Semester { get; set; }
        public int IdStudy { get; set; }
        public DateTime StartDate { get; set; }

    }
}
