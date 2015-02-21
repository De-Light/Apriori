using System;
using System.Collections.Generic;
using System.IO;

namespace Apriori
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;//设置控制台颜色：白（强迫症）

            string dataSet = "iris";//数据集名称
            char[] ABCDE = { 'A', 'B', 'C', 'D', 'E' };//五组数据集
            Console.WriteLine("DATASET:{0}", dataSet);
            AprioriClassic aprioriTest = new AprioriClassic((float)0.01, (float)0.1);//在这里修改minSup和minConf

            foreach (char letter in ABCDE)
            {
                DateTime start = DateTime.Now;
                //训练集路径（经过改造的）
                string trainDataUri = @"D:\Temp File\Data\data 46\" + dataSet + "_" + letter + "-6.txt";
                //测试集路径（经过改造的）
                string testDataUri = @"D:\Temp File\Data\data 46\" + dataSet + "_" + letter + "-4.txt";
                Console.WriteLine("测试&训练" + letter + "组");
                //清除上一次训练结果
                aprioriTest.ClearRegulatins();
                //读取新的训练集
                aprioriTest.LoadData(trainDataUri);
                //开始训练
                aprioriTest.Train();
                //初始化测试（Test）
                int testTimes = 0;//测试次数
                int successTime = 0;//成功次数
                StreamReader FileLoader = new StreamReader(testDataUri);
                //开始训练并统计成功次数
                while (true)
                {
                    string currentLine = FileLoader.ReadLine();
                    if (currentLine == null)
                    {
                        break;
                    }
                    string testResult;
                    string[] result = currentLine.Split(new char[] { ',' });
                    testTimes++;
                    testResult = aprioriTest.Test(currentLine);
                    if (testResult == result[result.Length - 1])
                    {
                        successTime++;
                    }
                }
                DateTime finish = DateTime.Now;
                //输出
                Console.WriteLine("REGULATIONS:{0}", aprioriTest.regulationNum);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SuccessRate:{0}%", Math.Round(successTime * 100.0f / testTimes, 2));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("TIMESPENDED:{0}", (finish - start).ToString());
            }
            Console.ReadKey();
        }
    }
}
