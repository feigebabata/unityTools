using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Looplist : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public bool IsVertical=true;
	public GameObject Item;
	public Scrollbar Scroll; 
	public Action<int,GameObject> OnShow,OnHide;

	int m_count;
	Dictionary<int,GameObject> m_showList = new Dictionary<int,GameObject>();
	List<GameObject> m_hideList = new List<GameObject>();
	List<float> m_itemSizes=new List<float>();
	RectTransform m_rect_T;
	float m_listPos=0;

    public int Count
    {
        get
        {
            return m_count;
        }
    }

	public float ListSize
	{
		get
		{
			float size = 0;
			for (int i = 0; i < m_itemSizes.Count; i++)
			{
				size+=m_itemSizes[i];
			}
			return size;
		}
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
		if(IsVertical)
		{
			m_listPos+=eventData.delta.y;

			if(m_listPos<0)
			{
				m_listPos=0;
			}

			if(ListSize<=m_rect_T.rect.height)
			{
				m_listPos=0;
			}
			else if(ListSize-m_listPos<m_rect_T.rect.height)
			{
				m_listPos = ListSize-m_rect_T.rect.height;
			}
			if(Scroll)
			{
				Scroll.value = m_listPos/(ListSize-m_rect_T.rect.height);
			}
		}
		else
		{
			m_listPos-=eventData.delta.x;
			
			if(m_listPos<0)
			{
				m_listPos=0;
			}

			if(ListSize<=m_rect_T.rect.width)
			{
				m_listPos=0;
			}
			else if(ListSize-m_listPos<m_rect_T.rect.width)
			{
				m_listPos = ListSize-m_rect_T.rect.width;
			}
			if(Scroll)
			{
				Scroll.value = m_listPos/(ListSize-m_rect_T.rect.width);
			}
		}
		updateList();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.LogWarning(eventData.position);
    }

	void Awake()
	{
		m_rect_T = transform as RectTransform;
		// if(IsVertical)
		// {
		// 	m_rect_T.pivot = new Vector2(0.5f,1);
		// 	m_rect_T.anchorMin = new Vector2(0.5f,1);
		// 	m_rect_T.anchorMax = new Vector2(0.5f,1);
		// }
		// else
		// {
		// 	m_rect_T.pivot = new Vector2(1,0.5f);
		// 	m_rect_T.anchorMin = new Vector2(0,0.5f);
		// 	m_rect_T.anchorMax = new Vector2(0,0.5f);
		// }
		Item.gameObject.SetActive(false);
	}

	public void Init(int _count=0,Action<int,GameObject> _onShow=null,Action<int,GameObject> _onHide=null)
	{
		if(Item==null)
		{
			Debug.LogError("Item == null");
			return;
		}
		OnShow+= _onShow;
		OnHide+= _onHide;
		m_count = _count;
		var rect = Item.transform as RectTransform;
		float size = IsVertical ? rect.sizeDelta.y : rect.sizeDelta.x;
		for (int i = m_itemSizes.Count; i < _count; i++)
		{
			m_itemSizes.Add(size);
		}

		updateList();
		if(Scroll)
		{
			Scroll.size = IsVertical? m_rect_T.rect.height/ListSize : m_rect_T.rect.width/ListSize;
		}
	}

	void updateList()
	{
		int idx = 0;
		float idxPos=0;
		int minIdx=idx,maxIdx=-1;
		for (int i = 0; i < m_itemSizes.Count-1; i++)
		{
			if(idxPos<=m_listPos && idxPos+m_itemSizes[i]>m_listPos)
			{
				break;
			}
			idx++;
			minIdx=idx;
			idxPos+=m_itemSizes[i];
		}

		if(IsVertical)
		{
			var keys = m_showList.Keys.GetEnumerator();
			List<int> rm = new List<int>();
			while(keys.MoveNext())
			{
				if(itemPos(keys.Current)+m_itemSizes[keys.Current]<m_listPos || itemPos(keys.Current)>=m_listPos+m_rect_T.rect.height)
				{
					rm.Add(keys.Current);
				}
			}
			// Debug.Log("rm1.Count:"+rm.Count);
			
			for (int i = 0; i < rm.Count; i++)
			{
				GameObject item = m_showList[rm[i]];
				m_showList.Remove(rm[i]);
				if(OnHide!=null)
				{
					OnHide(idx,item);
				}
				item.SetActive(false);
				m_hideList.Add(item);
			}

			while(idx<Count)
			{
				if(!m_showList.ContainsKey(idx))
				{
					GameObject item = getItem(idx);
					item.gameObject.SetActive(true);
					m_showList.Add(idx,item);
					if(OnShow!=null)
					{
						OnShow(idx,item);
						// Debug.Log((m_showList[idx].transform as RectTransform).sizeDelta.y);
						m_itemSizes[idx] = (m_showList[idx].transform as RectTransform).sizeDelta.y;
						// Debug.Log(m_itemSizes[idx]);
						
						if(Scroll)
						{
							Scroll.size = m_rect_T.rect.height/ListSize;
						}
					}
				}
				idxPos = itemPos(idx);
				float buttom = idxPos + m_itemSizes[idx];
				Vector3 pos = m_showList[idx].transform.localPosition;
				pos.y = -(idxPos-m_listPos);
				m_showList[idx].transform.localPosition = pos;
				maxIdx = idx;
				idx++;
				// Debug.LogFormat("{2}:{0}  {1}",buttom,m_listPos+m_rect_T.rect.height,idx);
				if(idx==Count || buttom>m_listPos+m_rect_T.rect.height)
				{
					// Debug.Log("break");
					break;
				}
				idxPos=buttom;
			}
			// Debug.Log("---------------------");

			// Debug.LogFormat("{0}  {1}",minIdx,maxIdx);
			
			keys = m_showList.Keys.GetEnumerator();
			rm.Clear();
			while(keys.MoveNext())
			{
				if(keys.Current<minIdx || keys.Current>maxIdx)
				{
					rm.Add(keys.Current);
				}
			}
			// Debug.Log("rm2.Count:"+rm.Count);
			
			for (int i = 0; i < rm.Count; i++)
			{
				GameObject item = m_showList[rm[i]];
				m_showList.Remove(rm[i]);
				if(OnHide!=null)
				{
					OnHide(idx,item);
				}
				item.SetActive(false);
				m_hideList.Add(item);
			}

		}
		else
		{
			var keys = m_showList.Keys.GetEnumerator();
			List<int> rm = new List<int>();
			while(keys.MoveNext())
			{
				if(itemPos(keys.Current)+m_itemSizes[keys.Current]<m_listPos || itemPos(keys.Current)>=m_listPos+m_rect_T.rect.width)
				{
					rm.Add(keys.Current);
				}
			}
			
			for (int i = 0; i < rm.Count; i++)
			{
				GameObject item = m_showList[rm[i]];
				m_showList.Remove(rm[i]);
				if(OnHide!=null)
				{
					OnHide(idx,item);
				}
				item.SetActive(false);
				m_hideList.Add(item);
			}

			while(idx<Count)
			{
				if(!m_showList.ContainsKey(idx))
				{
					GameObject item = getItem(idx);
					item.gameObject.SetActive(true);
					m_showList.Add(idx,item);
					if(OnShow!=null)
					{
						OnShow(idx,item);
						m_itemSizes[idx] = (m_showList[idx].transform as RectTransform).sizeDelta.x;
						
						if(Scroll)
						{
							Scroll.size = m_rect_T.rect.width/ListSize;
						}
					}
				}
				idxPos = itemPos(idx);
				float buttom = idxPos + m_itemSizes[idx];
				Vector3 pos = m_showList[idx].transform.localPosition;
				pos.x = idxPos-m_listPos;
				m_showList[idx].transform.localPosition = pos;
				maxIdx = idx;
				idx++;
				if(idx==Count || buttom>m_listPos+m_rect_T.rect.width)
				{
					break;
				}
				idxPos=buttom;
			}

			// Debug.LogFormat("{0}  {1}",minIdx,maxIdx);
			
			keys = m_showList.Keys.GetEnumerator();
			rm.Clear();
			while(keys.MoveNext())
			{
				if(keys.Current<minIdx || keys.Current>maxIdx)
				{
					rm.Add(keys.Current);
				}
			}
			// Debug.Log("rm.Count:"+rm.Count);
			
			for (int i = 0; i < rm.Count; i++)
			{
				GameObject item = m_showList[rm[i]];
				m_showList.Remove(rm[i]);
				if(OnHide!=null)
				{
					OnHide(idx,item);
				}
				item.SetActive(false);
				m_hideList.Add(item);
			}

		}
	}

	float itemPos(int _idx)
	{
		float pos = 0;
		for (int i = 0; i < _idx ; i++)
		{
			pos+=m_itemSizes[i];
		}
		return pos;
	}

	GameObject createItem(int _idx)
	{
		GameObject item = Instantiate(Item,transform,false);
		item.name = _idx.ToString();
		return item;
	}

	GameObject getItem(int _idx)
	{
		if(m_hideList.Count>0)
		{
			var item = m_hideList[0];
			m_hideList.RemoveAt(0);
			item.name = _idx.ToString();
			return item;
		}
		return createItem(_idx);
	}

	public void MoveBottom()
	{
		if(IsVertical)
		{
			if(ListSize<m_rect_T.rect.height)
			{
				m_listPos=0;
			}
			else
			{
				m_listPos = ListSize - m_rect_T.rect.height;
			}
		}
		else
		{
			if(ListSize<m_rect_T.rect.width)
			{
				m_listPos=0;
			}
			else
			{
				m_listPos = ListSize - m_rect_T.rect.width;
			}
		}
		updateList();
		Scroll.value=1;
	}

}
