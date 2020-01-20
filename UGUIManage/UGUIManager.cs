using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiuDanUnity;

public class UGUIManager : IManager 
{
	public static class Config
	{
		/// <summary>
		/// 隐藏界面时的位置
		/// </summary>
		/// <returns></returns>
		public static readonly Vector3 HidePos = new Vector3(3000,0,0);
	}

	/// <summary>
	/// ui界面栈记录
	/// </summary>
	/// <returns></returns>
	UIStack m_uiStack = new UIStack();

	/// <summary>
	/// ui界面回收池
	/// </summary>
	/// <returns></returns>
	UIPool m_uiPool = new UIPool(5);  

	/// <summary>
	/// ui界面父节点
	/// </summary>      
	Transform m_panelParent;

	/// <summary>
	/// ui界面ab文件 加载后不卸载 只在Clear的时候卸载
	/// </summary>
	AssetBundle m_panelAB;

	/// <summary>
	/// 清空所有ui界面
	/// </summary>
	public void Clear()
	{
		Loger.d(Color.green,"[UGUIManager.Clear]");

		while(m_uiStack.Peek())
		{
			var ui = m_uiStack.Pop();
			destroy(ui);
		}

		if(m_panelAB)
		{
			m_panelAB.Unload(true);
			m_panelAB=null;
		}
		m_uiPool.Clear();
		m_uiStack.Clear();
		destroyAll();
		m_panelParent=null;
		Resources.UnloadUnusedAssets();
	}
	
	public void Update()
	{
		//监听系统返回键
		if(Input.GetKeyDown(KeyCode.Escape) && m_uiStack.Count>0)
		{
			m_uiStack.Peek().OnClickBack();
		}
	}

	/// <summary>
	/// 关闭当前界面
	/// </summary>
	public void CloseCurUI()
	{
		destroy(m_uiStack.Pop());
		if(m_uiStack.Count>0)
		{
			show(m_uiStack.Peek(),null);
		}
	}

	public void Open<T>(OpenMode _mode=OpenMode.Open,object _data=null) where T : UIBase
	{
		if(!m_panelParent)
		{
			if(!GameObject.Find("UIRoot/Canvas/Panels"))
			{
				GameObject uiRoot = loadPrefab("UIRoot");
				uiRoot.name="UIRoot";
				uiRoot.transform.position=Vector3.zero;
			}
			m_panelParent = GameObject.Find("UIRoot/Canvas/Panels").transform;
		}

		Type newUIType = typeof(T);
		if(m_uiStack.Count>0)
		{
			switch(_mode)
			{
				case OpenMode.Open:
				{
					if(m_uiStack.Peek().GetType()!=newUIType)
					{
						hide(m_uiStack.Peek());
						create(newUIType,_data);
					}
				}
				break;
				case OpenMode.Top:
				{
					if(m_uiStack.Peek().GetType()!=newUIType)
					{
						hide(m_uiStack.Peek());
						UIBase ui = m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;});
						if(ui)
						{
							m_uiStack.Remove(ui);
							m_uiStack.Push(ui);
							ui.m_State = State.Hide2Show;
							show(ui,_data);
						}
						else
						{
							create(newUIType,_data);
						}
					}
				}
				break;
				case OpenMode.Overlay:
				{
					create(newUIType,_data);
				}
				break;
				case OpenMode.Back:
				{
					if(m_uiStack.Peek().GetType()!=newUIType)
					{
						if(m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;})==null)
						{
							hide(m_uiStack.Peek());
							create(newUIType,_data);
						}
						else
						{
							while(m_uiStack.Peek().GetType()!=newUIType)
							{
								destroy(m_uiStack.Pop());
							}
							UIBase ui = m_uiStack.Peek();
							ui.m_State = State.Hide2Show;
							show(ui,_data);
						}
					}
				}
				break;
			}
		}
		else
		{
			create(newUIType,_data);
		}
	}

	/// <summary>
	/// 创建ui界面 优先从回收池中获取
	/// </summary>
	/// <param name="_uiType"></param>
	/// <param name="_data"></param>
	void create(Type _uiType,object _data)
	{
		UIBase ui = m_uiPool.Pull(_uiType);
		if(ui==null)
		{
			ui = loadUI(_uiType);
			ui.gameObject.name = _uiType.Name;
			ui.m_State = State.Create;
		}
		else
		{
			ui.m_State = State.Destroy2Create;
		}
		ui.gameObject.SetActive(true);
		ui.transform.localPosition = Config.HidePos;
		m_uiStack.Push(ui);
		ui.OnCreate();
		ui.m_State = State.Create2Show;
		show(ui,_data);
	}

	/// <summary>
	/// 隐藏界面 ui位置移出视图之外 活动状态仍未true
	/// </summary>
	/// <param name="_ui"></param>
	void hide(UIBase _ui)
	{
		if(_ui.m_State == State.Hide)
		{
			return;
		}
		_ui.m_State = State.Show2Hide;
		_ui.OnHide();
		_ui.m_State = State.Hide;
		_ui.transform.localPosition = Config.HidePos;
	}

	/// <summary>
	/// 移除界面 活动状态设为false 收入回收池
	/// </summary>
	/// <param name="_ui"></param>
	void destroy(UIBase _ui)
	{
		hide(_ui);
		_ui.m_State = State.Hide2Destory;
		_ui.OnDestroy();
		_ui.m_State = State.Destroy;
		_ui.gameObject.SetActive(false);
		m_uiPool.Push(_ui);

	}

	UIBase loadUI(Type _uiType)
	{        
		return loadPrefab(_uiType.Name).GetComponent<UIBase>();
	}

	/// <summary>
	/// 从ab文件中获取预制件的实例
	/// </summary>
	/// <param name="_name"></param>
	/// <returns></returns>
	GameObject loadPrefab(string _name)
	{
		GameObject go = null;
		#if UNITY_EDITOR
		if (AssetBundles.AssetBundleConfig.IsEditorMode)
		{
			go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/BasicAssets/TwoRoomPanel/"+_name+".prefab");
			go = GameObject.Instantiate(go,m_panelParent);
			return go;
		}
		#endif
		if(!m_panelAB)
		{
			string path = Application.streamingAssetsPath+"/AssetBundles/basicassets/tworoompanel.assetbundle";

			m_panelAB = AssetBundle.LoadFromFile(path);
		}
		go = m_panelAB.LoadAsset<GameObject>(_name+".prefab");
		go = GameObject.Instantiate(go,m_panelParent);
		return go;
	}

	/// <summary>
	/// 显示界面 ui界面移至视图内
	/// </summary>
	/// <param name="_ui"></param>
	/// <param name="_data"></param>
	void show(UIBase _ui,object _data)
	{
		if(_ui.m_State==State.Show)
		{
			return;
		}
		_ui.transform.SetSiblingIndex(m_panelParent.childCount-1);
		_ui.transform.localPosition = Vector3.zero;
		_ui.OnShow(_data);
		_ui.m_State = State.Show;
	}

	/// <summary>
	/// 删除所有ui界面的Gameobject
	/// </summary>
	void destroyAll()
	{
		if(m_panelParent)
		{
			var desArr = new GameObject[m_panelParent.childCount];
			for (int i = 0; i < m_panelParent.childCount; i++)
			{
				desArr[i] = m_panelParent.GetChild(i).gameObject;
			}
			for (int i = 0; i < desArr.Length; i++)
			{
				GameObject.Destroy(desArr[i]);
			}
		}
	}

    public void Init()
    {

    }

    public void Reset()
    {
        
    }

	public GameObject GetUIPrefab(string _name)
	{
		return loadPrefab(_name);
	}

	/// <summary>
	/// ui界面基类 子类名需要和UI预制件名称相同
	/// </summary>
    public class UIBase : MonoBehaviour
	{
		[HideInInspector]
		public State m_State;

		/// <summary>
		/// 系统返回键和UI返回键的响应 按需求在子类中可自定义响应逻辑
		/// </summary>
		public virtual void OnClickBack()
		{
			InstanceManager.Manager<UGUIManager>().CloseCurUI();
		}

		public virtual void OnCreate()
		{

		}

		public virtual void OnShow(object _data)
		{
			
		}

		public virtual void OnHide()
		{
			
		}

		public virtual void OnDestroy()
		{
			
		}
	}

	/// <summary>
	/// 界面当前状态
	/// </summary>
	public enum State
	{
		Create,
		Create2Show,
		Show,
		Show2Hide,
		Hide,
		Hide2Show,
		Hide2Destory,
		Destroy,
		Destroy2Create,
	}

	public enum OpenMode
	{
		/// <summary>
		/// 打开界面在栈顶则忽略 否则新建界面并隐藏栈顶界面 默认打开模式
		/// </summary>
		Open,

		/// <summary>
		/// 新建界面不隐藏栈顶界面 常用于弹窗之类
		/// </summary>
		Overlay,

		/// <summary>
		/// 打开界面在栈顶则忽略 栈中无则新建否则弹栈至打开界面
		/// </summary>
		Back,

		/// <summary>
		/// 打开界面在栈顶则忽略 栈中无则新建否则将打开界面移至栈顶
		/// </summary>
		Top,
	}

	public class UIPool
	{
		public int MaxCount;
		List<UIBase> m_uiList = new List<UIBase>();
        private int v;

        public UIPool(int v)
        {
            this.MaxCount = v;
        }

        public void Push(UIBase _ui)
		{
			m_uiList.Add(_ui);
		}

		public UIBase Pull(Type type)
		{
			var ui = m_uiList.Find((_ui)=>{return _ui.GetType()==type;});
			if(ui)
			{
				m_uiList.Remove(ui);
			}
			return ui;
		}
		
		public int Count
		{
			get
			{
				return m_uiList.Count;
			}
		}

		public void Clear()
		{
			m_uiList.Clear();
		}
	}

	public class UIStack
	{
		List<UIBase> m_uiList = new List<UIBase>();
		
		public int Count
		{
			get
			{
				return m_uiList.Count;
			}
		}

		public void Clear()
		{
			m_uiList.Clear();
		}

		public void Push(UIBase _ui)
		{
			// Debug.Log("pop "+_ui.GetType());
			m_uiList.Insert(0,_ui);
		}

		public UIBase Peek()
		{
			if(Count>0)
			{
				return m_uiList[0];
			}
			return null;
		}

		public UIBase Pop()
		{
			if(Count>0)
			{
				var ui = m_uiList[0];
				m_uiList.RemoveAt(0);
				// Debug.Log("pop "+ui.GetType());
				return ui;
			}
			return null;
		}

		public bool Remove(UIBase _ui)
		{
			return m_uiList.Remove(_ui);
		}

		public UIBase Find(Predicate<UIBase> _match)
		{
			return m_uiList.Find(_match);
		}
	}
}

