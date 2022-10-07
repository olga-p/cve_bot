using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.DTO
{
    internal class CVEdto
    {
        public CVEDataMeta CVE_data_meta { get; set; }
        public Description description { get; set; }
        public References references { get; set; }
    }
}
