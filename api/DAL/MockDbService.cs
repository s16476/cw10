using api.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DAL
{
    public class MockDbService 
    {
        private static IEnumerable<Student> _students;

        static MockDbService()
        {
            _students = new List<Student>
            {
                //new Student{ IdStudent=1, FirstName="Jan", LastName="Kowalski"},
                //new Student{ IdStudent=2, FirstName="Anna", LastName="Majewski"},
                //new Student{ IdStudent=3, FirstName="Andrzej", LastName="Andrzejewicz"}
            };
        }

        public IEnumerable<Enrollment> GetEnrollmentsByStudentId(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
