using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pcvr : MonoBehaviour
{
		static pcvr _Instance;
		public static pcvr GetInstance()
		{
				if (_Instance == null) {
						GameObject obj = new GameObject();
						DontDestroyOnLoad(obj);
						obj.name = "pcvr";
						_Instance = obj.AddComponent<pcvr>();
						XKOpenCGCamera.GetInstance();
				}
				return _Instance;
		}

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {

		}
}