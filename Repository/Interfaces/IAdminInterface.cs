using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace repositories.Interfaces
{
    public interface IAdminInterface
    {
        public Task<List<Query>> GetAllQueriesSolved();
        public Task<List<Query>> GetAllQueriesInProgress();
        public Task<List<Query>> GetAllQueriesOpen();
        public Task<List<Query>> GetAllQueries();
    }
}