#r "System.Configuration"
#r "System.Data"
#r "System.Web"

using System.Net;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string command = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "command", true) == 0)
        .Value;

    if(command == "check")
    {
        string ip = req.GetQueryNameValuePairs()
            .FirstOrDefault(q => string.Compare(q.Key, "ip", true) == 0)
            .Value;
        
        if(ip == null || !CheckIP(ip))
        {
            //nothing to check or not found
            return req.CreateResponse(HttpStatusCode.NotFound, "Not found."); 
        }
        else
        {
            //found
            return req.CreateResponse(HttpStatusCode.Found, "Found.");
        }
            
    }
    else if (command == "add")
    {
        LogIP(req);     
        return req.CreateResponse(HttpStatusCode.OK, "OK Boss.");
    }
    else if (command == "list")
    {
        return req.CreateResponse(HttpStatusCode.OK, ListIP());
    }
    else
    {
        return req.CreateResponse(HttpStatusCode.NotFound, "I don't know what to do.");
    }
}

public static void LogIP(HttpRequestMessage req)
{
    string ip = ((HttpContextWrapper)req.Properties["MS_HttpContext"]).Request.UserHostAddress;

    string cs = ConfigurationManager.ConnectionStrings["VariaCN"].ConnectionString;
    using (SqlConnection cn = new SqlConnection(cs))
    using (SqlCommand cmd = new SqlCommand())
    {
        cn.Open();
        try
        {
            cmd.Connection = cn;
            cmd.CommandText = "INSERT INTO [varia].MOO_IPs (IP, Timestamp) values ('"+ip+"', GETDATE())";
            cmd.ExecuteNonQuery();
        }
        finally
        {
            cn.Close();
        }
    }
}

public static bool CheckIP(string ip)
{
    string cs = ConfigurationManager.ConnectionStrings["VariaCN"].ConnectionString;
    using (SqlConnection cn = new SqlConnection(cs))
    using (SqlCommand cmd = new SqlCommand())
    {
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.CommandText = "MOO_Check";

        SqlParameter pRV = new SqlParameter();
        pRV.Direction = System.Data.ParameterDirection.ReturnValue;
        cmd.Parameters.Add(pRV);

        SqlParameter pIP = new SqlParameter("@IP", ip);
        cmd.Parameters.Add(pIP);

        cn.Open();
        try
        {
            cmd.Connection = cn;
            cmd.ExecuteNonQuery();
            return (int)pRV.Value == 1;
        }
        finally
        {
            cn.Close();
        }
    }
}

public static void ListIP()
{
    return "";
}
