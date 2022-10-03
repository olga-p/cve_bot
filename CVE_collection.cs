using System.IO.Compression;
using System.Text.Json;
using System.Net;
using Npgsql;
using Bot.DTO;



namespace Bot
{
    internal class CVE_collection
    {
        private string connString = "Host=127.0.0.1;Username=postgres;Password=cdrhawa7;Database=cve_db";
        private NpgsqlConnection nc;

        public CVE_collection()
        {
            nc = new NpgsqlConnection(connString);
        }

        public int GetNewCves()
        {
            WebClient wc = new WebClient();

            DirectoryInfo archDir = new DirectoryInfo("arch");

            if (archDir.Exists)
            {
                archDir.Delete(true);
            }
            archDir.Create();
            var zipPath = "arch/json.zip";

            wc.DownloadFile("https://nvd.nist.gov/feeds/json/cve/1.1/nvdcve-1.1-recent.json.zip", zipPath);

            ZipFile.ExtractToDirectory(zipPath, archDir.FullName);

            int count = 0;
            foreach (FileInfo file in archDir.GetFiles("*.json"))
            {
                var fs = file.Open(FileMode.Open);
                byte[] b = new byte[file.Length];
                
                if (fs.Read(b, 0, b.Length) == file.Length)
                {
                    var text = JsonDocument.Parse(b);
                    var items = text.Deserialize<CVE_items_dto>();
                    if (items != null)
                    {
                        foreach (var item in items.CVE_Items)
                        {
                            var refs = new List<string>();
                            foreach (var refCVE in item.cve.references.reference_data)
                            {
                                refs.Add(refCVE.url);
                            }
                            CVE newCve = new CVE(item.cve.CVE_data_meta.ID, item.cve.description.description_data[0].value, refs);
                            if (LoadCVE2DB(newCve))
                            {
                                count++;
                            }
                            //Console.WriteLine(item.cve.CVE_data_meta.ID);
                        }
                    }
                }

            }
            return count;
        }

        private bool LoadCVE2DB(CVE cve)
        {
            nc.Close();
            nc.Open();
            NpgsqlCommand npgc = new NpgsqlCommand($"SELECT * from cve WHERE cve_id = '{cve.Id}'", nc);
            NpgsqlDataReader ndr = npgc.ExecuteReader();
            if (!ndr.HasRows)
            {
                ndr.Close();
                var description = cve.Description.Replace("'", " ");
                npgc.CommandText = $"INSERT into cve VALUES ('{cve.Id}', '{description}') ";
                npgc.ExecuteNonQuery();

                foreach (string url in cve.References)
                {
                    npgc.CommandText = $"INSERT INTO refs (ref_url, cve_id) VALUES ('{url}', '{cve.Id}') ";

                    npgc.ExecuteNonQuery();
                }

            }
            nc.Close();
            return !ndr.HasRows;
        }

        public CVE GetCVEById(string cve)
        {
            nc.Open();

            cve = cve.ToUpper();
            NpgsqlCommand npgc = new NpgsqlCommand($"SELECT * from cve WHERE cve_id = '{cve}'", nc);
            NpgsqlDataReader ndr = npgc.ExecuteReader();
           
            if (ndr.Read())
            {
                var cveId = ndr.GetValue(0).ToString();
                var desc = ndr.GetValue(1).ToString();
                var nc2 = new NpgsqlConnection(connString);
                nc2.Open();
                NpgsqlCommand qwe = new NpgsqlCommand($"SELECT * from refs WHERE cve_id = '{cveId}'", nc2);
                NpgsqlDataReader rdr = qwe.ExecuteReader();
                var refs = new List<string>();
                while (rdr.Read())
                {
                    refs.Add(rdr.GetValue(1).ToString());
                }               
                rdr.Close();
                nc2.Close();
                nc.Close();

                return new CVE(cveId, desc, refs);
            }
            ndr.Close();
            nc.Close();
            return null;
        }
        
        public List<CVE> GetCVEByKeyword(string keyword)
        {
            nc.Open();
            NpgsqlCommand npgc = new NpgsqlCommand($"SELECT * from cve WHERE description ILIKE '%{keyword}%'  ORDER BY id DESC LIMIT 10", nc);
            NpgsqlDataReader ndr = npgc.ExecuteReader();
            var cveList = new List<CVE>();
            while (ndr.Read())
            {
                var cveId = ndr.GetValue(0).ToString();
                var desc = ndr.GetValue(1).ToString();
                var nc2 = new NpgsqlConnection(connString);
                nc2.Open();
                NpgsqlCommand qwe = new NpgsqlCommand($"SELECT * from refs WHERE cve_id = '{cveId}'", nc2);
                NpgsqlDataReader rdr = qwe.ExecuteReader();
                var refs = new List<string>();
                while (rdr.Read())
                {
                    refs.Add(rdr.GetValue(1).ToString());
                }
                rdr.Close();
                nc2.Close();
                cveList.Add(new CVE(cveId, desc, refs));

            }
            ndr.Close();
            nc.Close();
            return cveList;
        }
    }
}
