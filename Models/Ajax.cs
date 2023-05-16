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
    }
}
