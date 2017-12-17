using System;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.Devices.Client;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ButtonPressIoT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static string connectionString = "HostName=kapjklhg.azure-devices.net;DeviceId=RaspBerryPiCalvin;SharedAccessKey=eLBqIqmhh3rlr5qU1r4FDjCt3vHcgdroJVou/1OOSVA=";

        private const int BUTTON_PIN = 5;
        private GpioPin buttonPin;
        private int counter = 0;
        public MainPage()
        {
            this.InitializeComponent();
            InitGPIO();
            for (int i = 0; i < 100; i++)
            {
                SendDeviceToCloudMessagesAsync();
            }
        }
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            buttonPin = gpio.OpenPin(BUTTON_PIN);

            if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;

            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                counter++;
            }

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    GpioStatus.Text = "Button Pressed";
                }
                else
                {
                    GpioStatus.Text = "Button Released";
                }
            });
        }
        static async void SendDeviceToCloudMessagesAsync()
        {
            string iotHubUri = "kapjklhg.azure-devices.net"; // ! put in value !
            string deviceId = "RaspBerryPiCalvin"; // ! put in value !
            string deviceKey = "eLBqIqmhh3rlr5qU1r4FDjCt3vHcgdroJVou/1OOSVA="; // ! put in value !

            var deviceClient = DeviceClient.Create(iotHubUri,
                    AuthenticationMethodFactory.
                        CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                    TransportType.Http1);

            var str = "Hello, Cloud! nkjfnsekjfnekjnfjksenfjksenfjk;senfjk;senfjk;senfjk;senfjk;senfjske;nfj;ksenfjk;s";
            var message = new Message(Encoding.ASCII.GetBytes(str));

            await deviceClient.SendEventAsync(message);
        }
    }
}
