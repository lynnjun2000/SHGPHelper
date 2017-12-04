using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ImgServer.UU
{
    public class UUCodeWrapper
    {
        public const string UUDLLName = "UUWiseHelper_x64.dll";
        /// <summary>
        /// 校验dll
        /// </summary>
        /// <param name="softId">软件id</param>
        /// <param name="softKey">软件key</param>
        /// <param name="guid">随机guid串</param>
        /// <param name="fileMd5">dll文件的md5值</param>
        /// <param name="fileCrc">dll文件的crc值</param>
        /// <param name="checkResult">校验返回信息</param>
        [DllImport(UUDLLName)]
        public static extern void uu_CheckApiSign(int softId, string softKey, string guid, string fileMd5, string fileCrc, StringBuilder checkResult);
        /// <summary>
        /// 设置软件信息
        /// </summary>
        /// <param name="softId">软件id</param>
        /// <param name="softKey">软件的key</param>
        [DllImport(UUDLLName)]
        public static extern void uu_setSoftInfo(int softId, string softKey);

        /// <summary>
        /// 登陆系统
        /// </summary>
        /// <param name="u">用户名</param>
        /// <param name="p">密码</param>
        /// <returns>
        /// 大于0  正常登陆
        /// -1001 系统异常
        /// -1002 无法连接远程
        /// -1003 服务器配置错误
        /// -1 参数错误，用户名为空或密码为空
        /// -2 用户不存在
        /// -3 密码错误
        /// -4 账户被锁定
        /// -5 非法登录
        /// -6 用户点数不足，请及时充值
        /// -8 系统维护中
        /// -9 其他
        /// </returns>
        [DllImport(UUDLLName)]
        public static extern int uu_login(string u, string p);

        /// <summary>
        /// 注册用户 大于0表示成功
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_reguser(string u, string p, int softid, string softkey);

        /// <summary>
        /// 报告错误 大于等于0表示成功
        /// -1 参数错误或者不全
        //-2 未登录，KEY无效  
        //-3 表示软件KEY错误
        //-4 服务器出错
        //-7 软件代码倍禁用
        //-8 服务未启动
        /// </summary>
        /// <param name="codeid"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_reportError(int codeid);

        /// <summary>
        /// 获取用户余额
        /// 返回整形
        /// - 1 表示用户名或者密码为空
        /// -3 表示用户名密码错误
        /// -4 表示账户被锁定
        /// -6 表示余额不足
        /// -1001 系统异常
        /// -1002 无法连接远程
        /// </summary>
        /// <param name="codeid"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_getScore(string username, string password);

        /// <summary>
        ///  充值卡充值 大于0表示卡充值金额
        ///  -1 参数错误或者不全
        ///-2 软件id错误
        ///-3 SKEY无效
        ///-4 充值卡错误
        ///-5 PKEY错误
        ///-6 用户不存在 
        ///-7 软件代码为空，软件未注册
        ///-8 系统繁忙
        ///-9 服务器错误
        ///-101 充值卡号不存在
        ///-102 充值卡无效或者已使用
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="card">卡号</param>
        /// <param name="softId">软件id</param>
        /// <param name="softKey">软件通信key（开发者后台自行定义的）</param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_pay(string username, string card, int softId, string softKey);

        /// <summary>
        /// 根据文件路径和验证码类型传识别验证码 同时返回验证码的id
        /// </summary>
        /// <param name="path"></param>
        /// <param name="codeType"></param>
        /// <param name="codeid"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_recognizeByCodeTypeAndPath(string path, int codeType, StringBuilder result);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="picContent"></param>
        /// <param name="codeLength"></param>
        /// <param name="codeType"></param>
        /// <param name="codeId"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_recognizeByCodeTypeAndBytes(byte[] picContent, int codeLength, int codeType, StringBuilder result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picContent"></param>
        /// <param name="codeLength"></param>
        /// <param name="codeType"></param>
        /// <param name="codeId"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_recognizeByCodeTypeAndUrl(string url, string inCookie, int codeType, string cookieResult, StringBuilder result);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="picContent"></param>
        /// <param name="codeLength"></param>
        /// <param name="codeType"></param>
        /// <param name="codeId"></param>
        /// <returns></returns>
        [DllImport(UUDLLName)]
        public static extern int uu_SysCallOneParam(int repeatTime, int maxRepeat);



    }
}
