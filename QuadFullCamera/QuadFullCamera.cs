using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FG
{
	[RequireComponent(typeof(Camera))]
	public class QuadFullCamera : MonoBehaviour 
	{
		Camera m_camera;
		int m_width,m_height;
		bool m_orthographic;
		float m_orthographicSize;
		float m_fieldOfView;
		public QuadFullCameraChild[] m_Quads;


		// Use this for initialization
		void Awake () 
		{
			m_camera = GetComponent<Camera>();
			resetLastData();
			resetQuadT();
		}

		// void Start()
		// {
			
		// }
		
		// Update is called once per frame
		void Update () 
		{
			if(m_Quads!=null && m_Quads.Length>0)
			{
				if(m_camera.pixelWidth != m_width || 
				m_camera.pixelHeight != m_height ||
				m_camera.orthographic != m_orthographic ||
				m_camera.orthographicSize != m_orthographicSize ||
				m_camera.fieldOfView != m_fieldOfView
				)
				{
					resetLastData();
					resetQuadT();
				}
			}
		}

		void resetLastData()
		{
			m_width = m_camera.pixelWidth;
			m_height = m_camera.pixelHeight;
			m_orthographic = m_camera.orthographic;
			m_orthographicSize = m_camera.orthographicSize;
			m_fieldOfView = m_camera.fieldOfView;

		}

		void resetQuadT()
		{
			if(m_orthographic)
			{
				for (int i = 0; i < m_Quads.Length; i++)
				{
					Vector2 scale = m_Quads[i].transform.localScale;
					scale.y = m_orthographicSize*2;
					scale.x = scale.y*m_width/m_height;
					m_Quads[i].transform.localScale = scale;
					m_Quads[i].SetCrop(m_width,m_height);
				}
			}
			else
			{
				float offset = 0;
				float tan = Mathf.Tan(Mathf.Deg2Rad*m_fieldOfView/2);
				for (int i = 0; i < m_Quads.Length; i++)
				{
					offset = m_Quads[i].transform.localPosition.z;
					Vector2 scale = m_Quads[i].transform.localScale;
					scale.y = offset*tan*2;
					scale.x = scale.y*m_width/m_height;
					m_Quads[i].transform.localScale = scale;
					m_Quads[i].SetCrop(m_width,m_height);
				}
			}
		}
	}
}
