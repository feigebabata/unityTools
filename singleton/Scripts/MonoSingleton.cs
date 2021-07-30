using UnityEngine;

// namespace FGUFW.Core
// {
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		private static T mInstance = null;

		public static T I
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = GameObject.FindObjectOfType(typeof(T)) as T;
					if (mInstance == null)
					{
						GameObject go = new GameObject(typeof(T).Name);
						mInstance = go.AddComponent<T>();
					}
				}

				return mInstance;
			}
		}


		private void Awake()
		{
			if (mInstance == null)
			{
				mInstance = this as T;

				if(IsDontDestroyOnLoad())
				{
					DontDestroyOnLoad(gameObject);
				}

				Init();
			}
			else
			{
				Debug.LogError("mono单例重复");
			}
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		void OnDestroy()
		{
			Dispose();
			MonoSingleton<T>.mInstance = null;
		}
	
		protected virtual void Init()
		{

		}

		public void DestroySelf()
		{
			UnityEngine.Object.Destroy(gameObject);
		}

		public virtual void Dispose()
		{

		}

		protected abstract bool IsDontDestroyOnLoad();

	}
// }
