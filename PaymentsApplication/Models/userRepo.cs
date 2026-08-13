using Microsoft.Data.SqlClient;

namespace PaymentsApplication.Models
{
    public class userRepo
    {

        //local
        //private string connectionString = @"Server=.;Database=testDB;Integrated Security=True;TrustServerCertificate=True;";

        //server
        //private string connectionString = @"Server=Server=.\MSSQLSERVER02;Database=testDB;Integrated Security=True;TrustServerCertificate=True;";
        ApplicationDbContext context = new ApplicationDbContext();

        public User? getUserDataByPhoneNumber(string ph)
        {
            if(string.IsNullOrEmpty(ph))
            {
                return null;
            }

            return context.Users.FirstOrDefault(x => x.PhoneNumber == ph);
        }



        public bool InsertUser(User user)
        {

            if (user == null)
            {
                return false;
            }

            context.Users.Add(user);
            context.SaveChanges();
            return true;

        }

        public List<User> getallUsers()
        {
            return context.Users.ToList<User>();
        }

        public bool updateUser(User user)
        {

            User? oldUser = context.Users.FirstOrDefault(x => x.PhoneNumber == user.PhoneNumber);
            oldUser.PhoneNumber = user.PhoneNumber;
            oldUser.Balance = user.Balance;
            oldUser.Password = user.Password;
            oldUser.UserName = user.UserName;
            oldUser.UserEmail = user.UserEmail;
            context.SaveChanges();

            return true;
        }
    }
}
