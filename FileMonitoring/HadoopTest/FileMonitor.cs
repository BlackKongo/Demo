using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MyFile
{
    /// <summary>
    /// 文件夹监控
    /// </summary>
    /// <remarks>
    /// 郭子豪
    /// </remarks>
    class FileMonitor
    {
        public static List<string> pathList = new List<string>();//存放变动文件的路径

        protected static FileSystemWatcher[] watcherArr=null;

        #region 开始监控
        /// <summary>
        /// 启动监控
        /// </summary>
        /// <param name="path">监控路径</param>
        /// <param name="filter">文件格式（*.*）</param>
        public static void WatcherStart(List<string> path, string filter) {
            watcherArr = new FileSystemWatcher[path.Count()];
            for (int i = 0; i< path.Count(); i++) {
                watcherArr[i] = new FileSystemWatcher();
                watcherArr[i].Path = path[i];
                watcherArr[i].Filter = "*.*";
                watcherArr[i].Created += new FileSystemEventHandler(OnProcess);//文件新建
                watcherArr[i].Changed += new FileSystemEventHandler(OnProcess);
                watcherArr[i].Deleted += new FileSystemEventHandler(OnProcess);
                watcherArr[i].NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;//设置类型
                watcherArr[i].EnableRaisingEvents = true;//启动
            }
        }
        #endregion

        #region 停止监控
        public static void WatcherStop() {
            for (int i = 0; i < watcherArr.Count(); i++) {
                if (watcherArr[i] != null)
                {
                    watcherArr[i].EnableRaisingEvents = false;//关闭
                    watcherArr[i].Created -= new FileSystemEventHandler(OnProcess);
                    watcherArr[i].Changed -= new FileSystemEventHandler(OnProcess);
                    watcherArr[i].Deleted -= new FileSystemEventHandler(OnProcess);
                }
            }

        }
        #endregion

        #region 调用方法
        public static void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                OnCreated(source, e);
            }
            //else if (e.ChangeType == WatcherChangeTypes.Changed)
            //{
            //    OnChanged(source, e);
            //}
            //else if (e.ChangeType == WatcherChangeTypes.Deleted)
            //{
            //    OnDeleted(source, e);
            //}

        }

         //<add key = "path2" value="E:\tmp"/>
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("文件新建事件处理逻辑 {0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
            pathList.Add(e.FullPath);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("文件改变事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("文件删除事件处理逻辑{0}  {1}   {2}", e.ChangeType, e.FullPath, e.Name);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine("文件重命名事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
        }
        #endregion
    }
}
