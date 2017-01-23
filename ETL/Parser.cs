using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace ETL
{
    static class Parser
    {
        public static string ParseToJson(Product prod, string filename)
        {
            var json = new JavaScriptSerializer().Serialize(prod);
            using (StreamWriter stream = new StreamWriter(filename))
            {
                stream.Write(json.ToString());
            }

            return json.ToString();
        }

        public static string ParseToXml(Product prod, string filename)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter sr = new StreamWriter("export.xml"))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                XmlSerializer xml = new XmlSerializer(typeof(Product));
                xml.Serialize(writer, prod);
                byte[] bytes = stream.ToArray();

                sr.Write(Encoding.UTF8.GetString(bytes));

                return Encoding.UTF8.GetString(bytes);
            }
        }

        public static void saveCsv(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("export.csv", sb.ToString());
        }

    }
}

