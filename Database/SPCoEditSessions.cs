using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPCoEdit.Database
{
    [Table("SPCoEdit_Sessions")]
    public class SPCoEditSessions
    {
        [Key]
        public long ID { get; set; }
        public long NodeID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
