using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace ETL
{
    public partial class Form1 : Form
    {
        Product prod;
        string xml;
        LoadManager LoadManager;

        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            button3.Enabled = false;
            button5.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ExtractManager manager = new ExtractManager("http://www.ceneo.pl/", extract.Text, "#tab=reviews");
                this.prod = manager.ExtractProduct();
                this.prod.ReviewList = manager.ExtractReview(prod.Product_Id);
                button2.Enabled = true;
            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.Message + "Produkt o takim Id nie istnieje");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string xmlResult = Parser.ParseToXml(prod, saveFileDialog1.FileName);
            this.xml = xmlResult;
            Form2 frm = new Form2();
            frm.ShowFile(this.xml);
            frm.Show();
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                LoadManager = new LoadManager(xml);
                LoadManager.LoadProductToDb();
                string reviewResut = LoadManager.LoadReviewToDb();
                MessageBox.Show("Dodano rekordy do bazy");
                DataTable dtGrid = LoadManager.SelectRecords("Product.Mark, Product.Model, Product.Type, Product.Comments, Review.Autor, Review.Stars, Review.Summary, Review.Date, Review.Recomend, Review.VoteYes, Review.VoteNo, Review.Advantages, Review.Disadvantages ");
                DataTable dt = LoadManager.SelectRecords("Product.*, Review.* ");
                Parser.saveCsv(dt);
                dataGridView1.DataSource = dtGrid;
                button5.Enabled = true;
                MessageBox.Show(reviewResut);
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                ExtractManager manager = new ExtractManager("http://www.ceneo.pl/", extract.Text, "#tab=reviews");
                this.prod = manager.ExtractProduct();
                this.prod.ReviewList = manager.ExtractReview(prod.Product_Id);
                button2.Enabled = true;

                string xmlResult = Parser.ParseToXml(prod, saveFileDialog1.FileName);
                this.xml = xmlResult;
                Form2 frm = new Form2();
                frm.ShowFile(this.xml);
                frm.Show();
                button3.Enabled = true;
                button2.Enabled = false;

                LoadManager = new LoadManager(xml);
                LoadManager.LoadProductToDb();
                LoadManager.LoadReviewToDb();
                MessageBox.Show("Dodano rekordy do bazy");
                DataTable dtGrid = LoadManager.SelectRecords("Product.Mark, Product.Model, Product.Type, Product.Comments, Review.Autor, Review.Stars, Review.Summary, Review.Date, Review.Recomend, Review.VoteYes, Review.VoteNo, Review.Advantages, Review.Disadvantages ");
                DataTable dt = LoadManager.SelectRecords("Product.*, Review.* ");
                Parser.saveCsv(dt);
                dataGridView1.DataSource = dtGrid;
                button5.Enabled = true;

            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.Message + "Produkt o takim Id nie istnieje");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int number = LoadManager.DeleteAllRows();
            MessageBox.Show("Usunięto wszystkie rekordy z bazy ("+ number +")");
            DataTable dt = LoadManager.SelectRecords("Review.* ");
            dataGridView1.DataSource = dt;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
