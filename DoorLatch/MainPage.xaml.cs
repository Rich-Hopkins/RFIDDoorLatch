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

        public MainPage()
        {
            this.InitializeComponent();
	        initCommunication();
		}

		private async void initCommunication()
		{
			var settings = new I2cConnectionSettings(0x40);
			settings.BusSpeed = I2cBusSpeed.StandardMode;
			string aqs = I2cDevice.GetDeviceSelector("I2C1");
			var dis = await DeviceInformation.FindAllAsync(aqs);
			device = await I2cDevice.FromIdAsync(dis[0].Id, settings);
			periodicTimer = new Timer(this.TimerCallback, null, 0, 100);
		}

		private void TimerCallback(object state)
		{
			byte[] RegAddrBuf = new byte[] { 8 };
			byte[] ReadBuf = new Byte[7];

			try
			{ 
				device.Read(ReadBuf);
			}
			catch (Exception f)
			{
				Debug.WriteLine(f.Message);
			}			


			char[] cArray = System.Text.Encoding.UTF8.GetString(ReadBuf, 0, 3).ToCharArray();
			string str = new string(cArray);
			//int val = BitConverter.ToInt16(ReadBuf, 0);
			Debug.WriteLine(str);

			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				BadgeNum.Text = str.ToString();
			});

			//int val = int.Parse(str);

			int val = 0;
			bool result = int.TryParse(str, out val);
			if (result)
			{
				bool tmpBool = val < 500 ? true : false;

				if (tmpBool != lightOn)
				{
					SwitchLight(tmpBool);
					lightOn = tmpBool;
				}
			}

		}

	    private void SwitchLight(bool lightOn)
	    {
		    string c = lightOn ? "1" : "0";
		    byte[] b = System.Text.Encoding.UTF8.GetBytes(c);
			
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
