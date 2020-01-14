using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGUIManager : MonoSingleton<UGUIManager> 
{
	public static class Config
	{
		public static readonly Vector3 HidePos = new Vector3(3000,0,0);
	}
	UIStack m_uiStack = new UIStack();
	UIPool m_uiPool = new UIPool(5);        
	Transform m_panelParent;
	AssetBundle m_panelAB;

	protected override void Init()
	{
		m_panelParent = GameObject.Find("Canvas/Panels").transform;
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape) && m_uiStack.Count>0)
		{
			m_uiStack.Peek().OnClickBack();
		}
	}

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
		string _name = _uiType.Name;
		GameObject go = null;
		#if UNITY_EDITOR
		if (AssetBundles.AssetBundleConfig.IsEditorMode)
		{
			go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/BasicAssets/TwoRoomPanel/"+_name+".prefab");
			go = Instantiate(go,m_panelParent);
			return go.GetComponent<UIBase>();
		}
		#endif
		if(!m_panelAB)
		{
			string path = Application.dataPath+"/StreamingAssets/AssetBundles/basicassets/tworoompanel.assetbundle";
			m_panelAB = AssetBundle.LoadFromFile(path);
		}
		go = m_panelAB.LoadAsset<GameObject>(_name+".prefab");
		go = Instantiate(go,m_panelParent);
		return go.GetComponent<UIBase>();
	}

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

	public class UIBase : MonoBehaviour
	{
		[HideInInspector]
		public State m_State;
		public virtual void OnClickBack()
		{
			UGUIManager.Instance.CloseCurUI();
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
		Open,
		Overlay,
		Back,
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

