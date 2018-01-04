using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace System.Windows.Forms.GMap.NET.WindowsForms
{
    public class RouteStyle
    {
        public RouteStyle(string n, Color c,int w, DashStyle d, float f,float[] ff)
        {
            Name = n;
            RouteColor = c;
            RouteWidth = w;
            RouteDash = d;
            StyleOffset = f;
            DashPattern = ff;
        }
       public RouteStyle()
        {

        }
        public string Name { get; set; }
        public float[] DashPattern { get; set; }
        /// <summary>
        /// 线路得pen
        /// </summary>
        public Color RouteColor { get; set; }
        public int RouteWidth { get; set; }
        /// <summary>
        /// 线路得样式
        /// </summary>
        public DashStyle RouteDash { get; set; }
        /// <summary>
        /// 当定义多个线路样式时，鼠标上下得便宜量；基本是线路宽度得一半，也可以自定义
        /// </summary>
        public float StyleOffset { get; set; }

    }
}
