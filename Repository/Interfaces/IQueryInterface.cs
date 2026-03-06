namespace querymanagment.Models;

public interface IQueryInterface
{
public Task<int> Create(Query query);
public Task<int> Update(Query query);
public Task<int> Delete(int id);
public Task<List<Query>> GetAll();
public Task<Query> GetById(int id);


// for user side
public Task<List<Query>> GetByUserId(int userid);


//for employee side 
public Task<List<Query>> GetByEmployeeId(int empid);


//for admin side 
public Task<int> AssignEmployee(int queryid,int empid);

//for employee side 
public Task<int> UpdateStatus(int queryid ,int empid, string status , string? comment);
// public Task<int> AddComment(int queryid , string comment);


// for admin side 

public Task<List<Query>> GetUnassignedQueries();

}
