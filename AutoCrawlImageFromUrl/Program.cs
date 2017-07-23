using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace AutoCrawlImageFromUrl
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageCrawler imgCra = new ImageCrawler();
            Console.WriteLine("Hay nhap duong link trang web can lay anh: ");
            imgCra.DuongLink = Console.ReadLine();
            Console.WriteLine("Nhap kich thuoc file toi thieu (kB): ");
            var kichThuocFileInString = Console.ReadLine();
            imgCra.KichThuocFile = Convert.ToInt32(kichThuocFileInString);
            imgCra.AutoCrawlImageFromUrl();
            Console.ReadKey();
        }        
    }

    class ImageCrawler
    {
        public string DuongLink { get; set; }
        public int KichThuocFile { get; set; }

        public void AutoCrawlImageFromUrl()
        {
            Task<string> htmlTask = RequestHtmlAsync(DuongLink);
            string html = htmlTask.Result;
            List<string> imgUrls = FindImageUrl(html);
            DownloadImageAsync(imgUrls);
        }
        // Request html document
        private async Task<string> RequestHtmlAsync(string diaChiFile)
        {
            var client = new HttpClient();
            var response = new HttpResponseMessage();
            response = await client.GetAsync(diaChiFile);
            //response = Task.Run(client.GetAsync(diaChiFile));
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        // Find URLs of Images in html document which matching the minimum size of image
        private List<string> FindImageUrl(string target)
        {
            // Load the Html into the agility pack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(target);


            // Now, using LINQ to get all Images
            List<HtmlNode> imageNodes = null;
            imageNodes = doc.DocumentNode.SelectNodes("//img").ToList();
            int nodeNum = imageNodes.Count;
            Console.WriteLine(nodeNum);
            List<string> imageUrls = new List<string>();
            foreach (HtmlNode node in imageNodes)
            {
                string src = node.Attributes["src"].Value;
                string httppattern = "^http.+jpg$";
                Regex httpregex = new Regex(httppattern);
                if (httpregex.IsMatch(src))
                {
                    imageUrls.Add(src);
                }
            }
            Console.WriteLine(imageUrls.Count());
            //foreach (var item in imageUrls)
            //{
            //    Console.WriteLine(item);
            //}
            //Console.ReadKey();


            //var imageNodes = doc.DocumentNode.Descendants("img")
            //                   .Select(e => e.GetAttributeValue("src", null))
            //                   .Where(s => !String.IsNullOrEmpty(s));
            //Console.WriteLine(imageNodes);



            //HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//img");
            //Console.WriteLine(linkNodes.Count);
            //Console.ReadLine();
            //foreach (HtmlNode linkNode in linkNodes)
            //{
            //    //HtmlAttribute link = linkNode.Attributes["href"];
            //    HtmlNode imageNode = linkNode.SelectSingleNode("//img");
            //    HtmlAttribute src = imageNode.Attributes["src"];

            //   // string imageLink = link.Value;
            //    string imageUrl = src.Value;
            //    Console.WriteLine(imageUrl);
            //}
            //Console.ReadLine();

            return imageUrls;
        }
        // Download all files whose link is contained in a String Array
        private async void DownloadImageAsync(List<string> target)
        {
            string rootFolder = "d:/ImageCrawler/";
            Console.WriteLine("Hay dat ten thu muc!: ");
            string saveLocation = Console.ReadLine();
            System.IO.Directory.CreateDirectory(rootFolder + saveLocation);
            int i = 1;
            foreach (var item in target)
            {
                var client2 = new HttpClient();
                var response = new HttpResponseMessage();
                response = await client2.GetAsync(item);
                var responseContent = await response.Content.ReadAsByteArrayAsync();

                var file = new FileStream(rootFolder+saveLocation+"/"+i+".jpg", FileMode.OpenOrCreate);
                var bw = new BinaryWriter(file);
                bw.Write(responseContent);
                bw.Flush();
                bw.Close();
                Console.WriteLine(i+" Completed...");
                i++;
            }
            Console.WriteLine("Da hoan tat download hinh anh. Vi tri luu: " + rootFolder + saveLocation);
        }
    }
}
