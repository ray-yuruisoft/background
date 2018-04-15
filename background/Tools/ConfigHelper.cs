﻿using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace background.Tools
{
    public class ConfigHelper
    {
        private static ILog log = LogManager.GetLogger(LogHelper.repository.Name, typeof(ConfigHelper));
        private static IConfigurationRoot config;
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            string basePath = Directory.GetCurrentDirectory();
            config = new ConfigurationBuilder()
                .AddInMemoryCollection()                 //将配置文件的数据加载到内存中
                                           .SetBasePath(basePath)   //指定配置文件所在的目录
                                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                                           .Build();    //编译成对象  

            Action changeCallBack = () =>
            {
                Console.WriteLine("配置文件修改");
                LoadDataManager();
            };

            ChangeToken.OnChange(() => config.GetReloadToken(), changeCallBack);

        }
        /// <summary>
        /// 配置文件修改后触发
        /// </summary>
        public static void LoadDataManager()
        {
            
        }

        /// <summary>
        /// 获取配置文件的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSettings(string key)
        {
            string value = "";
            try
            {
                value = config[key];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                log.Error("GetAppSettings:" + ex.Message);
            }
            return value;
        }
        /// <summary>
        /// 设置配置文件的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetAppSettings(string key, string value)
        {
            try
            {
                config[key] = value;
            }
            catch (Exception ex)
            {
                log.Error("SetAppSettings:" + ex.Message);
            }
        }
    }
}
