namespace Repository;

public class NotifcationMessage
{

    // type could be either new user registered and new query created
     public string Type { get; set; }   
    public string Username { get; set; }

    //if for query then we need titale of query
    public string QueryTitle { get; set; } 
    public DateTime CreatedAt { get; set; }
}
