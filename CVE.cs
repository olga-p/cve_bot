using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    internal class CVE
    {
        public string Id { get; }

        public string Description { get; }

        public List<string> References { get; }

        public CVE(string id, string desc, List<string> refs)
        {
            Id = id;   
            Description = desc;
            References = refs;
        }
    

    }
}
