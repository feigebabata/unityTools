using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

public class MultiThreadDownload : MonoSingleton<MultiThreadDownload>
{
    private System.Object m_lock_obj = new System.Object();
    private Dictionary<string,DownloadUnit> m_downloading = new Dictionary<string, DownloadUnit>();
    private Dictionary<WebClient,string> m_clientUrls = new Dictionary<WebClient, string>();
    private Queue<DownloadUnit> m_downloadQueue = new Queue<DownloadUnit>();
    private List<string> m_removeKeys = new List<string>();
    const int MAX_DOWNLOAD_COUNT=5;

    public void Download(string _url,string _savaPath,Action<bool> _callback)
    {
        lock(m_lock_obj)
        {
            string key = _url+_savaPath;
            if(m_downloading.ContainsKey(key))
            {
                m_downloading[key].Callback+=_callback;
            }
            else
            {
                DownloadUnit state = new DownloadUnit();
                state.Url=_url;
                state.SavaPath=_savaPath;
                state.Callback=_callback;
                m_downloadQueue.Enqueue(state);
                downloadQueueReset();
            }
        }
    }

    public void Download(string _url,string _savaPath,string _md5,Action<bool> _callback)
    {
        Download(_url,_savaPath,(_isSucc)=>
        {
            if(_isSucc)
            {
                string localFileMD5 = MD5Compare.GetInstance().GetMD5HashFromFile(_savaPath);
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
                m_removeKeys.Clear();
                foreach (var kv in m_downloading)
                {
                    if(kv.Value.IsDone)
                    {
                        m_removeKeys.Add(kv.Key);
                    }
                }
                for (int i = 0; i < m_removeKeys.Count; i++)
                {
                    DownloadUnit state = m_downloading[m_removeKeys[i]];
                    m_downloading.Remove(m_removeKeys[i]);
                    state.Callback(state.IsSucc);
                    downloadQueueReset();
                }
            }
        }
    }

    void downloadQueueReset()
    {
        if(m_downloading.Count<MAX_DOWNLOAD_COUNT && m_downloadQueue.Count>0)
        {
            DownloadUnit item = m_downloadQueue.Dequeue();
            string key = item.Url+item.SavaPath;
            m_downloading[key]=item;
            downloadAsyn(item);
        }
    }

    void downloadAsyn(DownloadUnit _state)
    {
        if(Directory.Exists(Path.GetDirectoryName(_state.SavaPath)))
        {
            if (File.Exists(_state.SavaPath))
            {
                File.Delete(_state.SavaPath);
            }
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_state.SavaPath));
        }
        WebClient client = new WebClient();
        m_clientUrls[client] = _state.Url+_state.SavaPath;
        client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(downloadFileCompleted);
        try
        {
            client.DownloadFileAsync(new Uri(_state.Url),_state.SavaPath);
        }
        catch (System.Exception _e)
        {
            err(_e);
            downloadingResult(false,client);
        }
    }

    private void downloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        // Loger.d("线程:"+ System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
        bool isSucc = e.Error==null;
        if(e.Error!=null)
        {
            err(e.Error);
        }
        WebClient client = sender as WebClient;
        downloadingResult(isSucc,client);
    }

    void downloadingResult(bool _isSucc,WebClient _client)
    {
        string key = m_clientUrls[_client];
        lock (m_lock_obj)
        {
            m_clientUrls.Remove(_client);
            m_downloading[key].IsSucc=_isSucc;
            m_downloading[key].IsDone=true;
        }
        _client.Dispose();
    }

    void err(Exception _e)
    {
        Loger.e(_e.Message);
    }

    class DownloadUnit
    {
        public string Url;
        public string SavaPath;
        public Action<bool> Callback;
        public bool IsDone;
        public bool IsSucc;
    }

}
