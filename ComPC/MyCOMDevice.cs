using UnityEngine;
using System.Collections;
using System.Threading;
using System;
using System.IO.Ports;
using System.Text;
using System.Runtime.InteropServices;

public class MyCOMDevice : MonoBehaviour
{	
		public class ComThreadClass
		{
				public string ThreadName;
				static SerialPort _SerialPort;
				public static int BufLenRead = 39;
				public static int BufLenReadEnd = 4;
				public static  int BufLenWrite = 32;
				public static byte[] ReadByteMsg = new byte[BufLenRead];
				public static byte[] WriteByteMsg = new byte[BufLenWrite];
				static string RxStringData;
				static string _NewLine = "ABCD"; //0x41 0x42 0x43 0x44
				public static int ReadTimeout = 0x07d0;
				public static int WriteTimeout = 0x07d0;
				public static bool IsStopComTX;
				public static bool IsReadMsgComTimeOut;
				public static string ComPortName = "COM1";
				/// <summary>
				/// set IsLoadingLevel is true when loading level, otherwise set IsLoadingLevel is false.
				/// </summary>
				public static bool IsLoadingLevel;
				public static bool IsReadComMsg;
				public static bool IsTestWRPer;
				public static int WriteCount;
				public static int ReadCount;
				public ComThreadClass(string name)
				{
						ThreadName = name;
						OpenComPort();
				}

				public static void OpenComPort()
				{
						if (_SerialPort != null) {
								return;
						}

						_SerialPort = new SerialPort(ComPortName, 38400, Parity.None, 8, StopBits.One);
						_SerialPort.NewLine = _NewLine;
						_SerialPort.Encoding = Encoding.GetEncoding("iso-8859-1");
						_SerialPort.ReadTimeout = ReadTimeout;
						_SerialPort.WriteTimeout = WriteTimeout;
						if (_SerialPort != null)
						{
								try
								{
										if (_SerialPort.IsOpen)
										{
												_SerialPort.Close();
												Debug.Log("Closing port, because it was already open!");
										}
										else
										{
												_SerialPort.Open();
												if (_SerialPort.IsOpen) {
														IsFindDeviceDt = true;
														Debug.Log("COM open sucess");
												}
										}
								}
								catch (Exception exception)
								{
										Debug.LogError("error:COM already opened by other PRG... " + exception);
								}
						}
						else
						{
								Debug.Log("Port == null");
						}
				}

				public void Run()
				{
						do
						{
								IsTestWRPer = false;
								if (IsReadMsgComTimeOut) {
										CloseComPort();
										break;
								}

								if (IsLoadingLevel || IsStopComTX) {
										if (IsStopComTX) {
												IsReadComMsg = false;
										}
										Thread.Sleep(1000);
										continue;
								}

								COMTxData();
//								if (pcvr.IsJiaoYanHid || pcvr.IsSlowLoopCom) {
//										Thread.Sleep(100);
//								}
//								else {
//										Thread.Sleep(25);
//								}
								Thread.Sleep(1);
								COMRxData();
								IsTestWRPer = true;
								//Thread.Sleep(25);
						}
						while (_SerialPort.IsOpen);
						CloseComPort();
						Debug.Log("Close run thead...");
				}

				void COMTxData()
				{
//						if (XkGameCtrl.IsGameOnQuit) {
//								return;
//						}

						try
						{
								pcvr.GetInstance().SendMessage();
								IsReadComMsg = false;
								_SerialPort.Write(WriteByteMsg, 0, WriteByteMsg.Length);
								_SerialPort.DiscardOutBuffer();
								WriteCount += WriteByteMsg.Length;
						}
						catch (Exception exception)
						{
//								if (XkGameCtrl.IsGameOnQuit) {
//										return;
//								}
								Debug.LogError("Tx error:COM!!! " + exception);
						}
				}

				void COMRxData()
				{
//						if (XkGameCtrl.IsGameOnQuit) {
//								return;
//						}

						try
						{
								RxStringData = _SerialPort.ReadLine();
								ReadByteMsg = _SerialPort.Encoding.GetBytes(RxStringData);
								_SerialPort.DiscardInBuffer();
								ReadCount += (ReadByteMsg.Length + BufLenReadEnd);
								IsReadComMsg = true;
								ReadMsgTimeOutVal = 0f;
						}
						catch (Exception exception)
						{
//								if (XkGameCtrl.IsGameOnQuit) {
//										return;
//								}
//
//								if (GameTypeCtrl.AppTypeStatic == AppGameType.LianJiServer) {
//										return;
//								}

								Debug.LogError("Rx error:COM..." + exception);
								IsReadMsgComTimeOut = true;
								IsReadComMsg = false;
						}
				}

				public static void CloseComPort()
				{
						IsReadComMsg = false;
						if (_SerialPort == null || !_SerialPort.IsOpen) {
								return;
						}
						_SerialPort.DiscardOutBuffer();
						_SerialPort.DiscardInBuffer();
						_SerialPort.Close();
						_SerialPort = null;
				}
		}

		static ComThreadClass _ComThreadClass;
		static Thread ComThread;
		public static bool IsFindDeviceDt;
		public static float ReadMsgTimeOutVal;
		static float TimeLastVal;
		static float TimeUnitDelta = 1f;
		public static uint CountRestartCom;
		static MyCOMDevice _Instance;
		public static MyCOMDevice GetInstance()
		{
				if (_Instance == null) {
						GameObject obj = new GameObject("_MyCOMDevice");
						DontDestroyOnLoad(obj);
						_Instance = obj.AddComponent<MyCOMDevice>();

//						if (pcvr.IsTestHardWare) {
//								TestComPort.GetInstance();
//						}
				}
				return _Instance;
		}

		// Use this for initialization
		void Start()
		{
				StartCoroutine(OpenComThread());
		}

		IEnumerator OpenComThread()
		{
				ReadMsgTimeOutVal = 0f;
				ComThreadClass.IsReadMsgComTimeOut = false;
				ComThreadClass.IsReadComMsg = false;
				ComThreadClass.IsStopComTX = false;
				if (_ComThreadClass == null) {
						_ComThreadClass = new ComThreadClass(ComThreadClass.ComPortName);
				}
				else {
						ComThreadClass.CloseComPort();
				}

				if (ComThread != null) {
						CloseComThread();
						ComThread = null;
				}
				yield return new WaitForSeconds(2f);

				ComThreadClass.OpenComPort();
				if (ComThread == null) {
						ComThread = new Thread(new ThreadStart(_ComThreadClass.Run));
						ComThread.Start();
				}
		}

		public void RestartComPort()
		{
				if (!ComThreadClass.IsReadMsgComTimeOut) {
						return;
				}
				CountRestartCom++;
				ScreenLog.Log("Restart ComPort "+ComThreadClass.ComPortName+", time "+(int)Time.realtimeSinceStartup);
				ScreenLog.Log("CountRestartCom: "+CountRestartCom);
				StartCoroutine(OpenComThread());
		}

		void CheckTimeOutReadMsg()
		{
				if (ComThreadClass.IsReadComMsg || ComThreadClass.IsLoadingLevel) {
						ReadMsgTimeOutVal = 0f;
						return;
				}
				ReadMsgTimeOutVal += TimeUnitDelta;

				if ((ReadMsgTimeOutVal * 1000) > (3f * ComThreadClass.ReadTimeout)) {
						ScreenLog.Log("CheckTimeOutReadMsg -> The app should restart to open the COM!");
						ComThreadClass.IsReadMsgComTimeOut = true;
						RestartComPort();
				}
		}

		void Update()
		{
//				if (GameTypeCtrl.AppTypeStatic == AppGameType.LianJiServer) {
//						return;
//				}

				if (Time.realtimeSinceStartup - TimeLastVal < TimeUnitDelta) {
						return;
				}
				TimeLastVal = Time.realtimeSinceStartup;

				if (!ComThreadClass.IsReadComMsg && !ComThreadClass.IsLoadingLevel) {
						CheckTimeOutReadMsg();
				}
		}

		void CloseComThread()
		{
				if (ComThread != null) {
						ComThread.Abort();
				}
		}

		void OnApplicationQuit()
		{
				Debug.Log("OnApplicationQuit...ComDevice");
//				XkGameCtrl.IsGameOnQuit = true;
				ComThreadClass.CloseComPort();
				Invoke("CloseComThread", 2f);
		}
}