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
            if (string.IsNullOrEmpty(query.QName)) query.QName = "%%";
            else query.QName = "%" + query.QName.Trim() + "%";

            try
            {
                if (string.IsNullOrEmpty(server))
                    throw new Exception("連線逾時，請重新連線");

                using(var conn = db.Connection(server, null, user, pwd))
                {
                    sql = "SELECT D.[name] AS [DatabaseName], D.[database_id] AS [DatabaseID], D.[create_date] AS [CreateTime], "
                        + "CAST(SUM(F.size) * 8.0 / 1024 AS DECIMAL(10,2)) AS [UsedStorage] FROM sys.databases AS D JOIN sys.master_files AS F "
                        + "ON D.[database_id] = F.[database_id] WHERE D.database_id > 4 AND D.[name] LIKE @qname GROUP BY D.[name], D.[database_id], D.[create_date]";
                    sql = CyTool.QueryWithPage(sql, query.Config);
                    DynamicParameters para = new DynamicParameters();
                    para.Add("qname", query.QName);
                    qryResult = conn.Query<Database>(sql, para).ToList();
                    info.ObjectData = qryResult;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void QueryTable(string server, string user, string pwd, string catalog, Ajax.QueryTable query, SqlInfo info)
        {
            string sql;
            List<Table> qryResult = null;
            if (string.IsNullOrEmpty(query.QName)) query.QName = "%%";
            else query.QName = "%" + query.QName.Trim() + "%";

            try
            {
                if (string.IsNullOrEmpty(server))
                    throw new Exception("連線逾時，請重新連線");

                using (var conn = db.Connection(server, catalog, user, pwd))
                {
                    sql = "SELECT T.[name] AS [TableName], T.[object_id] AS [TableID], P.[value] AS [Description], T.[max_column_id_used] AS [ColumnCount], "
                        + "T.[create_date] AS [CreateTime] FROM sys.tables AS T LEFT JOIN sys.extended_properties AS P ON T.[object_id] = P.[major_id] "
                        + "AND P.[minor_id] = 0 AND P.[name] = 'MS_Description' WHERE T.[name] LIKE @qname";
                    sql = CyTool.QueryWithPage(sql, query.Config);
                    DynamicParameters para = new DynamicParameters();
                    para.Add("qname", query.QName);
                    qryResult = conn.Query<Table>(sql, para).ToList();
                    info.ObjectData = qryResult;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void QueryColumn(string server, string user, string pwd, string catalog, Ajax.QueryColumn query, SqlInfo info)
        {
            string sql;
            List<Column> qryResult = null;
            if (string.IsNullOrEmpty(query.QName)) query.QName = "%%";
            else query.QName = "%" + query.QName.Trim() + "%";

            try
            {
                if (string.IsNullOrEmpty(server))
                    throw new Exception("連線逾時，請重新連線");

                using (var conn = db.Connection(server, catalog, user, pwd))
                {
                    sql = "SELECT DISTINCT C.[name] AS [ColumnName], C.[column_id] as [ColumnID], P.[value] AS[Description], C.[system_type_id] AS[DataType], C.[max_length] AS[DataLength], C.[is_nullable] AS[IsNull], " 
                        + "C.[is_identity] AS[IsIdentity], D.[definition] AS[DefaultValue], CASE MAX(CAST(X.[is_primary_key] AS INT)) WHEN 1 THEN 1 ELSE NULL END AS[IsPrimaryKey], " 
                        + "STUFF((SELECT distinct ',' + ID.[name] FROM sys.index_columns AS IC LEFT JOIN sys.indexes AS ID ON IC.[index_id] = ID.[index_id] AND IC.[object_id] = ID.[object_id] "
                        + "WHERE IC.[object_id] = I.[object_id] and IC.[column_id] = C.[column_id] FOR XML PATH('')), 1, 1, '') AS[IndexName] FROM sys.columns AS C "
                        + "LEFT JOIN sys.extended_properties AS P ON C.[column_id] = P.[minor_id] AND C.[object_id] = P.[major_id] "
                        + "LEFT JOIN sys.default_constraints AS D ON C.[default_object_id] = D.[object_id] "
                        + "LEFT JOIN sys.index_columns AS I ON C.[object_id] = I.[object_id] AND C.[column_id] = I.[column_id] "
                        + "LEFT JOIN sys.indexes AS X ON I.[object_id] = X.[object_id] AND I.[index_id] = X.[index_id] "
                        + "WHERE C.[object_id] = @tableid AND C.[name] LIKE @qname GROUP BY C.[name],C.[column_id],P.[value],C.[system_type_id],C.[max_length],C.[is_nullable],C.[is_identity],D.[definition],I.[object_id],I.[column_id]";
                    sql = CyTool.QueryWithPage(sql, query.Config);
                    DynamicParameters para = new DynamicParameters();
                    para.Add("tableid", query.TableID);
                    para.Add("qname", query.QName);
                    qryResult = conn.Query<Column>(sql, para).ToList();
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
