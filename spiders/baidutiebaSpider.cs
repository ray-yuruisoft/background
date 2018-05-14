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
using System.Text;
using System.Text.RegularExpressions;

namespace spiders
{

    [TaskName("baidutiebaSpider")]
    public class baidutiebaSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {

            StartUrl(arguments);
            AddPipeline(new baidutiebaPipeline());
            AddPageProcessor(new baidutiebaPageProcessor());
            this.Site.DownloadFiles = true;
            this.SkipTargetUrlsWhenResultIsEmpty = false;

            InitDatabaseAndTables();
            RegisterEvent();

        }

        private void StartUrl(params string[] arguments)
        {

            if (arguments.Length != 0)
            {
                this.Downloader = new MyDownloader(bool.Parse(arguments[1]));
                if (arguments.Length == 2)
                {
                    Cache.Instance.Set("searchkey", arguments[0]);
                    var code = System.Web.HttpUtility.UrlEncode(arguments[0]);
                    AddStartUrl($"https://tieba.baidu.com/f?kw={code}&ie=utf-8&pn=0");
                }
                else
                {
                    AddStartUrl("https://" + arguments[2]);
                }
            }
            else
            {
                AddStartUrl("https://tieba.baidu.com/f?kw=%E5%A6%96%E7%B2%BE%E7%9A%84%E5%B0%BE%E5%B7%B4&ie=utf-8&pn=0");
            }
        }
        private void InitDatabaseAndTables()
        {
            Dictionary<string, string> tables = new Dictionary<string, string>();
            tables.Add("baidutieba_imgs", "`id` int(11) NOT NULL AUTO_INCREMENT,`spiderguid` varchar(32) NOT NULL,`postguid` varchar(32) NOT NULL,`url` varchar(255) NOT NULL,`filename` varchar(255) NOT NULL,`originalpath` varchar(255) NOT NULL,`floorlevel` int(11) NOT NULL,`floorindex` int(11) NOT NULL,`posttime` datetime NOT NULL,PRIMARY KEY (`id`)");
            tables.Add("baidutieba_posts", "`id` int(11) NOT NULL AUTO_INCREMENT,`guid` varchar(32) NOT NULL,`spiderguid` varchar(32) NOT NULL,`url` varchar(255) NOT NULL,`title` varchar(100) NOT NULL,`landlord` varchar(100) NOT NULL,`depth` int(5) NOT NULL,PRIMARY KEY (`id`)");
            tables.Add("baidutieba_spiders", "`id` int(11) NOT NULL AUTO_INCREMENT,`guid` varchar(32) DEFAULT NULL,`starturl` varchar(255) DEFAULT NULL,`createtime` datetime DEFAULT NULL,`tiebaname` varchar(100) DEFAULT NULL,PRIMARY KEY (`id`)");
            InitDatabaseAndTables("spider", tables);
        }
        private void RegisterEvent()
        {

            this.OnCompleted += (Spider spider) =>
            {
                Logger.Log(spider.Identity, "index.html generate start.", Level.Info);
                GenerateHtml();
                Logger.Log(spider.Identity, "index.html generate complete.", Level.Info);
            };

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
        private void GenerateHtml()
        {

            var datas = baidutiebaPipeline.con.Query<baidutiebaPipeline.baidutieba_imgs>($"select * from baidutieba_imgs where spiderguid = '{Identity}'");
            if (datas.Count() == 0) return;

            List<string> content = new List<string>();
            foreach (var item in datas.GroupBy(c => c.postguid))
            {
                content.Add($"<div>帖子开始</div>");
                foreach (var bottom in item.OrderBy(c => c.floorlevel).ThenBy(c => c.floorindex))
                {
                    content.Add($"<img src='./{Identity}/{bottom.filename}'>");
                }
                content.Add($"<div>帖子结束</div>");
            }

            baidutiebaPipeline.con.Close();
            baidutiebaPipeline.con.Dispose();

            var imghtml = String.Join("\r\n", content);
            var indexTemppath = Directory.GetCurrentDirectory() + $"{Env.PathSeperator}Template.html";
            string indexTemp = File.ReadAllText(indexTemppath, Encoding.UTF8);
            var index = indexTemp.Replace("$", imghtml);
            File.WriteAllText(Directory.GetCurrentDirectory() + $"{Env.PathSeperator}download{Env.PathSeperator}{Cache.Instance.Get("searchkey")}.html", index, Encoding.UTF8);
        }

    }

    public class baidutiebaPipeline : BasePipeline
    {

        public static DbConnection con = Env.DataConnectionStringSettings.CreateDbConnection();
        public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
        {
            foreach (var a in resultItems)
            {
                if (a.Results.ContainsKey("spiderInfo"))
                {
                    var spiderInfo = a.Results["spiderInfo"] as baidutieba_spiders;
                    spiderInfo.guid = spider.Identity;
                    spiderInfo.starturl = spider.Site.StartRequests.First().Url;
                    con.Execute("INSERT INTO baidutieba_spiders (guid,starturl,createtime,tiebaname) VALUES (@guid,@starturl,@createtime,@tiebaname)", spiderInfo);
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
                            intervalPath = $"{Env.PathSeperator}{new Uri(c.url).LocalPath.Replace("//", "").Replace("/", "")}";
                        }
                        else
                        {
                            intervalPath = new Uri(c.url).LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
                        }
                        var t = intervalPath.Split(Env.PathSeperator[0]);
                        c.filename = t[t.Length - 1];
                        c.originalpath = $"{Path.Combine(Env.BaseDirectory, "download")}{Env.PathSeperator}{spider.Identity}{intervalPath}";
                        c.spiderguid = spider.Identity;
                        con.Execute("INSERT INTO baidutieba_imgs (spiderguid,postguid,url,filename,originalpath,floorlevel,floorindex,posttime) VALUES (@spiderguid,@postguid,@url,@filename,@originalpath,@floorlevel,@floorindex,@posttime)", c);
                    }
                }
                if (a.Results.ContainsKey("post"))
                {
                    var post = a.Results["post"] as baidutieba_posts;
                    post.landlord = Regex.Replace(post.landlord, @"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD", "");
                    post.title = Regex.Replace(post.title, @"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD", "");
                    post.spiderguid = spider.Identity;
                    con.Execute("INSERT INTO baidutieba_posts (guid,spiderguid,url,title,landlord,depth) VALUES (@guid,@spiderguid,@url,@title,@landlord,@depth)", post);

                    var key = post.url.KeepExceptLastNumbers().Replace("?see_lz=1&pn=", "");
                    Tuple<string, int> tuple = Cache.Instance.Get(key);
                    Cache.Instance.Remove(key);
                    if (tuple.Item2 > 1)
                    {
                        Cache.Instance.Set(key, new Tuple<string, int>(tuple.Item1, tuple.Item2));
                    }
                }
            }
        }

        #region Model

        public class baidutieba_imgs
        {
            public int id { get; set; }
            public string spiderguid { get; set; }
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
            public string starturl { get; set; }
            public DateTime createtime { get; set; }
            public string tiebaname { get; set; }
        }

        #endregion

    }

    public class baidutiebaPageProcessor : BasePageProcessor
    {
        private static Tools.Logger Logger = new Tools.Logger("baidutiebaPageProcessor");
        protected override void Handle(Page page)
        {

            //1 首页 列表第1项 https://tieba.baidu.com/f?kw=%E6%B5%B7%E8%B4%BC%E7%8E%8B&ie=utf-8&pn=0
            //2 首页 列表第2项 https://tieba.baidu.com/f?kw=%E6%B5%B7%E8%B4%BC%E7%8E%8B&ie=utf-8&pn=50
            //3 帖子 列表第1项 https://tieba.baidu.com/p/5686265280?see_lz=1&pn=1
            //4 帖子 列表第2项 https://tieba.baidu.com/p/5686265280?see_lz=1&pn=2

            var type = GetHandleType(page);
            switch (type)
            {
                case HandleType.FirstPageFirstlist:
                    {
                        try
                        {
                            if (!FirstPageFirstlistHandler(page))
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"FirstPageFirstlistHandler Error. this message is '{e.Message}'");
                        }
                    }
                    break;
                case HandleType.FirstPageSecondlist:
                    {
                        try
                        {
                            if (!FirstPageSecondlistHandler(page))
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"FirstPageSecondlistHandler Error. this message is '{e.Message}'");
                        }
                    }
                    break;
                case HandleType.PostPageFirstlist:
                    {
                        try
                        {
                            if (!PostPageFirstlistHandler(page))
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"PostPageFirstlistHandler Error. this message is '{e.Message}'");
                        }
                    }
                    break;
                case HandleType.PostPageSecondlist:
                    {
                        try
                        {
                            if (!PostPageSecondlistHandler(page))
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"PostPageSecondlistHandler Error. this message is '{e.Message}'");
                        }
                    }
                    break;
                case HandleType.Wrong:
                    {
                        Logger.Error($"Wrong HandleType,page url is '{page.Url}'");
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }
        private HandleType GetHandleType(Page page)
        {

            HandleType key = HandleType.Wrong;
            var url = page.Url;
            //概率多，放前面
            if (url.IndexOf("://tieba.baidu.com/p/") != -1)
            {
                if (url.IndexOf("pn=1") != -1)
                {
                    key = HandleType.PostPageFirstlist;
                }
                else
                {
                    key = HandleType.PostPageSecondlist;
                }
            }
            else if (url.IndexOf("://tieba.baidu.com/f?kw=") != -1)
            {
                //没有pn=0,则为0
                var reg = new Regex(@"pn\=[1-9]");
                if (reg.IsMatch(url))
                {
                    key = HandleType.FirstPageSecondlist;
                }
                else
                {
                    key = HandleType.FirstPageFirstlist;
                }
            }
            return key;

        }
        public enum HandleType
        {

            //1 首页 列表第1项 https://tieba.baidu.com/f?kw=%E6%B5%B7%E8%B4%BC%E7%8E%8B&ie=utf-8&pn=0
            //2 首页 列表第2项 https://tieba.baidu.com/f?kw=%E6%B5%B7%E8%B4%BC%E7%8E%8B&ie=utf-8&pn=50
            //3 帖子 列表第1项 https://tieba.baidu.com/p/5686265280?see_lz=1&pn=1
            //4 帖子 列表第2项 https://tieba.baidu.com/p/5686265280?see_lz=1&pn=2

            Wrong = 0,
            FirstPageFirstlist = 1,
            FirstPageSecondlist = 2,
            PostPageFirstlist = 3,
            PostPageSecondlist = 4,

        };

        private bool FirstPageFirstlistHandler(Page page)
        {

            page.Content = page
                .Content
                .Replace("<!--", "")
                .Replace("-->", "")
                ;

            #region parse XPAH

            string tiebaname = "";
            try
            {
                tiebaname = page
               .Selectable
               .Select
               (Selectors.XPath("//*[@class='card_title']/a/text()"))
               .GetValue()
               .Replace("\n", "")
               .Replace(" ", "")
               ;
            }
            catch (Exception e)
            {
                Logger.Error($"in FirstPageFirstlistHandler,tiebaname can not be parsed by XPAH. this message is {e.Message}");
                return false;
            }
            if (tiebaname.IsNullOrEmpty()) return false;

            //https://tieba.baidu.com/f?kw=%E7%AE%80%E9%98%B3&ie=utf-8&pn=117050
            //^http[s]?\:\/\/tieba\.baidu\.com\/f\?kw\=.*?pn=\d+?$
            string endurl = "";//只有1页时，可以为空
            string endurlReg = @"^http[s]?\:\/\/tieba\.baidu\.com\/f\?kw\=.*?pn=\d+?$";
            try
            {
                endurl = page
               .Selectable
               .Select
               (Selectors.XPath(@"//*[@id='frs_list_pager']/a[last()]"))
               .Links()
               .GetValue()
               ;
            }
            catch (Exception e)
            {
                Logger.Error($"in FirstPageFirstlistHandler,endurl can not be parsed by XPAH. this message is {e.Message}");
                return false;
            }

            #endregion

            //可能只有1页
            if (endurl != null)
            {
                if (!new Regex(endurlReg).IsMatch(endurl))
                {
                    Logger.Error($"in FirstPageFirstlistHandler,endurl can not be matched by Regex. this message is {endurl}");
                    return false;
                }

                var num = endurl.KeepLastNumbers().ToInt32();
                var pagenum = num / 50 + 1;
                List<string> listurls = new List<string>();
                var urlprefix = endurl.KeepExceptLastNumbers();
                for (var i = 1; i <= pagenum; i++)
                {
                    listurls.Add($"{urlprefix}{i * 50}");
                }
                page.AddTargetRequests(listurls);
            }

            #region spiderInfo

            baidutiebaPipeline.baidutieba_spiders spider = new baidutiebaPipeline.baidutieba_spiders
            {
                createtime = DateTime.Now,
                guid = Guid.NewGuid().ToString("N"),
                tiebaname = tiebaname
            };
            page.AddResultItem("spiderInfo", spider);

            #endregion

            return AddPostPage(page);

        }
        private bool FirstPageSecondlistHandler(Page page)
        {
            page.Content = page
                .Content
                .Replace("<!--", "")
                .Replace("-->", "")
                ;

            return AddPostPage(page);
        }
        private bool PostPageFirstlistHandler(Page page)
        {

            #region parse XPAH

            int postpagenum = 0;
            try
            {
                postpagenum = page
               .Selectable
               .Select
               (Selectors.XPath(@"//*[@id='thread_theme_5']/div[1]/ul/li[2]/span[2]/text()"))
               .GetValue()
               .ToInt32()
               ;
            }
            catch (Exception e)
            {
                Logger.Error($"PostPageFirstlistHandler Error. this message is {e.Message}");
                return false;
            }
            if (postpagenum < 1)
            {
                Logger.Error("PostPageFirstlistHandler Error. this message is 'postpagenum is smaller than 1'");
                return false;
            }

            string landlord = "";
            try
            {
                landlord = page
               .Selectable
               .Select
               (Selectors.XPath(@"//div[@class='d_author']//li[@class='d_name']/a/text()"))
               .GetValue()
               ;
            }
            catch (Exception e)
            {
                Logger.Error($"PostPageFirstlistHandler Error. this message is {e.Message}");
                return false;
            }
            if (landlord.IsNullOrEmpty())
            {
                Logger.Error("PostPageFirstlistHandler Error. this message is 'landlord IsNullOrEmpty'");
                return false;
            }

            string title = "";
            try
            {
                title = page
                .Selectable
                .Select
                (Selectors.XPath(@"//*[contains(@class,'core_title_txt')]/text()"))
                .GetValue()
                ;
            }
            catch (Exception e)
            {
                Logger.Error($"PostPageFirstlistHandler Error. this message is {e.Message}");
                return false;
            }
            if (title.IsNullOrEmpty())
            {
                Logger.Error("PostPageFirstlistHandler Error. this message is 'title IsNullOrEmpty'");
                return false;
            }

            #endregion

            //*[contains(@class,'d_post_content_main')]
            var posturlprefix = page.Url.KeepExceptLastNumbers().Replace("?see_lz=1&pn=", "");

            try
            {
                Cache.Instance.Set(posturlprefix, new Tuple<string, int>(Guid.NewGuid().ToString("N"), postpagenum));
            }
            catch (Exception e)//有可能重复键
            {
                Logger.Error($"Cache Instance Set posturlprefix Error.this message is {e.Message}");
                return false;
            }

            var postpageurls = new List<string>();
            for (var i = 2; i <= postpagenum; i++)
            {
                postpageurls.Add($"{posturlprefix}?see_lz=1&pn={i}");
            }
            page.AddTargetRequests(postpageurls);


            //增加缓存，帖子列表其他页 从该处获取 guid
            Tuple<string, int> tuple = Cache.Instance.Get(posturlprefix);
            if (tuple == null)
            {
                Logger.Error("PostPageFirstlistHandler Error this message is 'Cache.Instance.Get(posturlprefix) is null'");
                return false;
            }
            baidutiebaPipeline.baidutieba_posts baidutieba_Posts = new baidutiebaPipeline.baidutieba_posts
            {
                guid = tuple.Item1,
                landlord = landlord,
                url = page.Url,
                title = title,
                depth = 1
            };
            page.AddResultItem("post", baidutieba_Posts);
            return AddDownloadLink(page);
        }
        private bool PostPageSecondlistHandler(Page page)
        {
            return AddDownloadLink(page);
        }

        /// <summary>
        /// 添加帖子
        /// </summary>
        private bool AddPostPage(Page page)
        {
            IEnumerable<string> elements = null;
            try
            {
                elements = page
                .Selectable
                .SelectList
                (Selectors.XPath(@"//code[@id='pagelet_html_frs-list/pagelet/thread_list']//li[@class=' j_thread_list clearfix']//div[contains(@class,'threadlist_title')]"))
                .Links()
                .GetValues()
                ;
            }
            catch (Exception e)
            {
                Logger.Error($"AddPostPage Error. this message is {e.Message}");
                return false;
            }
            if (elements == null || elements.Count() == 0)//这里会出现验证码
            {
                Logger.Warn($"AddPostPage Warn. this message is 'elements is null or elements`s count equal 0' and 'page TargetUrl is {page.TargetUrl}'");
                return false;
            }
            //https://tieba.baidu.com/p/5698708065
            //^http[s]?\:\/\/tieba\.baidu\.com\/p.*?\d+?$
            var elementsReg = @"^http[s]?\:\/\/tieba\.baidu\.com\/p.*?\d+?$";
            List<string> targetlinks = new List<string>();
            foreach (var item in elements)
            {
                if (new Regex(elementsReg).IsMatch(item))
                {
                    targetlinks.Add(item + "?see_lz=1&pn=1");//注意  添加 ?see_lz=1 是只看楼主
                }
            }
            if (targetlinks.Count() == 0)
            {
                Logger.Warn($"AddPostPage Warn. this message is 'targetlinks has no matched url'");
                return false;
            }
            page.AddTargetRequests(targetlinks);
            return true;
        }
        /// <summary>
        /// 添加下载链接
        /// </summary>
        /// <param name="page"></param>
        private bool AddDownloadLink(Page page)
        {

            List<baidutiebaPipeline.baidutieba_imgs> models = new List<baidutiebaPipeline.baidutieba_imgs>();
            int floorsCount = 0;
            try
            {
                floorsCount = page.Selectable.SelectList(Selectors.XPath(@"//*[contains(@class,'d_post_content_main')]")).Nodes().Count();
            }
            catch (Exception e)
            {
                Logger.Error($"AddDownloadLink Error. this message is {e.Message}");
            }
            if (floorsCount < 1)
            {
                Logger.Error($"AddDownloadLink Error. this message is 'floorsCount is smaller than 1'");
                return false;
            }
            //所有楼层的div集合

            for (var i = 1; i <= floorsCount; i++)
            {
                List<string> imgs = null;
                try
                {
                    imgs = page
                       .Selectable
                       .SelectList
                       (Selectors.XPath
                        ($"(//*[contains(@class,'d_post_content_main')])[{i}]//img/@src")
                       )
                       .GetValues()
                       .ToList()
                       ;
                }
                catch (Exception e)
                {
                    Logger.Error($"AddDownloadLink Error.this message is {e.Message}");
                }
                if (imgs == null || imgs.Count() == 0)//该页面无采集链接
                {
                    break;
                }

                //yyyy-MM-dd HH:mm
                var reg = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}");
                string posttimedata = "";
                try
                {
                    posttimedata = page
                   .Selectable
                   .Select
                   (
                       Selectors.XPath
                       ($"(//div[@class = 'p_postlist']/div)[{i}]")
                   )
                   .GetValue()
                   ;
                }
                catch (Exception e)
                {
                    Logger.Error($"AddDownloadLink Error. this message is {e.Message}");
                    return false;
                }

                var posttime = reg.Match(posttimedata).ToString();
                for (var k = 0; k < imgs.Count(); k++)
                {
                    var temp = imgs[k].Split('?')[0].Split('.');
                    var imgType = temp[temp.Length - 1];
                    if (imgType == "png" || imgType == "gif") break;
                    baidutiebaPipeline.baidutieba_imgs model = new baidutiebaPipeline.baidutieba_imgs();
                    model.floorindex = k + 1;
                    model.floorlevel = i;
                    model.posttime = posttime.IsDateTime("yyyy-MM-dd HH:mm") ? DateTime.Parse(posttime) : DateTime.MinValue;
                    model.url = imgs[k];
                    model.postguid = Cache.Instance.Get(page.Url.Split('?')[0]).Item1;
                    models.Add(model);
                }
            }

            if (models.Count() == 0)
            {
                page.Skip = true;
                return false;
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

            return true;

        }
    }

}
