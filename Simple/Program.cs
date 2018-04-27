using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
    class Program
    {
        static void Main(string[] args)
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

            var a = vs.FirstOrDefault(c => c.id == 6);
            a.name = "mayuru";
            var b = vs.FirstOrDefault(c => c.id == 6);

    }
}

    public class test
    {
        public int id { get; set; }
        public string name { get; set; }
    }

}
