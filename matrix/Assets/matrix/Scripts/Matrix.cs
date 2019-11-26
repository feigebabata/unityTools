using System.Collections;
using System.Collections.Generic;
using System;

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

		public Matrix(int _h,int _v)
		{
			m_shape = new Shape(){H=_h,V=_v};
            m_array = new float[m_shape.H*m_shape.V];
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
