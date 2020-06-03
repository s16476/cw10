using api.DTOs;
using api.exceptions;
using api.models;
using api.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace api.DAL
{
    public class SqlDbService : IDbService
    {


        public IEnumerable<Student> GetStudents()
        {
            var db = new apbdContext();
            var students = db.Student.ToList();
            return students;
        }

        public Student GetStudent(string index)
        {
            var db = new apbdContext();
            var students = db.Student.Find(index);
            return students;
        }

        public Student UpdateStudent(Student student)
        {
            if (String.IsNullOrWhiteSpace(student.IndexNumber))
            {
                throw new InvalidArgumentException("Nie podano numeru indexu studenta do zmiany");
            }
            var db = new apbdContext();
            db.Attach(student);

            if (student.FirstName != null && student.FirstName.Trim().Length > 0)
            {
                db.Entry(student).Property("FirstName").IsModified = true;
            }

            if (student.LastName != null && student.LastName.Trim().Length > 0)
            {
                db.Entry(student).Property("LastName").IsModified = true;
            }

            if (student.BirthDate != null)
            {
                db.Entry(student).Property("BirthDate").IsModified = true;
            }

            db.SaveChanges();

            return GetStudent(student.IndexNumber);
        }

        public void DeleteStudent(string index)
        {
            var db = new apbdContext();
            var s = new Student
            {
                IndexNumber = index
            };
            db.Attach(s);
            db.Remove(s);
            db.SaveChanges();
        }

        public Enrollment EnrollStudentToStudies(StudentEnrollment enrollments)
        {
            if (!IsValidEnrollments(enrollments))
            {
                throw new InvalidArgumentException("Brakujące dane wejściowe");
            }

            var db = new apbdContext();

            var salt = PasswordService.CreateSalt();
            var s = new Student
            {
                FirstName = enrollments.FirstName,
                LastName = enrollments.LastName,
                BirthDate = enrollments.BirthDate,
                IndexNumber = enrollments.IndexNumber,
                Salt = salt,
                Password = PasswordService.Create(enrollments.Password, salt)
            };
            var st = db.Studies.Where(s => s.Name == enrollments.Studies).Single();

            Enrollment en;
            en = db.Enrollment.Where(en => en.IdStudy == st.IdStudy && en.Semester == 1).Single();
            if (en == null)
            {
                var newEnrollment = new Enrollment
                {
                    Semester = 1,
                    IdStudy = st.IdStudy,
                    StartDate = new DateTime()
                };
                db.Enrollment.Add(newEnrollment);
                db.SaveChanges();
                en = newEnrollment;
            }
            s.IdEnrollmentNavigation = en;
            db.Student.Add(s);
            db.SaveChanges();
            return en;
        }

        public Enrollment PromoteStudents(StudentsPromotion promotion)
        {
            var db = new apbdContext();
            var res = db.Enrollment.FromSqlRaw("EXECUTE PromoteStudents {0}, {1}", promotion.Studies, promotion.Semester).AsEnumerable().First();
            return res;
        }


        private Boolean IsValidStudent(Student student)
        {
            if (String.IsNullOrWhiteSpace(student.FirstName) ||
                String.IsNullOrWhiteSpace(student.LastName) ||
                String.IsNullOrWhiteSpace(student.Password) ||
                student.BirthDate == null ||
                student.BirthDate.Equals(new DateTime()) ||
                student.BirthDate > DateTime.Now.AddYears(-18))
            {
                return false;
            }
            return true;
        }

        private Boolean IsValidEnrollments(StudentEnrollment enrollments)
        {
            if (String.IsNullOrWhiteSpace(enrollments.IndexNumber) ||
                String.IsNullOrWhiteSpace(enrollments.FirstName) ||
                String.IsNullOrWhiteSpace(enrollments.LastName) ||
                String.IsNullOrWhiteSpace(enrollments.Studies) ||
                enrollments.BirthDate == null ||
                enrollments.BirthDate.Equals(new DateTime()))
            {
                return false;
            }
            return true;
        }
    }
}
