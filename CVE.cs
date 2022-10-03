using System.Text;

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

        public string PrintCVEInfo()
        {
            var sb = new StringBuilder();
            sb.Append($"{Id}: \n");
            sb.Append($"{Description}: \n");
            foreach(string r in References)
            {
                sb.Append($"{r}: \n");
            }
            return sb.ToString();
        }
    }
}
