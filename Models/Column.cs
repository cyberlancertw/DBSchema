namespace DBSchema.Models
{
    public class Column
    {
        public string ColumnName { get; set; }
        public string ColumnID { get; set; }
        //public DateTime CreateTime { get; set; }
        public string Description { get; set; }
        public int DataType { get; set; }
        public int DataLength { get; set; }
        public bool IsNull { get; set; }
        public bool IsIdentity { get; set; }
        public string DefaultValue { get; set; }
        public string IndexName { get; set; }
        public int datacount { get; set; }
    }
}
