using background.Caches;
using EasyCaching.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace background.InversionOfControl
{
    public class ProgramContainer
    {

        public static void Init()
        {
            IServiceCollection services = new ServiceCollection();

            //注入
            services
                .AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions() { }))
                .AddTransient<ICacheService, MemoryCacheService>()
                .AddDefaultInMemoryCache()
                ;

            //构建容器
            Program.serviceProvider = services.BuildServiceProvider();
        }

    }
}
