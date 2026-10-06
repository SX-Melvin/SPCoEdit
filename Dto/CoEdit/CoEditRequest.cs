namespace SPCoEdit.Dto.CoEdit
{
    public class CoEditRequest
    {
        public string FileName { get; set; }
        public long NodeID { get; set; }
        public long UserID { get; set; }
        public int Version { get; set; }
    }
}
