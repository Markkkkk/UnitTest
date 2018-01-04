using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.DataOperate
{
    public static class GisSqliteOperate
    {

        private static readonly string DATA_DB = Environment.CurrentDirectory + "\\LocalData\\city_data.db";
        private static readonly string DataConnectionString = string.Format("Data Source=\"{0}\";Page Size=32768;Pooling=True", DATA_DB);

        /// <summary>
        /// 查询区划数据
        /// </summary>
        /// <param name="parent_gid"></param>
        /// <returns></returns>
        public static DataTable GetAllData()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection())
                {
                    cn.ConnectionString = DataConnectionString;
                    cn.Open();
                    {
                        SQLiteCommand cmd = new SQLiteCommand(string.Format("SELECT gid, parent_gid, name, dl, dr, tl, tr, pic_count  FROM city_province_2004 WHERE name <> '';"), cn);
                        cmd.CommandType = CommandType.Text;
                        SQLiteDataReader dReader = cmd.ExecuteReader();
                        dt.Load(dReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*Error* -- SQLiteError: " + ex);
            }
            return dt;
        }

        public static bool UpdatePicCount(string count, string gid)
        {
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection())
                {
                    cn.ConnectionString = DataConnectionString;
                    cn.Open();
                    {
                        SQLiteCommand cmd = new SQLiteCommand(string.Format("UPDATE city_province_2004 SET pic_count=@count WHERE gid =  '{0}';", gid), cn);
                        cmd.Parameters.Add("count", DbType.String).Value = count;
                        Console.WriteLine(cmd.ExecuteNonQuery().ToString());
                    }
                }
                Console.WriteLine("-Success- Gid:{0}", gid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("*Error* -- SQLiteError: " + ex);
                return false;
            }
        }
    }
}
