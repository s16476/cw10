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

        public Student GetStudent(string index);

        public Student UpdateStudent(Student student);

        public void DeleteStudent(string index);

        public Enrollment EnrollStudentToStudies(StudentEnrollment enrollments);

        public Enrollment PromoteStudents(StudentsPromotion promotion);

    }
}
