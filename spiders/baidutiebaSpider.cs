using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace spiders
{

    [TaskName("baidutiebaSpider")]
    public class baidutiebaSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {
            Identity = Identity ?? "baidu tieba";
            AddStartUrl("https://tieba.baidu.com/f?kw=%E5%A6%96%E7%B2%BE%E7%9A%84%E5%B0%BE%E5%B7%B4&ie=utf-8&pn=0");
            AddPipeline(new baidutiebaPipeline());
            AddPageProcessor(new baidutiebaPageProcessor());
            this.Site.DownloadFiles = true;
        }
    }

    public class baidutiebaPipeline : BasePipeline
    {
        public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
        {

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
                List<string> imgs = new List<string>();
                foreach (var item in elements)
                {
                    imgs.Add(item.GetValue());
                }

                page.AddTargetRequests(imgs);
                page.AddResultItem("img", imgs);
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

            page.AddTargetRequest("https://tieba.baidu.com/p/5634130238");
            //page.AddTargetRequests(targetlinks);
            page.AddResultItem("targetlinks", targetlinks);

            #endregion
        }
    }


}
