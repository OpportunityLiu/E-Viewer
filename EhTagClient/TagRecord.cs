using ExClient.Tagging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EhTagClient
{
    [System.Diagnostics.DebuggerDisplay(@"[{TagNamespace}:{TagConetnt}]")]
    public class TagRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TagId { get; internal set; }

        public Namespace TagNamespace { get; internal set; }

        public string TagConetnt { get; internal set; }

        public Tag AsTag() => new Tag(TagNamespace, TagConetnt);
    }
}
