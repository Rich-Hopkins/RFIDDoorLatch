using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using Windows.System.Threading;
using Windows.System.Diagnostics;
using System.Threading;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DoorLatch
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private I2cDevice device;
		private Timer periodicTimer;
		private bool lightOn = false;
		private bool initialized = false;
		public MainPage()
		{
			this.InitializeComponent();
			initCommunication();
		}

		private async void initCommunication()
		{
			try
			{
				var settings = new I2cConnectionSettings(8);
				settings.BusSpeed = I2cBusSpeed.StandardMode;
				string aqs = I2cDevice.GetDeviceSelector("I2C1");
				var dis = await DeviceInformation.FindAllAsync(aqs);
				device = await I2cDevice.FromIdAsync(dis[0].Id, settings);
				periodicTimer = new Timer(this.TimerCallback, null, 0, 1000);
				initialized = true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		private void TimerCallback(object state)
		{
			if (!initialized)
			{
				initCommunication();
				return;
			}
			byte[] RegAddrBuf = new byte[] { 8 };
			byte[] ReadBuf = new Byte[11];
			char[] cArray = new char[11];
			try
			{
				device.Read(ReadBuf);
				cArray = System.Text.Encoding.UTF8.GetString(ReadBuf, 0, 11).ToCharArray();
			}
			catch (Exception f)
			{
				Debug.WriteLine(f.Message);
			}

			if (cArray[0] == '0' && cArray.Length == 11)
			{
				
			string str = new string(cArray);
			Debug.WriteLine(str);

			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				BadgeNum.Text = str.ToString();
			});
			ParseData(str);
			}
		}

		private void ParseData(string badgeDoorData)
		{
			string doorNum = badgeDoorData.Substring(1, 2);
			string badgeNum = badgeDoorData.Substring(3);
			if (badgeNum == "6014EAD5")
			{
				SendData("1" + doorNum + badgeNum);
			}
			else
			{
				SendData("0" + doorNum + badgeNum);
			}
		}

		private void SendData(string authorization)
		{
			byte[] b = System.Text.Encoding.UTF8.GetBytes(authorization);

			try
			{
				device.Write(b);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}
	}
}
