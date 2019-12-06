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
				if(m_shape.X*m_shape.Y != value.X*value.Y)
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

        public float this[int _x,int _y]
        {
            get
            {
                return m_array[_x+m_shape.X*_y];
            }
            set
            {
                m_array[_x+m_shape.X*_y] = value;
            }
        }

		public Matrix(int _x,int _y,params float[] _array)
		{
			m_shape = new Shape(){X=_x,Y=_y};
			if(_array==null || _array.Length==0)
			{
            	m_array = new float[_x*_y];
			}
			else
			{
				if(_array.Length==_x*_y)
				{
					m_array = _array;
				}
				else if(_array.Length>_x*_y)
				{
					m_array = new float[_x*_y];
					Array.Copy(_array,m_array,m_array.Length);
				}
				else
				{
					m_array = new float[_x*_y];
					Array.Copy(_array,m_array,_array.Length);
				}
			}
		}

        public override string ToString()
        {
			StringBuilder sb = new StringBuilder();
			sb.Append(m_shape);
			for(int y=0;y<m_shape.Y;y++)
			{
				sb.Append("\n");
				for(int x=0;x<m_shape.X;x++)
				{
					sb.Append(m_array[y*m_shape.X+x]+" , ");
				}
			}
            return sb.ToString();
        }

		public static Matrix One(int _x)
		{
			float[] array = new float[_x*_x];
			for (int i = 0; i < _x; i++)
			{
				array[i*_x+i]=1;
			}
			return new Matrix(_x,_x,array);
		}

		public static Matrix operator* (Matrix _m1,Matrix _m2)
        {
            if(_m1.Shape.X!=_m2.Shape.Y)
            {
                throw new Exception($"[FG.Matrix.*]:矩阵相乘需 左列=右行 {_m1.Shape.X},{_m2.Shape.Y}");
            }
			var mat = new Matrix(_m2.Shape.X,_m1.Shape.Y);
            for (int y = 0; y < mat.Shape.Y; ++y)
            {
                for (int x = 0; x < mat.Shape.X; x++)
				{
					for (int i = 0; i < _m1.Shape.X; i++)
					{
						mat[x,y]+= _m1[i,y] * _m2[x,i];
					}
				}               
            }
            return mat;
        }

	}

	public struct Shape
	{
		public int X;
		public int Y;

        public override bool Equals(object obj)
        {
            return this == (Shape)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }

		public static bool operator == (Shape _s1,Shape _s2)
		{
			return _s1.X==_s2.X && _s1.Y==_s2.Y;
		}

		public static bool operator != (Shape _s1,Shape _s2)
		{
			return _s1.X!=_s2.X || _s1.Y!=_s2.Y;
		}
	}
}
