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

public class TestOpenCGCamera : MonoBehaviour
{
		string FilePath;
		string ImgPath = "";
		static TestOpenCGCamera mThis;
		DeviceHandle mDeviceHandle = IntPtr.Zero;
		long LastTimeVal;
		int CameraFrameVal = 60;
		int CGCamFrameCount;
		int SnapshotCount;
		const int SnapshotNum = 2;
		public bool IsPlayCGCam;
		public bool IsShowCGCamFrame;
		Texture2D img = null;

		[DllImport("user32")]  
		static extern IntPtr GetForegroundWindow();

		// Use this for initialization
		void Start () {
				mThis = this;
				FilePath = UnityEngine.Application.dataPath + "/../CGCamera";
				if (!Directory.Exists(FilePath)) {
						Directory.CreateDirectory(FilePath);
				}
				ImgPath = FilePath+"/CGCmaraTmp.png";

				uint[] adwVersion = new uint[4];
				CGAPI.DeviceGetSDKVersion(adwVersion);
				SearchDevices();
				SelectedCGCameraDevice();
				if (mDeviceHandle != IntPtr.Zero)
				{
						Debug.Log("Device start...");
						CGAPI.DeviceStart(mDeviceHandle);
						SetCGCameraInfo();
				}
		}

		//拍照并且保存.
		private void Snapshot()
		{
				if (mDeviceHandle != DeviceHandle.Zero)
				{
						DeviceStatus devStatus = CGAPI.CaptureFile(mDeviceHandle, ImgPath, emDSFileType.FILE_PNG);
						if (DeviceStatus.STATUS_OK == devStatus){
								//Debug.Log("保存成功! "+ImgPath);
								//MessageBox.Show(ImgPath, "保存成功");
						} else{
								MessageBox.Show(ImgPath, "保存失败");
						}
				}
		}

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
												EnumDeviceParam edp = (EnumDeviceParam)Marshal.PtrToStructure((IntPtr)((int)ptr + i * Marshal.SizeOf(new EnumDeviceParam())),
																												typeof(EnumDeviceParam));
												string strDevice = String.Format("{0} : {1}", edp.lpDeviceDesc, edp.devIndex);
												Debug.Log("DeviceInfo "+strDevice);
										}
								}
								Marshal.FreeHGlobal(ptr);
						}
				}
		}

		public void OnRecvFrame(IntPtr pDevice, IntPtr pImageBuffer, ref DeviceFrameInfo pFrInfo, IntPtr lParam)
		{
				IntPtr pRGB24Buff = CGAPI.DeviceISP(mDeviceHandle, pImageBuffer, ref pFrInfo);
				if (pRGB24Buff == IntPtr.Zero) {
						return;
				}
				//CGAPI.DeviceDisplayRGB24(mDeviceHandle, pRGB24Buff, ref pFrInfo); //显示图像有关.

				//Debug.Log("OnRecvFrame...");
				//重点->这里是将IntPtr转换为byte[]数组.
				int countBuf = (int)(pFrInfo.uiWidth * pFrInfo.uiHeight);
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
								//Debug.Log("i " + i + ", val " + bufHandle[i]);
						}
				}

				if (IsPlayCGCam) {
						if (SnapshotCount > 4) {
								SnapshotCount = 0;
						}

						if (SnapshotCount == SnapshotNum) {
								Snapshot();
						}
						SnapshotCount++;
				}

				if (IsShowCGCamFrame) {
						DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
						DateTime nowTime = DateTime.Now;
						long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
						if (LastTimeVal == 0) {
								LastTimeVal = unixTime;
						}
						else {
								CGCamFrameCount++;
								long dTimeVal = unixTime - LastTimeVal;
								if (dTimeVal >= 1000) {
										CameraFrameVal = CGCamFrameCount;
										CGCamFrameCount = 0;
										LastTimeVal = unixTime;
										//Console.WriteLine("dTime " + unixTime + ", camZhenLv " + CamZhenLvVal);
								}
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

								IntPtr renderPtr = GetForegroundWindow();
								devStatus = CGAPI.DeviceInitEx(mDeviceHandle, rfCallBack, IntPtr.Zero, renderPtr, true);
								if (DeviceStatus.STATUS_OK == devStatus)
								{
										Debug.Log("Open CGCamera...");
								}
						}
				}
		}

		void OnGUI()
		{
				if (IsPlayCGCam) {
						if (Time.frameCount % 4 == 0 && SnapshotCount != SnapshotNum) {
								StartCoroutine(GetTexture());
						}

						if (img != null) {
								GUI.DrawTexture(new Rect(10, 10, img.width, img.height), img);
						}
				}

				if (IsShowCGCamFrame) {
						GUI.color = Color.green;
						GUI.Label(new Rect(10f, 270f, 100f, 30), "CameraFps "+CameraFrameVal);
				}
		}

		IEnumerator GetTexture()
		{
				WWW www = new WWW("file://"+ImgPath);
				yield return www;
				if (www.isDone && www.error == null)
				{
						img = www.texture;
				}
		}

		//设置CGCamera的通用参数.
		void SetCGCameraInfo()
		{
				if (mDeviceHandle == IntPtr.Zero) {
						return;
				}
				CGAPI.SetMirror(mDeviceHandle, emMirrorDirection.MD_HORIZONTAL, false);
				CGAPI.SetMirror(mDeviceHandle, emMirrorDirection.MD_VERTICAL, false);
				CGAPI.SetAnalogGain(mDeviceHandle, 63);
				CGAPI.SetFrameSpeed(mDeviceHandle, emDeviceFrameSpeed.HIGH_SPEED, false);
				CGAPI.SetFrameSpeedTune(mDeviceHandle, 0);

				//在这里设置摄像头的分辨率.
//				ResolutionParam param = new ResolutionParam();
//				CGAPI.GetResolution(mDeviceHandle, ref param);
//				param.type = 1; //摄像机输出图像的旋转有关.
//				param.dri.devROISize.iHOffset = 0;
//				param.dri.devROISize.iVOffset = 0;
//				param.dri.devROISize.iWidth = 320;
//				param.dri.devROISize.iHeight = 240;
//				CGAPI.SetResolution(mDeviceHandle, param);
		}

		//设置CGCamera的参数,用于调整摄像头位置.
		void SetPlayCGCameraInfo()
		{
				if (mDeviceHandle == IntPtr.Zero) {
						return;
				}
				CGAPI.SetExposureTime(mDeviceHandle, 16383);
		}

		//设置CGCamera的参数,
		void SetFindPointCGCameraInfo()
		{
				if (mDeviceHandle == IntPtr.Zero) {
						return;
				}
				CGAPI.SetExposureTime(mDeviceHandle, 239);
		}

		void Update()
		{
				if (Input.GetKeyUp(KeyCode.P)) {
						SetPlayCGCameraInfo();
				}

				if (Input.GetKeyUp(KeyCode.L)) {
						SetFindPointCGCameraInfo();
				}
		}
}