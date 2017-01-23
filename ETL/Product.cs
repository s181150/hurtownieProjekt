using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL
{
    [Serializable]
    public class Product
    {
        public List<Review> ReviewList { get; set; }
        public int Product_Id { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Comments { get; set; }
    }
}
