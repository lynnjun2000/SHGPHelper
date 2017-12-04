using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;

namespace ImgServer.UU
{
    public class UUCodeClient
    {
        public UUCodeClient()
        {
            StringBuilder info = new StringBuilder();
            bool succ = Init(ref info);
            if (!succ) throw new Exception(info.ToString());
            succ = Login(ref info);
            if (!succ) throw new Exception(info.ToString());
        }

        private bool Init(ref StringBuilder info)
        {
            string strSoftID = uuCode.Default.SoftID.Trim();
            int softId = int.Parse(strSoftID);
            string softKey = uuCode.Default.SoftKey1.Trim();
            Guid guid = Guid.NewGuid();
            string strGuid = guid.ToString().Replace("-", "").Substring(0, 32).ToUpper();
            string DLLPath = System.Environment.CurrentDirectory + "\\" + UUCodeWrapper.UUDLLName;
            //string DLLPath = "E:\\work\\UUWiseHelper 新版http协议\\输出目录\\UUWiseHelper.dll";
            string strDllMd5 = GetFileMD5(DLLPath);
            CRC32 objCrc32 = new CRC32();
            string strDllCrc = String.Format("{0:X}", objCrc32.FileCRC(DLLPath));
            //CRC不足8位，则前面补0，补足8位
            int crcLen = strDllCrc.Length;
            if (crcLen < 8)
            {
                int miss = 8 - crcLen;
                for (int i = 0; i < miss; ++i)
                {
                    strDllCrc = "0" + strDllCrc;
                }
            }
            //下面是软件id对应的dll校验key。在开发者后台-我的软件里面可以查的到。
            string strCheckKey = uuCode.Default.CheckKey.Trim().ToUpper();
            string yuanshiInfo = strSoftID + strCheckKey + strGuid + strDllMd5.ToUpper() + strDllCrc.ToUpper();
            info.Append(yuanshiInfo);
            //richTextBox1.Text += yuanshiInfo + "\n";
            string localInfo = MD5Encoding(yuanshiInfo);
            StringBuilder checkResult = new StringBuilder();
            UUCodeWrapper.uu_CheckApiSign(softId, softKey, strGuid, strDllMd5, strDllCrc, checkResult);
            string strCheckResult = checkResult.ToString();
            if (localInfo.Equals(strCheckResult))
            {
                info.Append("Dll校验成功！");
                return true;
            }
            else
            {
                info.Append("Dll校验失败！服务器返回信息为" + strCheckResult + "本地校验信息为" + localInfo + "\n");
                return false;
            }
        }

        /// <summary>
        /// 获取文件MD5校验值
        /// </summary>
        /// <param name="filePath">校验文件路径</param>
        /// <returns>MD5校验字符串</returns>
        private string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] md5byte = md5.ComputeHash(fs);
            int i, j;
            StringBuilder sb = new StringBuilder(16);
            foreach (byte b in md5byte)
            {
                i = Convert.ToInt32(b);
                j = i >> 4;
                sb.Append(Convert.ToString(j, 16));
                j = ((i << 4) & 0x00ff) >> 4;
                sb.Append(Convert.ToString(j, 16));
            }
            return sb.ToString();
        }

        /// <summary>
        /// MD5 加密字符串
        /// </summary>
        /// <param name="rawPass">源字符串</param>
        /// <returns>加密后字符串</returns>
        private static string MD5Encoding(string rawPass)
        {
            // 创建MD5类的默认实例：MD5CryptoServiceProvider
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                // 以十六进制格式格式化
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private bool Login(ref StringBuilder info)
        {
            bool isLogin;
            /*	优优云DLL 文件MD5值校验
	         *  用处：近期有不法份子采用替换优优云官方dll文件的方式，极大的破坏了开发者的利益
	         *  用户使用替换过的DLL打码，导致开发者分成变成别人的，利益受损，
	         *  所以建议所有开发者在软件里面增加校验官方MD5值的函数
	         *  如何获取文件的MD5值，通过下面的GetFileMD5(文件)函数即返回文件MD5
	         */

            string DLLPath = System.Environment.CurrentDirectory + "\\" + UUCodeWrapper.UUDLLName;
            string Md5 = GetFileMD5(DLLPath);
            //string AuthMD5 = "79dd7e248b7ec70e2ececa19b51c39c6";//作者在编写软件时内置的比对用DLLMD5值，不一致时将禁止登录,具体需要各位自己先获取使用的DLL的MD5验证字符串
            // if (Md5 != AuthMD5)
            //{
            //    MessageBox.Show("此软件使用的是UU云1.1.0.9动态链接库版DLL，与您目前软件内DLL版本不符，请前往http://www.uuwise.com下载更换此版本DLL");
            //     return;
            // }

            string u = uuCode.Default.Uid.Trim();
            string p = uuCode.Default.Pwd.Trim();
            int res = UUCodeWrapper.uu_login(u, p);
            isLogin = res > 0;
            info.Append("登录返回结果:" + res.ToString() + "," + (isLogin ? "登陆成功" : "登录失败"));
            return isLogin;
            //DelegateSetRtbText("登录返回结果:" + res.ToString() + "," + (isLogin ? "登陆成功" : "登录失败"));
        }

        public int GetLeftScore()
        {
            return UUCodeWrapper.uu_getScore(uuCode.Default.Uid.Trim(),uuCode.Default.Pwd.Trim());
        }

        public bool GetCode(Image codeImg,out string realCode)
        {
            string strCheckKey = uuCode.Default.CheckKey.Trim();
            MemoryStream ms = new MemoryStream();
            try
            {
                codeImg.Save(ms, ImageFormat.Bmp);
                byte[] buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Flush();
                //新版本dll需要预先分配50个字节的空间，否则dll会崩溃！！！！
                StringBuilder res = new StringBuilder(50);
                int codeId = UUCodeWrapper.uu_recognizeByCodeTypeAndBytes(buffer, buffer.Length, uuCode.Default.CodeType, res);
                return CheckResult(res.ToString(),out realCode);
                //string resultCode = CheckResult(res.ToString(), Convert.ToInt32(uuCode.Default.SoftID.Trim()), codeId, strCheckKey);
                //return resultCode;
                //m_codeID = codeId;
                //DelegateSetRtbText(string.Format("Code ID:{0}, 识别结果:{1}", codeId, resultCode.ToString()));
            }
            finally
            {
                ms.Close();
                ms.Dispose();
            }
        }

        //private string CheckResult(string result, int softId, int codeId, string checkKey)
        private bool CheckResult(string result, out string codeInfo)
        {
            //对验证码结果进行校验，防止dll被替换
            if (string.IsNullOrEmpty(result))
            {
                codeInfo = "码不能识别";
                return false;
            }
            else
            {
                if (result[0] == '-')
                {
                    //服务器返回的是错误代码
                    codeInfo = result;
                    return false;
                }
                else
                {

                    string[] modelReult = result.Split('_');
                    //解析出服务器返回的校验结果
                    string strServerKey = modelReult[0];
                    //string strCodeResult = modelReult[1];
                    codeInfo = modelReult[1];
                    return true;
                    ////本地计算校验结果
                    //string localInfo = softId.ToString() + checkKey + codeId.ToString() + strCodeResult.ToUpper();
                    //string strLocalKey = MD5Encoding(localInfo).ToUpper();
                    ////相等则校验通过
                    //if (strServerKey.Equals(strLocalKey))
                    //    return strCodeResult;
                    //return "结果校验不正确";
                }
            }
        }

        private class CodeThreadInfo
        {
            public Image CodeImg;
            public AutoResetEvent WaitEvent;
            public String RealCode;
        }

        //private Dictionary<string, List<Thread>> _codeThreadMap = new Dictionary<string, List<Thread>>();
        public bool GetCodeByMutilThread(Image codeImg, int threadCount, out string realCode,out string cost)
        {
            //Guid guid = Guid.NewGuid();
            //string taskKey = guid.ToString();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<Thread> threadList = new List<Thread>();
            AutoResetEvent waitEvent = new AutoResetEvent(false);
            CodeThreadInfo threadParam = new CodeThreadInfo();
            threadParam.CodeImg = codeImg;
            threadParam.WaitEvent = waitEvent;
            for (int i = 0; i < threadCount; i++)
            {
                Thread codeThread = new Thread(new ParameterizedThreadStart(CodeThreadsFunc));
                threadList.Add(codeThread);
            }
            
            foreach (Thread codeThread in threadList)
            {
                codeThread.Start(threadParam);
            }
            if (waitEvent.WaitOne(1000 * 5))
            {
                realCode = threadParam.RealCode;
                sw.Stop();
                cost =(sw.ElapsedMilliseconds / (float)1000).ToString("F2");
                return true;
            }
            else
            {
                realCode = "Timeout";
                sw.Stop();
                cost = (sw.ElapsedMilliseconds / (float)1000).ToString("F2");
                return false;
            }

        }
        private void CodeThreadsFunc(object param)
        {
            CodeThreadInfo threadParamObj = (CodeThreadInfo)param;
            string realCode;
            if (GetCode(threadParamObj.CodeImg, out realCode))
            {
                threadParamObj.RealCode = realCode;
                threadParamObj.WaitEvent.Set();
            }
        }
    }
}
