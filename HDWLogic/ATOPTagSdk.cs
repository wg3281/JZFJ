﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AblePick;
using BusinessLogic;
using BusinessLogic.Common;
using BusinessLogic.SortingTask;
using LY.FuelStationPOS.Protocol;
using MonitorMain;

namespace HDWLogic
{
    public class ATOPTagSdk
    {
        public struct Diagnosis
        {
            public string[] Port;

            public void Init(int count)
            {
                Port = new string[count];
            }
        }


        private System.Windows.Forms.Timer timerRcv;
        public static ATOPTagSdk _AtopTag;
        private SerialPortDao serialPortDao;
        const int SUMCMD_CONFIRM_BUTTON = 6;
        const int SUBCMD_SHORTAGE_BUTTON = 7;
        const int SUBCMD_DIAGRESULT = 9;
        const int SUBCMD_COMU_ERROR = 10;
        const int SUBCMD_UNEXECUTED = 12;
        const int SUBCMD_KEY_JAM = 13;
        const int SUBCMD_NO_LIGHT_PUSH = 100;
        const int SUBCMD_PRODUCT_FUNCTIONSETTING_INFO = 252;

        bool bAPIOpen, bIsCountingTest;
        byte iDigitPoint = 0, iTagMode, iCountingNum;
        short iLEDInterval, iNodeAddr;
        int GWCount, iNumData;
        int[] GWID = new int[1000];
        Diagnosis[] diagnosis = new Diagnosis[1000];

        public event EventHandler<EventArgs> OnAPIOpen;
        public event EventHandler<EventArgs> OnAPIClose;
        public event EventHandler<EventArgs> OnRecive;

        private MonitorLog monitorLog;

        public static ATOPTagSdk instance
        {
            get
            {
                if (null == _AtopTag)
                    _AtopTag = new ATOPTagSdk();
                return _AtopTag;
            }
        }


        public static void InitTags()
        {

            if (null == _AtopTag)
            {
                _AtopTag = new ATOPTagSdk();
            }
        }


        private ATOPTagSdk()
        {
            if (Tags == null)
            {
                Tags = new Dictionary<int, Tag>();
            }


            //打开串口路由
            Dap_Open();
            //熄灭所有标签
            TagClear();
            //获取控制器状态
            GetGWStatus();

            //开启消息回传监听
            this.timerRcv = new System.Windows.Forms.Timer();
            this.timerRcv.Tick += new System.EventHandler(timerRcv_Tick);
            timerRcv.Enabled = true;

            //serialPortDao = SerialPortDao.GetSerialPortDao();
            //serialPortDao.Received += new PortDataReceivedEventHandle(serialPortDao_Received);
            //try
            //{
            //    serialPortDao.Open();
            //}
            //catch (Exception)
            //{

            //    throw new Exception("串口打开失败");
            //}
        }

        /// <summary>
        /// 需要给人操作所显示的烟仓及数量列表
        /// 两个进程间依靠此列表进行按钮的获取
        /// </summary>
        static public Dictionary<int,Tag> Tags { get; set; }


        object o = new object();

        /// <summary>
        /// 接收按钮信号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPortDao_Received(object sender, PortDataReciveEventArgs e)
        {
            if (Tags != null && Tags.Keys.Contains(Convert.ToInt32(e.Data[0])))
            {
                lock (o)
                {
                    Tags[Convert.ToInt32(e.Data[0])].Qty = 0;
                }
                Dictionary<int, int> sendcontext = new Dictionary<int, int>();
                sendcontext.Add(Convert.ToInt32(e.Data[0]), 0);
                NixielightSDK.SendByMODBUS(sendcontext);
            }

        }


        /// <summary>
        /// 获取按钮的状态是否为已按下
        /// </summary>
        /// <returns>true已全部按下 false还没按完</returns>
        public bool GetPlcPressTagReady()
        {
            if (Tags == null)
            {
                return false;
            }

            //获取显示列表中是否有数量不为0（表示按钮未按）
            foreach (KeyValuePair<int, Tag> keyValuePair in Tags)
            {
                try
                {
                    if (keyValuePair.Value.Qty != 0)
                    {
                        //还有按钮未按
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                
            }
            //全部按完
            return true;
        }



        #region 标签控制SDK
        /// <summary>
        /// API 函数初始化
        /// </summary>
        /// <returns></returns>
        private int APIOpen()
        {
            if (!bAPIOpen)
            {
                int ret = 0;
                try
                {
                    ret = Dapapi.AB_API_Open();
                    if (ret < 0)
                        return -1;
                    else
                    {
                        bAPIOpen = true;
                        return 1;
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception(e.Message);
                }

            }
            return 0;
        }

        /// <summary>
        /// API注销
        /// </summary>
        /// <returns></returns>
        private int APIClose()
        {
            if (bAPIOpen)
            {
                Dapapi.AB_API_Close();
                bAPIOpen = false;
                return 1;
            }
            else
                return 0;
        }

        /// <summary>
        /// 控制器打开
        /// </summary>
        /// <returns></returns>
        private int Dap_Open()
        {
            int posspace, postab, pos;

            //txAddrList.Clear();

            GWCount = 0;
            if (!System.IO.File.Exists("IPINDEX"))
            {
                MessageBox.Show("串口路由地址文件不存在!");
                return -1;
            }
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader("IPINDEX"))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        GWCount++;
                        posspace = line.IndexOf(" ");   //find space
                        postab = line.IndexOf((char)9); //find tab

                        if (posspace <= 0) posspace = postab;
                        if (postab <= 0) postab = posspace;
                        pos = System.Math.Min(posspace, postab);
                        if (pos <= 0)
                        {
                            MessageBox.Show("串口路由地址文件格式错误!");
                            return -1;
                        }

                        //txAddrList.AppendText(line + "\r\n");

                        GWID[GWCount - 1] = int.Parse(line.Substring(0, pos));
                        diagnosis[GWCount - 1].Init(2);
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("读取串口路由地址文件失败!" + e.Message);
                return -1;
            }

            if (APIOpen() < 0)
            {
                MessageBox.Show("初始化串口API失败!");
                return -1;
            }


            //写日志
            MonitorLog monitorLog = MonitorLog.NewMonitorLog();
            monitorLog.LOGNAME = "设备初始化";
            monitorLog.LOGINFO = "初始化串口API成功";
            monitorLog.LOGLOCATION = "设备";
            monitorLog.LOGTYPE = 0;
            monitorLog.Save();

            //ShowMsg("API Open Success!");

            for (int i = 0; i < GWCount; i++)
            {
                if (Dapapi.AB_GW_Open(GWID[i]) < 0)
                {
                    MessageBox.Show("串口控制器'" + GWID[i] + "'打开失败!");
                }
            }

            return 1;
        }

        /// <summary>
        /// 控制器关闭
        /// </summary>
        private void Dap_Close()
        {
            //TagClear
            APIClose();
        }

        /// <summary>
        /// 熄灭所有标签
        /// </summary>
        private void TagClear()
        {
            for (int i = 0; i < GWCount; i++)
            {
                if (Dapapi.AB_GW_Status(GWID[i]) == 7)
                {
                    Dapapi.AB_LB_DspNum(GWID[i], -252, 0, 0, -3);
                    Dapapi.AB_LB_DspNum(GWID[i], 252, 0, 0, -3);
                    Dapapi.AB_LED_Dsp(GWID[i], -252, 0, 0);
                    Dapapi.AB_LED_Dsp(GWID[i], 252, 0, 0);
                    Dapapi.AB_BUZ_On(GWID[i], -252, 0);
                    Dapapi.AB_BUZ_On(GWID[i], 252, 0);
                    Dapapi.AB_LB_DspStr(GWID[i], -252, "", 0, -3);
                    Dapapi.AB_LB_DspStr(GWID[i], 252, "", 0, -3);

                    //12-digits Alphanumerical display
                    Dapapi.AB_AHA_ClrDsp(GWID[i], -252);
                    Dapapi.AB_AHA_ClrDsp(GWID[i], 252);
                    Dapapi.AB_AHA_BUZ_On(GWID[i], -252, 0);
                    Dapapi.AB_AHA_BUZ_On(GWID[i], 252, 0);
                }
            }
        }

        /// <summary>
        /// 清空消息对列
        /// </summary>
        private void ClearGWQuene()
        {
            byte[] cData = new byte[200];

            int gwid;
            short tagNode, subCmd, msgType, dataCnt;

            gwid = 0;
            tagNode = 0;
            subCmd = -1;
            msgType = 0;
            dataCnt = 0;
            while (Dapapi.AB_Tag_RcvMsg(ref gwid, ref tagNode, ref subCmd, ref msgType, cData, ref dataCnt) > 0)
            {
            }
        }

        /// <summary>
        /// 获取控制器状态
        /// </summary>
        private int GetGWStatus()
        {
            bool bGoOn;
            int ret, timeStart;

            for (int i = 0; i < GWCount; i++)
            {
                Dapapi.AB_GW_Open(GWID[i]);
                ret = Dapapi.AB_GW_Status(GWID[i]);

                if (ret != 7)
                {
                    bGoOn = true;
                    timeStart = System.Environment.TickCount;
                    while (bGoOn)
                    {
                        ret = Dapapi.AB_GW_Status(GWID[i]);
                        if (ret == 7)
                            bGoOn = false;
                        else if (System.Environment.TickCount - timeStart > 3000)
                        {
                            bGoOn = false;
                        }
                    }
                }

                if (ret == 7)
                {
                    //写日志
                    MonitorLog monitorLog = MonitorLog.NewMonitorLog();
                    monitorLog.LOGNAME = "设备初始化";
                    monitorLog.LOGINFO = "电子标签串口路由ID:" + GWID[i] + " 连接成功, 状态 :" + ret;
                    monitorLog.LOGLOCATION = "设备";
                    monitorLog.LOGTYPE = 0;
                    monitorLog.Save();
                    return 1;
                }
                else
                {
                    MessageBox.Show("电子标签串口路由ID:" + GWID[i] + " 连接失败, 状态 :" + ret);
                    //写日志
                    MonitorLog monitorLog = MonitorLog.NewMonitorLog();
                    monitorLog.LOGNAME = "设备初始化";
                    monitorLog.LOGINFO = "电子标签串口路由ID:" + GWID[i] + " 连接失败, 状态 :" + ret;
                    monitorLog.LOGLOCATION = "设备";
                    monitorLog.LOGTYPE = 0;
                    monitorLog.Save();
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取连结错误代码
        /// </summary>
        /// <param name="retcode"></param>
        /// <returns></returns>
        private string GetConnectErr(int retcode)
        {
            switch (retcode)
            {
                case -3:
                    return "Parameter data is error!";
                case -2:
                    return "TCP is not created yet!";
                case -1:
                    return "DAP_IP out of range !";
                case 0:
                    return "Closed";
                case 1:
                    return "Open";
                case 2:
                    return "Listening";
                case 3:
                    return "Connection is Pending";
                case 4:
                    return "Resolving the host name";
                case 5:
                    return "Host is resolved";
                case 6:
                    return "Waiting to Connect";
                case 7:
                    return "Connect OK";
                case 8:
                    return "Connection is closing";
                case 9:
                    return "State error has occurred";
                case 10:
                    return "Connection state is undetermined";
                default:
                    return "Unkown error code";
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="msg"></param>
        //private void ShowMsg(String msg)
        //{
        //    DateTime dt = DateTime.Now;

        //    if (txMsg.Text != "")
        //    {
        //        txMsg.AppendText("\r\n");
        //    }

        //    txMsg.AppendText(dt.ToString("T") + @"  " + msg);
        //    txMsg.ScrollToCaret();
        //}

        
        /// <summary>
        /// 检查字符是否是数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsNumber(String value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!Char.IsDigit(value, i))
                {
                    return false;
                }
            }

            return true;
        }


        private void timerRcv_Tick(object sender, EventArgs e)
        {
            RcvMsg();
        }


        /// <summary>
        /// 接收标签信息
        /// </summary>
        private void RcvMsg()
        {
            int gwid, ret;
            short tagNode, subCmd, msgType, dataCnt;
            short gwPort, keyType, maxTag;
            byte[] rcvData = new byte[200];
            Dapapi.Tccb ccb_data;
            string tmpStr, rcvStr;


            gwid = 0;       //all gateway    
            tagNode = 0;    //all tagnode
            subCmd = -1;    //all subcmd
            msgType = 0;
            dataCnt = 200;

            Dapapi.AB_GW_Status(0);
            ret = Dapapi.AB_Tag_RcvMsg(ref gwid, ref tagNode, ref subCmd, ref msgType, rcvData, ref dataCnt);
            if (ret > 0)
            {
                rcvStr = System.Text.Encoding.Default.GetString(rcvData);
                if (tagNode < 0)
                    gwPort = 1;
                else
                    gwPort = 2;

                tmpStr = "GW_ID:" + gwid + ",GW Port:" + gwPort + ",TagNode:" + System.Math.Abs(tagNode) + ",SubCmd:" + subCmd + ",Data:" + rcvStr;

                switch (subCmd)
                {
                    case SUMCMD_CONFIRM_BUTTON:
                        //写日志
                        monitorLog = MonitorLog.NewMonitorLog();
                        monitorLog.LOGNAME = "接收到标签信号";
                        monitorLog.LOGINFO = tmpStr;
                        monitorLog.LOGLOCATION = "设备";
                        monitorLog.LOGTYPE = 0;
                        monitorLog.Save();
                        //ShowMsg(tmpStr);


                        if (Tags.ContainsKey(System.Math.Abs(Convert.ToInt32(tagNode))))
                        {
                            Tags[System.Math.Abs(Convert.ToInt32(tagNode))].Qty = 0;
                        }
                        break;


                    default:
                        monitorLog = MonitorLog.NewMonitorLog();
                        monitorLog.LOGNAME = "设备初始化";
                        monitorLog.LOGINFO = tmpStr;
                        monitorLog.LOGLOCATION = "设备";
                        monitorLog.LOGTYPE = 0;
                        monitorLog.Save();
                        break;
                }
            }
        }


        private int DiagResult(byte[] ccb_data, ref string tagstr)
        {
            int k, tmp, maxid;

            tagstr = "";
            tmp = 0;
            maxid = 0;

            for (k = 1; k <= 250; k++)
            {
                if ((k - 1) % 8 == 0)
                {
                    tmp = ccb_data[3 + (k - 1) / 8];
                }
                if (tmp % 2 != 1)
                {
                    maxid = k;
                    tagstr = tagstr + "1";
                }
                else
                {
                    tagstr = tagstr + "0";
                }
                tmp = tmp / 2;
            }

            tagstr = tagstr.Substring(0, maxid);

            return maxid;
        }

        /// <summary>
        /// 发送订单到数码管
        /// </summary>
        public void SetOrderNixielight(SortingLineTask[] sortingLineTasks)
        {
            //熄灭所有标签
            TagClear();

            if (Tags == null)
            {
                Tags = new Dictionary<int, Tag>();
            }

            //int sortinglineboxcount = SortingLineBoxList.GetLineBoxList().Count;

            try
            {

                //foreach (SortingLineBox sortingLineBox in SortingLineBoxList.GetLineBoxList())
                //{
                //    Tags.Add(Convert.ToInt32(sortingLineBox.LineBoxCode), new Tag("0", Convert.ToInt32(sortingLineBox.LineBoxCode), 0));
                //}


                SortingSubLine[] sortingSubLineList = SortingSubLineList.GetSubSortingLineList().OrderBy(o => o.sequence).ToArray();


                //初始化标签的内容
                for (int j = 1; j < sortingLineTasks.Length; j++)
                {
                    if (sortingLineTasks[j] != null)
                    {
                        //当前子线包含的所有任务明细列表
                        IEnumerable<SortingLineTaskDetail> sortingLineTaskDetails =
                            sortingLineTasks[j].SortingLineTaskDetails.GetAreaDetails(sortingSubLineList[j - 1].sublineCode);

                        foreach (SortingLineTaskDetail detail in sortingLineTaskDetails)
                        {
                            try
                            {
                                Tags.Add(Convert.ToInt32(detail.LINEBOXCODE), new Tag(sortingLineTasks[j].ID, Convert.ToInt32(detail.LINEBOXCODE), detail.QTY));
                                //Tags[Convert.ToInt32(detail.LINEBOXCODE)] = new Tag(sortingLineTasks[j].ID,
                                //    Convert.ToInt32(detail.LINEBOXCODE), detail.QTY);
                            }
                            catch (Exception)
                            {


                            }

                        }
                    }
                }

                //发送标签内容 
                foreach (KeyValuePair<int, Tag> keyValuePair in Tags)
                {
                    Dapapi.AB_LB_DspNum(GWID[0], (short)keyValuePair.Key, keyValuePair.Value.Qty, iDigitPoint, iLEDInterval);
                }
            }
            catch (Exception)
            {


            }

            
        }


        

        
        #endregion


    }

    public class Tag
    {
        public Tag(string taskno,int lineboxcode,int totqty)
        {
            TaskNo = taskno;
            LineboxCode = lineboxcode;
            TotQty = totqty;
            Qty = totqty;
        }
        public string TaskNo;
        
        public int LineboxCode;
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotQty;
        /// <summary>
        /// 完成后显示的数量
        /// </summary>
        public int Qty;
    }

}
