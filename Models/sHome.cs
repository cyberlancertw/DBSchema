using Dapper;
using Microsoft.AspNetCore.Hosting.Server;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.Drawing;
using System.Text;

namespace DBSchema.Models
{
    public class sHome
    {
        private readonly DB db;
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor accessor;

        public sHome(DB db, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            env = environment;
            accessor = httpContextAccessor;
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

        public void ExportXLSX(Ajax.ExportFile model, SqlInfo info)
        {
            model.FileName += ".xlsx";
            bool useTitleColor = !string.IsNullOrEmpty(model.TitleColor) && model.TitleColor.Length == 12;
            bool useSpecialColor = !string.IsNullOrEmpty(model.SpecialColor) && model.SpecialColor.Length == 12;
            try
            {
                AdjustTableName(model.TableData);
                RenewProgress(++model.FinishCount, model.TaskCount, model.ProgressID);
                using (var fs = new FileStream(model.FileName, FileMode.Create, FileAccess.Write))
                {
                    IWorkbook wkBook = new XSSFWorkbook();

                    XSSFCellStyle styleTitle = (XSSFCellStyle)wkBook.CreateCellStyle();
                    XSSFFont fontTitle = (XSSFFont)wkBook.CreateFont();
                    if (useTitleColor)
                    {
                        fontTitle.SetColor(new XSSFColor(new byte[] { Convert.ToByte(model.TitleColor.Substring(0, 2), 16), Convert.ToByte(model.TitleColor.Substring(2, 2), 16), Convert.ToByte(model.TitleColor.Substring(4, 2), 16) }));
                        if (!model.TitleColor.Substring(6).ToLower().Equals("ffffff"))
                        {
                            styleTitle.FillPattern = FillPattern.SolidForeground;
                            styleTitle.SetFillForegroundColor(new XSSFColor(new byte[] { Convert.ToByte(model.TitleColor.Substring(6, 2), 16), Convert.ToByte(model.TitleColor.Substring(8, 2), 16), Convert.ToByte(model.TitleColor.Substring(10, 2), 16) }));
                        }
                        fontTitle.FontName = "標楷體";
                        fontTitle.FontHeightInPoints = 12;
                        styleTitle.SetFont(fontTitle);
                        styleTitle.WrapText = true;
                        styleTitle.BorderLeft = BorderStyle.Thin;
                        styleTitle.BorderBottom = BorderStyle.Thin;
                        styleTitle.BorderRight = BorderStyle.Thin;
                        styleTitle.BorderTop = BorderStyle.Thin;
                        styleTitle.VerticalAlignment = VerticalAlignment.Center;
                    }

                    XSSFCellStyle styleTitleCenter = (XSSFCellStyle)wkBook.CreateCellStyle();
                    if (useTitleColor)
                    {
                        if (!model.TitleColor.Substring(6).ToLower().Equals("ffffff"))
                        {
                            styleTitleCenter.FillPattern = FillPattern.SolidForeground;
                            styleTitleCenter.SetFillForegroundColor(new XSSFColor(new byte[] { Convert.ToByte(model.TitleColor.Substring(6, 2), 16), Convert.ToByte(model.TitleColor.Substring(8, 2), 16), Convert.ToByte(model.TitleColor.Substring(10, 2), 16) }));
                        }
                        styleTitleCenter.SetFont(fontTitle);
                        styleTitleCenter.WrapText = true;
                        styleTitleCenter.BorderLeft = BorderStyle.Thin;
                        styleTitleCenter.BorderBottom = BorderStyle.Thin;
                        styleTitleCenter.BorderRight = BorderStyle.Thin;
                        styleTitleCenter.BorderTop = BorderStyle.Thin;
                        styleTitleCenter.VerticalAlignment = VerticalAlignment.Center;
                        styleTitleCenter.Alignment = HorizontalAlignment.Center;
                    }

                    XSSFCellStyle styleSpecial = (XSSFCellStyle)wkBook.CreateCellStyle();
                    XSSFFont fontSpecial = (XSSFFont)wkBook.CreateFont();
                    if (useSpecialColor)
                    {
                        fontSpecial.SetColor(new XSSFColor(new byte[] { Convert.ToByte(model.SpecialColor.Substring(0, 2), 16), Convert.ToByte(model.SpecialColor.Substring(2, 2), 16), Convert.ToByte(model.SpecialColor.Substring(4, 2), 16) }));
                        if (!model.SpecialColor.Substring(6).ToLower().Equals("ffffff"))
                        {
                            styleSpecial.FillPattern = FillPattern.SolidForeground;
                            styleSpecial.SetFillForegroundColor(new XSSFColor(new byte[] { Convert.ToByte(model.SpecialColor.Substring(6, 2), 16), Convert.ToByte(model.SpecialColor.Substring(8, 2), 16), Convert.ToByte(model.SpecialColor.Substring(10, 2), 16) }));
                        }
                        fontSpecial.FontName = "標楷體";
                        fontSpecial.FontHeightInPoints = 12;
                        styleSpecial.SetFont(fontSpecial);
                        styleSpecial.WrapText = true;
                        styleSpecial.BorderLeft = BorderStyle.Thin;
                        styleSpecial.BorderBottom = BorderStyle.Thin;
                        styleSpecial.BorderRight = BorderStyle.Thin;
                        styleSpecial.BorderTop = BorderStyle.Thin;
                        styleSpecial.VerticalAlignment = VerticalAlignment.Center;
                    }

                    XSSFCellStyle styleSpecialCenter = (XSSFCellStyle)wkBook.CreateCellStyle();
                    if (useSpecialColor)
                    {
                        if (!model.SpecialColor.Substring(6).ToLower().Equals("ffffff"))
                        {
                            styleSpecialCenter.FillPattern = FillPattern.SolidForeground;
                            styleSpecialCenter.SetFillForegroundColor(new XSSFColor(new byte[] { Convert.ToByte(model.SpecialColor.Substring(6, 2), 16), Convert.ToByte(model.SpecialColor.Substring(8, 2), 16), Convert.ToByte(model.SpecialColor.Substring(10, 2), 16) }));
                        }
                        styleSpecialCenter.SetFont(fontSpecial);
                        styleSpecialCenter.WrapText = true;
                        styleSpecialCenter.BorderLeft = BorderStyle.Thin;
                        styleSpecialCenter.BorderBottom = BorderStyle.Thin;
                        styleSpecialCenter.BorderRight = BorderStyle.Thin;
                        styleSpecialCenter.BorderTop = BorderStyle.Thin;
                        styleSpecialCenter.VerticalAlignment = VerticalAlignment.Center;
                        styleSpecialCenter.Alignment = HorizontalAlignment.Center;
                    }

                    ICellStyle styleNormal = wkBook.CreateCellStyle();
                    IFont fontNormal = wkBook.CreateFont();
                    fontNormal.FontName = "標楷體";
                    fontNormal.FontHeightInPoints = 12;
                    styleNormal.SetFont(fontNormal);
                    styleNormal.WrapText = true;
                    styleNormal.BorderLeft = BorderStyle.Thin;
                    styleNormal.BorderBottom = BorderStyle.Thin;
                    styleNormal.BorderRight = BorderStyle.Thin;
                    styleNormal.BorderTop = BorderStyle.Thin;
                    styleNormal.VerticalAlignment = VerticalAlignment.Center;

                    ICellStyle styleNormalCenter = wkBook.CreateCellStyle();
                    styleNormalCenter.SetFont(fontNormal);
                    styleNormalCenter.WrapText = true;
                    styleNormalCenter.BorderLeft = BorderStyle.Thin;
                    styleNormalCenter.BorderBottom = BorderStyle.Thin;
                    styleNormalCenter.BorderRight = BorderStyle.Thin;
                    styleNormalCenter.BorderTop = BorderStyle.Thin;
                    styleNormalCenter.VerticalAlignment = VerticalAlignment.Center;
                    styleNormalCenter.Alignment = HorizontalAlignment.Center;

                    ISheet sheet = wkBook.CreateSheet("資料表總覽");
                    int[] columnWidth = { 7, 28, 60 };
                    for (int i = 0; i < 3; i++)
                    {
                        sheet.SetColumnWidth(i, columnWidth[i] * 256);
                    }

                    int rowIndex = 0;
                    IRow row = sheet.CreateRow(rowIndex);
                    row.Height = 2 * 256;

                    ICell cell = row.CreateCell(0);
                    if (useTitleColor)
                    {
                        cell.CellStyle = styleTitleCenter;
                        cell.SetCellValue("序號");

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleTitle;
                        cell.SetCellValue("資料庫名稱");

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleTitle;
                        cell.SetCellValue("製表時間");
                    }
                    else
                    {
                        cell.CellStyle = styleNormalCenter;
                        cell.SetCellValue("序號");

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue("資料庫名稱");

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue("製表時間");
                    }

                    rowIndex++;
                    row = sheet.CreateRow(rowIndex);
                    row.Height = 2 * 256;
                    cell = row.CreateCell(0);
                    cell.CellStyle = styleNormalCenter;
                    cell.SetCellValue(0);

                    cell = row.CreateCell(1);
                    cell.CellStyle = styleNormal;
                    cell.SetCellValue(model.Catalog);

                    cell = row.CreateCell(2);
                    cell.CellStyle = styleNormal;
                    cell.SetCellValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));
                    //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 4));
                    //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 6, 7));

                    rowIndex += 2;
                    row = sheet.CreateRow(rowIndex);

                    row.Height = 2 * 256;

                    if (useTitleColor)
                    {
                        cell = row.CreateCell(0);
                        cell.CellStyle = styleTitleCenter;
                        cell.SetCellValue("序號");

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleTitle;
                        cell.SetCellValue("資料表名稱");

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleTitle;
                        cell.SetCellValue("資料表描述");
                        
                        //cell = row.CreateCell(7);
                        //cell.CellStyle = styleTitle;
                        //cell.SetCellValue("欄位數量");
                    }
                    else
                    {
                        cell = row.CreateCell(0);
                        cell.CellStyle = styleNormalCenter;
                        cell.SetCellValue("序號");

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue("資料表名稱");

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue("資料表描述");

                        //cell = row.CreateCell(3);
                        //cell.CellStyle = styleNormal;
                        //cell.SetCellValue("欄位數量");
                    }

                    //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                    //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 6));

                    for (int i = 0, n = model.TableData.Count; i < n; i++)
                    {
                        Table table = model.TableData[i];
                        rowIndex++;
                        row = sheet.CreateRow(rowIndex);
                        
                        row.Height = (short)((2 + CountBreakLine(table.Description)) * 256);
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 6));

                        cell = row.CreateCell(0);
                        cell.CellStyle = styleNormalCenter;
                        cell.SetCellValue(i + 1);

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue(table.TableName);

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue(table.Description);

                        //cell = row.CreateCell(3);
                        //cell.CellStyle = styleNormal;
                        //cell.SetCellValue(table.ColumnCount);
                    }

                    RenewProgress(++model.FinishCount, model.TaskCount, model.ProgressID);

                    int[] detailColumnWidth = { 7, 28, 50, 12, 20, 9, 7, 7, 10, 7 };

                    for (int i = 0, n = model.TableData.Count; i < n; i++)
                    {
                        Table table = model.TableData[i];
                        sheet = wkBook.CreateSheet(model.TableData[i].TableName);
                        
                        for (int j = 0; j < 10; j++)
                        {
                            sheet.SetColumnWidth(j, detailColumnWidth[j] * 256);
                        }
                        rowIndex = 0;
                        row = sheet.CreateRow(rowIndex);
                        row.Height = 2 * 256;
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 4));
                        if (useTitleColor)
                        {
                            cell = row.CreateCell(0);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("序號");

                            cell = row.CreateCell(1);
                            cell.CellStyle = styleTitle;
                            cell.SetCellValue("資料表名稱");

                            cell = row.CreateCell(2);
                            cell.CellStyle = styleTitle;
                            cell.SetCellValue("資料表描述");
                        }
                        else
                        {
                            cell = row.CreateCell(0);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("序號");

                            cell = row.CreateCell(1);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue("資料表名稱");

                            cell = row.CreateCell(2);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue("資料表描述");
                        }

                        rowIndex++;
                        row = sheet.CreateRow(rowIndex);
                        row.Height = (short)((2 + CountBreakLine(table.Description)) * 256);
                        cell = row.CreateCell(0);
                        cell.CellStyle = styleNormalCenter;
                        cell.SetCellValue(0);

                        cell = row.CreateCell(1);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue(table.TableName);

                        cell = row.CreateCell(2);
                        cell.CellStyle = styleNormal;
                        cell.SetCellValue(table.Description);

                        rowIndex += 2;
                        row = sheet.CreateRow(rowIndex);
                        row.Height = 2 * 256;
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 5));
                        //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 7, 8));
                        if (useTitleColor)
                        {
                            cell = row.CreateCell(0);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("序號");
                            cell = row.CreateCell(1);
                            cell.CellStyle = styleTitle;
                            cell.SetCellValue("欄位名稱");
                            cell = row.CreateCell(2);
                            cell.CellStyle = styleTitle;
                            cell.SetCellValue("欄位描述");
                            cell = row.CreateCell(3);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("資料類型");
                            cell = row.CreateCell(4);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("範圍");
                            cell = row.CreateCell(5);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("預設值");
                            cell = row.CreateCell(6);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("空值");
                            cell = row.CreateCell(7);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("索引");
                            cell = row.CreateCell(8);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("自動遞增");
                            cell = row.CreateCell(9);
                            cell.CellStyle = styleTitleCenter;
                            cell.SetCellValue("主鍵");
                        }
                        else
                        {
                            cell = row.CreateCell(0);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("序號");

                            cell = row.CreateCell(1);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue("欄位名稱");

                            cell = row.CreateCell(2);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue("欄位描述");

                            cell = row.CreateCell(3);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("資料類型");

                            cell = row.CreateCell(4);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("範圍");

                            cell = row.CreateCell(5);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("預設值");

                            cell = row.CreateCell(6);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("空值");

                            cell = row.CreateCell(7);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("索引");

                            cell = row.CreateCell(8);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("自動遞增");

                            cell = row.CreateCell(9);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue("主鍵");
                        }
                        for (int s = 0, t = model.ColumnData[i].Count; s < t; s++)
                        {
                            rowIndex++;
                            Column column = model.ColumnData[i][s];
                            row = sheet.CreateRow(rowIndex);
                            row.Height = (short)((2 + CountBreakLine(column.Description)) * 256);
                            //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 2));
                            //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 5));
                            //sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 7, 8));

                            cell = row.CreateCell(0);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(s + 1);

                            cell = row.CreateCell(1);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue(column.ColumnName);

                            cell = row.CreateCell(2);
                            cell.CellStyle = styleNormal;
                            cell.SetCellValue(column.Description);

                            cell = row.CreateCell(3);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(GetDataType(column));

                            cell = row.CreateCell(4);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(GetDataLength(column));

                            cell = row.CreateCell(5);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(GetDefaultValue(column));

                            cell = row.CreateCell(6);
                            if(useSpecialColor && !column.IsNull)
                            {
                                cell.CellStyle = styleSpecialCenter;
                            }
                            else
                            {
                                cell.CellStyle = styleNormalCenter;
                            }
                            cell.SetCellValue(GetIsNull(column));

                            cell = row.CreateCell(7);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(GetIndex(column));

                            cell = row.CreateCell(8);
                            cell.CellStyle = styleNormalCenter;
                            cell.SetCellValue(GetIdentity(column));

                            cell = row.CreateCell(9);
                            if (useSpecialColor && column.IsPrimaryKey)
                            {
                                cell.CellStyle = styleSpecialCenter;
                            }
                            else
                            {
                                cell.CellStyle = styleNormalCenter;
                            }
                            cell.SetCellValue(GetPrimaryKey(column));                            
                        }
                    }
                    RenewProgress(++model.FinishCount, model.TaskCount, model.ProgressID);
                    wkBook.Write(fs, false);
                    RenewProgress(++model.FinishCount, model.TaskCount, model.ProgressID);
                }
                info.StringData = model.FileName;
                info.Success = true;
            }
            catch (Exception e)
            {
                info.Message = e.Message;
            }
        }

        public void ExportPDF(Ajax.ExportFile model, SqlInfo info)
        {
            model.FileName += ".pdf";
            info.StringData = model.FileName;
            info.Message = "工事中";
        }

        public void ExportDOCX(Ajax.ExportFile model, SqlInfo info)
        {
            model.FileName += ".docx";
            info.StringData = model.FileName;
            info.Message = "工事中";
        }

        public void ExportCSV(Ajax.ExportFile model, SqlInfo info)
        {
            model.FileName += ".csv";
            try
            {
                using (var fs = new FileStream(model.FileName, FileMode.Create, FileAccess.Write))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("資料庫名稱,").Append(model.Catalog).Append(",製表時間,").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine().AppendLine();
                    builder.Append("序號,資料表名稱,資料表描述,欄位數量").AppendLine();
                    for(int i = 0, n = model.TableData.Count; i < n; i++)
                    {
                        Table table = model.TableData[i];
                        builder.Append((i + 1).ToString()).Append(",").Append(table.TableName).Append(",").Append(table.Description == null ? "" : table.Description.Replace("\n", "，").Replace('\r', ' ').Replace(",", "，")).Append(",").Append(table.ColumnCount.ToString()).AppendLine();
                    }
                    fs.Write(Encoding.UTF8.GetBytes(builder.ToString()));
                }
                info.StringData = model.FileName;
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
            model.FinishCount = 1;
            model.TableData = new List<Table>();
            model.ColumnData = new List<List<Column>>();
            try
            {
                using (var conn = db.Connection(model.Server, model.Catalog, model.User, model.Pwd))
                { 
                    if (model.ExportType.Equals("exportAll"))
                    {
                        sqlTable = "SELECT [object_id] AS [TableID] FROM sys.tables ORDER BY [name];";
                        model.TableID = conn.Query<string>(sqlTable).ToArray();
                    }
                    if (model.TableID.Length == 0) throw new Exception("無選擇資料表，故無檔案匯出");
                    model.TaskCount = 5 + model.TableID.Length;
                    RenewProgress(model.FinishCount, model.TaskCount, model.ProgressID);
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
                        model.TableData.Add(table);
                        List<Column> data = conn.Query<Column>(sqlColumn, para).ToList();
                        model.ColumnData.Add(data);
                        RenewProgress(++model.FinishCount, model.TaskCount, model.ProgressID);
                    }
                }
                
                model.FileName ="wwwroot/temp/" + model.Catalog + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                switch (model.FileType)
                {
                    case "exportXlsx":
                        ExportXLSX(model, info); break;
                    case "exportPdf":
                        ExportPDF(model, info); break;
                    case "exportDocx":
                        ExportDOCX(model, info); break;
                    case "exportCsv":
                        ExportCSV(model, info); break;
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
                case 61: return "yyyy-MM-dd HH:mm:ss";
                case 104: return "0 至 1";
                case 167:
                    if (item.DataLength < 0) return "0 至 MAX";
                    else return "0 至 " + item.DataLength;
                case 231:
                    if (item.DataLength < 0) return "0 至 MAX";
                    else return "0 至 " + item.DataLength / 2;
                case 239: return (item.DataLength / 2).ToString();
                case 40: return "yyyy-MM-dd";
                case 41: return "HH:mm:ss";
                case 35: return "0 至 65,535 bytes";
                case 52: return "-32,768 至 32,767";
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
        private void AdjustTableName(List<Table> TableData)
        {
            for (int i = 0, n = TableData.Count; i < n; i++)
            {
                // 工作表不可以有 [ ] * / \ : ?
                TableData[i].TableName = TableData[i].TableName.Replace("[", "［").Replace("]", "］").Replace("*", "＊").Replace("/", "／").Replace("\\", "＼").Replace("?", "？").Replace(":", "：").Replace("'", "、");
                // 工作表長度限 31 字
                if (TableData[i].TableName.Length > 31) TableData[i].TableName = TableData[i].TableName.Substring(0, 31);
            }
        }

        private int CountBreakLine(string data)
        {
            if (string.IsNullOrEmpty(data)) return 0;
            int count = 0;
            for (int i = 0, l = data.Length; i < l; i++)
            {
                if (data[i] == '\n') count++;
            }
            return count;
        }
        async private void RenewProgress(int finishCount, double taskCount, string progressID)
        {
            var session = accessor.HttpContext.Session;
            await Task.Run(() =>
            {
                session.SetString(progressID, (finishCount / taskCount).ToString());
                session.CommitAsync();
            });

        }
    }
}
