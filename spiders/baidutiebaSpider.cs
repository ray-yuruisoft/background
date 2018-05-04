using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace spiders
{

    [TaskName("baidutiebaSpider")]
    public class baidutiebaSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {
            Identity = "baidu tieba";
            AddStartUrl("https://tieba.baidu.com/f?kw=%E5%A6%96%E7%B2%BE%E7%9A%84%E5%B0%BE%E5%B7%B4&ie=utf-8&pn=0");
            AddPipeline(new baidutiebaPipeline());
            AddPageProcessor(new baidutiebaPageProcessor());
            this.Site.DownloadFiles = true;
            this.SkipTargetUrlsWhenResultIsEmpty = false;

            Dictionary<string, string> tables = new Dictionary<string, string>();
            tables.Add("baidutiebaimgs", "`id` int(11) NOT NULL AUTO_INCREMENT,`postguid` varchar(32) NOT NULL,`url` varchar(100) NOT NULL,`relativepath` varchar(100) NOT NULL,`originalpath` varchar(100) DEFAULT NULL,PRIMARY KEY(`id`)");
            tables.Add("baidutiebaposts", "`id` int(11) NOT NULL AUTO_INCREMENT,`guid` varchar(32) NOT NULL,`title` varchar(100) DEFAULT NULL,`author` varchar(100) DEFAULT NULL,`tiebaname` varchar(100) DEFAULT NULL,`url` varchar(100) NOT NULL,`identity` varchar(100) NOT NULL,`starturl` varchar(100) NOT NULL,`depth` int(5) NOT NULL,`createtime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP,PRIMARY KEY(`id`)");
            InitDatabaseAndTables("spider", tables);

        }
        private void InitDatabaseAndTables(string database, Dictionary<string, string> tables)
        {
            using (var conn = Env.DataConnectionStringSettings.CreateDbConnection())
            {
                conn.MyExecute($"CREATE SCHEMA IF NOT EXISTS `{database}` DEFAULT CHARACTER SET utf8mb4 ;");
                foreach (var item in tables)
                {
                    conn.MyExecute($"CREATE TABLE IF NOT EXISTS `{database}`.`{item.Key}` ({item.Value}) DEFAULT CHARSET=utf8mb4;");
                }
            }
        }
    }

    public class baidutiebaPipeline : BasePipeline
    {

        private static DbConnection con = Env.DataConnectionStringSettings.CreateDbConnection();
        public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
        {
            foreach (var a in resultItems)
            {

                var b = a.Results["img"] as ResultItem;
                b.baidutiebaposts.identity = spider.Identity;
                b.baidutiebaposts.starturl = spider.Site.StartRequests[0].Url;



                try
                {
                    var pp = spider.Site.StartRequests.FirstOrDefault(c => c.Url == b.baidutiebaposts.url);
                }
                catch (Exception e)
                {

                    ;
                }




                b.baidutiebaposts.depth = spider.Site.StartRequests.First(c => c.Url == b.baidutiebaposts.url).Depth;
                foreach (var c in b.list)
                {
                    var intervalPath = new Uri(c.url).LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
                    c.relativepath = $"{Env.PathSeperator}{spider.Identity}{intervalPath}";
                    c.originalpath = $"{Path.Combine(Env.BaseDirectory, "download")}{Env.PathSeperator}{spider.Identity}{intervalPath}";
                    con.Execute("INSERT INTO baidutiebaposts (title,guid,author,tiebaname,url,identity,starturl,depth,createtime) VALUES (@title,@guid,@author,@tiebaname,@url,@identity,@starturl,@depth,@createtime)", c);
                }
                con.Execute("INSERT INTO baidutiebaimgs (postguid,url,relativepath,originalpath) VALUES (@postguid,@url,@relativepath,@originalpath)", b.baidutiebaposts);

            }
        }
    }

    public class baidutiebaPageProcessor : BasePageProcessor
    {
        protected override void Handle(Page page)
        {
            var url = page.Url;
            if (url.IndexOf("https://tieba.baidu.com/p/") != -1)
            {
                //[@class='d_post_content_main  d_post_content_firstfloor']
                var elements = page.Selectable.SelectList(Selectors.XPath(@"//div[@class='d_post_content_main  d_post_content_firstfloor']//cc//img/@src")).Nodes();
                var title = page
                    .Selectable
                    .Select(Selectors.XPath(@"//*[@id='pb_content']/div[@class='left_section']//h3/@title"))
                    .GetValue()
                    .Replace(" ", "")
                    ;
                var author = page
                    .Selectable
                    .Select(Selectors.XPath(@"//*[@id='pb_content']/div[@class='left_section']//a[@class='p_author_name j_user_card'][last()]/text()"))
                    .GetValue()
                    .Replace(" ", "")
                    ;
                var tiebaname = page
                    .Selectable
                    .Select(Selectors.XPath(@"//*[@id='container']/div/div[1]/div[2]/div[2]/a"))
                    .GetValue()
                    .Replace(" ", "")
                    ;

                List<string> imgs = new List<string>();
                foreach (var item in elements)
                {
                    imgs.Add(item.GetValue());
                }

                page.AddTargetRequests(imgs);


                ResultItem resultItem = new ResultItem();
                resultItem.baidutiebaposts.url = url;
                resultItem.baidutiebaposts.guid = Guid.NewGuid().ToString("N");
                resultItem.baidutiebaposts.title = title;
                resultItem.baidutiebaposts.createtime = DateTime.Now;
                resultItem.baidutiebaposts.author = author;
                resultItem.baidutiebaposts.tiebaname = tiebaname;

                foreach (var item in imgs)
                {
                    resultItem.list.Add(new baidutiebaimgs
                    {
                        postguid = resultItem.baidutiebaposts.guid,
                        url = item
                    });
                }
                page.AddResultItem("img", resultItem);
                return;
            }

            #region 列表页

            page.Content = page
                .Content
                .Replace("<!--", "")
                .Replace("-->", "")
                ;
            //利用 Selectable 查询并构造自己想要的数据对象   
            var totalElements = page.Selectable.SelectList(Selectors.XPath(@"//code[@id='pagelet_html_frs-list/pagelet/thread_list']//li[@class=' j_thread_list clearfix']//div[@class='threadlist_title pull_left j_th_tit ']")).Nodes();
            List<string> targetlinks = new List<string>();
            foreach (var element in totalElements)
            {
                targetlinks.Add(element.Links().GetValue());
            }

            //page.AddTargetRequest("https://tieba.baidu.com/p/5634130238");
            page.AddTargetRequests(targetlinks);
            //page.AddResultItem("targetlinks", targetlinks);

            #endregion
        }
    }

    public class baidutiebaimgs
    {
        public int id { get; set; }
        public string postguid { get; set; }
        public string url { get; set; }
        public string relativepath { get; set; }
        public string originalpath { get; set; }
    }

    public class baidutiebaposts
    {

        public int id { get; set; }
        public string guid { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string tiebaname { get; set; }
        public string url { get; set; }
        public string identity { get; set; }
        public string starturl { get; set; }
        public int depth { get; set; }
        public DateTime createtime { get; set; }
    }

    public class ResultItem
    {
        public ResultItem()
        {
            list = new List<baidutiebaimgs>();
            baidutiebaposts = new baidutiebaposts();
        }
        public baidutiebaposts baidutiebaposts { get; set; }
        public List<baidutiebaimgs> list { get; set; }
    }


}
