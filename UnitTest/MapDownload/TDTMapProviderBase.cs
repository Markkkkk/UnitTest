using System;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using System.Diagnostics;

namespace UnitTest.MapDownload
{
    public abstract class TDTMapProviderBase : GMapProvider
    {
        bool isTrue = true;
        public TDTMapProviderBase()
        {
            MaxZoom = null;
            RefererUrl = "http://map.tianditu.com";
            //Copyright = string.Format("©{0} 天地图 Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);    
        }

        public override PureProjection Projection
        {
            get { return MercatorProjection.Instance; }
        }

        GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] {};
                    // ContoursProvider.Instance,,GMapProviders.GoogleChinaMap， this, TDTLabelProvider.Instance
                }
                return overlays;
            }
        }
    }

    public class TDTMapProvider : TDTMapProviderBase
    {
        public static readonly TDTMapProvider Instance;

        readonly Guid id = new Guid("EF3DD303-3F74-4938-BF40-232D0595EE88");
        public override Guid Id
        {
            get { return id; }
        }

        readonly string name = "等高线";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        static TDTMapProvider()
        {
            Instance = new TDTMapProvider();
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            try
            {
                string url = MakeTileImageUrl(pos, zoom, LanguageStr);
                return GetTileImageUsingHttp(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            var num = (pos.X + pos.Y) % 4 + 1;
            //string url = string.Format(UrlFormat, num, pos.X, pos.Y, zoom);

            string url = string.Format(UrlFormat, zoom, pos.Y, pos.X);
            return url;
        }
        //static readonly string UrlFormat = StaticFunction.TDTMapURL;
        //static readonly string UrlFormat = "http://webrd04.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=7";
        //static readonly string UrlFormat = "http://t0.tianditu.com/vec_c/wmts/wmts?Service=WMTS&Request=GetTile&Version=1.0.0&Style=Default&Format=tiles&serviceMode=KVP&layer=vec&TileMatrixSet=c&TileMatrix={0}&TileRow={1}&TileCol={2}";
        static readonly string UrlFormat = "http://t0.tianditu.com/vec_w/wmts/wmts?Service=WMTS&Request=GetTile&Version=1.0.0&Style=Default&Format=tiles&serviceMode=KVP&layer=vec&TileMatrixSet=w&TileMatrix={0}&TileRow={1}&TileCol={2}";
    }
}