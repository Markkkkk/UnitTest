using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.DataOperate;

namespace UnitTest.MapDownload
{   
    public class MapPointOperate
    {
        
        GMapProvider provider = TDTMapProvider.Instance;

        public MapPointOperate()
        {

        }

        public void CalcPicCountAndSave()
        {
            DataTable dt = new DataTable();

            dt = GisSqliteOperate.GetAllData();

            for (int j = 0; j < dt.Rows.Count; j++)
            {
                string jgid = dt.Rows[j]["gid"].ToString();
                string jname = dt.Rows[j]["name"].ToString();
                string jparent_gid = dt.Rows[j]["parent_gid"].ToString();
                string jdl = dt.Rows[j]["dl"].ToString();
                string jdr = dt.Rows[j]["dr"].ToString();
                string jtl = dt.Rows[j]["tl"].ToString();
                string jtr = dt.Rows[j]["tr"].ToString();

                double tllat2 = dt.Rows[j]["tl"].ToString().Equals(string.Empty) ? 0.0 : double.Parse(dt.Rows[j]["tl"].ToString().Split(' ')[1].ToString());
                double tllng2 = dt.Rows[j]["tl"].ToString().Equals(string.Empty) ? 0.0 : double.Parse(dt.Rows[j]["tl"].ToString().Split(' ')[0].ToString());
                double trlng2 = dt.Rows[j]["tr"].ToString().Equals(string.Empty) ? 0.0 : double.Parse(dt.Rows[j]["tr"].ToString().Split(' ')[0].ToString());
                double dllat2 = dt.Rows[j]["dl"].ToString().Equals(string.Empty) ? 0.0 : double.Parse(dt.Rows[j]["dl"].ToString().Split(' ')[1].ToString());

                RectLatLng rect = new RectLatLng(tllat2, tllng2, trlng2 - tllng2, tllat2 - dllat2);


            }
        }

        private int GetPicCount(RectLatLng rect, int zoom, int padding)
        {

            int ret = 0;

            //list = provider.Projection.GetAreaTileList(area, zoom, 0);

            return ret;

        }
    }
}