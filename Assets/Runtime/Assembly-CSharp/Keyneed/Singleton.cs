using UnityEngine;

namespace Keyneed
{
	/// <summary>
	/// Generic MonoBehaviour Singleton base class used throughout Keyneed architecture.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = FindObjectOfType<T>();
				}
				return m_Instance;
			}
		}

		protected virtual void Awake()
		{
			if (m_Instance == null)
			{
				m_Instance = this as T;
			}
			else if (m_Instance != this)
			{
				Destroy(gameObject);
			}
		}
	}
}
