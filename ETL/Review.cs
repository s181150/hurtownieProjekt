using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL
{
    public enum Recommend
    {
        Niepolecam = 0,
        Polecam = 1

    }
    public class Review
    {
        public string Review_Id { get; set; }
        public string Disadvantages { get; set; }
        public string Advantages { get; set; }
        public string Summary { get; set; }
        public string Stars { get; set; }
        public string Autor { get; set; }
        public DateTime Date { get; set; }
        public Recommend Recomend { get; set; }
        public int VoteYes { get; set; }
        public int VoteNo { get; set; }
        public int Product_Id { get; set; }
    }
}
