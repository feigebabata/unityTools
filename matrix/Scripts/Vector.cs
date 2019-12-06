using System;

namespace FG
{
    public struct Vector
    {
        public int Dimension{get; private set;}

        float[] m_array;

        public Vector(params float[] _data)
        {
            Dimension = _data.Length;
            m_array = _data;
        }

        public float Magnitude
        {
            get
            {
                float sum = 0;
                for (int i = 0; i < Dimension; ++i)
                {
                    sum+=m_array[i]*m_array[i];
                }
                return (float)Math.Sqrt(sum);
            }
        }

        public Vector Normalize
        {
            get
            {
                float[] arr = new float[Dimension];
                for (int i = 0; i < arr.Length; ++i)
                {
                    arr[i] = Array[i]/Magnitude;                
                }
                return new Vector(arr);
            }
        }

        public float Sum
        {
            get
            {
                float sum = 0;
                for (int i = 0; i < Dimension; ++i)
                {
                    sum+=m_array[i];
                }
                return sum;
            }
        }

        public float[] Array
        {
            get
            {
                return m_array;
            }
            set
            {
                if(value.Length!=Dimension)
                {
                    throw new Exception($"[FG.Vector.Array]:向量维度不能变 {Dimension}=>{value.Length}");
                }
                m_array = value;
            }
        }

        public void Mul (Vector _v1)
        {
            if(_v1.Dimension!=Dimension)
            {
                throw new Exception($"[FG.Vector.Mul]:向量维度需相等 {Dimension},{_v1.Dimension}");
            }
            for (int i = 0; i < Dimension; ++i)
            {
                Array[i] *= _v1.Array[i];                
            }
        }

        public void Mul (float _v1)
        {
            for (int i = 0; i < Dimension; ++i)
            {
                Array[i] *= _v1;                
            }
        }

        public void Add (Vector _v1)
        {
            if(_v1.Dimension!=Dimension)
            {
                throw new Exception($"[FG.Vector.Add]:向量维度需相等 {Dimension},{_v1.Dimension}");
            }
            for (int i = 0; i < Dimension; ++i)
            {
                Array[i] += _v1.Array[i];                
            }
        }

        public void Subtract (Vector _v1)
        {
            if(_v1.Dimension!=Dimension)
            {
                throw new Exception($"[FG.Vector.Subtract]:向量维度需相等 {Dimension},{_v1.Dimension}");
            }
            for (int i = 0; i < Dimension; ++i)
            {
                Array[i] -= _v1.Array[i];                
            }
        }

        public static Vector operator* (Vector _v,float _w)
        {
            float[] arr = new float[_v.Dimension];
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = _v.Array[i]*_w;                
            }
            return new Vector(arr);
        }

        public static Vector operator* (Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                throw new Exception($"[FG.Vector.*]:向量维度需相等 {_v1.Dimension},{_v1.Dimension}");
            }
            float[] arr = new float[_v1.Dimension];
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = _v1.Array[i]* _v2.Array[i];                
            }
            return new Vector(arr);
        }

        public static Vector operator+ (Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                throw new Exception($"[FG.Vector.+]:向量维度需相等 {_v1.Dimension},{_v1.Dimension}");
            }
            float[] arr = new float[_v1.Dimension];
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = _v1.Array[i]+_v2.Array[i];                
            }
            return new Vector(arr);
        }

        public static Vector operator- (Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                throw new Exception($"[FG.Vector.-]:向量维度需相等 {_v1.Dimension},{_v1.Dimension}");
            }
            float[] arr = new float[_v1.Dimension];
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = _v1.Array[i]-_v2.Array[i];                
            }
            return new Vector(arr);
        }

        public static float Dot(Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                throw new Exception($"[FG.Vector.Dot]:点乘的两个向量维度需相等 {_v1.Dimension},{_v1.Dimension}");
            }
            float val=0;
            for (int i = 0; i < _v1.Array.Length; ++i)
            {
                val += _v1.Array[i]*_v2.Array[i];             
            }
            return val;
        }

        public override bool Equals(object obj)
        {
            return this == (Vector)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator == (Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                return false;
            }
            for (int i = 0; i < _v1.Dimension; i++)
            {
                if(_v1.Array[i]!=_v2.Array[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator != (Vector _v1,Vector _v2)
        {
            if(_v1.Dimension!=_v2.Dimension)
            {
                return true;
            }
            for (int i = 0; i < _v1.Dimension; i++)
            {
                if(_v1.Array[i]!=_v2.Array[i])
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}