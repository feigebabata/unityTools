using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Unity日志工具 宏LOGER启用
/// </summary>
public static class Loger
{
    public static bool Enable = true;

    [Conditional("LOGER")]
    public static void d(Color _color, object _text, params object[] _vals)
    {
        if (!Enable)
        {
            return;
        }
        if (_vals != null && _vals.Length>0)
        {
            _text = string.Format(_text.ToString(), _vals);
        }
        UnityEngine.Debug.Log(string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(_color), _text));
    }

    [Conditional("LOGER")]
    public static void d(object _text, params object[] _vals)
    {
        if (!Enable)
        {
            return;
        }
        if (_vals == null || _vals.Length == 0)
        {
            UnityEngine.Debug.Log(_text);
        }
        else
        {
            UnityEngine.Debug.LogFormat(_text.ToString(), _vals);
        }
    }

    [Conditional("LOGER")]
    public static void w(object _text, params object[] _vals)
    {
        if (!Enable)
        {
            return;
        }
        if (_vals == null || _vals.Length == 0)
        {
            UnityEngine.Debug.LogWarning(_text);
        }
        else
        {
            UnityEngine.Debug.LogWarningFormat(_text.ToString(), _vals);
        }
    }

    [Conditional("LOGER")]
    public static void e(object _text, params object[] _vals)
    {
        if (!Enable)
        {
            return;
        }
        if (_vals == null || _vals.Length == 0)
        {
            UnityEngine.Debug.LogError(_text);
        }
        else
        {
            UnityEngine.Debug.LogErrorFormat(_text.ToString(), _vals);
        }
    }
}
