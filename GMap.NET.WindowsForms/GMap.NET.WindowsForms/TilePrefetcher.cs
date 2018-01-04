namespace GMap.NET
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;
    using GMap.NET.Internals;
    using System;
    using GMap.NET.MapProviders;
    using System.Threading;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;
    using System.Drawing;

    /// <summary>
    /// form helping to prefetch tiles on local db
    /// </summary>
    public partial class TilePrefetcher : Form
    {
        BackgroundWorker worker = new BackgroundWorker();
        List<GPoint> list;
        int zoom;
        GMapProvider provider;
        int sleep;
        int all;
        public bool ShowCompleteMessage = false;
        RectLatLng area;
        GMap.NET.GSize maxOfTiles;
        public GMapOverlay Overlay;
        int retry;
        public bool Shuffle = true;

        public Keys key { get; set; }


        // 参数 for SQLite 离线地图存储
        // 2017-09-17 by Mark Lee
        private int gid { get; set; }
        private int parentGid { get; set; }

        public TilePrefetcher(int gid, int parentGid)
        {
            InitializeComponent();
            this.gid = gid;
            this.parentGid = parentGid;

            Cache.Instance.CacheLocation = Environment.CurrentDirectory + "\\GmapCache";
            GMaps.Instance.OnTileCacheComplete += new TileCacheComplete(OnTileCacheComplete);
            GMaps.Instance.OnTileCacheStart += new TileCacheStart(OnTileCacheStart);
            GMaps.Instance.OnTileCacheProgress += new TileCacheProgress(OnTileCacheProgress);

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        readonly AutoResetEvent done = new AutoResetEvent(true);

        void OnTileCacheComplete()
        {
            if (!IsDisposed)
            {
                done.Set();

                MethodInvoker m = delegate
                {
                    label2.Text = "所有切片已保存...";
                };
                Invoke(m);
            }
        }

        void OnTileCacheStart()
        {
            if (!IsDisposed)
            {
                done.Reset();

                MethodInvoker m = delegate
                {
                    label2.Text = "储存切片中...";
                };
                Invoke(m);
            }
        }

        void OnTileCacheProgress(int left)
        {
            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    label2.Text = left + " 张剩余...";
                };
                Invoke(m);
            }
        }

        public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep, int retry)
        {
            if (!worker.IsBusy)
            {
                this.label1.Text = "...";
                this.progressBarDownload.Value = 0;

                this.area = area;
                this.zoom = zoom;
                this.provider = provider;
                this.sleep = sleep;
                this.retry = retry;

                GMaps.Instance.UseMemoryCache = false;
                GMaps.Instance.CacheOnIdleRead = false;
                GMaps.Instance.BoostCacheEngine = true;

                if (Overlay != null)
                {
                    Overlay.Markers.Clear();
                }

                worker.RunWorkerAsync();

                this.ShowDialog();
            }
        }

        public void Stop()
        {
            GMaps.Instance.OnTileCacheComplete -= new TileCacheComplete(OnTileCacheComplete);
            GMaps.Instance.OnTileCacheStart -= new TileCacheStart(OnTileCacheStart);
            GMaps.Instance.OnTileCacheProgress -= new TileCacheProgress(OnTileCacheProgress);

            done.Set();

            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }

            GMaps.Instance.CancelTileCaching();

            done.Close();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ShowCompleteMessage)
            {
                if (!e.Cancelled)
                {
                    MessageBox.Show(this, "下载完成! => " + ((int)e.Result).ToString() + " / " + all);
                }
                else
                {
                    MessageBox.Show(this, "下载完成! => " + ((int)e.Result).ToString() + " / " + all);
                }
            }

            list.Clear();

            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.CacheOnIdleRead = true;
            GMaps.Instance.BoostCacheEngine = false;

            worker.Dispose();

            this.Close();
        }

        bool CacheTiles(int zoom, GPoint p)
        {
            foreach (var pr in provider.Overlays)
            {
                Exception ex;
                PureImage img;

                // tile number inversion(BottomLeft -> TopLeft)
                if (pr.InvertedAxisY)
                {
                    img = GMaps.Instance.GetImageFrom(pr, new GPoint(p.X, maxOfTiles.Height - p.Y), zoom, out ex);
                }
                else // ok
                {
                    img = GMaps.Instance.GetImageFrom(pr, p, zoom, out ex);
                }

                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public readonly Queue<GPoint> CachedTiles = new Queue<GPoint>();

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (list != null)
            {
                list.Clear();
                list = null;
            }
            list = provider.Projection.GetAreaTileList(area, zoom, 0);
            maxOfTiles = provider.Projection.GetTileMatrixMaxXY(zoom);
            all = list.Count;

            int countOk = 0;
            int retryCount = 0;

            if (Shuffle)
            {
                Stuff.Shuffle<GPoint>(list);
            }

            lock (this)
            {
                CachedTiles.Clear();
            }

            for (int i = 0; i < all; i++)
            {
                if (worker.CancellationPending)
                    break;

                GPoint p = list[i];
                {
                    // 参数 for SQLite 离线地图存储
                    // 2017-09-17 by Mark Lee
                    p.gid = this.gid;
                    p.parentGid = this.parentGid;
                    if (CacheTiles(zoom, p))
                    {
                        if (Overlay != null)
                        {
                            lock (this)
                            {
                                CachedTiles.Enqueue(p);
                            }
                        }
                        countOk++;
                        retryCount = 0;
                    }
                    else
                    {
                        if (++retryCount <= retry) // retry only one
                        {
                            i--;
                            System.Threading.Thread.Sleep(1111);
                            continue;
                        }
                        else
                        {
                            retryCount = 0;
                        }
                    }
                }

                worker.ReportProgress((int)((i + 1) * 100 / all), i + 1);

                if (sleep > 0)
                {
                    System.Threading.Thread.Sleep(sleep);
                }
            }

            e.Result = countOk;

            if (!IsDisposed)
            {
                done.WaitOne();
            }

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.label1.Text = "正在下载 (" + zoom + ") 层级地图切片: " + ((int)e.UserState).ToString() + " / " + all + ", 完成: " + e.ProgressPercentage.ToString() + "%";

            this.progressBarDownload.Value = e.ProgressPercentage;

            if (Overlay != null)
            {
                GPoint? l = null;

                lock (this)
                {
                    if (CachedTiles.Count > 0)
                    {
                        l = CachedTiles.Dequeue();
                    }
                }

                // 注释原因是：实际运行此段代码没有意义
                //if (l.HasValue)
                //{
                //    var px = Overlay.Control.MapProvider.Projection.FromTileXYToPixel(l.Value);
                //    var p = Overlay.Control.MapProvider.Projection.FromPixelToLatLng(px, zoom);

                //    var r1 = Overlay.Control.MapProvider.Projection.GetGroundResolution(zoom, p.Lat);
                //    var r2 = Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, p.Lat);
                //    var sizeDiff = r2 / r1;

                //    //GMapMarkerTile m = new GMapMarkerTile(p, (int)(Overlay.Control.MapProvider.Projection.TileSize.Width / sizeDiff));
                //   // Overlay.Markers.Add(m);
                //}
            }
        }

        private void Prefetch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.key = Keys.Escape;
                this.Close();
            }
        }

        private void Prefetch_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Stop();
        }
    }

    /// <summary>
    /// 用作地图下载在控件可视范围内的展示
    /// </summary>
    public class GMapMarkerTile : GMapMarker
    {
        static Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

        public GMapMarkerTile(PointLatLng p, int size) : base(p)
        {
            Size = new System.Drawing.Size(size, size);
        }

        public override void OnRender(Graphics g)
        {
            g.FillRectangle(Fill, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
        }
    }
}
