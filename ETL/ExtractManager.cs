using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ETL
{
    class ExtractManager
    {
        private string url;
        string productId;
        string lastCharacters;
        string wholeUrl;

        public ExtractManager(string adress, string id, string last)
        {
                this.productId = id;
                this.lastCharacters = last;
                this.url = adress;

                StringBuilder sb = new StringBuilder();
                sb.Append(adress);
                sb.Append(id);
                sb.Append(last);

                this.wholeUrl = sb.ToString();
        }

        public Product ExtractProduct()
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = CheckRedirect();
            Product prod = new Product();
            prod.Product_Id = int.Parse(productId);

            if (htmlDoc.DocumentNode != null)
            {
                HtmlAgilityPack.HtmlNode info = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='body']/div[1]/nav/dl/dd");

                if (info != null)
                {
                    int count = info.ChildNodes.Count;
                    prod.Type = info.ChildNodes[count - 4].InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd();
                    prod.Model = info.SelectSingleNode("//strong").InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd();
                    prod.Mark = info.SelectSingleNode("//strong").InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd().TrimStart(' ');

                    int index = prod.Mark.IndexOf(' ');
                    if (index > 0)
                        prod.Mark = prod.Mark.Substring(0, index);
                }

                prod.Comments = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='body']/div[2]/div/div/div[1]/article/div[2]/div[1]").InnerText;
            }

            return prod;
        }

        public List<Review> ExtractReview(int prodId)
        {
            List<Review> reviewsList = new List<Review>();
            int i = 1;
            HtmlAgilityPack.HtmlDocument htmlDoc = CheckRedirect();

            while (htmlDoc != null)
            {
                if (htmlDoc.DocumentNode != null)
                {
                    HtmlAgilityPack.HtmlNode reviews = htmlDoc.DocumentNode.SelectSingleNode("//ol[contains(@class,'product-reviews js_product-reviews js_reviews-hook')]");
                    if (reviews != null)
                    {
                        foreach (HtmlAgilityPack.HtmlNode node in reviews.ChildNodes)
                        {
                            if (node.Name != "#text")
                            {
                                Review rev = new Review();

                                rev.Autor = node.SelectSingleNode(node.XPath + "/header/div[contains(@class,'reviewer-cell')]/div").
                                    InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd();

                                HtmlAgilityPack.HtmlNode recomNode = node.SelectSingleNode(node.XPath + "/header/div[contains(@class,'reviewer-recommendation')]/div/em");
                                if (recomNode != null)
                                    rev.Recomend = (Recommend)Enum.Parse(typeof(Recommend), recomNode.InnerText.Replace(" ", string.Empty), true);
                                rev.VoteYes = int.Parse(node.SelectSingleNode(node.XPath + "/div/div[1]/span[contains(@class,'js_product-review-usefulness vote')]/button[1]/span").InnerText);
                                rev.VoteNo = int.Parse(node.SelectSingleNode(node.XPath + "/div/div[1]/span[contains(@class,'js_product-review-usefulness vote')]/button[2]/span").InnerText);
                                rev.Summary = node.SelectSingleNode(node.XPath + "/div/div[1]/p[contains(@class,'product-review-body')]").
                                    InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd();
                                rev.Stars = node.SelectSingleNode(node.XPath + "/div/div[1]/div/span[contains(@class,'review-score-count')]").InnerText.Replace("/5", string.Empty).Replace(",", ".");
                                string date = node.SelectSingleNode(node.XPath + "/div/div[1]/div/span[contains(@class,'review-time')]/time").Attributes["datetime"].Value.ToString();
                                rev.Date = DateTime.Parse(date);
                                rev.Advantages = ExtractListProsAndConsFromHtml(node, "/div/div[1]/div[contains(@class,'product-review-pros-cons')]", 1);
                                rev.Disadvantages = ExtractListProsAndConsFromHtml(node, "/div/div[1]/div[contains(@class,'product-review-pros-cons')]", 2);
                                rev.Product_Id = prodId;
                                rev.Review_Id = rev.Product_Id.ToString() + rev.Date.ToString();

                                reviewsList.Add(rev);
                            }
                        }

                    }
                }
                i = i + 1;
                this.wholeUrl = url + productId + "/opinie-" + i;
                htmlDoc = null;
                htmlDoc = CheckRedirect();

            }
            return reviewsList;
        }

        private string ExtractListProsAndConsFromHtml(HtmlAgilityPack.HtmlNode values, string xpath, int inf)
        {
            StringBuilder nodeList = new StringBuilder();

            if (values.SelectSingleNode(values.XPath + xpath + "/div[" + inf + "]").ChildNodes.Count != 1)
            {
                HtmlAgilityPack.HtmlNode nodes = values.SelectSingleNode(values.XPath + xpath + "/div[" + inf + "]" + "/ul");

                foreach (HtmlAgilityPack.HtmlNode node in nodes.ChildNodes)
                {
                    if (node.Name != "#text")
                    {
                        nodeList.Append(node.InnerText.Replace("\r\n", string.Empty).TrimStart().TrimEnd());
                        nodeList.Append(",");
                    }
                }
            }
            return nodeList.ToString();
        }

        private HtmlAgilityPack.HtmlDocument CheckRedirect()
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.OptionDefaultStreamEncoding = Encoding.UTF8;
            htmlDoc.OptionReadEncoding = false;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(wholeUrl);
            webRequest.Method = "GET";
            webRequest.ContentType = "text/html";
            webRequest.AllowAutoRedirect = false;

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream stream = response.GetResponseStream();

            if (response.Headers["Location"] == null)
            {
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    htmlDoc.LoadHtml(sr.ReadToEnd());
                }
            }
            else
            {
                return null;
            }
            return htmlDoc;
        }
    }
}