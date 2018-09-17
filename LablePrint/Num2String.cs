﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PosPrint
{
   
        /// <summary>
        /// 金额小写转中文大写类(附带一个数字转大写)。
        /// 整数支持到万亿；小数部分支持到分(超过两位将进行Banker舍入法处理)
        /// </summary>
        public class Num2String
        {
            String[] Ls_ShZ = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖", "拾" };
            String[] Ls_DW_Zh = { "元", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "万" };
            String[] Num_DW = { "", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "万" };
            String[] Ls_DW_X = { "角", "分" };
            Boolean XSh_bool = false;//是否含有小数，默认没有(0则视为没有)
            Boolean ZhSh_bool = true;//是否含有整数,默认有(0则视为没有)

            /// <summary>
            /// 金额小写转中文大写。
            /// 整数支持到万亿；小数部分支持到分(超过两位将进行Banker舍入法处理)
            /// </summary>
            /// <param name="Num">需要转换的双精度浮点数</param>
            /// <returns>转换后的字符串</returns>
            public string NumGetStr(decimal Num)
            {
                string NumStr;//整个数字字符串
                string NumStr_Zh;//整数部分
                string NumSr_X = "";//小数部分
                string NumStr_DQ;//当前的数字字符
                string NumStr_R = "";//返回的字符串

                Num = Math.Round(Num, 2);//四舍五入取两位

                //各种非正常情况处理
                if (Num < 0)
                    return "不转换欠条";
                if (Num > 9999999999999)
                    return "很难想象谁会有这么多钱！";
                if (Num == 0)
                    return Ls_ShZ[0];

                //判断是否有整数
                if (Num < 1)
                    ZhSh_bool = false;

                NumStr = Num.ToString();

                NumStr_Zh = NumStr;//默认只有整数部分
                if (NumStr_Zh.Contains("."))
                {//分开整数与小数处理
                    NumStr_Zh = NumStr.Substring(0, NumStr.IndexOf("."));
                    NumSr_X = NumStr.Substring((NumStr.IndexOf(".") + 1), (NumStr.Length - NumStr.IndexOf(".") - 1));
                }


                if (NumSr_X == "" || int.Parse(NumSr_X) <= 0)
                {//判断是否含有小数部分
                    XSh_bool = true;
                }

                if (ZhSh_bool)
                {//整数部分处理
                    NumStr_Zh = Reversion_Str(NumStr_Zh);//反转字符串

                    for (int a = 0; a < NumStr_Zh.Length; a++)
                    {//整数部分转换
                        NumStr_DQ = NumStr_Zh.Substring(a, 1);
                        if (int.Parse(NumStr_DQ) != 0)
                            NumStr_R = Ls_ShZ[int.Parse(NumStr_DQ)] + Ls_DW_Zh[a] + NumStr_R;
                        else if (a == 0 || a == 4 || a == 8)
                        {
                            if (NumStr_Zh.Length > 8 && a == 4)
                                continue;
                            NumStr_R = Ls_DW_Zh[a] + NumStr_R;
                        }
                        else if (int.Parse(NumStr_Zh.Substring(a - 1, 1)) != 0)
                            NumStr_R = Ls_ShZ[int.Parse(NumStr_DQ)] + NumStr_R;

                    }

                    if (XSh_bool)
                        return NumStr_R + "整";

                    //NumStr_R += "零";
                }

                for (int b = 0; b < NumSr_X.Length; b++)
                {//小数部分转换
                    NumStr_DQ = NumSr_X.Substring(b, 1);
                    if (int.Parse(NumStr_DQ) != 0)
                        NumStr_R += Ls_ShZ[int.Parse(NumStr_DQ)] + Ls_DW_X[b];
                    else if (b != 1 && ZhSh_bool)
                        NumStr_R += Ls_ShZ[int.Parse(NumStr_DQ)];
                }

                return NumStr_R;

            }

            /// <summary>
            /// 数字转大写(可以选择带单位或不带单位)
            /// </summary>
            /// <param name="NumStr">需要转换的数字字符串</param>
            /// <param name="Dw">是否带单位</param>
            /// <returns>转换后的字符串</returns>
            public String LowercaseGetCap(String NumStr, Boolean Dw)
            {//其实和上面的转换一样的，只是所取的单位不同而已。偶试了试还是这个方法转换直接易懂，至于效率我就没有测试了
                String CapStr = "";
                String NumStr_LS;

                if (NumStr == String.Empty)
                    return String.Empty;

                if (Dw)
                    NumStr = this.Reversion_Str(NumStr);//如果需要带单位，则先反转字符串

                try
                {
                    for (Int32 c = 0; c < NumStr.Length; c++)
                    {
                        NumStr_LS = NumStr.Substring(c, 1);
                        if (Dw)
                        {
                            if (int.Parse(NumStr_LS) != 0)
                                CapStr = Ls_ShZ[int.Parse(NumStr_LS)] + Num_DW[c] + CapStr;
                            else if (c == 0 || c == 4 || c == 8)
                            {
                                if (NumStr_LS.Length > 8 && c == 4)
                                    continue;
                                CapStr = Num_DW[c] + CapStr;
                            }
                            else if (int.Parse(NumStr.Substring(c - 1, 1)) != 0)
                                CapStr = Ls_ShZ[int.Parse(NumStr_LS)] + CapStr;
                        }
                        else
                            CapStr += Ls_ShZ[int.Parse(NumStr_LS)];
                    }

                    return CapStr;
                }
                catch (Exception Err)
                {
                    return "转换错误！" + Err.Message;
                }
            }

            /// <summary>
            /// 反转字符串
            /// </summary>
            /// <param name="Rstr">需要反转的字符串</param>
            /// <returns>反转后的字符串</returns>
            private String Reversion_Str(String Rstr)
            {
                Char[] LS_Str = Rstr.ToCharArray();
                Array.Reverse(LS_Str);
                String ReturnSte = "";
                ReturnSte = new String(LS_Str);//反转字符串

                return ReturnSte;
            }
        }
    }