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
            Console.WriteLine("Hay nhap duong link trang web can crawl anh: ");
            imgCra.DuongLink = Console.ReadLine();
            Console.WriteLine("Chon che do Crawl (\"a\" hoac \"b\"):");
            imgCra.FindingMode = Console.ReadLine();
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
        public string FileType { get; set; }
        public string FindingMode { get; set; }

        public void AutoCrawlImageFromUrl()
        {
            Task<string> htmlTask = RequestHtmlAsync(DuongLink);
            string html = htmlTask.Result;
            List<string> imgUrls = null;
            if (FindingMode == "a")
            {
                imgUrls = FindImageUrlBasedImgTag(html);
            }else if(FindingMode == "b")
            {
                imgUrls = FindImageUrlBasedAhrefTag(html);
            }
            DownloadImageAsync(imgUrls);
            Console.WriteLine("Chuong trinh dang chay ...");
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

        // Find URLs of Images base <img>
        private List<string> FindImageUrlBasedImgTag(string target)
        {
            // Load the Html into the agility pack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(target);

            // Now, using LINQ to get all Images
            List<HtmlNode> imageNodes = null;
            imageNodes = doc.DocumentNode.SelectNodes("//img").ToList();
            int nodeNum = imageNodes.Count;
            Console.WriteLine("Trang web co {0} hinh anh.", nodeNum);
            List<string> imageUrls = new List<string>();
            foreach (HtmlNode node in imageNodes)
            {
                string src = node.Attributes["src"].Value;
                string httppattern = "^http.+";
                Regex httpregex = new Regex(httppattern);
                if (httpregex.IsMatch(src))
                {
                    imageUrls.Add(src);
                    Console.WriteLine(src);
                }
                //else
                //{
                //    imageUrls.Add(DuongLink + src);
                //    Console.WriteLine(DuongLink + src);
                //}
            }
            Console.WriteLine("Co {0} link anh http.", imageUrls.Count());
            return imageUrls;
        }

        // Find URLs of Images base <a href>
        private List<string> FindImageUrlBasedAhrefTag(string target)
        {
            //Alert
            Console.WriteLine("Su dung FindImageUrlBasedAhref!");
            // Load the Html into the agility pack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(target);

            // Now, using LINQ to get all Images
            List<HtmlNode> imageNodes = null;
            imageNodes = doc.DocumentNode.SelectNodes("//a[@href]").ToList();
            int nodeNum = imageNodes.Count;
            Console.WriteLine("Trang web co {0} <a href>.", nodeNum);
            List<string> imageUrls = new List<string>();
            foreach (HtmlNode node in imageNodes)
            {
                string href = node.Attributes["href"].Value;
                string jpgpattern = ".+jpg$";
                string httpPattern = "^http.+jpg$";
                Regex jpgregex = new Regex(jpgpattern);
                Regex httpregex = new Regex(httpPattern);
                if (httpregex.IsMatch(href))
                {
                    imageUrls.Add(href);
                    Console.WriteLine(href);
                }
                else if (jpgregex.IsMatch(href))
                {
                    imageUrls.Add(DuongLink + href);
                    Console.WriteLine(DuongLink + href);
                }
            }
            Console.WriteLine("Tim thay {0} link anh.", imageUrls.Count());
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
                //Tim extension cua file anh
                string fileTypePattern = "...$";
                Regex fileTypeRegex = new Regex(fileTypePattern);
                Match fileTypeMatch = fileTypeRegex.Match(item);
                FileType = fileTypeMatch.Value;
                //Request image using it's URL
                var client2 = new HttpClient();
                var response = new HttpResponseMessage();
                response = await client2.GetAsync(item);
                var responseContent = await response.Content.ReadAsByteArrayAsync();
                var fileSize = response.Content.Headers.ContentLength;
                //Save to disk
                long kichThuocFile = KichThuocFile * 1024;
                if (fileSize >= kichThuocFile && FileType != "gif")
                {
                    var file = new FileStream(rootFolder + saveLocation + "/" + i + "." + FileType, FileMode.OpenOrCreate);
                    var bw = new BinaryWriter(file);
                    bw.Write(responseContent);
                    bw.Flush();
                    bw.Close();
                    Console.WriteLine("{0}.{1} Download completed...{2} Bytes", i, FileType, fileSize);
                    i++;
                }
            }
            Console.WriteLine("Da hoan tat download " + (i - 1) + " hinh anh co kich thuoc tren {0} kBytes. Vi tri luu: " + rootFolder + saveLocation, KichThuocFile);
        }
    }
}
