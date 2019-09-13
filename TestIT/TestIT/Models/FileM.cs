using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestIT.DB;
using TestIT.Interfaces;

namespace TestIT.Models
{
    [Table]
    public class FileM : IStorage
    {
        [PrimaryKey]
        public int ID { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string Description { get; set; }
        [ForeignKey(typeof(FileExtensionM))]
        public int FileExtentionID { get; set; }
        [ForeignKey(typeof(FolderM))]
        public int FolderID { get; set; }
        public string Content { get; set; }
    }
}
