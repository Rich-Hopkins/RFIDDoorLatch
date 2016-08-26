using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.UI.Xaml.Controls;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;


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
		private DispatcherTimer lightTimer;
		private DispatcherTimer shutdownTimer;
		private bool lightOn = false;
		private bool initialized = false;
		private GpioPin bit0Pin;
		private GpioPin bit1Pin;
		private GpioPin doorPin;
		private GpioPin shutdownPin;
		private GpioPin ledPin;
		private GpioPinValue pinValue;
		private bool gpioStatus = false;

		public MainPage()
		{
			this.InitializeComponent();
			initCommunication();
			InitGpio();
			InitLightTimer();
			InitShutdownTimer();
		}

		private void InitGpio()
		{
			var gpio = GpioController.GetDefault();
			if (gpio == null)
			{
				return;
			}
			bit0Pin = gpio.OpenPin(20);
			bit0Pin.SetDriveMode(GpioPinDriveMode.Output);
			bit0Pin.Write(GpioPinValue.Low);
			bit1Pin = gpio.OpenPin(19);
			bit1Pin.SetDriveMode(GpioPinDriveMode.Output);
			bit1Pin.Write(GpioPinValue.Low);
			doorPin = gpio.OpenPin(21);
			doorPin.SetDriveMode(GpioPinDriveMode.Output);
			doorPin.Write(GpioPinValue.Low);
			ledPin = gpio.OpenPin(16);
			ledPin.SetDriveMode(GpioPinDriveMode.Output);
			ledPin.Write(GpioPinValue.High);
			shutdownPin = gpio.OpenPin(13);
			shutdownPin.SetDriveMode(GpioPinDriveMode.Input);
			shutdownPin.DebounceTimeout = TimeSpan.FromMilliseconds(200);
			shutdownPin.ValueChanged += ShutdownPin_ValueChanged;
			ResetPins();
		}

		private void InitLightTimer()
		{
			lightTimer = new DispatcherTimer();
			lightTimer.Interval = TimeSpan.FromMilliseconds(4000);
			lightTimer.Tick += LightTimer_Tick;
			lightTimer.Stop();
		}

		private void LightTimer_Tick(object sender, object e)
		{
			(sender as DispatcherTimer).Stop();
			ResetPins();
		}

		private void InitShutdownTimer()
		{
			shutdownTimer = new DispatcherTimer();
			shutdownTimer.Interval = TimeSpan.FromMilliseconds(5000);
			shutdownTimer.Tick += ShutdownTimer_Tick;
			shutdownTimer.Stop();
		}

		private void ShutdownTimer_Tick(object sender, object e)
		{
			shutdownTimer.Stop();
			GpioPinValue pv = shutdownPin.Read();
			if (pv == GpioPinValue.High) ShutdownPi();
		}

		private void ShutdownPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			var shutdownTask = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				if (shutdownPin.Read() == GpioPinValue.High && !shutdownTimer.IsEnabled)
				{
					shutdownTimer.Start();
				}
			});
		}

		private void ResetPins()
		{
			bit0Pin.Write(GpioPinValue.Low);
			bit1Pin.Write(GpioPinValue.High);
			doorPin.Write(GpioPinValue.Low);
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
				periodicTimer = new Timer(this.TimerCallback, null, 0, 500);
				initialized = true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		private async void TimerCallback(object state)
		{
			byte[] RegAddrBuf = new byte[] { 8 };
			byte[] ReadBuf = new Byte[8];
			char[] cArray = new char[8];
			try
			{
				device.Read(ReadBuf);
				if (ReadBuf[0] == 0 && ReadBuf[1] == 255 && ReadBuf[2] == 255 && ReadBuf[3] == 255 && ReadBuf[4] == 255 &&
					ReadBuf[5] == 255 && ReadBuf[6] == 255 && ReadBuf[7] == 255) return;
				if (ReadBuf[0] == 70 && ReadBuf[1] == 70 && ReadBuf[2] == 70 && ReadBuf[3] == 70 && ReadBuf[4] == 70 &&
					ReadBuf[5] == 70 && ReadBuf[6] == 70 && ReadBuf[7] == 70) return;
				cArray = System.Text.Encoding.UTF8.GetString(ReadBuf, 0, 8).ToCharArray();
				if (cArray[0] == 'F')
				{

				}
			}
			catch (Exception f)
			{
				Debug.WriteLine(f.Message);
			}


			string str = new string(cArray);
			Debug.WriteLine(str);

			var uiTask = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				txtBadgeNum.Text = str.ToString();
				DateTime time = DateTime.Now;
				string format = "hh:mm:ss tt";
				txtTimeStamp.Text = time.ToString(format);
			});
			ParseData(str);

		}
		
		private async void ParseData(string badgeNum)
		{
			if (badgeNum == "6014EAD5")
			{
				SetAccessGranted(true);
			}
			else
			{
				SetAccessGranted(false);
			}
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				lightTimer.Start();
			});
		}

		private void SetAccessGranted(bool accessGranted)
		{
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				if (accessGranted)
				{
					txtAccessGranted.Text = "Yes";
					bit0Pin.Write(GpioPinValue.High);
					bit1Pin.Write(GpioPinValue.High);
					doorPin.Write(GpioPinValue.High);
				}
				else
				{
					txtAccessGranted.Text = "No";
					bit0Pin.Write(GpioPinValue.High);
					bit1Pin.Write(GpioPinValue.Low);
					doorPin.Write(GpioPinValue.Low);
				}
			});
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			doorPin.Write(GpioPinValue.Low);
			bit0Pin.Write(GpioPinValue.Low);
			bit1Pin.Write(GpioPinValue.Low);
			ledPin.Write(GpioPinValue.Low);
		}
		
		private async void ShutdownPi()
		{
			String URL = "http://localhost:8080/api/control/shutdown";
			System.Diagnostics.Debug.WriteLine(URL);
			StreamReader SR = await PostJsonStreamData(URL);
		}

		private async Task<StreamReader> PostJsonStreamData(String URL)
		{
			HttpWebRequest wrGETURL = null;
			Stream objStream = null;
			StreamReader objReader = null;
			try
			{
				wrGETURL = (HttpWebRequest) WebRequest.Create(URL);
				wrGETURL.Method = "POST";
				wrGETURL.Credentials = new NetworkCredential("Administrator", "6432968");
				HttpWebResponse Response = (HttpWebResponse) (await wrGETURL.GetResponseAsync());
				if (Response.StatusCode == HttpStatusCode.OK)
				{
					objStream = Response.GetResponseStream();
					objReader = new StreamReader(objStream);
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("GetData " + e.Message);
			}
			return objReader;
		}
	}
}
