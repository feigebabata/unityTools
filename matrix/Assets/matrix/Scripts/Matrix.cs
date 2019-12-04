using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace FG
{
	public struct Matrix 
	{
		Shape m_shape;
        float[] m_array;
		public Shape Shape
		{
			get
			{
				return m_shape;
			}
			set
			{
				if(m_shape.H*m_shape.V != value.H*value.V)
				{
					throw new Exception($"[FG.Matrix.Shape]:矩阵大小不能变 {m_shape}=>{value}");
				}
				m_shape = value;
			}
		}

        public int Length
        {
            get{return m_array.Length;}
        }

        public float this[int h_idx,int v_int]
        {
            get
            {
                return m_array[h_idx+m_shape.H*v_int];
            }
            set
            {
                m_array[h_idx+m_shape.H*v_int] = value;
            }
        }

		public Matrix(int _h,int _v,params float[] _array)
		{
			m_shape = new Shape(){H=_h,V=_v};
			if(_array==null || _array.Length==0)
			{
            	m_array = new float[_h*_v];
			}
			else
			{
				if(_array.Length==_h*_v)
				{
					m_array = _array;
				}
				else if(_array.Length>_h*_v)
				{
					m_array = new float[_h*_v];
					Array.Copy(_array,m_array,m_array.Length);
				}
				else
				{
					m_array = new float[_h*_v];
					Array.Copy(_array,m_array,_array.Length);
				}
			}
		}

        public override string ToString()
        {
			StringBuilder sb = new StringBuilder();
			sb.Append(m_shape.H);
			sb.Append(" , ");
			sb.Append(m_shape.V);
			for(int v=0;v<m_shape.V;v++)
			{
				sb.Append("\n");
				for(int h=0;h<m_shape.H;h++)
				{
					sb.Append(m_array[v*m_shape.H+h]+" , ");
				}
			}
            return sb.ToString();
        }

	}

	public struct Shape
	{
		public int H;
		public int V;

        public override string ToString()
        {
            return $"[{H},{V}]";
        }
	}
}
