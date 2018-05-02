using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCore
{
    class Program
    {
        static void Main(string[] args)
        {

            string xml = 
                @"<?xml version=""1.0"" encoding=""ISO - 8859 - 1"" ?>" +
                 "<bookstore>" +
                     "<book>" +
                        @"<title lang = ""eng""> Harry Potter </title>" +
                        "<price> 29.99 </price>" +
                     "</book>" +
                     "<book>" +
                        @"<title lang = ""eng""> Learning XML </title>" +
                        "<price> 39.95 </price>" +
                     "</book>" +
                 "</bookstore> ";

            HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
            document.LoadHtml(xml);
            HtmlNode htmlNode = document.DocumentNode;
            var nodes = htmlNode.SelectNodes("//title//price");



            Console.ReadKey();

        }
    }

}
