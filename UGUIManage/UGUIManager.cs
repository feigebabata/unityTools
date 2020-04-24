using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGUIManager 
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
	UIPool m_uiPool = new UIPool();  

	/// <summary>
	/// ui界面父节点
	/// </summary>      
	Transform m_panelParent;


	/// <summary>
	/// 清空所有ui界面
	/// </summary>
	public void Clear()
	{
		Loger.d("[UGUIManager.Clear]");
		while(m_uiStack.Peek())
		{
			var ui = m_uiStack.Pop();
			unfocus(ui);
			hide(ui);
			destroy(ui);
		}
		m_uiPool.Clear();
		m_uiStack.Clear();
		destroyAll();
		m_panelParent=null;
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
		if(m_uiStack.Count>0)
		{
			UIBase oldUI = m_uiStack.Pop();
			Loger.d("[UGUIManager.CloseCurUI] {0}",oldUI);
			unfocus(oldUI);
			hide(oldUI);
			destroy(oldUI);
			if(m_uiStack.Count>0)
			{
				UIBase newUI = m_uiStack.Peek();
				show(newUI,null);
				focus(newUI);
			}
		}
	}

	public void Open<T>(OpenMode _mode=OpenMode.Open,object _data=null) where T : UIBase
	{
		Type newUIType = typeof(T);
		Loger.d("[UGUIManager.Open] {0}",newUIType);
		if(!m_panelParent)
		{
			if(!GameObject.Find("UIRoot/Canvas/Panels"))
			{
				GameObject uiRoot = loadUIPanel("UIRoot");
				uiRoot.name="UIRoot";
				uiRoot.transform.position=Vector3.zero;
			}
			m_panelParent = GameObject.Find("UIRoot/Canvas/Panels").transform;
		}

		if(m_uiStack.Count>0)
		{
			UIBase oldUI,newUI=null;
			oldUI = m_uiStack.Peek();
			switch(_mode)
			{
				case OpenMode.Open:
				{
					if(oldUI.GetType()!=newUIType)
					{
						unfocus(oldUI);
						hide(oldUI);
						create(newUIType);
						newUI = m_uiStack.Peek();
						show(newUI,_data);
						focus(newUI);
					}
				}
				break;
				case OpenMode.Top:
				{
					if(oldUI.GetType()!=newUIType)
					{
						unfocus(oldUI);
						hide(oldUI);
						newUI = m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;});
						if(newUI)
						{
							m_uiStack.Remove(newUI);
							m_uiStack.Push(newUI);
						}
						else
						{
							create(newUIType);
							newUI = m_uiStack.Peek();
						}
						show(newUI,_data);
						focus(newUI);
					}
				}
				break;
				case OpenMode.Overlay:
				{
					unfocus(oldUI);
					create(newUIType);
					newUI = m_uiStack.Peek();
					show(newUI,_data);
					focus(newUI);
				}
				break;
				case OpenMode.Back:
				{
					if(oldUI.GetType()!=newUIType)
					{
						if(m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;})==null)
						{
							unfocus(oldUI);
							hide(oldUI);
							create(newUIType);
							newUI = m_uiStack.Peek();
							show(newUI,_data);
							focus(newUI);
						}
						else
						{
							while(m_uiStack.Peek().GetType()!=newUIType)
							{
								unfocus(m_uiStack.Peek());
								hide(m_uiStack.Peek());
								destroy(m_uiStack.Pop());
							}
							UIBase ui = m_uiStack.Peek();
							show(ui,_data);
							focus(ui);
						}
					}
				}
				break;
			}
		}
		else
		{
			create(newUIType);
			show(m_uiStack.Peek(),_data);
			focus(m_uiStack.Peek());
		}
	}

	/// <summary>
	/// 关闭指定界面
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public void Close<T>() where T : UIBase
	{
		if(m_uiStack.Count>0)
		{
			Type uiType = typeof(T);
			if(m_uiStack.Peek().GetType()==uiType)
			{
				CloseCurUI();
			}
			else
			{
				var ui = m_uiStack.Find((_ui)=>{return _ui.GetType()==uiType;});
				if(ui!=null)
				{
					m_uiStack.Remove(ui);
				}

			}
		}

	}

	void focus(UIBase _ui)
	{
		if(_ui.m_State == UICycleState.UnFocus || _ui.m_State == UICycleState.Show)
		{
			_ui.m_State = UICycleState.Focus;
			_ui.OnFocus();
		}
	}

	void unfocus(UIBase _ui)
	{
		if(_ui.m_State == UICycleState.Focus)
		{
			_ui.m_State = UICycleState.UnFocus;
			_ui.OnUnFocus();
		}
	}

	/// <summary>
	/// 创建ui界面 优先从回收池中获取
	/// </summary>
	/// <param name="_uiType"></param>
	/// <param name="_data"></param>
	void create(Type _uiType)
	{
		UIBase ui = m_uiPool.Pull(_uiType);
		if(ui==null)
		{
			GameObject prefab = loadUIPanel(_uiType.Name,m_panelParent);
			ui = prefab.GetComponent<UIBase>();
			ui.gameObject.name = _uiType.Name;
		}
		ui.gameObject.SetActive(true);
		ui.transform.localPosition = Config.HidePos;
		m_uiStack.Push(ui);
		ui.OnCreate();
		ui.m_State = UICycleState.Create;
	}

	/// <summary>
	/// 隐藏界面 ui位置移出视图之外 活动状态仍未true
	/// </summary>
	/// <param name="_ui"></param>
	void hide(UIBase _ui)
	{
		if(_ui.m_State == UICycleState.UnFocus)
		{
			_ui.OnHide();
			_ui.m_State = UICycleState.Hide;
			_ui.transform.localPosition = Config.HidePos;
		}
	}

	/// <summary>
	/// 移除界面 活动状态设为false 收入回收池
	/// </summary>
	/// <param name="_ui"></param>
	void destroy(UIBase _ui)
	{
		_ui.OnDestroy();
		_ui.m_State = UICycleState.Destroy;
		_ui.gameObject.SetActive(false);
		m_uiPool.Push(_ui);

	}


	/// <summary>
	/// 显示界面 ui界面移至视图内
	/// </summary>
	/// <param name="_ui"></param>
	/// <param name="_data"></param>
	void show(UIBase _ui,object _data)
	{
		if(_ui.m_State==UICycleState.Create || _ui.m_State==UICycleState.Hide)
		{
			_ui.transform.SetSiblingIndex(m_panelParent.childCount-1);
			_ui.transform.localPosition = Vector3.zero;
			_ui.OnShow(_data);
			_ui.m_State = UICycleState.Show;
		}
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

	GameObject loadUIPanel(string _path,Transform _parent=null)
	{
		
	}

	public void Init()
	{
		
	}

	public void Reset()
	{
		
	}

	/// <summary>
	/// ui界面基类 子类名需要和UI预制件名称相同
	/// </summary>
	public class UIBase : MonoBehaviour,IUICycle
	{
		[HideInInspector]
		public UICycleState m_State;

		/// <summary>
		/// 系统返回键和UI返回键的响应 按需求在子类中可自定义响应逻辑
		/// </summary>
		public virtual void OnClickBack()
		{
			
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

		public virtual void OnFocus()
		{
			
		}

		public virtual void OnUnFocus()
		{
			
		}
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
		List<UIBase> m_uiList = new List<UIBase>();

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

	public interface IUICycle
	{
		void OnCreate();
		void OnShow(object _data);
		void OnFocus();
		void OnUnFocus();
		void OnHide();
		void OnDestroy();
	}    
	
	public enum UICycleState
	{
		None=0,
		Create=3,//创建视图之外
		Show=2,//移至视图内
		Focus=1,//有焦点
		UnFocus=-1,//无焦点
		Hide=-2,//移至视图外
		Destroy=-3,//销毁或添加至回收池
	}
}

