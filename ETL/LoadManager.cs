using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ETL
{
    class LoadManager
    {
        private string xml;
        private MemoryStream ms;

        public LoadManager(string xml)
        {
            this.xml = xml;
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);
            this.ms = new MemoryStream(encodedString);
        }

        public void LoadProductToDb()
        {
            DataSet ds = new DataSet("ds");
            ds.Tables.Add("Product");
            ds.Tables["Product"].Columns.Add("Product_Id");
            ds.Tables["Product"].Columns.Add("Type");
            ds.Tables["Product"].Columns.Add("Mark");
            ds.Tables["Product"].Columns.Add("Model");
            ds.Tables["Product"].Columns.Add("Comments");

            ms.Flush();
            ms.Position = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(ms);
            doc.DocumentElement.RemoveAllAttributes();
            ms.SetLength(0);
            doc.Save(ms);

            using (SqlConnection con = new SqlConnection(Properties.Resources.ConnectionString))
            using (var command = new SqlCommand("mergeProduct", con) { CommandType = CommandType.StoredProcedure })
            {
                ms.Flush();
                ms.Position = 0;
                ds.ReadXml(ms);

                command.Parameters.Add(new SqlParameter("@myTableType", ds.Tables["Product"]));
                con.Open();
                command.ExecuteNonQuery();
            }
        }

        public string LoadReviewToDb()
        {
            DataSet ds2 = new DataSet("ReviewList");
            ds2.Tables.Add("Review");
            ds2.Tables["Review"].Columns.Add("Review_Id");
            ds2.Tables["Review"].Columns.Add("Autor");
            ds2.Tables["Review"].Columns.Add("Stars", typeof(float));
            ds2.Tables["Review"].Columns.Add("Summary");
            ds2.Tables["Review"].Columns.Add("Date");
            ds2.Tables["Review"].Columns.Add("Recomend");
            ds2.Tables["Review"].Columns.Add("VoteYes");
            ds2.Tables["Review"].Columns.Add("VoteNo");
            ds2.Tables["Review"].Columns.Add("Advantages");
            ds2.Tables["Review"].Columns.Add("Disadvantages");
            ds2.Tables["Review"].Columns.Add("Product_Id");

            ms.Flush();
            ms.Position = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(ms);
            doc.DocumentElement.RemoveAllAttributes();
            ms.SetLength(0);
            doc.Save(ms);

            using (SqlConnection con = new SqlConnection(Properties.Resources.ConnectionString))
            using (var command = new SqlCommand("mergeReview", con) { CommandType = CommandType.StoredProcedure })
            {
                ms.Flush();
                ms.Position = 0;
                ds2.ReadXml(ms);

                command.Parameters.Add(new SqlParameter("@myTableType", ds2.Tables["Review"]));
                con.Open();
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    string result = "";
                    while (dr.Read())
                    {
                        result = result + dr.GetString(0) + ":" + dr.GetInt32(1).ToString() +"\n";
                    }
                    return result;
                }
            }
        }

        public DataTable SelectRecords(string table)
        {
            string query = "select " + table + "from Product Left join Review on Review.Product_Id = Product.Product_Id";
            using (SqlConnection con = new SqlConnection(Properties.Resources.ConnectionString))
            using (var command = new SqlCommand(query, con))
            {
                con.Open();
                SqlDataReader dr = command.ExecuteReader();
                var tb = new DataTable();
                tb.Load(dr);
                return tb;
            }
        }

        public int DeleteAllRows()
        {
            using (SqlConnection con = new SqlConnection(Properties.Resources.ConnectionString))
            using (var command = new SqlCommand("DeleteReview", con) { CommandType = CommandType.StoredProcedure })
            {
                command.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;
                con.Open();
                command.ExecuteNonQuery();
                return Convert.ToInt32(command.Parameters["@count"].Value);
            }
        }
    }
}
