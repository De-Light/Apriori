using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Apriori
{
    /// <summary>
    /// Apriori-Classic
    /// </summary>
    class AprioriClassic
    {
        Dictionary<string[], string> Regulations = new Dictionary<string[], string>();      //储存对规则的处理<List<数值>,结果>
        //频繁项字典 经过Sup和Conf筛选的K-候选项集
        private Dictionary<List<string>, Dictionary<string, int>> FrequentItems = new Dictionary<List<string>, Dictionary<string, int>>();
        //K-候选项集 格式：<频繁项，<结果，结果中计数>>
        private Dictionary<List<string>, Dictionary<string, int>> K_Items = new Dictionary<List<string>, Dictionary<string, int>>();
        private List<List<string>> RawData = new List<List<string>>();           //原始数据
        private int itemLength;                                                   //字段数

        public float MinSupport{get;set;}                                              //最小支持度
        public float MinConfidence{get;set;}                                          //最小置信度

        public AprioriClassic(float minSup, float minConf)
        {
            MinSupport = minSup;
            MinConfidence = minConf;
        }
        #region ===============>训练Func
        /// <summary>
        /// Apriori训练
        /// </summary>
        /// <returns></returns>
        public void Train(string trainDataUri)
        {
            //Console.WriteLine("--------Training----------");

            //Console.WriteLine("minSup:{0}  minConf:{1}", minSup, minConf);

            LoadData(trainDataUri);

            #region 添加1-候选项集
            foreach (List<string> thing in RawData)
            {
                for (int i = 0; i < itemLength; i++)
                {
                    bool contain = true;
                    foreach (List<string> item in K_Items.Keys)
                    {
                        if (item[0] == Convert.ToString(thing[i]))
                        {
                            contain = false;
                            break;
                        }
                    }
                    if (contain)
                    {
                        List<string> array = new List<string>();
                        array.Add(Convert.ToString(thing[i]));
                        K_Items.Add(array, new Dictionary<string, int>());
                    }
                }
            }
            #endregion

            #region 开始迭代
            int K_count = 0;
            while (K_Items.Count > 1)
            {
                K_count++;
                // Console.Write("{0}--正在产生{1}频繁项集.", DateTime.Now, K_count);

                #region 在每条源数据中查找候选项
                //Console.Write(".");
                foreach (List<string> item in K_Items.Keys)
                {
                    for (int i = 0; i < RawData.Count; i++)
                    {
                        bool contain = true;
                        for (int j = 0; j < K_count; j++)
                        {
                            if (!RawData[i].Contains(item[j].ToString()))
                            {
                                contain = false;
                                break;
                            }
                        }
                        if (contain)
                        {
                            string result = RawData[i][itemLength];              //读取i条源数据的结果

                            if (K_Items[item].Keys.Contains(result))            //添加或自增result的计数
                            {
                                int count = K_Items[item][result];
                                K_Items[item][result] = count + 1;
                            }
                            else
                            {
                                K_Items[item].Add(result, 1);
                            }
                        }
                    }
                }
                #endregion

                #region 根据MinSup筛选
                // Console.Write(".");
                List<List<string>> deleteItem = new List<List<string>>();
                foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in K_Items)
                {
                    int count = item.Value.Values.ToArray().Sum();
                    if (count < MinSupport*RawData.Count)
                    {
                        deleteItem.Add(item.Key);
                    }
                }
                for (int i = 0; i < deleteItem.Count; i++)
                {
                    K_Items.Remove(deleteItem[i]);
                }
                #endregion

                #region 将K频繁项添加到频繁项集中
                //Console.Write(".");
                foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in K_Items)
                {
                    FrequentItems.Add(item.Key, item.Value);
                }
                #endregion

                #region 产生K+1项
                //Console.Write(".");
                List<List<string>> OldItems = K_Items.Keys.ToList();
                List<List<string>> NewItems = new List<List<string>>();
                for (int i = 0; i < OldItems.Count; i++)
                {
                    for (int j = i + 1; j < OldItems.Count; j++)
                    {
                        if (OldItems[i][K_count - 1] != OldItems[j][K_count - 1])
                        {
                            bool couldMove = true;
                            if (K_count != 1)
                            {
                                for (int k = 0; k < K_count - 1; k++)
                                {
                                    if (OldItems[i][k] != OldItems[j][k])
                                    {
                                        couldMove = false;
                                        break;
                                    }
                                }
                            }
                            if (couldMove)
                            {
                                List<string> temp = new List<string>(OldItems[i]);
                                temp.Add(OldItems[j][K_count - 1]);
                                temp.Sort();
                                if (NewItems.Count == 0)                  //做引
                                {
                                    NewItems.Add(temp);
                                    continue;
                                }
                                bool isExist = false;
                                foreach (List<string> item in NewItems)     //查重
                                {
                                    bool listEqual = true;
                                    for (int k = 0; k <= K_count; k++)
                                    {
                                        if (temp[k] != item[k])
                                        {
                                            listEqual = false;
                                        }
                                    }
                                    if (listEqual)
                                    {
                                        isExist = true;
                                    }
                                }
                                if (!isExist)
                                {
                                    NewItems.Add(temp);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 产生新的候选字典
                //Console.Write(".");
                K_Items.Clear();
                foreach (List<string> item in NewItems)
                {
                    K_Items.Add(item, new Dictionary<string, int>());
                }
                //Console.Write("Complited.\n");
                #endregion
            }
            #endregion

            #region 计算置信度 输出关联规则
            //Console.WriteLine("{0}--根据置信度生成关联规则", DateTime.Now);
           // List<string> Regulations = new List<string>();      //存放返回的规则库
            foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in FrequentItems)
            {
               /* string regular = "";
                foreach (string num in item.Key)
                {
                    regular = regular + num.ToString() + ",";
                }
                regular = regular.Remove(regular.Length - 1, 1);*/
                float totalCount = 0;
                foreach (KeyValuePair<string, int> sitem in item.Value)
                {
                    totalCount += sitem.Value;
                }
                foreach (KeyValuePair<string, int> sitem in item.Value)
                {
                    float conf = sitem.Value / totalCount;
                    if (conf > MinConfidence)
                    {
                        string[] items = item.Key.ToArray();
                        Regulations.Add(items, sitem.Key);
                        //string temp = regular + "|" + sitem.Key;         //规则格式："规则项|规则结果"
                        //Regulations.Add(temp);

                        /*      消除重复的规则项
                        bool exist = false;
                        foreach (string  reg in Regulations)
                        {
                            if (reg == temp)
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist)
                        {
                            Regulations.Add(temp);
                        }
                         * */
                    }
                }
            }
            #endregion
            /*
             #region  输出规则库
            List<string> Regulations = new List<string>();
            foreach (KeyValuePair<List<double>,Dictionary<string,int>> item in FrequentItems )
            {
                string regulation = item.Key
            }
            #endregion
            */
            Console.WriteLine("Regulations Trained：{0}", Regulations.Count);
            //return new List<string>() ;
        }
        #endregion
       

        #region ============>测试Func
        /// <summary>
        /// 测试Func
        /// </summary>
        /// <param name="Source">规则库</param>
        /// <param name="fileUrl">测试训练集文件</param>
        public void Test(string fileUrl)
        {
            //Console.WriteLine("-----------testing------------");

            //#region 对string规则的处理
           
            //foreach (string item in Source)                                                             //对string规则的处理
            //{
            //    string[] temps = item.Split(new char[] { '|' });
            //    string[] numbTemp = temps[0].Split(new char[] { ',' });
            //    List<string> numbs = new List<string>();
            //    foreach (string numb in numbTemp)
            //    {
            //        numbs.Add(Convert.ToString(numb));
            //    }
            //    Regulations.Add(numbs, temps[1]);
            //}
            //#endregion

            int testTime = 0;       //判断次数
            int success = 0;        //成功次数


            StreamReader FileLoader = new StreamReader(fileUrl);    //读取器
            while (true)
            {
                #region ----------->读取&计数
                string currentLine = FileLoader.ReadLine();         //读取行，若为空结束，否则计数器++
                if (currentLine == null)
                {
                    break;
                }
                testTime++;
                #endregion

                #region 判断块
                Dictionary<string, double> testNote = new Dictionary<string, double>();        //用来记录规则判断的结果
                foreach (KeyValuePair<string[], string> regs in Regulations)          //读取行后，开始与每一条数据进行比对
                {
                    int weight = 0;                                                      //权值，为规则项数
                    bool isContain = true;
                    foreach (string num in regs.Key)                                     //判断是否存在当前规则
                    {
                        if (!currentLine.Contains(num.ToString()))
                        {
                            isContain = false;
                            break;
                        }
                        else
                        {
                            weight++;
                        }
                    }
                    if (isContain)                                                      //若存在，将规则指向的结果存入结果记录器中
                    {
                        if (testNote.Keys.Contains(regs.Value))
                        {
                            double count = testNote[regs.Value];
                            testNote[regs.Value] = count + weight * Math.Log(weight, 2) + weight;       //权值计算：（遍历规则库）权值+=该条规则的项数+Log（项数）
                        }
                        else
                        {
                            testNote.Add(regs.Value, 1);
                        }
                    }
                }
                #endregion

                #region 整理结果记录，将最大值取出
                KeyValuePair<string, double> max = new KeyValuePair<string, double>(" ", 0);
                foreach (KeyValuePair<string, double> item in testNote)
                {
                    if (max.Value < item.Value)
                    {
                        max = item;
                    }
                }
                #endregion

                #region 与当前行的最后一项（正确结果）对比，成功则success++
                string[] currentLines = currentLine.Split(',');
                //Console.WriteLine("规则库判断：{0}正确结果：{1}", max.Key, currentLines[currentLines.Length - 1]);
                if (max.Key == currentLines[currentLines.Length - 1])
                {
                    success++;
                    // Console.Write("-------success\n");
                }
                else
                {
                    //Console.Write("-------failed\n");
                }
                // Console.ReadKey();
                #endregion
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("success percentage:{0,5}%", (success * 100f / testTime * 1f));
            Console.ForegroundColor = ConsoleColor.White;
        }
        #endregion


        public void LoadData(string url)
        {
            //Console.WriteLine("-----------Loading Data----------");
            
            //RawData.Add(new List<string>(new string[5] { "8", "3", "6", "4", "0" }));
            //RawData.Add(new List<string>(new string[5] { "2", "5", "3", "4", "-1" }));
            //RawData.Add(new List<string>(new string[5] { "6", "3", "5", "2", "-1" }));
            //RawData.Add(new List<string>(new string[5] { "5", "2", "4", "1", "0" }));
            

            StreamReader FileLoader = new StreamReader(url);
            while (true)
            {
                string currentLine = FileLoader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                RawData.Add(new List<string>(currentLine.Split(new char[] { ',' })));
            }
            itemLength = RawData[0].Count - 1;         //确定字段长度 

            //Console.WriteLine("Data Load：{0}", RawData.Count);
        }

        public void ClearRegulatins()
        {
            Regulations.Clear();
        }
    }
}