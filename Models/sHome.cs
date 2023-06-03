using Dapper;
using Microsoft.AspNetCore.Hosting.Server;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.Text;

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
                        + "T.[create_date] AS [CreateTime], M.[name] AS [Schema] FROM sys.tables AS T LEFT JOIN sys.extended_properties AS P ON T.[object_id] = P.[major_id] "
                        + "AND P.[minor_id] = 0 AND P.[name] = 'MS_Description' LEFT JOIN sys.schemas AS M ON T.[schema_id] = M.[schema_id] WHERE T.[name] LIKE @qname";
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
                        + "LEFT JOIN sys.extended_properties AS P ON C.[column_id] = P.[minor_id] AND C.[object_id] = P.[major_id] AND P.[class] = 1 "
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

        public void GetTableDescription(Ajax.QueryTableDescription model, SqlInfo info)
        {
            string sql = string.Empty;
            string result = string.Empty;
            try
            {
                using (var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                {
                    sql = "SELECT TOP(1) P.[value] AS [TableDescription] FROM sys.tables AS T LEFT JOIN sys.extended_properties AS P ON P.[major_id] = T.[object_id] AND P.[minor_id] = 0 AND P.class = 1 "
                        + "WHERE T.[name] = @tablename AND T.[object_id] = @tableid";
                    DynamicParameters para = new DynamicParameters();
                    para.Add("tablename", model.TableName);
                    para.Add("tableid", model.TableID);
                    result = conn.Query<string>(sql, para).FirstOrDefault();
                    info.StringData = result;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void UpdateTableDescription(Ajax.UpdateTableDescription model, SqlInfo info)
        {
            string sql = string.Empty;
            try
            {
                using(var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                {
                    if (model.InsertNew)
                        sql = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA', @level0name=@tableschema, @level1type=N'TABLE', @level1name=@tablename";
                    else if (string.IsNullOrEmpty(model.Description))
                        sql = "EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=@tableschema, @level1type=N'TABLE', @level1name=@tablename";
                    else
                        sql = "EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA',@level0name=@tableschema, @level1type=N'TABLE', @level1name=@tablename";

                    DynamicParameters para = new DynamicParameters();
                    para.Add("description", model.Description.Trim());
                    para.Add("tablename", model.TableName);
                    para.Add("tableschema", model.TableSchema);
                    conn.Execute(sql, para);
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void GetColumnDescription(Ajax.QueryColumnDescription model, SqlInfo info)
        {
            string sql = string.Empty;
            string result = string.Empty;
            try
            {
                using (var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                {
                    sql = "SELECT TOP(1) P.[value] AS [ColumnDescription] FROM sys.all_columns AS C LEFT JOIN sys.extended_properties AS P ON C.[object_id] = P.[major_id] "
                        + "AND C.[column_id] = P.[minor_id] AND P.[class] = 1 WHERE C.[object_id] = @tableid AND C.[column_id] = @columnid";
                    DynamicParameters para = new DynamicParameters();
                    para.Add("tableid", model.TableID);
                    para.Add("columnid", model.ColumnID);
                    result = conn.Query<string>(sql, para).FirstOrDefault();
                    info.StringData = result;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void UpdateColumnDescription(Ajax.UpdateColumnDescription model, SqlInfo info)
        {
            string sql = string.Empty;
            try
            {
                using (var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                {
                    if (model.InsertNew)
                        sql = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@description , @level0type=N'SCHEMA',@level0name=@tableschema, @level1type=N'TABLE',@level1name=@tablename, @level2type=N'COLUMN',@level2name=@columnname";
                    else if (string.IsNullOrEmpty(model.Description))
                        sql = "EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA',@level0name=@tableschema, @level1type=N'TABLE',@level1name=@tablename, @level2type=N'COLUMN',@level2name=@columnname";
                    else
                        sql = "EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@description , @level0type=N'SCHEMA',@level0name=@tableschema, @level1type=N'TABLE',@level1name=@tablename, @level2type=N'COLUMN',@level2name=@columnname";

                    DynamicParameters para = new DynamicParameters();
                    para.Add("description", model.Description.Trim());
                    para.Add("tableschema", model.TableSchema);
                    para.Add("tablename", model.TableName);
                    para.Add("columnname", model.ColumnName);
                    conn.Execute(sql, para);
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void ReadExportTable(string server, string user, string pwd, string catalog, SqlInfo info)
        {
            string sql = string.Empty;
            List<TransferItem> result = null;
            try
            {
                using(var conn = db.Connection(server, catalog, user, pwd))
                {
                    sql = "SELECT [name] AS [Text], [object_id] AS [Value] FROM sys.tables ORDER BY [name];";
                    result = conn.Query<TransferItem>(sql).ToList();
                    info.ObjectData = result;
                    info.Success = true;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void ExportXLSX(string FileName, string Catalog, List<Table> TableData, List<List<Column>> ColumnData, SqlInfo info)
        {
            try
            {
                using(var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    IWorkbook wkBook = new XSSFWorkbook();
                    ISheet sheet = wkBook.CreateSheet("資料表總覽");
                    int rowIndex = 0;
                    IRow row = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < 8; i++)
                        sheet.AutoSizeColumn(i, false);
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 4));
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 6, 7));
                    row.CreateCell(0).SetCellValue("資料庫名稱");
                    row.CreateCell(2).SetCellValue(Catalog);
                    row.CreateCell(5).SetCellValue("製表時間");
                    row.CreateCell(6).SetCellValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    rowIndex += 2;
                    row = sheet.CreateRow(rowIndex);
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 6));
                    row.CreateCell(0).SetCellValue("序號");
                    row.CreateCell(1).SetCellValue("資料表名稱");
                    row.CreateCell(3).SetCellValue("資料表描述");
                    row.CreateCell(7).SetCellValue("欄位數量");

                    for (int i = 0, n = TableData.Count; i < n; i++)
                    {
                        Table table = TableData[i];
                        rowIndex++;
                        row = sheet.CreateRow(rowIndex);
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 6));
                        row.CreateCell(0).SetCellValue((i + 1).ToString());
                        row.CreateCell(1).SetCellValue(table.TableName);
                        row.CreateCell(3).SetCellValue(table.Description);
                        row.CreateCell(7).SetCellValue(table.ColumnCount);
                    }

                    for (int i = 0, n = TableData.Count; i < n; i++)
                    {
                        if (TableData[i].TableName.Length > 31) TableData[i].TableName = TableData[i].TableName.Substring(0, 31); // sheet 長度有限制
                        sheet = wkBook.CreateSheet(TableData[i].TableName);
                        rowIndex = 0;
                        row = sheet.CreateRow(rowIndex);
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 4));
                        row.CreateCell(0).SetCellValue("資料表名稱");
                        row.CreateCell(2).SetCellValue(TableData[i].TableName);
                        rowIndex += 2;
                        row = sheet.CreateRow(rowIndex);
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 5));
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 7, 8));
                        row.CreateCell(0).SetCellValue("序號");
                        row.CreateCell(1).SetCellValue("欄位名稱");
                        row.CreateCell(3).SetCellValue("欄位描述");
                        row.CreateCell(6).SetCellValue("資料類型");
                        row.CreateCell(7).SetCellValue("範圍");
                        row.CreateCell(9).SetCellValue("預設值");
                        row.CreateCell(10).SetCellValue("空值");
                        row.CreateCell(11).SetCellValue("索引");
                        row.CreateCell(12).SetCellValue("自動遞增");
                        row.CreateCell(13).SetCellValue("主鍵");
                        for (int s = 0, t = ColumnData[i].Count; s < t; s++)
                        {
                            rowIndex++;
                            Column column = ColumnData[i][s];
                            row = sheet.CreateRow(rowIndex);
                            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 5));
                            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 7, 8));
                            row.CreateCell(0).SetCellValue((s + 1).ToString());
                            row.CreateCell(1).SetCellValue(column.ColumnName);
                            row.CreateCell(3).SetCellValue(column.Description);
                            row.CreateCell(6).SetCellValue(GetDataType(column));
                            row.CreateCell(7).SetCellValue(GetDataLength(column));
                            row.CreateCell(9).SetCellValue(GetDefaultValue(column));
                            row.CreateCell(10).SetCellValue(GetIsNull(column));
                            row.CreateCell(11).SetCellValue(GetIndex(column));
                            row.CreateCell(12).SetCellValue(GetIdentity(column));
                            row.CreateCell(13).SetCellValue(GetPrimaryKey(column));
                        }

                    }
                    wkBook.Write(fs, false);
                }
                info.StringData = FileName;
                info.Success = true;
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void ExportPDF(string FileName, string Catalog, List<Table> TableData, List<List<Column>> ColumnData, SqlInfo info)
        {

        }

        public void ExportDOCX(string FileName, string Catalog, List<Table> TableData, List<List<Column>> ColumnData, SqlInfo info)
        {

        }

        public void ExportCSV(string FileName, string Catalog, List<Table> TableData, List<List<Column>> ColumnData, SqlInfo info)
        {
            try
            {
                using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("資料庫名稱,").Append(Catalog).Append(",製表時間,").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine().AppendLine();
                    builder.Append("序號,資料表名稱,資料表描述,欄位數量").AppendLine();
                    for(int i = 0, n = TableData.Count; i < n; i++)
                    {
                        Table table = TableData[i];
                        builder.Append((i + 1).ToString()).Append(",").Append(table.TableName).Append(",").Append(table.Description == null ? "" : table.Description.Replace("\n", "，").Replace('\r', ' ').Replace(",", "，")).Append(",").Append(table.ColumnCount.ToString()).AppendLine();
                    }
                    fs.Write(Encoding.UTF8.GetBytes(builder.ToString()));
                }
                info.Success = true;
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }

        }
        public void ExportFile(Ajax.ExportFile model, SqlInfo info)
        {
            string sqlTable = string.Empty, sqlColumn = String.Empty;
            List<Table> tableData = new List<Table>();
            List<List<Column>> columnData = new List<List<Column>>();
            try
            {
                using(var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                {
                    if (model.ExportType.Equals("exportAll"))
                    {
                        sqlTable = "SELECT [object_id] AS [TableID] FROM sys.tables ORDER BY [name];";
                        model.TableID = conn.Query<string>(sqlTable).ToArray();
                    }
                    if (model.TableID.Length == 0) throw new Exception("無選擇資料表，故無檔案匯出");

                    sqlTable = "SELECT T.[name] AS [TableName], P.[value] AS [Description], T.[max_column_id_used] AS [ColumnCount] FROM sys.tables AS T "
                        + "LEFT JOIN sys.extended_properties AS P ON T.[object_id] = P.[major_id] AND P.[minor_id] = 0 AND P.[name] = 'MS_Description' WHERE T.[object_id] = @tableid";


                    sqlColumn = "SELECT DISTINCT C.[name] AS [ColumnName], C.[column_id] as [ColumnID], P.[value] AS[Description], C.[system_type_id] AS[DataType], C.[max_length] AS[DataLength], C.[is_nullable] AS[IsNull], "
                        + "C.[is_identity] AS[IsIdentity], D.[definition] AS[DefaultValue], CASE MAX(CAST(X.[is_primary_key] AS INT)) WHEN 1 THEN 1 ELSE NULL END AS[IsPrimaryKey], "
                        + "STUFF((SELECT distinct ',' + ID.[name] FROM sys.index_columns AS IC LEFT JOIN sys.indexes AS ID ON IC.[index_id] = ID.[index_id] AND IC.[object_id] = ID.[object_id] "
                        + "WHERE IC.[object_id] = I.[object_id] and IC.[column_id] = C.[column_id] FOR XML PATH('')), 1, 1, '') AS[IndexName] FROM sys.columns AS C "
                        + "LEFT JOIN sys.extended_properties AS P ON C.[column_id] = P.[minor_id] AND C.[object_id] = P.[major_id] AND P.[class] = 1 "
                        + "LEFT JOIN sys.default_constraints AS D ON C.[default_object_id] = D.[object_id] "
                        + "LEFT JOIN sys.index_columns AS I ON C.[object_id] = I.[object_id] AND C.[column_id] = I.[column_id] "
                        + "LEFT JOIN sys.indexes AS X ON I.[object_id] = X.[object_id] AND I.[index_id] = X.[index_id] "
                        + "WHERE C.[object_id] = @tableid GROUP BY C.[name],C.[column_id],P.[value],C.[system_type_id],C.[max_length],C.[is_nullable],C.[is_identity],D.[definition],I.[object_id],I.[column_id]";
                    for (int i = 0, n = model.TableID.Length; i < n; i++)
                    {
                        DynamicParameters para = new DynamicParameters();
                        para.Add("tableid", model.TableID[i]);
                        Table? table = conn.Query<Table>(sqlTable, para).FirstOrDefault();
                        if (table == null) throw new Exception("匯出前查詢資料發生異常：" + model.TableID[i] + "資料表不存在");
                        tableData.Add(table);
                        List<Column> data = conn.Query<Column>(sqlColumn, para).ToList();
                        columnData.Add(data);
                    }
                }
                string filename ="wwwroot/temp/" + model.Catalog + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                switch (model.FileType)
                {
                    case "exportXlsx":
                        ExportXLSX(filename + ".xlsx", model.Catalog, tableData, columnData, info); break;
                    case "exportPdf":
                        ExportPDF(filename + ".pdf", model.Catalog, tableData, columnData, info); break;
                    case "exportDocx":
                        ExportDOCX(filename + ".docx", model.Catalog, tableData, columnData, info); break;
                    case "exportCsv":
                        ExportCSV(filename + ".csv", model.Catalog, tableData, columnData, info); break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        private string GetDataType(Column item)
        {
            switch (item.DataType)
            {
                case 56: return "INT";
                case 167: return "VARCHAR";
                case 231: return "NVARCHAR";
                case 61: return "DATETIME";
                case 104: return "BIT";
                case 99: return "NTEXT";
                case 106: return "DECIMAL";
                case 62: return "FLOAT";
                case 52: return "SMALLINT";
                case 48: return "TINYINT";
                case 40: return "DATE";
                case 35: return "TEXT";
                case 41: return "TIME";
                case 239: return "NCHAR";
                case 34: return "IMAGE";
                case 241: return "XML";
                default: return item.DataType.ToString();
            }
        }

        private string GetDataLength(Column item)
        {
            switch (item.DataType)
            {
                case 48: return "0 至 255";
                case 56: return "-2,147,483,648 至 2,147,483,647";
                case 104: return "0 至 1";
                case 167:
                    if (item.DataLength < 0) return "0 至 MAX";
                    else return "0 至 " + item.DataLength;
                case 231:
                    if (item.DataLength < 0) return "0 至 MAX";
                    else return "0 至 " + item.DataLength / 2;
                case 239: return (item.DataLength / 2).ToString();
                default: return item.DataLength.ToString();
            }
        }

        private string GetDefaultValue(Column item)
        {
            return item.DefaultValue == null ? "" : item.DefaultValue;
        }

        private string GetIsNull(Column item)
        {
            return item.IsNull ? "" : "不可";
        }

        private string GetIndex(Column item)
        {
            return string.IsNullOrEmpty(item.IndexName) ? "" : "有";
        }

        private string GetPrimaryKey(Column item)
        {
            return item.IsPrimaryKey ? "是" : "";
        }

        private string GetIdentity(Column item)
        {
            return item.IsIdentity ? "是" : "";
        }
    }
}
