using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using Npgsql;


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

        public void GetNewCves()
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
                            LoadCVE2DB(newCve);
                            //Console.WriteLine(item.cve.CVE_data_meta.ID);
                        }
                    }
                }

            }

            Console.WriteLine("Done");
        }

        private void LoadCVE2DB(CVE cve)
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
                int rows_changed = npgc.ExecuteNonQuery();

                foreach (string url in cve.References)
                {
                    npgc.CommandText = $"INSERT INTO refs (ref_url, cve_id) VALUES ('{url}', '{cve.Id}') ";

                    rows_changed = npgc.ExecuteNonQuery();
                }

            }
            nc.Close();
        }

        public void GetCVEById(string cve)
        {
            nc.Close();
            nc.Open();
            NpgsqlCommand npgc = new NpgsqlCommand($"SELECT * from cve WHERE cve_id = '{cve}'", nc);
            NpgsqlDataReader ndr = npgc.ExecuteReader();
            while (ndr.Read())
            {
                var desc = ndr.GetValue(1).ToString();
                Console.WriteLine(desc);
            }
        }

        private void GetCVEByKeyword(string keyword)
        {
            nc.Close();
            nc.Open();
            NpgsqlCommand npgc = new NpgsqlCommand($"SELECT * from cve WHERE description LIKE '%{keyword}%'", nc);
            NpgsqlDataReader ndr = npgc.ExecuteReader();

        }

    }

    class CVE_items_dto
    {
        public CVE_item_dto[] CVE_Items { get; set; }
    }
    class CVE_item_dto
    {
        public cve_dto cve { get; set; }
        
    }

    class cve_dto
    {
        public CVE_data_meta_dto CVE_data_meta { get; set; }
        public description_dto description { get; set; }
        public references_dto references { get; set; }
    }

    class CVE_data_meta_dto
    {
        public string ID { get; set; }
    }
    class description_dto
    {
        public description_data_dto[] description_data { get; set; }
    }
    class description_data_dto
    {
        public string value { get; set; }
    }
    class references_dto
    {
        public reference_data_dto[] reference_data { get; set; }
    }
    class reference_data_dto
    {
        public string url { get; set; }
    }
}
