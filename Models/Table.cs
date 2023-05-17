namespace DBSchema.Models
{
    public class Table
    {
        public string TableName { get; set; }
        public int TableID { get; set; }
        public DateTime CreateTime { get; set; }
        public string Description { get; set; }
        public int ColumnCount { get; set; }
        public int datacount { get; set; }
    }
}
