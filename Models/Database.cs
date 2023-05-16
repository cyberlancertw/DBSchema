namespace DBSchema.Models
{
    public class Database
    {
        public string DatabaseName { get; set; }
        public int DatabaseID { get; set; }
        public int UsedStorage { get; set; }
        public DateTime CreateTime { get; set; }
        public int datacount { get; set; }
    }
}
