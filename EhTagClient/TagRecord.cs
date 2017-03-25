using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExClient;

namespace EhTagClient
{
    public class TagRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TagId { get; internal set; }

        public string TagConetnt { get; internal set; }

        public Namespace TagNamespace { get; internal set; }

        public Tag AsTag() => new Tag(TagNamespace, TagConetnt);
    }
}
