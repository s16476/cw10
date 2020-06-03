using api.DTOs;
using api.exceptions;
using api.models;
using api.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Authentication;

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

        public Student CreateStudent(Student student)
        {

            if (!IsValidStudent(student))
            {
                throw new InvalidArgumentException("Dane nie przeszły walidacji");
            }
            student.Salt = PasswordService.CreateSalt();
            student.Password = PasswordService.Create(student.Password, student.Salt);
            var db = new apbdContext();
            db.Student.Add(student);
            db.SaveChanges();
            return student;
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



        //================================= stare






        public IEnumerable<Enrollment> GetEnrollmentsByStudentId(string id)
        {
            List<Enrollment> _enrollments = new List<Enrollment>();
            using (var connection = new SqlConnection("Data Source=localhost;Initial Catalog=apbd;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select e.* from Enrollment e join Student s on e.IdEnrollment = s.IdEnrollment where s.IndexNumber = @id";
                command.Parameters.AddWithValue("id", id);

                connection.Open();
                var data = command.ExecuteReader();
                while (data.Read())
                {
                    var enrollment = new Enrollment();
                    enrollment.IdEnrollment = Convert.ToInt32(data["IdEnrollment"]);
                    enrollment.IdStudy = Convert.ToInt32(data["IdStudy"]);
                    enrollment.Semester = Convert.ToInt32(data["Semester"]);
                    enrollment.StartDate = Convert.ToDateTime(data["StartDate"]);
                    _enrollments.Add(enrollment);
                }

            }

            return _enrollments;
        }

        public Enrollment EnrollStudentToStudies(StudentEnrollment enrollments)
        {
            if (!IsValidEnrollments(enrollments))
            {
                throw new InvalidArgumentException("Brakujące dane wejściowe");
            }

            using (var connection = new SqlConnection("Data Source=localhost;Initial Catalog=apbd;Integrated Security=True"))
            {
                connection.Open();
                var tran = connection.BeginTransaction();

                //Czy istnieje kierunek
                var studies = new Studies();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;


                    command.CommandText = "select * from studies s where name = @studiesTitle";
                    command.Parameters.AddWithValue("studiesTitle", enrollments.Studies);
                    using (var data = command.ExecuteReader())
                    {
                        if (data.Read())
                        {
                            studies.IdStudy = Convert.ToInt32(data["IdStudy"]);
                            studies.Name = data["Name"].ToString();
                        }
                        else
                        {
                            tran.Rollback();
                            throw new InvalidArgumentException("Kierunek nie istnieje");
                        }
                    }
                }

                //Najnowszy semestr wybranego kierunku
                var enrollment = new Enrollment();
                var enrollmentId = 0;
                var isEnrollmentExists = true;
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;

                    command.CommandText = "select top 1 * from Enrollment where IdStudy = @studyId order by StartDate desc";
                    command.Parameters.AddWithValue("studyId", studies.IdStudy);

                    using (var data = command.ExecuteReader())
                    {
                        command.Connection = connection;
                        command.Transaction = tran;
                        //Pobranie jeśli istnieje
                        if (data.Read())
                        {
                            enrollment.IdEnrollment = Convert.ToInt32(data["IdEnrollment"]);
                            enrollmentId = enrollment.IdEnrollment;
                            enrollment.IdStudy = Convert.ToInt32(data["IdStudy"]);
                            enrollment.Semester = Convert.ToInt32(data["Semester"]);
                            enrollment.StartDate = Convert.ToDateTime(data["StartDate"]);
                        }
                        else
                        {
                            isEnrollmentExists = false;
                        }
                    }
                }
                //Dodanie jeśli nie istnieje 
                if (!isEnrollmentExists)
                {
                    //utworzenie nowego ID
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.Transaction = tran;
                        {
                            using (var commandSelect = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = tran;
                                command.CommandText = "select top 1 * from Enrollment order by IdEnrollment desc";
                                using (var dataid = command.ExecuteReader())
                                {
                                    dataid.Read();
                                    enrollmentId = Convert.ToInt32(dataid["IdEnrollment"]) + 1;
                                }
                            }
                        }
                    }

                    //Zapisanie nowego semestru
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.Transaction = tran;
                        using (var commandSelect = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = tran;
                            command.CommandText = "insert into Enrollment values (@id, 1, @studyId, GETDATE())";
                            command.Parameters.AddWithValue("studyId", studies.IdStudy);
                            command.Parameters.AddWithValue("id", enrollmentId);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                //Czy prawidłowy index
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;
                    command.CommandText = "select * from Student where IndexNumber =  @index";
                    command.Parameters.AddWithValue("index", enrollments.IndexNumber);
                    using (var data4 = command.ExecuteReader())
                    {
                        if (data4.Read())
                        {
                            data4.Close();
                            tran.Rollback();
                            throw new InvalidArgumentException("Index już istnieje");
                        }
                    }
                }

                //Dodajemy studenta wraz z informacją o wpisie na semestr
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;
                    command.CommandText = "insert into Student values (@index, @firstName, @lastName, @birthDate, @enrollment, @password, @salt)";
                    command.Parameters.AddWithValue("index", enrollments.IndexNumber);
                    command.Parameters.AddWithValue("firstName", enrollments.FirstName);
                    command.Parameters.AddWithValue("lastName", enrollments.LastName);
                    command.Parameters.AddWithValue("birthDate", enrollments.BirthDate);
                    command.Parameters.AddWithValue("enrollment", enrollmentId);

                    var salt = PasswordService.CreateSalt();
                    command.Parameters.AddWithValue("salt", salt);
                    command.Parameters.AddWithValue("password", PasswordService.Create(enrollments.Password, salt));

                    command.ExecuteNonQuery();
                }

                tran.Commit();
                return enrollment;
            }

        }


        private Boolean IsValidStudent(Student student)
        {
            if (String.IsNullOrWhiteSpace(student.FirstName) ||
                String.IsNullOrWhiteSpace(student.LastName) ||
                String.IsNullOrWhiteSpace(student.Password) ||
                (student.BirthDate == null || student.BirthDate.Equals(new DateTime()) || student.BirthDate > DateTime.Now.AddYears(-18))) {
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

        public Enrollment PromoteStudents(StudentsPromotion promotion)
        {
            var enrollment = new Enrollment();
            using (SqlConnection con = new SqlConnection("Data Source=localhost;Initial Catalog=apbd;Integrated Security=True"))
            {
                using (SqlCommand cmd = new SqlCommand("PromoteStudents", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Studies", SqlDbType.VarChar).Value = promotion.Studies;
                    cmd.Parameters.Add("@Semester", SqlDbType.Int).Value = promotion.Semester;

                    con.Open();

                    using (var data = cmd.ExecuteReader())
                    {
                        data.Read();
                        enrollment.IdEnrollment = Convert.ToInt32(data["IdEnrollment"]);
                        enrollment.IdStudy = Convert.ToInt32(data["IdStudy"]);
                        enrollment.Semester = Convert.ToInt32(data["Semester"]);
                        enrollment.StartDate = Convert.ToDateTime(data["StartDate"]);
                    }
                }
            }
            return enrollment;
        }

        public Student FindStudentToLogin(string login, string password)
        {

            var student = new Student();

            using (var connection = new SqlConnection("Data Source=localhost;Initial Catalog=apbd;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select s.* from Student s where s.IndexNumber = @login";
                command.Parameters.AddWithValue("login", login);

                connection.Open();
                var data = command.ExecuteReader();
                if (data.Read())
                {
                    student.FirstName = data["FirstName"].ToString();
                    student.LastName = data["LastName"].ToString();
                    student.IndexNumber = data["IndexNumber"].ToString();
                    student.Salt = data["Salt"].ToString();
                    student.Password = data["Password"].ToString();
                }
                else
                {
                    throw new AuthenticationException("Nie można zalogowac użytkownika");
                }
            }

            if (PasswordService.Validate(password, student.Salt, student.Password))
            {
                return student;
            }
            else
            {
                throw new AuthenticationException("Nie można zalogowac użytkownika");
            }

        }


    }
}
