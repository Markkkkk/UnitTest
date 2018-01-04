
namespace GMap.NET.WindowsForms
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Windows.Forms;
    using GMap.NET.ObjectModel;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// GMap.NET overlay
    /// </summary>
    [Serializable]
#if !PocketPC
    public class GMapOverlay : ISerializable, IDeserializationCallback, IDisposable
#else
   public class GMapOverlay: IDisposable
#endif
    {
        bool isVisibile = true;

        /// <summary>
        /// is overlay visible
        /// </summary>
        public bool IsVisibile
        {
            get
            {
                return isVisibile;
            }
            set
            {
                if (value != isVisibile)
                {
                    isVisibile = value;

                    if (Control != null)
                    {
                        if (isVisibile)
                        {
                            Control.HoldInvalidation = true;
                            {
                                ForceUpdate();
                            }
                            Control.Refresh();
                        }
                        else
                        {
                            if (Control.IsMouseOverMarker)
                            {
                                Control.IsMouseOverMarker = false;
                            }

                            if (Control.IsMouseOverPolygon)
                            {
                                Control.IsMouseOverPolygon = false;
                            }

                            if (Control.IsMouseOverRoute)
                            {
                                Control.IsMouseOverRoute = false;
                            }
#if !PocketPC
                            Control.RestoreCursorOnLeave();
#endif

                            if (!Control.HoldInvalidation)
                            {
                                Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// overlay Id
        /// </summary>
        public string Id;


        //public Mutex mutex;
        /// <summary>
        /// list of markers, should be thread safe
        /// </summary>
        public readonly ObservableCollectionThreadSafe<GMapMarker> Markers = new ObservableCollectionThreadSafe<GMapMarker>();

        /// <summary>
        /// list of routes, should be thread safe
        /// </summary>
        public readonly ObservableCollectionThreadSafe<GMapRoute> Routes = new ObservableCollectionThreadSafe<GMapRoute>();

        /// <summary>
        /// list of polygons, should be thread safe
        /// </summary>
        public readonly ObservableCollectionThreadSafe<GMapPolygon> Polygons = new ObservableCollectionThreadSafe<GMapPolygon>();

        GMapControl control;
        public GMapControl Control
        {
            get
            {
                return control;
            }
            internal set
            {
                control = value;
            }
        }

        public GMapOverlay()
        {
            CreateEvents();
        }

        public GMapOverlay(string id)
        {
            Id = id;
            CreateEvents();
            //  mutex = new Mutex();
        }
        public int DataTypeID
        {
            get; set;
        }
        public FeatureType FeatureType
        {
            get; set;
        }

        void CreateEvents()
        {
            Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
            Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
            Polygons.CollectionChanged += new NotifyCollectionChangedEventHandler(Polygons_CollectionChanged);
        }

        void ClearEvents()
        {
            Markers.CollectionChanged -= new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
            Routes.CollectionChanged -= new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
            Polygons.CollectionChanged -= new NotifyCollectionChangedEventHandler(Polygons_CollectionChanged);
        }

        public void Clear()
        {
            Markers.Clear();
            Routes.Clear();
            Polygons.Clear();
        }

        void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (GMapPolygon obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = this;
                        if (Control != null)
                        {
                            Control.UpdatePolygonLocalPosition(obj);
                        }
                    }
                }
            }

            if (Control != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (Control.IsMouseOverPolygon)
                    {
                        Control.IsMouseOverPolygon = false;
#if !PocketPC
                        Control.RestoreCursorOnLeave();
#endif
                    }
                }

                if (!Control.HoldInvalidation)
                {
                    Control.Invalidate();
                }
            }
        }

        void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (GMapRoute obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = this;
                        if (Control != null)
                        {
                            //注释掉的是自带方法
                            Control.UpdateRouteLocalPosition(obj);
                            //此处调用自己写的方法,并且加入自己的判断； by gujian 20170908
                            //if (obj.RouteGraphicsPath[(int)Control.Zoom].PointCount > 0)
                            //{
                            //    return;
                            //}
                            //else
                            //{
                            //    Control.UpdateRouteLocalPosition(obj, (int)Control.Zoom);
                            //}
                        }
                    }
                }
            }

            if (Control != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (Control.IsMouseOverRoute)
                    {
                        Control.IsMouseOverRoute = false;
#if !PocketPC
                        Control.RestoreCursorOnLeave();
#endif
                    }
                }

                if (!Control.HoldInvalidation)
                {
                    Control.Invalidate();
                }
            }
        }

        void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (GMapMarker obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = this;
                        if (Control != null)
                        {
                            Control.UpdateMarkerLocalPosition(obj);
                        }
                    }
                }
            }

            if (Control != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (Control.IsMouseOverMarker)
                    {
                        Control.IsMouseOverMarker = false;
#if !PocketPC
                        Control.RestoreCursorOnLeave();
#endif
                    }
                }

                if (!Control.HoldInvalidation)
                {
                    Control.Invalidate();
                }
            }
        }

        /// <summary>
        /// updates local positions of objects
        /// </summary>
        internal void ForceUpdate()
        {
            if (Control != null)
            {
                //foreach (GMapMarker obj in Markers)
                //{
                //    if (obj.IsVisible)
                //    {
                //        Control.UpdateMarkerLocalPosition(obj);
                //    }
                //}
                for (int i = 0; i < Markers.Count; i++)
                {

                    GMapMarker obj = Markers[i];
                    if (obj.IsVisible)
                    {
                        Control.UpdateMarkerLocalPosition(obj);
                    }
                }
                foreach (GMapPolygon obj in Polygons)
                {
                    if (obj.IsVisible)
                    {
                        Control.UpdatePolygonLocalPosition(obj);
                    }
                }
                DateTime start = DateTime.Now;
                //foreach (GMapRoute obj in Routes)
                //{
                //    if (obj.IsVisible)
                //    {
                //        Control.UpdateRouteLocalPosition(obj);                       
                //    }                  
                //}
                for (int i = 0; i < Routes.Count; i++)
                {

                    GMapRoute obj = Routes[i];
                    if (obj.IsVisible)
                    {
                        Control.UpdateRouteLocalPosition(obj);
                    }
                }
                DateTime end = DateTime.Now;
                int delta = (int)(end - start).TotalMilliseconds;
                Debug.WriteLine("求本地坐标耗时：" + delta.ToString());
            }
        }
        public virtual void LableOnRender(Graphics g)
        {
            if (Control != null)
            {
                for (int i = 0; i < Markers.Count; i++)
                {
                    //if (Markers[i] == null || Markers[i].ToolTip == null)
                    //{
                    //    return;
                    //}
                    if (Markers[i].ToolTip != null && Markers[i].IsVisible)
                    {
                        if (!string.IsNullOrEmpty(Markers[i].ToolTipText) && (Markers[i].ToolTipMode == MarkerTooltipMode.Always || (Markers[i].ToolTipMode == MarkerTooltipMode.OnMouseOver && Markers[i].IsMouseOver)))
                        {
                            Markers[i].ToolTip.OnRender(g);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// renders objects/routes/polygons
        /// </summary>
        /// <param name="g"></param>
        public virtual void OnRender(Graphics g)
        {
            if (Control != null)
            {
                if (Control.RoutesEnabled)
                {
                    DateTime start = DateTime.Now;
                    //foreach (GMapRoute r in Routes)
                    //{
                    //    if (r.IsVisible)
                    //    {
                    //        //注释掉的是自带方法
                    //         r.OnRender(g);
                    //        //此处调用自己写的方法,并且加入自己的判断； by gujian 20170908
                    //        //if (r.RouteGraphicsPath[(int)Control.Zoom].PointCount > 0)
                    //        //{
                    //        //    r.OnRender(g, (int)Control.Zoom);
                    //        //}
                    //        //else
                    //        //{
                    //        //    r.OnRender(g);
                    //        //}
                    //    }
                    //}

                    for (int i = 0; i < Routes.Count; i++)
                    {
                        //if (Routes[i] == null)
                        //{
                        //   continue;
                        //}
                        if (Routes[i].IsVisible)
                        {
                            Routes[i].OnRender(g);
                        }
                    }

                    //  mutex.ReleaseMutex();

                    DateTime end = DateTime.Now;
                    int delta = (int)(end - start).TotalMilliseconds;
                    // Debug.WriteLine("绘制线耗时：" + delta.ToString());
                }

                if (Control.PolygonsEnabled)
                {
                    foreach (GMapPolygon r in Polygons)
                    {
                        if (r.IsVisible)
                        {
                            r.OnRender(g);
                        }
                    }
                }

                if (Control.MarkersEnabled)
                {
                    // markers
                    //foreach (GMapMarker m in Markers)
                    //{
                    //    //if(m.IsVisible && (m.DisableRegionCheck || Control.Core.currentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y)))
                    //    if (m.IsVisible || m.DisableRegionCheck)
                    //    {
                    //        m.OnRender(g);
                    //    }
                    //}

                    for (int i = 0; i < Markers.Count; i++)
                    {
                        //if (Markers[i] == null)
                        //{
                        //    return;
                        //}
                        if (Markers[i].IsVisible || Markers[i].DisableRegionCheck)
                        {
                            Markers[i].OnRender(g);
                        }
                    }

                    // tooltips above
                    //foreach (GMapMarker m in Markers)
                    //{
                    //    //if(m.ToolTip != null && m.IsVisible && Control.Core.currentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y))
                    //    if (m.ToolTip != null && m.IsVisible)
                    //    {
                    //        if (!string.IsNullOrEmpty(m.ToolTipText) && (m.ToolTipMode == MarkerTooltipMode.Always || (m.ToolTipMode == MarkerTooltipMode.OnMouseOver && m.IsMouseOver)))
                    //        {
                    //            m.ToolTip.OnRender(g);
                    //        }
                    //    }
                    //}

                    //for (int i = 0; i < Markers.Count; i++)
                    //{
                    //    if (Markers[i].ToolTip != null && Markers[i].IsVisible)
                    //    {
                    //        if (!string.IsNullOrEmpty(Markers[i].ToolTipText) && (Markers[i].ToolTipMode == MarkerTooltipMode.Always || (Markers[i].ToolTipMode == MarkerTooltipMode.OnMouseOver && Markers[i].IsMouseOver)))
                    //        {
                    //            Markers[i].ToolTip.OnRender(g);
                    //        }
                    //    }
                    //}
                }
            }
        }

#if !PocketPC
        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", this.Id);
            info.AddValue("IsVisible", this.IsVisibile);

            GMapMarker[] markerArray = new GMapMarker[this.Markers.Count];
            this.Markers.CopyTo(markerArray, 0);
            info.AddValue("Markers", markerArray);

            GMapRoute[] routeArray = new GMapRoute[this.Routes.Count];
            this.Routes.CopyTo(routeArray, 0);
            info.AddValue("Routes", routeArray);

            GMapPolygon[] polygonArray = new GMapPolygon[this.Polygons.Count];
            this.Polygons.CopyTo(polygonArray, 0);
            info.AddValue("Polygons", polygonArray);
        }

        private GMapMarker[] deserializedMarkerArray;
        private GMapRoute[] deserializedRouteArray;
        private GMapPolygon[] deserializedPolygonArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="GMapOverlay"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapOverlay(SerializationInfo info, StreamingContext context)
        {
            this.Id = info.GetString("Id");
            this.IsVisibile = info.GetBoolean("IsVisible");

            this.deserializedMarkerArray = Extensions.GetValue<GMapMarker[]>(info, "Markers", new GMapMarker[0]);
            this.deserializedRouteArray = Extensions.GetValue<GMapRoute[]>(info, "Routes", new GMapRoute[0]);
            this.deserializedPolygonArray = Extensions.GetValue<GMapPolygon[]>(info, "Polygons", new GMapPolygon[0]);

            CreateEvents();
        }

        #endregion

        #region IDeserializationCallback Members

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            // Populate Markers
            foreach (GMapMarker marker in deserializedMarkerArray)
            {
                marker.Overlay = this;
                this.Markers.Add(marker);
            }

            // Populate Routes
            foreach (GMapRoute route in deserializedRouteArray)
            {
                route.Overlay = this;
                this.Routes.Add(route);
            }

            // Populate Polygons
            foreach (GMapPolygon polygon in deserializedPolygonArray)
            {
                polygon.Overlay = this;
                this.Polygons.Add(polygon);
            }
        }

        #endregion
#endif

        #region IDisposable Members

        bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                ClearEvents();

                foreach (var m in Markers)
                {
                    m.Dispose();
                }

                foreach (var r in Routes)
                {
                    r.Dispose();
                }

                foreach (var p in Polygons)
                {
                    p.Dispose();
                }

                Clear();
            }
        }

        #endregion
    }
    public enum FeatureType
    {
        MarkerKind = 0,
        LineKind = 1,
        Polygon = 2
    }
}