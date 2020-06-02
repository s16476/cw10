using api.DTOs;
using api.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();

        public IEnumerable<Enrollment> GetEnrollmentsByStudentId(string id);

        public Enrollment EnrollStudentToStudies(StudentEnrollment enrollments);

        public Enrollment PromoteStudents(StudentsPromotion promotion);

        public Student FindStudentToLogin(string login, string password);
    }
}
