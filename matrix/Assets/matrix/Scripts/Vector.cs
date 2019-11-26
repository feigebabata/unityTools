using System;

namespace FG
{
    public struct Vector
    {
        public int Dimension{get; private set;}

        float[] m_array;
        float m_magnitude;

        public Vector(params float[] _data)
        {
            Dimension = _data.Length;
            m_array = _data;
            m_magnitude=0;
            resetMagnitude();
        }

        public float Magnitude
        {
            get
            {
                return m_magnitude;
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
                resetMagnitude();
            }
        }

        void resetMagnitude()
        {
            float sum = 0;
            for (int i = 0; i < Dimension; ++i)
            {
                sum+=m_array[i]*m_array[i];
            }
            m_magnitude = (float)Math.Sqrt(sum);
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
        
    }
}