
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Honsoft.MessageQueue;
namespace MyFile
{
    class FileHelper
    {
        /// <summary>
        /// 获取指定位置的文字
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> TextExtraction(string path)
        {                     
            List<string> list = new List<string>();
            FileInfo info = new FileInfo(path);
            if (info != null) {
                list.Add(info.Name);//添加文件名
            }
            string[] contextArr = File.ReadAllLines(path, Encoding.GetEncoding("gb2312"));
            list.Add(contextArr[1]);//添加条码
            string barCode = contextArr[1];
            for (int i = 13; i < contextArr.Length; i++)//添加不良图片位置
            {
                string context = contextArr[i];
                context = context.Split(';')[5];
                list.Add(context);
            }

            return list;
        }

        public static void FileDownload(string fileFullName,string localpath,string dirName) {
            FileOperation fs = new FileOperation();
            fs.FileDownLoad(fileFullName, localpath,dirName);
        }

        /// <summary>
        /// 字符串分割
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] splitStr(string str) {
            int i = str.LastIndexOf('/');
            string[] strArr = new string[] {
                str.Substring(0,i),
                str.Substring(i+1)
            };
            return strArr;
        }



    }
}