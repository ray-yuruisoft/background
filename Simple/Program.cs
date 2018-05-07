using background.Tools;
using DapperWrapper;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace SimpleCore
{
    class Program
    {
        static void Main(string[] args)
        {


            //var path = Directory.GetCurrentDirectory() + @"\test.html";

            //string xml = File.ReadAllText(path, Encoding.UTF8);

            //HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
            //document.LoadHtml(xml);
            //HtmlNode htmlNode = document.DocumentNode;
            //var nodes = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']");//实体

            //var nodes2 = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']//h3/@title");//标题

            //var nodes3 = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']//a[@class='p_author_name j_user_card'][last()]/text()");//作者


            //ConfigHelper.Init();
            //baidutieba_imgsData baidutiebaimgs = new baidutieba_imgsData();
            //var datas = baidutiebaimgs.SelectList();

            //List<string> content = new List<string>();

            //foreach (var item in datas.GroupBy(c => c.postguid))
            //{
            //    content.Add($"<div>帖子开始</div>");
            //    foreach (var bottom in item.OrderBy(c => c.floorlevel).ThenBy(c => c.floorindex))
            //    {
            //        content.Add($"<img src='./西区/2018_05_07/{bottom.filename}'>");
            //    }
            //    content.Add($"<div>帖子结束</div>");
            //}

            //var imghtml = String.Join("\r\n", content);
            //var indexTemppath = Directory.GetCurrentDirectory() + @"/indexTemp.html";
            //string indexTemp = File.ReadAllText(indexTemppath, Encoding.UTF8);
            //var index = indexTemp.Replace("$", imghtml);
            //File.WriteAllText(Directory.GetCurrentDirectory() + @"/index.html", index, Encoding.UTF8);

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.GZip,
                UseProxy = true,
                UseCookies = true,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10
            };

            var client = new HttpClient(handler);

            var respon = client.GetAsync("https://tieba.baidu.com/f?kw=%E7%AE%80%E9%98%B3&ie=utf-8&pn=0").Result;


            Console.ReadKey();

        }
    }

    public class baidutieba_imgsModel
    {
        public int id { get; set; }
        public string postguid { get; set; }
        public string url { get; set; }
        public string filename { get; set; }
        public string originalpath { get; set; }
        public int floorlevel { get; set; }
        public int floorindex { get; set; }
        public DateTime posttime { get; set; }
    }


    public class baidutieba_imgsData : DapperBase<baidutieba_imgsModel>
    {
        public override string connectionString => "home_spider";
        public override string tableName { get; set; }
    }


}
