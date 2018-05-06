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


            ConfigHelper.Init();
            baidutiebaimgsData baidutiebaimgs = new baidutiebaimgsData();
            var datas = baidutiebaimgs.SelectList();
            List<string> imgs = new List<string>();
            foreach(var item in datas)
            {
                imgs.Add($"<img src='.{item.relativepath}'>");
            }
            var imghtml = String.Join("\r\n", imgs);
            var indexTemppath = Directory.GetCurrentDirectory() + @"/indexTemp.html";
            string indexTemp = File.ReadAllText(indexTemppath, Encoding.UTF8);
            var index = indexTemp.Replace("$", imghtml);
            File.WriteAllText(Directory.GetCurrentDirectory() + @"/index.html",index,Encoding.UTF8);

            Console.ReadKey();

        }
    }

    public class baidutiebaimgsModel
    {
        public int id { get; set; }
        public string postguid { get; set; }
        public string url { get; set; }
        public string relativepath { get; set; }
        public string originalpath { get; set; }
    }


    public class baidutiebaimgsData : DapperBase<baidutiebaimgsModel>
    {
        public override string connectionString => "home_spider";
        public override string tableName { get ; set ; }
    }


}
