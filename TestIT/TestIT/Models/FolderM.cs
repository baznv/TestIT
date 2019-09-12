using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestIT.DB;

namespace TestIT.Models
{
    [Table]
    class FolderM
    {
        [PrimaryKey]
        public int ID { get; set; }
        [NotNull]
        public string Name { get; set; }
        public int ParentFolderID { get; set; } 
    }
}
