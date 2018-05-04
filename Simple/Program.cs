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
            var nodes = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']");//实体

            var nodes2 = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']//h3/@title");//标题

            var nodes3 = htmlNode.SelectSingleNode("//*[@id='pb_content']/div[@class='left_section']//a[@class='p_author_name j_user_card'][last()]/text()");//作者
            


            Console.ReadKey();

        }
    }

}
