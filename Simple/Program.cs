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


            var path = Directory.GetCurrentDirectory() + @"\test.html";

            string xml = File.ReadAllText(path, Encoding.UTF8);

            HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
            document.LoadHtml(xml);
            HtmlNode htmlNode = document.DocumentNode;
            var nodes = htmlNode.SelectNodes(@"//li[@class=' j_thread_list clearfix']");



            Console.ReadKey();

        }
    }

}
