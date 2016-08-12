using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
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
				txtBadgeNum.Text = str.ToString().Substring(2);
				DateTime time = DateTime.Now;
				string format = "hh:mm:ss tt";
				txtTimeStamp.Text = time.ToString(format);
			});
			ParseData(str);
			//Timer blankTimer = new Timer(this.BlankForm, null, TimeSpan.FromSeconds(5).Milliseconds, Timeout.Infinite);
			}
		}

		private void BlankForm(object state)
		{
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				txtBadgeNum.Text = "";
				txtTimeStamp.Text = "";
			});
		}

		private void ParseData(string badgeDoorData)
		{
			string doorNum = badgeDoorData.Substring(1, 2);
			string badgeNum = badgeDoorData.Substring(3);
			if (badgeNum == "6014EAD5")
			{
				SetAccessGranted(true);
				SendData("1" + doorNum + badgeNum);
			}
			else
			{
				SetAccessGranted(false);
				SendData("0" + doorNum + badgeNum);
			}
		}

		private void SetAccessGranted(bool accessGranted)
		{
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				if (accessGranted) txtAccessGranted.Text = "Yes";
				else txtAccessGranted.Text = "No";
			});
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
