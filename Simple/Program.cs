using background.Caches;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SimpleCore
{
    class Program
    {
        static void Main(string[] args)
        {

            MemoryCacheService defaultCache = new MemoryCacheService(DefaultCache.memoryCache);
            var a = defaultCache.GetOrCreate("test", DateTime.Now.AddDays(1000), DateTime.Now.AddDays(1000), () =>
             {

                 ConcurrentQueue<test> vs = new ConcurrentQueue<test>();
                 vs.Enqueue(new test { id = 1, name = "wangrui" });
                 vs.Enqueue(new test { id = 2, name = "wangrui2" });
                 vs.Enqueue(new test { id = 3, name = "wangrui3" });
                 vs.Enqueue(new test { id = 4, name = "wangrui4" });
                 vs.Enqueue(new test { id = 5, name = "wangrui5" });
                 vs.Enqueue(new test { id = 6, name = "wangrui6" });
                 vs.Enqueue(new test { id = 7, name = "wangrui7" });
                 vs.Enqueue(new test { id = 8, name = "wangrui8" });
                 vs.Enqueue(new test { id = 9, name = "wangrui9" });
                 return vs;
             });

            a.FirstOrDefault(c => c.id == 6).name = "mayuru";


            var b = defaultCache.Get("test") as ConcurrentQueue<test>;

            var d = b.FirstOrDefault(c => c.id == 6).name;

        }
    }


    public class test
    {
        public int id { get; set; }
        public string name { get; set; }
    }


}
