using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;
using Honsoft.MessageQueue;

namespace MyFile
{
    
    public partial class Form1 : Form
    {
        private System.Threading.Timer timer = null;
        private string id=null;
        private SqlConnection sqlConn = null;
        private string type { get; set; }
        public Form1()
        {
            InitializeComponent();
            button2_Click(null,null);
        }
    

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                #region 从配置文件读取信息
                string path1 = null;
                string path2 = null;
                //string path = folderBrowserDialog1.SelectedPath;
                 path1 = ConfigurationManager.AppSettings["path1"];
                 path2 = ConfigurationManager.AppSettings["path2"];
                textBox2.Text = path1;
                textBox3.Text = path2;
                List<string> pathlist = new List<string>();  
                type = ConfigurationManager.AppSettings["type"].ToString();

                if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
                {
                    textBox1.AppendText("未设置监控文件夹" + Environment.NewLine);
                    return;
                }
                else {
                    if (!string.IsNullOrEmpty(path1)) {
                        pathlist.Add(path1);

                    }
                    if (!string.IsNullOrEmpty(path2))
                    {
                        pathlist.Add(path2);
                    }
                }

                if (string.IsNullOrEmpty(type)) {
                    textBox1.AppendText("未设置类型" + Environment.NewLine);
                }                
                 id = ConfigurationManager.AppSettings["id"];

                foreach(string str in pathlist) {
                    textBox1.AppendText("开始监控:" + str + Environment.NewLine);
                }

                #endregion


                FileMonitor.WatcherStart(pathlist, "*.*");//启动监控

                timer = new System.Threading.Timer(new TimerCallback(ThreadTimers), textBox1, 0, 5 * 1000);//每5秒钟调用一次

                this.button2.Enabled = false;
                button3.Enabled = true;
                
            }
            catch (Exception ex) {
                textBox1.AppendText(ex.Message+ Environment.NewLine);
            }
            
        }

        void ThreadTimers(object obj)
        {
            
            
            int initial = FileMonitor.pathList.Count;

            for (int i = initial - 1; i >= 0; i--)
            {
                if (!FileUpLoad.FileState(FileMonitor.pathList[i]))//判断文件是否被占用
                {
                    string path = FileMonitor.pathList[i];
                  
                    FileMonitor.pathList.Remove(FileMonitor.pathList[i]);


                    //异步操作
                    Task task = new Task(() =>
                   {
                       FileUpLoad fileUpLoad = new FileUpLoad();
                       fileUpLoad.FileToUpLoad(path,(TextBox)obj,id,type.ToUpper());
                       if (type.ToUpper() == "AOI")
                       {
                           List<string> list=FileHelper.TextExtraction(path);
                           sqlConn = Connect.getConn();
                           string sql = "insert into demo_table(bar_code,file_name,hdfs_file_path,photo_hdfs_path) values('{0}','{1}','{2}','{3}')";
                           string photo_list = "";
                           for (int s = 2; s < list.Count; s++) {
                               if (fileUpLoad.FileToUpLoad(list[s], (TextBox)obj, id, type.ToUpper())) {
                                   int position = list[s].LastIndexOf(@"\");
                                   photo_list = photo_list + "AOI/jpg/" + list[s].Substring(position + 1) + ";";
                               }
                               
                           }
                           sql = string.Format(sql, list[1], list[0], "AOI/txt/" + list[0], photo_list);
                           Connect.ExecuteNoQuery(sqlConn, sql);
                       }
                       Console.WriteLine(path);
                   });

                    //启动
                    task.Start();

                }

            }
        }


        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
           this.button2.Enabled = true;
            FileMonitor.WatcherStop();
            this.button3.Enabled = false;
            
            textBox1.AppendText("监控停止" + Environment.NewLine);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)//当用户点击窗体右上角X按钮或(Alt + F4)时 发生          
            {
                e.Cancel = true;
                this.ShowInTaskbar = false;
                this.notifyIcon1.Icon = this.notifyIcon1.Icon;
                this.Hide();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition);
            }

            if (e.Button == MouseButtons.Left)
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
            }
        }

 

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("确认要退出吗？", "退出", messButton);
            if (dr == DialogResult.OK) {
                Application.Exit();
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string bar_code = textBox5.Text;
                string localPath = folderBrowserDialog1.SelectedPath;


                if (string.IsNullOrEmpty(bar_code))
                {
                    textBox1.AppendText("条码不能为空" + Environment.NewLine);
                    return;
                }

                if (string.IsNullOrEmpty(localPath))
                {
                    textBox1.AppendText("保存路径不能为空" + Environment.NewLine);
                    return;
                }

                if (comboBox1.Text == "AOI")
                {
                    AoiDownLoad(bar_code, localPath);
                }
                else
                {
                    string[] arr = FileHelper.splitStr(bar_code);
                    FileHelper.FileDownload(arr[1], localPath, arr[0]);
                }

                textBox1.Text = "下载完成";
            }
            catch (Exception ex) {
                textBox1.AppendText(ex.Message + Environment.NewLine);
            }
            

        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox4.Text = folderBrowserDialog1.SelectedPath;
        }


        public void AoiDownLoad(string bar_code,string localPath) {

              

                string sql = "select * from demo_table where bar_code='" + bar_code + "'";
                sqlConn = Connect.getConn();
                DataTable dt = Connect.ExecuteQuey(sqlConn, sql);

                if (dt.Rows.Count == 0)
                {
                    textBox1.AppendText("条码不存在" + Environment.NewLine);
                    return;
                }
                string filename = dt.Rows[0]["hdfs_file_path"].ToString();

                FileOperation file = new FileOperation();
                string[] indexArr = FileHelper.splitStr(filename);
                file.FileDownLoad(indexArr[1], localPath, indexArr[0]);




                string photo = dt.Rows[0]["photo_hdfs_path"].ToString();
                string[] photoList = photo.Split(';');
                for (int i = 0; i < photoList.Length - 1; i++)
                {
                    indexArr = FileHelper.splitStr(photoList[i]);
                    file.FileDownLoad(indexArr[1], localPath, indexArr[0]);
                }
            
        }
    }
}
