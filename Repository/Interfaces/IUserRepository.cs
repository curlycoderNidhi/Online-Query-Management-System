using Repository.Models;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<int> Register(User user);

        Task<User?> Login(LoginModel model);

        Task<int> SubmitQuery(Query query);

        Task<List<Query>> GetUserQueries(int userId);

        Task<bool> UpdateQuery(Query query);

        Task<bool> DeleteQuery(int queryId);
    }
}