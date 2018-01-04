using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class Route
    {
        public Route (int start, int end)
        {
            this.start = start;
            this.end = end;
        }
        public int start { get; set; }
        public int end { get; set; }

        public void Test()
        {

            Route r1 = new Route(43, 11);
            Route r2 = new Route(1, 29);
            Route r3 = new Route(12, 31);
            Route r4 = new Route(88, 25);
            Route r5 = new Route(29, 12);
            Route r6 = new Route(16, 10);
            Route r7 = new Route(31, 33);
            Route r8 = new Route(25, 43);
            Route r9 = new Route(10, 88);
            Route r10 = new Route(33, 16);

            List<Route> routeList = new List<Route>();
            routeList.Add(r1);
            routeList.Add(r2);
            routeList.Add(r3);
            routeList.Add(r4);
            routeList.Add(r5);
            routeList.Add(r6);
            routeList.Add(r7);
            routeList.Add(r8);
            routeList.Add(r9);
            routeList.Add(r10);

            for (int i = 0; i < routeList.Count; i++)
            {
                for (int j = i + 1; j < routeList.Count - i; j++)
                {
                    if (routeList[i].start == routeList[j].end)
                    {
                        routeList.Insert(i, routeList[j]);
                        routeList.RemoveAt(j + 1);
                        i--;
                        break;
                    }
                }
            }

            Console.Out.WriteLine(routeList.ToString());



        }
    }
}
