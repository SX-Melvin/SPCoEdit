namespace SPCoEdit.Dto.CoEdit
{
    public class IdleSessionGroup
    {
        public long NodeID { get; set; }
        public string WebUrl { get; set; }
        public int SessionCount { get; set; }
        public DateTime LatestSessionCreatedAt { get; set; }
    }
}
