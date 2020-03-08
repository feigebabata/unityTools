using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XiuDanUnity.App
{
	[RequireComponent(typeof(MeshRenderer))]
	public class QuadFullCameraChild : MonoBehaviour 
	{
		public string[] TexNames=new string[]{"_MainTex"};
		Material m_mat;
		

		public void SetCrop(int _width,int _height)
		{
            if(!m_mat)
            {
                m_mat = GetComponent<MeshRenderer>().material;
            }
			if(TexNames!=null && TexNames.Length>0)
			{
				for (int i = 0; i < TexNames.Length; i++)
				{
					if(m_mat.GetTexture(TexNames[i]))
					{
						setTexCrop(TexNames[i],_width,_height);
					}
					else
					{
						Debug.LogFormat("[QuadFullCameraChild.SetCrop]{0}无贴图:{1}",name,TexNames[i]);
					}
				}
			}
		}

        void setTexCrop(string _name,int _width,int _height)
        {
            Texture tex = m_mat.GetTexture(_name);
            Vector2 size = new Vector2(tex.width, tex.height);
            Vector2 viewSize = new Vector2(_width,_height);
            if(viewSize==Vector2.zero)
            {
                viewSize = new Vector2(Screen.width,Screen.height);
            }
            Vector2 tiling = Vector2.one;
            Vector2 offset = Vector2.zero;
            if (size.x / size.y > viewSize.x / viewSize.y)
            {
                size /= size.y;
                viewSize /= viewSize.y;
                tiling.x = viewSize.x / size.x;
                offset.x = (1 - tiling.x) / 2;
            }
            else
            {
                size /= size.x;
                viewSize /= viewSize.x;
                tiling.y = viewSize.y / size.y;
                offset.y = (1 - tiling.y) / 2;
            }
            m_mat.SetTextureScale(_name, tiling);
            m_mat.SetTextureOffset(_name, offset);
        }
	}
}
