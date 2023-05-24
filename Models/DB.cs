using System.Data.SqlClient;
using System.Data;

namespace DBSchema.Models
{
    public class DB
    {
        public DB()
        {

        }

        public IDbConnection Connection(string server, string? catalog, string? user, string? pwd)
        {
            String injection = "integrated security";
            if ((server.ToLower().IndexOf(injection) > -1) ||
                (user != null && user.ToLower().IndexOf(injection) > -1) ||
                (pwd != null && pwd.ToLower().IndexOf(injection) > -1))
                throw new Exception("連線異常");
            return new SqlConnection(GetConnectionString(server, catalog, user, pwd));
        }
        private string GetConnectionString(string server, string? catalog, string? user, string? pwd)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            if (server.Equals("."))
            {
                builder.IntegratedSecurity = true;
                if(catalog != null && !catalog.Equals(""))
                {
                    builder.InitialCatalog = catalog;
                }
            }
            else
            {
                builder.IntegratedSecurity= false;
                builder.DataSource = server;
                builder.UserID = user;
                builder.Password = pwd;
                if (catalog != null && !catalog.Equals(""))
                {
                    builder.InitialCatalog = catalog;
                }
            }
            return builder.ToString();
        }
    }
}
