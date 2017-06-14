using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using CGSDK;
using CGAPI = CGSDK.CGAPI;

using FactoryHandle = System.IntPtr;
using DeviceHandle = System.IntPtr;
using System.Windows.Forms;
using System.IO;

public class TestOpenCGCamera : MonoBehaviour {

	[DllImport("user32")]  
	static extern IntPtr GetForegroundWindow();

	string FilePath;
	MeshRenderer MeshRenderCom;
	// Use this for initialization
	void Start () {
		FilePath = UnityEngine.Application.dataPath + "/../CGCamera";
		if (!Directory.Exists(FilePath)) {
			Directory.CreateDirectory(FilePath);
		}

		mThis = this;
		MeshRenderCom = GetComponent<MeshRenderer>();
		uint[] adwVersion = new uint[4];
		CGAPI.DeviceGetSDKVersion(adwVersion);
		SearchDevices();
		SelectedCGCameraDevice();
		if (mDeviceHandle != IntPtr.Zero)
		{
			Debug.Log("Device start...");
			CGAPI.DeviceStart(mDeviceHandle);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyUp(KeyCode.P)) {
			bnSnapshot_Click();
		}
//		testSaveCount++;
//		if (testSaveCount == 100) {
//
//			DateTime time = DateTime.Now;
//			string strFile = "D:\\Image" + time.ToFileTime();
//			DeviceStatus devStatus = CGAPI.CaptureFile(mDeviceHandle, strFile, emDSFileType.FILE_BMP);
//			strFile += ".bmp";
//			if (DeviceStatus.STATUS_OK == devStatus){
//				MessageBox.Show(strFile, "保存成功");
//			} else{
//				MessageBox.Show(strFile, "保存失败");
//			}

			//string imgPath = FilePath + "/CGCamera.png";
//			string strFile = FilePath + "/CGCamera.png";
//			if (mDeviceHandle != DeviceHandle.Zero)
//			{
//				DeviceStatus devStatus = CGAPI.CaptureFile(mDeviceHandle, strFile, emDSFileType.FILE_PNG);
//				//strFile += ".png";
//
//				if (DeviceStatus.STATUS_OK == devStatus){
//					MessageBox.Show(strFile, "保存成功");
//				} else{
//					MessageBox.Show(strFile, "保存失败");
//				}
//			}
//			Debug.Log("path "+strFile);
			//bmp.Save(strFile, System.Drawing.Imaging.ImageFormat.Png);
//		}
	}


	private void bnSnapshot_Click()
	{
		if (mDeviceHandle != DeviceHandle.Zero)
		{
			DateTime time = DateTime.Now;
			string strFile = "D:\\Image" + time.ToFileTime();
			DeviceStatus devStatus = CGAPI.CaptureFile(mDeviceHandle, strFile, emDSFileType.FILE_BMP);
			strFile += ".bmp";
			if (DeviceStatus.STATUS_OK == devStatus){
				MessageBox.Show(strFile, "保存成功");
			} else{
				MessageBox.Show(strFile, "保存失败");
			}
		}
	}

	static TestOpenCGCamera mThis;
	DeviceHandle mDeviceHandle = IntPtr.Zero;

	void SearchDevices()
	{
		DeviceStatus devSatus = CGAPI.DeviceInitialSDK(IntPtr.Zero, false);
		if (DeviceStatus.STATUS_OK == devSatus)
		{
            int iCameraCounts = 0;
            devSatus = CGAPI.EnumDevice(IntPtr.Zero, ref iCameraCounts);
            if (DeviceStatus.STATUS_OK == devSatus) {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(new EnumDeviceParam()) * iCameraCounts);
                devSatus = CGAPI.EnumDevice(ptr, ref iCameraCounts);
                if (DeviceStatus.STATUS_OK == devSatus)
                {
                    for (int i = 0; i < iCameraCounts; i++)
                    {
                        EnumDeviceParam edp = (EnumDeviceParam)Marshal.PtrToStructure((IntPtr)((int)ptr + i * Marshal.SizeOf(new EnumDeviceParam())), typeof(EnumDeviceParam));
                        string strDevice = String.Format("{0} : {1}", edp.lpDeviceDesc, edp.devIndex);
						Debug.Log("DeviceInfo "+strDevice);
                    }
                }
                Marshal.FreeHGlobal(ptr);
            }
		}
	}

	int testSaveCount;
	public void OnRecvFrame(IntPtr pDevice, IntPtr pImageBuffer, ref DeviceFrameInfo pFrInfo, IntPtr lParam)
	{
		//Debug.Log("OnRecvFrame...");
		
		//重点->这里是将IntPtr转换为byte[]数组.
		int countBuf = 320 * 240;
        byte[] bufHandle = new byte[countBuf];
		Marshal.Copy(pImageBuffer, bufHandle, 0, countBuf);
        for (int i = 0; i < bufHandle.Length; i++)
        {
            if (bufHandle[i] == 0)
            {
                continue;
            }
            else
            {
                Debug.Log("i " + i + ", val " + bufHandle[i]);
            }
        }

		IntPtr pRGB24Buff = CGAPI.DeviceISP(mDeviceHandle, pImageBuffer, ref pFrInfo);
		if (pRGB24Buff != null)
		{
//			ushort timeTest = 0;
//			CGAPI.GetExposureTime(mDeviceHandle, ref timeTest);
//			Debug.Log("OnRecvFrame...timeTest ... "+timeTest);
            /*if (false)
            {
                Bitmap bmp = new Bitmap((int)pFrInfo.uiWidth, (int)pFrInfo.uiHeight, (int)pFrInfo.uiWidth, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, pRGB24Buff);
                System.Drawing.Imaging.ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bmp.Palette = palette;
            }*/
			
			/*if (false)
			{
				//重点->这里是将IntPtr转换为byte[]数组.
				int imgSize = (int)(pFrInfo.uiWidth * pFrInfo.uiHeight);
				byte[] bufHandle = new byte[imgSize];
				//当bufHandle用完后,一定要释放gch.
				GCHandle gch = GCHandle.Alloc(bufHandle, GCHandleType.Pinned);    //固定托管内存
				Marshal.WriteIntPtr(Marshal.UnsafeAddrOfPinnedArrayElement(bufHandle, 0), pImageBuffer);
				for (int i = 0; i < bufHandle.Length; i++)
				{
					if (bufHandle[i] == 0)
					{
						continue;
					}
				}
				gch.Free();
			}*/
			CGAPI.DeviceDisplayRGB24(mDeviceHandle, pRGB24Buff, ref pFrInfo);

			testSaveCount++;
			if (testSaveCount == 10) {
				bnSnapshot_Click();
			}
		}
	}

	public static void OnReceiveFrame(IntPtr pDevice, IntPtr pImageBuffer, ref DeviceFrameInfo pFrInfo, IntPtr lParam)
	{
		mThis.OnRecvFrame(pDevice, pImageBuffer, ref pFrInfo, lParam);    
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit...");
		CloseCGCamera();
	}

	private void CloseCGCamera()
	{
		if (mDeviceHandle != IntPtr.Zero)
		{
			Debug.Log("CloseCGCamera...");
			CGAPI.DeviceStop(mDeviceHandle);
			CGAPI.CloseDevice(mDeviceHandle);
			CGAPI.DeviceUnInit(mDeviceHandle);
			CGAPI.DeviceRelease(mDeviceHandle);
			mDeviceHandle = IntPtr.Zero;
			CGAPI.DeviceUnInitialSDK();
		}
	}

	MovieTexture MovieCamera;
	private void SelectedCGCameraDevice()
	{
		if (IntPtr.Zero != mDeviceHandle)
		{
			CGAPI.DeviceStop(mDeviceHandle);
			CGAPI.SyncCloseDevice(mDeviceHandle);
			CGAPI.DeviceUnInit(mDeviceHandle);
			CGAPI.DeviceRelease(mDeviceHandle);
			mDeviceHandle = IntPtr.Zero;
		}
		else
		{
			byte byDev = 1;
			DeviceStatus devStatus = CGAPI.OpenDeviceByUSBAddress(byDev, ref mDeviceHandle);
			if (DeviceStatus.STATUS_OK == devStatus)
			{
				ReceiveFrameProc rfCallBack = new ReceiveFrameProc(OnReceiveFrame);
				//不添加回调函数.
				//devStatus = CGAPI.DeviceInit(mDeviceHandle, panelVideo.Handle, false, true);
				//添加回调函数.
				//devStatus = CGAPI.DeviceInitEx(mDeviceHandle, rfCallBack, IntPtr.Zero, panelVideo.Handle, true);

				//IntPtr renderPtr = MeshRenderCom.materials[0].mainTexture.GetNativeTexturePtr();
				IntPtr renderPtr = GetForegroundWindow();
				//renderPtr = GetForegroundWindow();
				//devStatus = CGAPI.DeviceInit(mDeviceHandle, renderPtr, false, true);
//				MessageBox.Show("你显示的信息", "标题", MessageBoxButtons.OK);
				//devStatus = CGAPI.DeviceInitEx(mDeviceHandle, rfCallBack, IntPtr.Zero, renderPtr, true);
				//devStatus = CGAPI.DeviceInitEx(mDeviceHandle, null, IntPtr.Zero, renderPtr, true);

//				bool isGetMode = false;
//				CGAPI.DeviceSetUsedGetMode(mDeviceHandle, true);
//				CGAPI.DeviceGetUsedGetMode(mDeviceHandle,   ref isGetMode);
//				Debug.Log("isGetMode "+isGetMode);
				devStatus = CGAPI.DeviceInitEx(mDeviceHandle, rfCallBack, IntPtr.Zero, renderPtr, true);
				//devStatus = CGAPI.DeviceInitEx(mDeviceHandle, rfCallBack, IntPtr.Zero, IntPtr.Zero, true);
				if (DeviceStatus.STATUS_OK == devStatus)
				{
					Debug.Log("Open CGCamera...");
				}
			}
		}
	}

	public Texture2D img = null;  
	void OnGUI()  
	{  
		if (GUI.Button(new Rect(0, 0, 100, 20), "选择文件"))  
		{  

			OpenFileDialog od = new OpenFileDialog();  
			od.Title = "请选择头像图片";  
			od.Multiselect = false;  
			od.Filter = "图片文件(*.jpg,*.png,*.bmp)|*.jpg;*.png;*.bmp";  
			if (od.ShowDialog() == DialogResult.OK)  
			{
				if (File.Exists(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp"))  
				{  
					File.Delete(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp");  
					File.Copy(od.FileName, UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp");  
				}  
				else  
				{  
					File.Copy(od.FileName, UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp");  
				}  
				StartCoroutine(GetTexture("file://"+UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp"));  
//				if (File.Exists(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png"))  
//				{  
//					File.Delete(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png");  
//					File.Copy(od.FileName, UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png");  
//				}  
//				else  
//				{  
//					File.Copy(od.FileName, UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png");  
//				}  
//				StartCoroutine(GetTexture("file://"+UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png"));  
			}  
		}  
		if (img != null)  
		{  
			GUI.DrawTexture(new Rect(0, 20, img.width, img.height), img);  
		}  
	}  

	IEnumerator GetTexture(string url)  
	{  
		WWW www = new WWW(url);  
		yield return www;  
		if (www.isDone && www.error == null)  
		{  
			img = www.texture;  
			Debug.Log(img.width + "  " + img.height);  
			byte[] data = img.EncodeToPNG();  
			File.WriteAllBytes(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.bmp", data);  
			//File.WriteAllBytes(UnityEngine.Application.streamingAssetsPath + "/Temp/temp.png", data);  
		}  
	}  
}