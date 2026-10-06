using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPCoEdit.Database
{
    [Table("DTreeCore")]
    public class DTreeCore
    {
        public long DataID { get; set; }
        public long VersionNum { get; set; }
        public string Name { get; set; }
    }
}
