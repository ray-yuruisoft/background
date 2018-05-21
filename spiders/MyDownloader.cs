using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using System.Net;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core;
using System.Threading.Tasks;

namespace spiders
{
    /// <summary>
    /// Downloader using <see cref="HttpClient"/>
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 纯HTTP下载器
    /// </summary>
    public class MyDownloader : HttpClientDownloader
    {
        private readonly string _downloadFolder;
        public bool _customintervalPath;
        public MyDownloader(int timeout = 8000, bool customintervalPath = false) : base(timeout)
        {
            _downloadFolder = Path.Combine(Env.BaseDirectory, "download");
            _customintervalPath = customintervalPath;
        }
        public override Page SaveFile(Request request, HttpResponseMessage response, ISpider spider)
        {

            string intervalPath = null;
            if (_customintervalPath)
            {
                intervalPath = $"{Env.PathSeperator}{new Uri(request.Url).LocalPath.Replace("//", "").Replace("/", "")}";
            }
            else
            {
                intervalPath = new Uri(request.Url).LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
            }

            string filePath = $"{_downloadFolder}{Env.PathSeperator}{spider.Identity}{intervalPath}";
            if (!File.Exists(filePath))
            {
                try
                {
                    string folder = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                    }

                    File.WriteAllBytes(filePath, response.Content.ReadAsByteArrayAsync().Result);
                }
                catch (Exception e)
                {
                    Logger.Log(spider.Identity, "Storage file failed.", Level.Error, e);
                }
            }
            Logger.Log(spider.Identity, $"Storage file: {request.Url} success.", Level.Info);
            return new Page(request) { Skip = true };
        }
    }
}
