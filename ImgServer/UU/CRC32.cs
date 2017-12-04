using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ImgServer.UU
{
    class CRC32
    {
        protected ulong[] crc32Table;
        /**/
        /// <summary>
        /// 构造：初始化效验表
        /// </summary>
        public CRC32()
        {
            const ulong ulPolynomial = 0xEDB88320;
            ulong dwCrc;
            crc32Table = new ulong[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                dwCrc = (ulong)i;
                for (j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) == 1)
                        dwCrc = (dwCrc >> 1) ^ ulPolynomial;
                    else
                        dwCrc >>= 1;
                }
                crc32Table[i] = dwCrc;
            }
        }
        /**/
        /// <summary>
        /// 字节数组效验
        /// </summary>
        /// <param name="buffer">ref 字节数组</param>
        /// <returns></returns>
        public ulong ByteCRC(ref byte[] buffer)
        {
            ulong ulCRC = 0xffffffff;
            ulong len;
            len = (ulong)buffer.Length;
            for (ulong buffptr = 0; buffptr < len; buffptr++)
            {
                ulong tabPtr = ulCRC & 0xFF;
                tabPtr = tabPtr ^ buffer[buffptr];
                ulCRC = ulCRC >> 8;
                ulCRC = ulCRC ^ crc32Table[tabPtr];
            }
            return ulCRC ^ 0xffffffff;
        }
        /**/
        /// <summary>
        /// 字符串效验
        /// </summary>
        /// <param name="sInputString">字符串</param>
        /// <returns></returns>
        public ulong StringCRC(string sInputString)
        {
            byte[] buffer = Encoding.Default.GetBytes(sInputString);
            return ByteCRC(ref buffer);
        }
        /**/
        /// <summary>
        /// 文件效验
        /// </summary>
        /// <param name="sInputFilename">输入文件</param>
        /// <returns></returns>
        public ulong FileCRC(string sInputFilename)
        {
            FileStream inFile = new System.IO.FileStream(sInputFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bInput = new byte[inFile.Length];
            inFile.Read(bInput, 0, bInput.Length);
            inFile.Close();
            return ByteCRC(ref bInput);
        }
    }
}
