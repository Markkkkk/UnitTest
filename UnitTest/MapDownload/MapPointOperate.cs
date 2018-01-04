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
                string totalCount = string.Empty;
                for (int i = 1; i <= 18; i++)
                {
                    int count = GetPicCount(rect, i, 0);
                    Console.WriteLine("Name:{0}, gid:{1}, zoom:{2}, count:{3}", jname, jgid, i.ToString(), count.ToString());
                    if (i == 1)
                    {
                        totalCount += count.ToString();
                    } else
                    {
                        totalCount += "," + count.ToString();
                    }
                }
                Console.WriteLine(totalCount.ToString());
                GisSqliteOperate.UpdatePicCount(totalCount, jgid);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="zoom"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        private int GetPicCount(RectLatLng area, int zoom, int padding)
        {

            int ret = provider.Projection.GetAreaTileCount(area, zoom, 0);

            return ret;

        }
    }
}