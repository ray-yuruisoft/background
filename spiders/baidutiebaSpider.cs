using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace spiders
{

    [TaskName("baidutiebaSpider")]
    public class baidutiebaSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {
            if(arguments.Length!=0){
                Identity = arguments[0];
                this.Downloader = new MyDownloader(bool.Parse(arguments[1]));
                AddStartUrl("https://"+arguments[2]);
            }else{
                Identity = "baidutieba";
                AddStartUrl("https://tieba.baidu.com/f?kw=%E5%A6%96%E7%B2%BE%E7%9A%84%E5%B0%BE%E5%B7%B4&ie=utf-8&pn=0");
            }

            AddPipeline(new baidutiebaPipeline());
            AddPageProcessor(new baidutiebaPageProcessor());
            this.Site.DownloadFiles = true;
            this.SkipTargetUrlsWhenResultIsEmpty = false;

            Dictionary<string, string> tables = new Dictionary<string, string>();
            tables.Add("baidutieba_imgs", "`id` int(11) NOT NULL AUTO_INCREMENT,`postguid` varchar(32) NOT NULL,`url` varchar(255) NOT NULL,`filename` varchar(255) NOT NULL,`originalpath` varchar(255) NOT NULL,`floorlevel` int(11) NOT NULL,`floorindex` int(11) NOT NULL,`posttime` datetime NOT NULL,PRIMARY KEY (`id`)");
            tables.Add("baidutieba_posts", "`id` int(11) NOT NULL AUTO_INCREMENT,`guid` varchar(32) NOT NULL,`spiderguid` varchar(32) NOT NULL,`url` varchar(255) NOT NULL,`title` varchar(100) NOT NULL,`landlord` varchar(100) NOT NULL,`depth` int(5) NOT NULL,PRIMARY KEY (`id`)");
            tables.Add("baidutieba_spiders", "`id` int(11) NOT NULL AUTO_INCREMENT,`guid` varchar(32) DEFAULT NULL,`identity` varchar(100) DEFAULT NULL,`starturl` varchar(255) DEFAULT NULL,`createtime` datetime DEFAULT NULL,`tiebaname` varchar(100) DEFAULT NULL,PRIMARY KEY (`id`)");
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
                if (a.Results.ContainsKey("spiderInfo"))
                {
                    var spiderInfo = a.Results["spiderInfo"] as baidutieba_spiders;
                    spiderInfo.identity = spider.Identity;
                    spiderInfo.starturl = spider.Site.StartRequests[0].Url;
                    Cache.Instance.Set("spiderguid", spiderInfo.guid);
                    con.Execute("INSERT INTO baidutieba_spiders (guid,identity,starturl,createtime,tiebaname) VALUES (@guid,@identity,@starturl,@createtime,@tiebaname)", spiderInfo);
                }
                if (a.Results.ContainsKey("imgs"))
                {
                    var imgs = a.Results["imgs"] as List<baidutieba_imgs>;
                    foreach (var c in imgs)
                    {
                        string intervalPath = null;
                        var s = ((spider as Spider).Downloader as MyDownloader);
                        if (s._customintervalPath)
                        {
                            intervalPath = $"{Env.PathSeperator}{DateTime.Now.ToString("yyyy_MM_dd")}{Env.PathSeperator}{new Uri(c.url).LocalPath.Replace("//", "").Replace("/", "")}";
                        }
                        else
                        {
                            intervalPath = new Uri(c.url).LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
                        }
                        var t = intervalPath.Split(Env.PathSeperator[0]);
                        c.filename = t[t.Length - 1];
                        c.originalpath = $"{Path.Combine(Env.BaseDirectory, "download")}{Env.PathSeperator}{spider.Identity}{intervalPath}";
                        con.Execute("INSERT INTO baidutieba_imgs (postguid,url,filename,originalpath,floorlevel,floorindex,posttime) VALUES (@postguid,@url,@filename,@originalpath,@floorlevel,@floorindex,@posttime)", c);
                    }
                }
                if (a.Results.ContainsKey("post"))
                {
                    var post = a.Results["post"] as baidutieba_posts;
                    post.landlord = Regex.Replace(post.landlord, @"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD", "");
                    post.title = Regex.Replace(post.title, @"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD", "");
                    con.Execute("INSERT INTO baidutieba_posts (guid,spiderguid,url,title,landlord,depth) VALUES (@guid,@spiderguid,@url,@title,@landlord,@depth)", post);
                }
            }
        }
    }

    public class baidutiebaPageProcessor : BasePageProcessor
    {
        protected override void Handle(Page page)
        {


            #region 3.0帖子内 img添加


            #region 3.1帖子内 列表展开

            var posturlindex = page.Url.IndexOf("?see_lz=1&pn=1");
            if(posturlindex!=-1 && page.Url.Length == posturlindex+14)
            {
                
                var postpagenum = int.Parse(page.Selectable.Select(Selectors.XPath(@"//*[@id='thread_theme_5']/div[1]/ul/li[2]/span[2]/text()")).GetValue());
                var landlord = page.Selectable.Select(Selectors.XPath(@"//*[@id='j_p_postlist']/div[1]/div[1]/ul/li[3]/a/text()")).GetValue();
                var title = page.Selectable.Select(Selectors.XPath(@"//*[@id='j_core_title_wrap']/h3/text()")).GetValue();

                Cache.Instance.Set(page.Url.Substring(0, posturlindex), new Tuple<string, int>(Guid.NewGuid().ToString("N"), postpagenum));

                if(postpagenum > 1){
                    var postpageurls = new List<string>();
                    for (var i = 2; i <= postpagenum; i++)
                    {
                        postpageurls.Add(page.Url.Substring(0, posturlindex) + "?see_lz=1&pn=" + i.ToString());
                    }
                    page.AddTargetRequests(postpageurls);
                }

                //增加缓存，帖子列表其他页 从该处获取 guid
                baidutieba_posts baidutieba_Posts = new baidutieba_posts();
                Tuple<string, int> tuple = Cache.Instance.Get(page.Url.Substring(0, posturlindex));
                baidutieba_Posts.guid = tuple.Item1;
                baidutieba_Posts.landlord = landlord;
                baidutieba_Posts.spiderguid = Cache.Instance.Get("spiderguid");
                baidutieba_Posts.url = page.Url;
                baidutieba_Posts.title = title;
                baidutieba_Posts.depth = 1;
                page.AddResultItem("post",baidutieba_Posts);
            }

            #endregion

            //3.2
            if (page.Url.IndexOf("https://tieba.baidu.com/p/") != -1)
            {


                List<baidutieba_imgs> models = new List<baidutieba_imgs>();
                //所有楼层的div集合
                var floorsCount = page.Selectable.SelectList(Selectors.XPath(@"//*[contains(@class,'d_post_content_main')]")).Nodes().Count();
                for (var i = 1; i <= floorsCount; i++)
                {

                    var imgs = page
                        .Selectable
                        .SelectList
                        (Selectors.XPath
                         ($"(//*[contains(@class,'d_post_content_main')])[{i}]//img/@src")
                        )
                        .GetValues().ToList();
                    
                    var posttime = page
                        .Selectable
                        .Select
                        (
                            Selectors.XPath
                            ($"((//*[contains(@class,'d_post_content_main')])[{i}]//span[@class='tail-info'])[3]")
                        )
                        .GetValue();
                    
                    for (var k = 0; k < imgs.Count(); k++)
                    {
                        var temp = imgs[k].Split('?')[0].Split('.');
                        var imgType = temp[temp.Length - 1];
                        if (imgType == "png" || imgType == "gif") break;
                        baidutieba_imgs model = new baidutieba_imgs();
                        model.floorindex = k+1;
                        model.floorlevel = i;



                        //todo
                        model.posttime = DateTime.Now;




                        model.url = imgs[k];

                        try{

                            model.postguid = Cache.Instance.Get(page.Url.Split('?')[0]).Item1;


                        }catch(Exception e){


                            ;
                        }



                        models.Add(model);
                    }
                }

                if (models.Count() == 0)
                {
                    page.Skip = true;
                    return;
                }
                page.AddTargetRequests(models.Select(cw => cw.url));
                page.AddResultItem("imgs", models);



                //todo
                // 清理缓存，避免缓存 越来越多
                //var index = page.Url.IndexOf("?see_lz=1&pn=");
                //var num = int.Parse(page.Url.Substring(index + 13));
                //Tuple<string, int> tuple = Cache.Instance.Get(page.Url.Substring(0, index));
                //if (tuple.Item2 == num) 
                    //Cache.Instance.Remove(page.Url.Substring(0, index));

                return;
            }

            #endregion

            page.Content = page
                .Content
                .Replace("<!--", "")
                .Replace("-->", "")
                ;
            
            #region 1.首页处理 展开列表

            var pageurlindex = page.Url.IndexOf("&pn=0");
            if (pageurlindex != -1)
            {
                

                var tiebaname = page.Selectable.Select(Selectors.XPath("//*[@class='card_title']/a/text()")).GetValue().Replace("\n","").Replace(" ","");
                var endurl = page.Selectable.Select(Selectors.XPath(@"//*[@id='frs_list_pager']/a[11]")).Links().GetValue();
                var numarr = endurl.Split('=');
                var num = int.Parse(numarr[numarr.Length - 1]);
                var pagenum = num / 50 + 1;
                List<string> listurls = new List<string>();
                var urlprefix = page.Url.Substring(0, pageurlindex);
                for (var i = 1; i <= pagenum; i++)
                {
                    listurls.Add($"{urlprefix}&pn={i * 50}");
                }
                page.AddTargetRequests(listurls);

                #region spiderInfo

                baidutieba_spiders spider = new baidutieba_spiders();
                spider.createtime = DateTime.Now;
                spider.guid = Guid.NewGuid().ToString("N");
                spider.tiebaname = tiebaname;
                page.AddResultItem("spiderInfo",spider);

                #endregion

            }

            #endregion


            #region 2.添加帖子


            //利用 Selectable 查询并构造自己想要的数据对象   
            var totalElements = page
                .Selectable
                .SelectList
                (Selectors.XPath(@"//code[@id='pagelet_html_frs-list/pagelet/thread_list']//li[@class=' j_thread_list clearfix']//div[@class='threadlist_title pull_left j_th_tit ']"))
                .Nodes();
            List<string> targetlinks = new List<string>();
            foreach (var element in totalElements)
            {
                //注意  添加 ?see_lz=1 是只看楼主
                targetlinks.Add(element.Links().GetValue() + "?see_lz=1&pn=1");
            }

            //page.AddTargetRequest("https://tieba.baidu.com/p/5634130238");
            page.AddTargetRequests(targetlinks);
            //page.AddResultItem("targetlinks", targetlinks);

            #endregion
        }
    }

    public class baidutieba_imgs
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

    public class baidutieba_posts
    {
        public int id { get; set; }
        public string guid { get; set; }
        public string spiderguid { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string landlord { get; set; }
        public int depth { get; set; }
    }

    public class baidutieba_spiders 
    {
        public int id { get; set; }
        public string guid { get; set; }
        public string identity { get; set; }
        public string starturl { get; set; }
        public DateTime createtime { get; set; }
        public string tiebaname { get; set; }
    }

}
