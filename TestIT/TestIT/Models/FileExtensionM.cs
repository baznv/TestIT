using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestIT.DB;

namespace TestIT.Models
{
    [Table]
    class FileExtensionM
    {
        [PrimaryKey]
        public int ID { get; set; }
        [Unique]
        public string TypeFile { get; set; }
        public string Icon { get; set; }
    }
}
