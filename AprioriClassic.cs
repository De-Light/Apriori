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
        //原始数据
        private List<List<string>> RawData = new List<List<string>>();
        //K-候选项集 格式：<频繁项，<结果，结果中计数>>
        private Dictionary<List<string>, Dictionary<string, int>> K_Items = new Dictionary<List<string>, Dictionary<string, int>>();
        //频繁项字典 经过Sup和Conf筛选的K-候选项集
        private Dictionary<List<string>, Dictionary<string, int>> FrequentItems = new Dictionary<List<string>, Dictionary<string, int>>();
        //储存对规则的处理<List<数值>,结果>
        Dictionary<string[], string> Regulations = new Dictionary<string[], string>();
        //字段数
        private int itemLength;
        //上次训练时间
        private DateTime lastTrain = DateTime.Now;

        public float MinSupport { get; set; }                                              //最小支持度
        public float MinConfidence { get; set; }                                          //最小置信度
        public int regulationNum { get { return Regulations.Count; } }
        public AprioriClassic(float minSup, float minConf)
        {
            MinSupport = minSup;
            MinConfidence = minConf;
        }

        public AprioriClassic(float minSup, float minConf, string trainDataUri)
        {
            LoadData(trainDataUri);
            MinSupport = minSup;
            MinConfidence = minConf;
            this.Train();
        }
        #region ===============>训练Func
        /// <summary>
        /// Apriori训练
        /// </summary>
        /// <returns></returns>
        public void Train()
        {
            #region ------>添加1-候选项集
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

                #region 在每条源数据中查找候选项
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
                List<List<string>> deleteItem = new List<List<string>>();
                foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in K_Items)
                {
                    int count = item.Value.Values.ToArray().Sum();
                    if (count < MinSupport * RawData.Count)
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
                foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in K_Items)
                {
                    FrequentItems.Add(item.Key, item.Value);
                }
                #endregion

                #region 产生K+1项
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
                K_Items.Clear();
                foreach (List<string> item in NewItems)
                {
                    K_Items.Add(item, new Dictionary<string, int>());
                }
                #endregion
            }
            #endregion

            #region 计算置信度 输出关联规则
            foreach (KeyValuePair<List<string>, Dictionary<string, int>> item in FrequentItems)
            {
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
                    }
                }
            }
            #endregion
        }
        #endregion

        #region ============>测试Func
        /// <summary>
        /// 开始测试
        /// </summary>
        /// <param name="testData">要测试的数据</param>
        /// <returns>测试结果</returns>
        public string Test(string testData)
        {
            string currentLine = testData;

            #region 判断块
            Dictionary<string, double> testNote = new Dictionary<string, double>();  //用来记录规则判断的结果
            foreach (KeyValuePair<string[], string> regs in Regulations)             //读取行后，开始与每一条数据进行比对
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
            return max.Key;
        #endregion
        }
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