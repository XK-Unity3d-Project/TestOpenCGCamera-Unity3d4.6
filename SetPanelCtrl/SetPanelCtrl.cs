using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanelCtrl : MonoBehaviour
{
		public Image JiaoZhunCrossImg;
		public Sprite[] JiaoZhunCross;
		// Use this for initialization
		void Start()
		{
				JiaoZhunCrossImg.enabled = false;
		}

		// Update is called once per frame
		void Update()
		{
				if (Input.GetKeyUp(KeyCode.F4)) {
						InitJiaoZhunZuoBiao();
				}

				if (Input.GetKeyUp(KeyCode.G)) {
						ChangeJiaoZhunPic();
				}
		}

		void InitJiaoZhunZuoBiao()
		{
				if (JiaoZhunCrossImg == null || JiaoZhunCrossImg.enabled) {
						return;
				}
				Debug.Log("InitJiaoZhunZuoBiao!");
//				ZhunXingZB.Visible = false;
//				ZhunXingP1.Visible = false;
				IndexCrossJZ = 0;
				ChangeJiaoZhunPic();
				JiaoZhunCrossImg.enabled = true;
//				pcvr.m_CamCB.InitJiaoZhunZuoBiao();
		}

		byte IndexCrossJZ;
		public void ChangeJiaoZhunPic()
		{
				//Console.WriteLine("indexVal " + indexVal);
				switch (IndexCrossJZ)
				{
				case 0:
				case 1:
				case 2:
				case 3:
						JiaoZhunCrossImg.sprite = JiaoZhunCross[IndexCrossJZ];
						IndexCrossJZ = (byte)(IndexCrossJZ + 1);
						break;
				default:
						IndexCrossJZ = 0;
						JiaoZhunCrossImg.enabled = false;
//						ZhunXingZB.Visible = true;
//						ZhunXingP1.Visible = true;
						break;
				}
		}
}