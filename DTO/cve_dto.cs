using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.DTO
{
    internal class cve_dto
    {
        public CVE_data_meta_dto CVE_data_meta { get; set; }
        public description_dto description { get; set; }
        public references_dto references { get; set; }
    }
}
