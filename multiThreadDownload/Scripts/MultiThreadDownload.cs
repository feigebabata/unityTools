using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using AssetBundles;

namespace XiuDanUnity
{
    public class MultiThreadDownload : MonoSingleton<MultiThreadDownload>
    {
        public static class Config
        {
            public const int MAX_DOWNLOAD_COUNT=5;
            public const string TEMP_EXTENSION = ".dl";
        }

        private System.Object m_lock_obj = new System.Object();
        private Dictionary<string,DownloadUnit> m_downloading = new Dictionary<string, DownloadUnit>();
        private Dictionary<WebClient,string> m_client2Keys = new Dictionary<WebClient, string>();
        private List<DownloadUnit> m_downloadQueue = new List<DownloadUnit>();
        private List<string> m_removeKeys = new List<string>();

        public void Download(string _url,string _savePath,Action<bool> _callback)
        {
            Loger.d("[MultiThreadDownload.Download]:{0}\n{1}\n{2}",_url,_savePath,_callback);

            if(string.IsNullOrEmpty(_url))
            {
                err("[MultiThreadDownload.Download]下载地址不能为空:{0}",_url);
                _callback(false);
                return;
            }
            if(string.IsNullOrEmpty(_savePath))
            {
                err("[MultiThreadDownload.Download]存储地址不能为空:{0}",_savePath);
                _callback(false);
                return;
            }

            lock(m_lock_obj)
            {
                string key = getKey(_url,_savePath);
                if(m_downloading.ContainsKey(key))
                {
                    m_downloading[key].Callback+=_callback;
                }
                else
                {
			        var unit = m_downloadQueue.Find((_unit)=>{return _unit.Url == _url && _unit.SavePath == _savePath;});
                    if(unit!=null)
                    {
                        unit.Callback+=_callback;
                    }
                    else
                    {
                        DownloadUnit state = new DownloadUnit();
                        state.Url=_url;
                        state.SavePath=_savePath;
                        state.Callback+=_callback;
                        m_downloadQueue.Add(state);
                        downloadQueueReset();
                    }
                }
            }
            // UnityWebDownload.Instance.Download(_url,_savaPath,_callback);
        }

        public void Download(string _url,string _savaPath,string _md5,Action<bool> _callback)
        {
            Download(_url,_savaPath,(_isSucc)=>
            {
                if(_isSucc)
                {
                    string localFileMD5 = AssetBundleUtility.GetMD5HashFromFile(_savaPath);
                    if(_md5.Equals(localFileMD5))
                    {
                        _callback(true);
                    }
                    else
                    {
                        _callback(false);
                    }
                }
                else
                {
                    _callback(false);
                }
            });
        }

        public void Clear()
        {
            // throw new System.NotImplementedException();
        }

        void Awake()
        {
            // Loger.d("主线程:"+ System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        void Update()
        {
            lock (m_lock_obj)
            {
                if(m_downloading.Count>0)
                {
                    // Loger.d("下载状态更新 下载中:"+m_downloading.Count);
                    m_removeKeys.Clear();
                    foreach (var kv in m_downloading)
                    {
                        if(kv.Value.IsDone)
                        {
                            m_removeKeys.Add(kv.Key);
                        }
                    }
                    // Loger.d("下载状态更新 下载结束:"+m_removeKeys.Count);
                    for (int i = 0; i < m_removeKeys.Count; i++)
                    {
                        DownloadUnit state = m_downloading[m_removeKeys[i]];
                        
                        Loger.d("[MultiThreadDownload.Update]:{0} {1}",state.IsSucc,state.Url);
                        m_downloading.Remove(m_removeKeys[i]);
                        if(state.Callback!=null)
                        {
                            state.Callback(state.IsSucc);
                        }
                        downloadQueueReset();
                    }
                }
            }
        }

        void downloadQueueReset()
        {
            while(m_downloading.Count<Config.MAX_DOWNLOAD_COUNT && m_downloadQueue.Count>0)
            {
                DownloadUnit item = m_downloadQueue[0];
                m_downloadQueue.RemoveAt(0);
                string key = getKey(item.Url,item.SavePath);
                m_downloading[key]=item;
                downloadAsyn(item);
            }
        }

        void downloadAsyn(DownloadUnit _unit)
        {
            if(Directory.Exists(Path.GetDirectoryName(_unit.SavePath)))
            {
                if (File.Exists(_unit.SavePath))
                {
                    try
                    {
                        File.Delete(_unit.SavePath);
                    }
                    catch(Exception _e)
                    {
                        err("[MultiThreadDownload.downloadAsyn]:一个(存在)却删不掉的文件,{0}\n{1}",_e.Message,_unit.SavePath);
                        downloadingResult(false,null,getKey(_unit.Url,_unit.SavePath));
                        return;
                    }
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_unit.SavePath));
                }
                catch(Exception _e)
                {
                    err("[MultiThreadDownload.downloadAsyn]:创建文件夹失败,{0}\n{1}",_e.Message,_unit.SavePath);
                    downloadingResult(false,null,getKey(_unit.Url,_unit.SavePath));
                    return;
                }
            }
            WebClient client = new WebClient();
            m_client2Keys[client] = getKey( _unit.Url,_unit.SavePath);
            client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(downloadFileCompleted);
            try
            {
                client.DownloadFileAsync(new Uri(_unit.Url),_unit.SavePath+Config.TEMP_EXTENSION);
            }
            catch (System.Exception _e)
            {
                err(_e.Message);
                downloadingResult(false,client);
            }
        }

        private void downloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Loger.d("线程:"+ System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            bool isSucc = e.Error==null;
            if(e.Error!=null)
            {
                err(e.Error.Message);
            }
            WebClient client = sender as WebClient;
            downloadingResult(isSucc,client);
        }

        void downloadingResult(bool _isSucc,WebClient _client,string _key=null)
        {
            if(_client==null)
            {
                Loger.d("[MultiThreadDownload.downloadingResult]:{0} {1}",_isSucc,m_downloading[_key].Url);
                m_downloading[_key].IsSucc=_isSucc;
                m_downloading[_key].IsDone=true;
            }
            else
            {
                lock (m_lock_obj)
                {
                    string key = m_client2Keys[_client];
                    Loger.d("[MultiThreadDownload.downloadingResult]:{0} {1}",_isSucc,m_downloading[key].Url);
                    m_client2Keys.Remove(_client);
                    m_downloading[key].IsSucc=_isSucc;
                    m_downloading[key].IsDone=true;
                    try
                    {
                        FileInfo info = new FileInfo(m_downloading[key].SavePath+Config.TEMP_EXTENSION);
                        info.MoveTo(m_downloading[key].SavePath);
                    }
                    catch(Exception _e)
                    {
                        Loger.e("[MultiThreadDownload.downloadingResult]:{0} {1}",m_downloading[key].SavePath,_e);
                    }
                }
                _client.Dispose();
            }
        }

        void err(string _e,params object[] _objs)
        {
            Loger.e(_e,_objs);
        }

        string getKey(string _url,string _savePath)
        {
            return string.Format("{0}\n{1}",_url,_savePath);
        }
    

        class DownloadUnit
        {
            public string Url;
            public string SavePath;
            public Action<bool> Callback;
            public bool IsDone;
            public bool IsSucc;
        }

    }
}
