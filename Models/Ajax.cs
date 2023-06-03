namespace DBSchema.Models
{
    public class Ajax
    {
        public class CheckValid
        {
            public string server { get; set; }
            public string user { get; set; }
            public string pwd { get; set; }
        }

        public class QueryDatabase
        {
            public CyGridConfig Config { get; set; }
            public string QName { get; set; }
        }
        public class QueryTable
        {
            public CyGridConfig Config { get; set; }
            public string QName { get; set; }
        }
        public class QueryColumn
        {
            public CyGridConfig Config { get; set; }
            public string QName { get; set; }
            public int TableID { get; set; }
        }

        public class QueryTableDescription
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Pwd { get; set; }
            public string Catalog { get; set; }
            public string TableName { get; set; }
            public int TableID { set; get; }
        }

        public class UpdateTableDescription
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Pwd { get; set; }
            public string Catalog { get; set; }
            public bool InsertNew { get; set; }
            public string TableName { get; set; }
            public string TableSchema { get; set; }
            public string Description { get; set; }
        }

        public class QueryColumnDescription
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Pwd { get; set; }
            public string Catalog { get; set; }
            public int TableID { set; get; }
            public int ColumnID { get; set; }
        }

        public class UpdateColumnDescription
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Pwd { get; set; }
            public string Catalog { get; set; }
            public bool InsertNew { get; set; }
            public string TableName { get; set; }
            public string TableSchema { get; set; }
            public string Description { get; set; }
            public string ColumnName { get; set; }
        }

        public class ExportFile
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Pwd { get; set; }
            public string Catalog { get; set; }
            public string ExportType { get; set; }
            public string FileType { get; set; }
            public string[] TableID { get; set; }
        }
    }
}
