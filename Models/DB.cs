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
            return new SqlConnection(GetConnectionString(server, catalog, user, pwd));
        }
        private string GetConnectionString(string server, string? catalog, string? user, string? pwd)
        {
            if (server.Equals("."))
            {
                if(catalog == null || catalog.Equals(""))
                {
                    return "Data Source=.;Integrated Security=True";
                }
                else
                {
                    return "Data Source=.;Initial Catalog=" + catalog + ";Integrated Security=True";
                }
            }
            else
            {
                if (catalog == null || catalog.Equals(""))
                {
                    return "Data Source=" + server + ";User ID=" + user + ";Password=" + pwd;
                }
                else
                {
                    return "Data Source=" + server + ";Initial Catalog=" + catalog + ";User ID=" + user + ";Password=" + pwd;
                }
            }
        }
    }
}
