using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.MapDownload;

namespace UnitTest
{
    /** 
    * 
    *----------Dragon be here!----------/ 
    * 　　　┏┓　　　┏┓ 
    * 　　┏┛┻━━━┛┻┓ 
    * 　　┃　　　　　　　┃ 
    * 　　┃　　　━　　　┃ 
    * 　　┃　┳┛　┗┳　┃ 
    * 　　┃　　　　　　　┃ 
    * 　　┃　　　┻　　　┃ 
    * 　　┃　　　　　　　┃ 
    * 　　┗━┓　　　┏━┛ 
    * 　　　　┃　　　┃神兽保佑 
    * 　　　　┃　　　┃代码无BUG！ 
    * 　　　　┃　　　┗━━━┓ 
    * 　　　　┃　　　　　　　┣┓ 
    * 　　　　┃　　　　　　　┏┛ 
    * 　　　　┗┓┓┏━┳┓┏┛ 
    * 　　　　　┃┫┫　┃┫┫ 
    * 　　　　　┗┻┛　┗┻┛ 
    * ━━━━━━神兽出没━━━━━━
    */
    public class Program
    {
        static void Main(string[] args)
        {
            MapPointOperate op = new MapPointOperate();
            op.CalcPicCountAndSave();
            Console.Out.WriteLine("Let's begin.");
            Console.In.ReadLine();
            
        }
    }

}