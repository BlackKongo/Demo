using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using Honsoft.MessageQueue;


namespace MyFile
{
    /// <summary>
    /// 文件上传
    /// </summary>
    /// <remarks>
    /// 郭子豪
    /// </remarks>
    class FileUpLoad
    {
        FileOperation file = new FileOperation();
        #region 文件上传
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="path">路径</param>
        public bool FileToUpLoad(string path,TextBox textBox,string id,string fileClass)
        {
            
            try {
                if (!File.Exists(path))
                {
                    throw new Exception("此路径下文件不存在"+ Environment.NewLine);
                }
                
                FileInfo fi = new FileInfo(path);
                                      
                textBox.Invoke(new Action(()=> {//调用texBox所在的线程
                    int len=textBox.Text.Length;
                    if (len == 3000) {
                        textBox.Clear();
                    }
                    textBox.AppendText("开始发送文件" + fi.Name + Environment.NewLine);
                }));

                FileStream fs = fi.OpenRead();

                long FileLength = fs.Length;//文件总长度

                int bufferSize = 5120 * 100;

                byte[] buff = new byte[bufferSize];
                
                int state = 1;//开始
                int rNum = 0;//传输大小
                long FileStart = 0;

                while (FileLength > 0) {
                    fs.Position = FileStart;

                    if (FileLength <= bufferSize)
                    {
                        buff = new byte[FileLength];
                        rNum = fs.Read(buff, 0, Convert.ToInt32(FileLength));
                        state = 0;//传输结束
                    }
                    else {
                        rNum = fs.Read(buff, 0, buff.Length);

                    }
                    if (rNum == 0) {
                        break;
                    }

                    FileStart += rNum;
                    FileLength -= rNum;
                    string data = Convert.ToBase64String(buff);

                    Thread.Sleep(60);
                    file.FileUpLoad(fi.Name,data,state,null,fileClass);
                    state = 2;//传输中
                }

            fs.Close();
                textBox.Invoke(new Action(() => {
                    textBox.AppendText(fi.Name + "上传成功" + Environment.NewLine);
                }));
                return true;
            }
            catch (Exception ex)
            {
                textBox.Invoke(new Action(() => {
                    textBox.AppendText(ex.Message + Environment.NewLine);
                }));
                return false;
            }
            
        }

        #endregion

        #region BASE64字符串写入文件
        /// <summary>
        /// BASE64字符串写入文件
        /// </summary>
        /// <param name="msg"></param>
        public static void ByteToFile(string msg) {

            Dictionary<string, string> dic2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(msg);
            byte[] data = Convert.FromBase64String(dic2["data"]);
            string savePath = dic2["savePath"];

            FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fileStream);
            bw.Seek(0, SeekOrigin.End);
            bw.Write(data, 0, data.Length);
            bw.Close();
            fileStream.Close();
        }
        #endregion

        #region 获取MD5
        /// <summary>
        /// 获取md5
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string GetFileMD5(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bufferSize = 1048576;
            byte[] buff = new byte[bufferSize];
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.Initialize();
            long offset = 0;
            while (offset < fs.Length)
            {
                long readSize = bufferSize;
                if (offset + readSize > fs.Length)
                    readSize = fs.Length - offset;
                fs.Read(buff, 0, Convert.ToInt32(readSize));
                if (offset + readSize < fs.Length)
                    md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
                else
                    md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));
                offset += bufferSize;
            }
            if (offset >= fs.Length)
            {
                fs.Close();
                byte[] result = md5.Hash;
                md5.Clear();
                StringBuilder sb = new StringBuilder(32);
                for (int i = 0; i < result.Length; i++)
                    sb.Append(result[i].ToString("X2"));
                return sb.ToString();
            }
            else
            {
                fs.Close();
                return null;
            }
        }
        #endregion

        #region 判断文件状态
        /// <summary>
        /// 判断文件状态
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// 占用|没占用
        /// </returns>
        public static Boolean FileState(string path) {

            Boolean flag=false;
            FileStream fileStream = null;
            try
            {
                if (File.Exists(path))
                {
                    fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                }
            }
            catch(Exception ex)
            {
                flag = true;               
            }
            finally {
                if (null != fileStream) {
                    fileStream.Close();
                }
            }
            return flag;

        }
        #endregion
    }
}
