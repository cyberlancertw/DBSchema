using Dapper;
using Microsoft.AspNetCore.Hosting.Server;

namespace DBSchema.Models
{
    public class sHome
    {
        private readonly DB db;
        private readonly IWebHostEnvironment env;

        public sHome(DB db, IWebHostEnvironment environment)
        {
            this.db = db;
            env = environment;
        }

        public void CheckValid(Ajax.CheckValid model, SqlInfo info)
        {
            string sql = string.Empty, result;
            try
            {
                using (var conn = db.Connection(model.server, null, model.user, model.pwd))
                {
                    sql = "SELECT TOP(1) name from master.dbo.sysdatabases";
                    result = conn.Query<string>(sql).FirstOrDefault();
                    if (string.IsNullOrEmpty(result)) throw new Exception("連線失敗");
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }
        public void QueryDatabase(string server, string user, string pwd, Ajax.QueryDatabase query,SqlInfo info)
        {
            string sql;
            List<Database> qryResult = null;
            try
            {
                if (string.IsNullOrEmpty(server))
                    throw new Exception("連線逾時，請重新連線");

                using(var conn = db.Connection(server, null, user, pwd))
                {
                    sql = "SELECT [name] AS [DatabaseName], [dbid] AS [DatabaseID] FROM [master].[dbo].[sysdatabases] WHERE [dbid] > 4";
                    sql = CyTool.QueryWithPage(sql, query.Config);
                    qryResult = conn.Query<Database>(sql).ToList();
                    info.ObjectData = qryResult;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

    }
}
