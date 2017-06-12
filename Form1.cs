using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using CGSDK;
using CGAPI = CGSDK.CGAPI;

using FactoryHandle = System.IntPtr;
using DeviceHandle = System.IntPtr;

namespace Basic
{
//	public partial class Form1 : Form
    public partial class Form1
    {
        public Form1()
        {
//            InitializeComponent();
//            mThis = this;
            uint[] adwVersion = new uint[4];
            CGAPI.DeviceGetSDKVersion(adwVersion);
            SearchDevices();
            if (mDeviceHandle != IntPtr.Zero)
            {
//                bnStart.Text = "Stop";
                CGAPI.DeviceStart(mDeviceHandle);
            }
        }

        static Form1 mThis;

        DeviceHandle mDeviceHandle = IntPtr.Zero;

        void SearchDevices()
        {
//			DeviceStatus devSatus = CGAPI.DeviceInitialSDK(this.Handle, false);
            DeviceStatus devSatus = CGAPI.DeviceInitialSDK(IntPtr.Zero, false);
            if (DeviceStatus.STATUS_OK == devSatus)
            {
//                int iCameraCounts = 0;
//                devSatus = CGAPI.EnumDevice(IntPtr.Zero, ref iCameraCounts);
//                if (DeviceStatus.STATUS_OK == devSatus) {
//                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(new EnumDeviceParam()) * iCameraCounts);
//                    devSatus = CGAPI.EnumDevice(ptr, ref iCameraCounts);
//                    if (DeviceStatus.STATUS_OK == devSatus)
//                    {
//                        for (int i = 0; i < iCameraCounts; i++)
//                        {
//                            ComboBoxItem item = new ComboBoxItem();
//                            EnumDeviceParam edp = (EnumDeviceParam)Marshal.PtrToStructure((IntPtr)((int)ptr + i * Marshal.SizeOf(new EnumDeviceParam())), typeof(EnumDeviceParam));
//                            string strDevice = String.Format("{0} : {1}", edp.lpDeviceDesc, edp.devIndex);
//                            item.Text = strDevice;
//                            item.Value = ((edp.devIndex << 8) | edp.usbAddress);
//                            mThis.AddDevice(item);
//                        }
//                    }
//                    Marshal.FreeHGlobal(ptr);
//                }
//                if (cmbDevices.Items.Count > 0)
//                {
//                    cmbDevices.SelectedIndex = 0;
//                }
            }
        }

//        public static int EnumDeviceCB(ref EnumDeviceParam param, IntPtr lParam)
//        {
//            ComboBoxItem item = new ComboBoxItem();
//            string strDevice = String.Format("{0} : {1}", param.lpDeviceDesc, param.devIndex);
//            item.Text = strDevice;
//            item.Value = ( (param.devIndex << 8)  |  param.usbAddress );
//            mThis.AddDevice(item);
//            return 0;
//        }

        ReaderWriterLock m_readerWriterLock = new ReaderWriterLock();

        public void OnRecvFrame(IntPtr pDevice, IntPtr pImageBuffer, ref DeviceFrameInfo pFrInfo, IntPtr lParam)
        {
            IntPtr pRGB24Buff = IntPtr.Zero;
            if ((pRGB24Buff = CGAPI.DeviceISP(mDeviceHandle, pImageBuffer, ref pFrInfo)) != null)
            {
//                if (false)
//                {
//                    Bitmap bmp = new Bitmap((int)pFrInfo.uiWidth, (int)pFrInfo.uiHeight, (int)pFrInfo.uiWidth, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, pRGB24Buff);
//                    System.Drawing.Imaging.ColorPalette palette = bmp.Palette;
//                    for (int i = 0; i < 256; i++)
//                    {
//                        palette.Entries[i] = Color.FromArgb(i, i, i);
//                    }
//                    bmp.Palette = palette;
//                }
                CGAPI.DeviceDisplayRGB24(mDeviceHandle, pRGB24Buff, ref pFrInfo);
            }
        }

        public static void OnReceiveFrame(IntPtr pDevice, IntPtr pImageBuffer, ref DeviceFrameInfo pFrInfo, IntPtr lParam)
        {
            mThis.OnRecvFrame(pDevice, pImageBuffer, ref pFrInfo, lParam);    
        }

//        public void AddDevice(ComboBoxItem item)
//        {
//            cmbDevices.Items.Add(item);
//        }

        private void Form1_FormClosed()
        {
            if (mDeviceHandle != IntPtr.Zero)
            {
                CGAPI.DeviceStop(mDeviceHandle);
                CGAPI.CloseDevice(mDeviceHandle);
                CGAPI.DeviceUnInit(mDeviceHandle);
                CGAPI.DeviceRelease(mDeviceHandle);
                mDeviceHandle = IntPtr.Zero;
                CGAPI.DeviceUnInitialSDK();
            }
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
//            ComboBoxItem selItem = (ComboBoxItem)cmbDevices.SelectedItem;
//			  byte byDev =(byte) ((int)selItem.Value & 0xFF);
//            labTips.Text = String.Format("select id:{0}",byDev);
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
				byte byDev = 0;
                DeviceStatus devStatus = CGAPI.OpenDeviceByUSBAddress(byDev, ref mDeviceHandle);
                if (DeviceStatus.STATUS_OK == devStatus)
                {
                    ReceiveFrameProc rfCallBack = new ReceiveFrameProc(OnReceiveFrame);
                    //不添加回调函数.
                    //devStatus = CGAPI.DeviceInit(mDeviceHandle, panelVideo.Handle, false, true);
                    //添加回调函数.
					//devStatus = CGAPI.DeviceInitEx(mDeviceHandle, OnReceiveFrame, IntPtr.Zero, panelVideo.Handle, true);
					devStatus = CGAPI.DeviceInitEx(mDeviceHandle, OnReceiveFrame, IntPtr.Zero, IntPtr.Zero, true);
                    if (DeviceStatus.STATUS_OK == devStatus)
                    {
                    }
                }
//                labTips.Text = String.Format("{0}", devStatus);
            }
        }

//        private void bnStart_Click(object sender, EventArgs e)
//        {
//            DeviceStatus devStatus = CGSDK.DeviceStatus.STATUS_DEVICE_NOT_DETECTED;
//            if (mDeviceHandle != IntPtr.Zero)
//            {
//                if (bnStart.Text.Equals("Start"))
//                {
//                    bnStart.Text = "Stop";
//                    devStatus = CGAPI.DeviceStart(mDeviceHandle);
//                }
//                else
//                {
//                    bnStart.Text = "Start";
//                    devStatus = CGAPI.DeviceStop(mDeviceHandle);
//                }
//            }
//            labTips.Text = String.Format("{0}", devStatus);
//        }

//        private void bnSettings_Click(object sender, EventArgs e)
//        {
//            DeviceStatus devStatus = CGAPI.DeviceCreateSettingPage(mDeviceHandle, mThis.Handle, "");
//            devStatus = CGAPI.DeviceShowSettingPage(mDeviceHandle, true);
//            labTips.Text = String.Format("{0}", devStatus);
//        }
//
//        private void bnSnapshot_Click(object sender, EventArgs e)
//        {
//            if (mDeviceHandle != DeviceHandle.Zero)
//            {
//                DateTime time = DateTime.Now;
//                string strFile = "D:\\Image" + time.ToFileTime();
//                DeviceStatus devStatus = CGAPI.CaptureFile(mDeviceHandle, strFile, emDSFileType.FILE_BMP);
//                strFile += ".bmp";
//                if (DeviceStatus.STATUS_OK == devStatus){
//                    MessageBox.Show(strFile, "保存成功");
//                } else{
//                    MessageBox.Show(strFile, "保存失败");
//                }
//            }
//        }
    }
}
