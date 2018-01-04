
namespace GMap.NET.WindowsForms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.Serialization;
    using System.Windows.Forms;
    using GMap.NET;
    using System.Diagnostics;
    using System.Windows.Forms.GMap.NET.WindowsForms;

    /// <summary>
    /// GMap.NET route
    /// </summary>
    [Serializable]
#if !PocketPC
    public class GMapRoute : MapRoute, ISerializable, IDeserializationCallback, IDisposable
#else
    public class GMapRoute : MapRoute, IDisposable
#endif
    {
        GMapOverlay overlay;
        public GMapOverlay Overlay
        {
            get
            {
                return overlay;
            }
            internal set
            {
                overlay = value;
            }
        }
        public string LayerName { get; set; }
        /// <summary>
        /// 储存线路在Linekind中的ID，用于辨识线路属于哪类线
        /// </summary>
        public int DataTypeID { get; set; }
        private bool visible = true;
        /// <summary>
        /// 储存线路在主程序数据库中的ID
        /// </summary>
        public int GID { get; set; }
        /// <summary>
        /// 储存线路在主程序数据库中的ID
        /// </summary>
        public string PropertyID { get; set; }
        public string Layer_GUID { get; set; }
        public string GUID { get; set; }
        /// <summary>
        /// is marker visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return visible;
            }
            set
            {
                if (value != visible)
                {
                    visible = value;

                    if (Overlay != null && Overlay.Control != null)
                    {
                        if (visible)
                        {
                            //注释掉的是自带方法
                            Overlay.Control.UpdateRouteLocalPosition(this);
                            //此处调用自己写的方法,并且加入自己的判断； by gujian 20170908
                            //if (this.RouteGraphicsPath[(int)Overlay.Control.Zoom].PointCount > 0)
                            //{
                            //    return;
                            //}
                            //else
                            //{
                            //    Overlay.Control.UpdateRouteLocalPosition(this, (int)Overlay.Control.Zoom);
                            //}
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverRoute)
                            {
                                Overlay.Control.IsMouseOverRoute = false;
#if !PocketPC
                                Overlay.Control.RestoreCursorOnLeave();
#endif
                            }
                        }

                        {
                            if (!Overlay.Control.HoldInvalidation)
                            {
                                Overlay.Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// can receive input
        /// </summary>
        public bool IsHitTestVisible = false;

        private bool isMouseOver = false;
        //定义字段，用于存储线路之前的颜色。
        private Color beforeColor = Color.Transparent;
        private List<RouteStyle> _routeStyle = new List<RouteStyle>();

        public List<RouteStyle> RouteStyle { get { return _routeStyle; } set { _routeStyle = value; } }

        private List<Image> _listImage = new List<Image>();
        public List<Image> ListImge { get { return _listImage; } set { _listImage = value; } }
        public Color BeforeColor
        {
            get
            {
                return beforeColor;
            }
            set
            {
                beforeColor = value;
            }
        }

        /// <summary>
        /// is mouse over
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return isMouseOver;
            }
            internal set
            {
                isMouseOver = value;
            }
        }

#if !PocketPC
        /// <summary>
        /// Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsInside(int x, int y)
        {
            if (graphicsPath != null)
            {
                return graphicsPath.IsOutlineVisible(x, y, Stroke);
            }

            return false;
        }
        //定义一个存放path的链表，此链表用于存储线路各个缩放等级的graphicsPath信息；by gujian 20170908
        List<GraphicsPath> routeGraphicsPath = new List<GraphicsPath>(19);
        /// <summary>
        /// 定义一个属性，用于获取线路的缩放等级graphicsPath信息 by gujian 20170908
        /// </summary>
        public List<GraphicsPath> RouteGraphicsPath
        {
            get { return routeGraphicsPath; }
        }
        GraphicsPath graphicsPath;
        GraphicsPath graphicsStyleUpPath;
        GraphicsPath graphicsStyleDownPath;
        internal void UpdateGraphicsPath()
        {
            if (graphicsPath == null)
            {
                graphicsPath = new GraphicsPath();
            }
            else
            {
                try
                {
                    graphicsPath.Reset();
                }
                catch
                {
                }

            }

            {
                for (int i = 0; i < LocalPoints.Count; i++)
                {

                    try
                    {
                        GPoint p2 = LocalPoints[i];

                        if (i == 0)
                        {
                            graphicsPath.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                        }
                        else
                        {
                            System.Drawing.PointF p = graphicsPath.GetLastPoint();
                            graphicsPath.AddLine(p.X, p.Y, p2.X, p2.Y);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }



                }
            }
        }
        /// <summary>
        /// 编写根据缩放等级存储 by gujian 20170908
        /// </summary>
        /// <param name="zoom"></param>
        internal void UpdateGraphicsPath(int zoom)
        {
            if (graphicsPath == null)
            {
                graphicsPath = new GraphicsPath();
            }
            else
            {
                graphicsPath.Reset();
            }

            {
                for (int i = 0; i < LocalPoints.Count; i++)
                {
                    GPoint p2 = LocalPoints[i];

                    if (i == 0)
                    {
                        graphicsPath.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                    }
                    else
                    {
                        System.Drawing.PointF p = graphicsPath.GetLastPoint();
                        graphicsPath.AddLine(p.X, p.Y, p2.X, p2.Y);
                    }
                }
                routeGraphicsPath[zoom] = graphicsPath;
            }
        }

#endif
        /// <summary>
        /// 修改原有方法，重新加载，目的为了直接获取当前缩放等级下的 graphicsPath，by gujian 20170908
        /// </summary>
        /// <param name="g"></param>
        /// <param name="zoom">当前缩放等级</param>
        public virtual void OnRender(Graphics g, int zoom)
        {
#if !PocketPC
            if (IsVisible)
            {
                graphicsPath = routeGraphicsPath[zoom];
                g.DrawPath(Stroke, graphicsPath);

            }
#else
            if (IsVisible)
            {
                Point[] pnts = new Point[LocalPoints.Count];
                for (int i = 0; i < LocalPoints.Count; i++)
                {
                    Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
                    pnts[pnts.Length - 1 - i] = p2;
                }

                if (pnts.Length > 1)
                {
                    g.DrawLines(Stroke, pnts);
                }
            }
#endif
        }
        private void updateUpPath(float ooffset)
        {
            if (graphicsStyleUpPath == null)
            {
                graphicsStyleUpPath = new GraphicsPath();
            }
            else
            {
                graphicsStyleUpPath.Reset();
            }
            {
                for (int i = 0; i < graphicsPath.PathPoints.Length - 1; i++)
                {
                    PointF p = graphicsPath.PathPoints[i];
                    PointF p2 = graphicsPath.PathPoints[i + 1];

                    graphicsStyleUpPath.AddLine(p.X, p.Y + ooffset, p2.X, p2.Y + ooffset);
                }
            }
        }
        private void updateDownPath(float ooffset)
        {
            if (graphicsStyleDownPath == null)
            {
                graphicsStyleDownPath = new GraphicsPath();
            }
            else
            {
                graphicsStyleDownPath.Reset();
            }
            {
                for (int i = 0; i < graphicsPath.PathPoints.Length - 1; i++)
                {
                    PointF p2 = graphicsPath.PathPoints[i + 1];
                    PointF p = graphicsPath.PathPoints[i];
                    graphicsStyleDownPath.AddLine(p.X, p.Y + ooffset, p2.X, p2.Y + ooffset);
                }
            }
        }
        //        public virtual void OnRender(Graphics g)
        //        {
        //#if !PocketPC
        //            if (IsVisible)
        //            {
        //                if (graphicsPath != null)
        //                {
        //                    try
        //                    {
        //                       // g.SmoothingMode = SmoothingMode.HighQuality;
        //                        if (_routeStyle.Count == 0)
        //                        {
        //                            g.DrawPath(Stroke, graphicsPath);
        //                            return;
        //                        }
        //                        foreach (RouteStyle rs in _routeStyle)
        //                        {
        //                            if (rs.Name == "UpStyle")
        //                            {
        //                                updateUpPath(rs.StyleOffset);
        //                                Pen nP = new Pen(rs.RouteColor, rs.RouteWidth);
        //                                if (rs.RouteDash == DashStyle.Custom)
        //                                {
        //                                    nP.DashPattern = rs.DashPattern;
        //                                }
        //                                else
        //                                {
        //                                    nP.DashStyle = rs.RouteDash;
        //                                }
        //                                g.DrawPath(nP, graphicsStyleUpPath);
        //                            }
        //                            else if (rs.Name == "DownStyle")
        //                            {
        //                                updateDownPath(rs.StyleOffset);
        //                                Pen nP = new Pen(rs.RouteColor, rs.RouteWidth);
        //                                if (rs.RouteDash == DashStyle.Custom)
        //                                {
        //                                    nP.DashPattern = rs.DashPattern;
        //                                }
        //                                else
        //                                {
        //                                    nP.DashStyle = rs.RouteDash;
        //                                }
        //                                g.DrawPath(nP, graphicsStyleDownPath);
        //                            }
        //                            else if (rs.Name == "MiddleStyle")
        //                            {
        //                                Pen nP = new Pen(rs.RouteColor, rs.RouteWidth);
        //                                if (rs.RouteDash == DashStyle.Custom)
        //                                {
        //                                    nP.DashPattern = rs.DashPattern;
        //                                }
        //                                else
        //                                {
        //                                    nP.DashStyle = rs.RouteDash;
        //                                }
        //                                g.DrawPath(nP, graphicsPath);
        //                            }

        //                        }
        //                    }
        //                    catch
        //                    {
        //                    }

        //                }
        //            }
        //#else
        //            if (IsVisible)
        //            {
        //                Point[] pnts = new Point[LocalPoints.Count];
        //                for (int i = 0; i < LocalPoints.Count; i++)
        //                {
        //                    Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
        //                    pnts[pnts.Length - 1 - i] = p2;
        //                }

        //                if (pnts.Length > 1)
        //                {
        //                    g.DrawLines(Stroke, pnts);
        //                }
        //            }
        //#endif
        //        }
        public virtual void OnRender(Graphics g)
        {
#if !PocketPC
            if (IsVisible)
            {
                if (graphicsPath != null)
                {
                    try
                    {
                        if (_listImage.Count == 0)
                        {
                            g.DrawPath(Stroke, graphicsPath);
                        }
                        else
                        {
                            for (int i = 0; i < graphicsPath.PathPoints.Length - 1; i++)
                            {
                                TextureBrush tb = new TextureBrush(_listImage[0],WrapMode.TileFlipXY);
                                PointF p1 = graphicsPath.PathPoints[i];
                                PointF p2 = graphicsPath.PathPoints[i + 1];
                              
                                if (p1.X == p2.X)
                                {
                                    tb.RotateTransform(90, MatrixOrder.Prepend);
                                }
                                else
                                {
                                    double angle = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X));
                                    float aa = (float)(angle * 180 / Math.PI);
                                    tb.RotateTransform(aa, MatrixOrder.Prepend);
                                }
                                Pen np = new Pen(tb, 10);
                                g.DrawLine(np, p1, p2);
                                //  g.DrawPath(np, graphicsPath);
                            }

                        }
                    }
                    catch
                    {
                    }

                }
            }
#else
                    if (IsVisible)
                    {
                        Point[] pnts = new Point[LocalPoints.Count];
                        for (int i = 0; i < LocalPoints.Count; i++)
                        {
                            Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
                            pnts[pnts.Length - 1 - i] = p2;
                        }

                        if (pnts.Length > 1)
                        {
                            g.DrawLines(Stroke, pnts);
                        }
                    }
#endif
        }
        //        public virtual void OnRender(Graphics g)
        //        {
        //#if !PocketPC
        //            if (IsVisible)
        //            {
        //                if (graphicsPath != null)
        //                {
        //                    try
        //                    {
        //                        g.DrawPath(Stroke, graphicsPath);
        //                    }
        //                    catch 
        //                    {
        //                    }

        //                }
        //            }
        //#else
        //            if (IsVisible)
        //            {
        //                Point[] pnts = new Point[LocalPoints.Count];
        //                for (int i = 0; i < LocalPoints.Count; i++)
        //                {
        //                    Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
        //                    pnts[pnts.Length - 1 - i] = p2;
        //                }

        //                if (pnts.Length > 1)
        //                {
        //                    g.DrawLines(Stroke, pnts);
        //                }
        //            }
        //#endif
        //        }

#if !PocketPC
        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));
#else
        public static readonly Pen DefaultStroke = new Pen(Color.MidnightBlue);
#endif

        /// <summary>
        /// specifies how the outline is painted
        /// </summary>
        [NonSerialized]
        public Pen Stroke = DefaultStroke;

        public readonly List<GPoint> LocalPoints = new List<GPoint>();

        static GMapRoute()
        {
#if !PocketPC
            DefaultStroke.LineJoin = LineJoin.Round;
#endif
            DefaultStroke.Width = 5;
        }

        public GMapRoute(string name)
            : base(name)
        {

        }

        public GMapRoute(IEnumerable<PointLatLng> points, string name)
            : base(points, name)
        {

        }

#if !PocketPC
        #region ISerializable Members

        // Temp store for de-serialization.
        private GPoint[] deserializedLocalPoints;

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Visible", this.IsVisible);
            info.AddValue("LocalPoints", this.LocalPoints.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GMapRoute"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapRoute(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            //this.Stroke = Extensions.GetValue<Pen>(info, "Stroke", new Pen(Color.FromArgb(144, Color.MidnightBlue)));
            this.IsVisible = Extensions.GetStruct<bool>(info, "Visible", true);
            this.deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
        }

        #endregion

        #region IDeserializationCallback Members

        /// <summary>
        /// Runs when the entire object graph has been de-serialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            // Accounts for the de-serialization being breadth first rather than depth first.
            LocalPoints.AddRange(deserializedLocalPoints);
            LocalPoints.Capacity = Points.Count;
        }

        #endregion
#endif

        #region IDisposable Members

        bool disposed = false;

        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                LocalPoints.Clear();

#if !PocketPC
                if (graphicsPath != null)
                {
                    graphicsPath.Dispose();
                    graphicsPath = null;
                }
#endif
                base.Clear();
            }
        }

        #endregion
    }

    public delegate void RouteClick(GMapRoute item, MouseEventArgs e);
    public delegate void RouteEnter(GMapRoute item);
    public delegate void RouteLeave(GMapRoute item);
}
