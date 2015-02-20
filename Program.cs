using System;
using System.Collections.Generic;
using System.IO;

namespace Apriori
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;   //设置控制台颜色：白（强迫症）

            string dataSet = "iris";                       //数据集名称
            char[] ABCDE = { 'A', 'B', 'C', 'D', 'E' };     //五组数据集

            Console.WriteLine("DATA SET:{0}", dataSet);

            AprioriClassic aprioriTest = new AprioriClassic((float)0.01, (float)0.1);         //在这里修改minSup和minConf
            
            foreach (char letter in ABCDE)
            {
                DateTime start = DateTime.Now;
                string trainDataUri = @"D:\Temp File\Data\data 46\" + dataSet + "_" + letter + "-6.txt";        //训练集路径（经过改造的）
                string testDataUrl = @"D:\Temp File\Data\data 46\" + dataSet + "_" + letter + "-4.txt";         //测试集路径（经过改造的）
                Console.WriteLine("测试&训练" + letter + "组");

                aprioriTest.ClearRegulatins();

                aprioriTest.LoadData(trainDataUri);
                
                aprioriTest.Train();

                aprioriTest.Test(testDataUrl);

                DateTime finish = DateTime.Now;
                Console.WriteLine("REGULATIONS:{0}", aprioriTest.RegNum);
                Console.WriteLine("TIME SPENDED:{0}", (finish - start).ToString());
            }
            Console.ReadKey();
        }
    }
}
