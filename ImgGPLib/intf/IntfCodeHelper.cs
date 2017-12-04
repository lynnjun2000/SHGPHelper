using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImgGPLib.intf
{
    public interface IntfCodeHelper
    {
        /// <summary>
        /// 等比例放大位图
        /// </summary>
        /// <param name="sourceBmp"></param>
        /// <param name="rate">放大倍率</param>
        /// <returns></returns>
        Bitmap BmpScale(Bitmap sourceBmp, int rate);

        Bitmap PreProcessBmp(Bitmap sourceBmp, bool removeNoise, bool drawGrid);
        /// <summary>
        /// 对验证码进行预处理，灰度化，单色化
        /// </summary>
        /// <param name="sourceBmp">原始验证码</param>
        /// <param name="removeNoise">是否消除噪点</param>
        /// <returns></returns>
        Bitmap PreProcessBmp(Bitmap sourceBmp, bool removeNoise);

        /// <summary>
        /// 消除噪点
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Bitmap RemoveNoise(Bitmap source);
        Bitmap MakeBMPGray(Bitmap source);
        Bitmap MakeBMPSingle(Bitmap source,int flagvalue);

        /// <summary>
        /// 图片上打格子的像数大小
        /// </summary>
        /// <returns></returns>
        int GetBmpGridSize();

    }
}
