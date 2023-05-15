namespace DBSchema.Models
{
    public class SqlInfo
    {
        /// <summary>
        /// 動作是否成功
        /// </summary>
        public bool Success { get; set; } = false;
        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 傳遞物件
        /// </summary>
        public Object? ObjectData { get; set; }
        /// <summary>
        /// 傳遞物件2
        /// </summary>
        public Object? ObjectData2 { get; set; }
        /// <summary>
        /// 傳遞字串
        /// </summary>
        public string? StringData { get; set; }
        /// <summary>
        /// 傳遞數字
        /// </summary>
        public int IntData { get; set; }
    }
}
