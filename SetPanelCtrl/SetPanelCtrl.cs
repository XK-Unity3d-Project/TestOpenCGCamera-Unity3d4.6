using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;

public class SetPanelCtrl : MonoBehaviour
{
		public UnityEngine.UI.Image JiaoZhunCrossImg;
		public Sprite[] JiaoZhunCross;
		static SetPanelCtrl _Instance;
		public static SetPanelCtrl GetInstance()
		{
				return _Instance;
		}

		// Use this for initialization
		void Start()
		{
				_Instance = this;
				JiaoZhunCrossImg.enabled = false;
		}

		// Update is called once per frame
		void Update()
		{
				if (Input.GetKeyUp(KeyCode.F4)) {
						InitJiaoZhunZuoBiao();
				}

				if (Input.GetKeyUp(KeyCode.G)) {
//						ChangeJiaoZhunPic(); //test.
						if (JiaoZhunCrossImg.enabled)
						{
								CSampleGrabberCB.GetInstance().ActiveJiaoZhunZuoBiao();
						}
				}
				ChangeJiaoZhunPic();
		}

		void InitJiaoZhunZuoBiao()
		{
				if (JiaoZhunCrossImg == null || JiaoZhunCrossImg.enabled) {
						return;
				}
				Debug.Log("InitJiaoZhunZuoBiao!");
//				ZhunXingZB.Visible = false;
//				ZhunXingP1.Visible = false;
				XKPlayerCrossCtrl.SetAllCrossActive(false);
				IndexCrossJZ = 0;
				ChangeJiaoZhunPic();
				JiaoZhunCrossImg.enabled = true;

				if (CSampleGrabberCB.GetInstance() != null) {
						CSampleGrabberCB.GetInstance().InitJiaoZhunZuoBiao();	
				}
		}

		byte IndexCrossJZ;
		public void ChangeIndexCrossJZ()
		{
				//ScreenLog.Log("IndexCrossJZ " + IndexCrossJZ);
				switch (IndexCrossJZ)
				{
						case 0:
						case 1:
						case 2:
						case 3:
								IndexCrossJZ = (byte)(IndexCrossJZ + 1);
								break;
				}
		}

		public void ChangeJiaoZhunPic()
		{
				if (!JiaoZhunCrossImg.enabled) {
						return;	
				}

				switch (IndexCrossJZ)
				{
				case 0:
				case 1:
				case 2:
				case 3:
						JiaoZhunCrossImg.sprite = JiaoZhunCross[IndexCrossJZ];
						break;
				default:
						IndexCrossJZ = 0;
						JiaoZhunCrossImg.enabled = false;
						XKPlayerCrossCtrl.SetAllCrossActive(true);
						break;
				}
		}
}