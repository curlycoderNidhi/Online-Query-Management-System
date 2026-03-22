using Repository.Models;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<int> Register(User user);

        Task<User?> Login(UserLoginModel model);
        Task UpdatePassword(string email, string password);

         Task<User> GetByEmail(string email);
        Task<User?> GetById(int userId);
  

        // Task<int> SubmitQuery(Query query);

        // Task<List<Query>> GetUserQueries(int userId);

        // Task<bool> UpdateQuery(Query query);

        // Task<bool> DeleteQuery(int queryId);
    }
}