using System;
//using System.Threading;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.GeneralPurpose.Behaviors;
using System.Media;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raspberry.IO.Components.Sensors.Distance.HcSr04;
using System.Globalization;

using Raspberry.Timers;
namespace but2
{
	class MainClass
	{

		public static void Main2 ( string[] args){
		
			var driver = GpioConnectionSettings.DefaultDriver;
			var led1 = ConnectorPin.P1Pin11.Output();
			var led2 = ConnectorPin.P1Pin13.Output();
			var butPin = ProcessorPin.Pin18;
			var filename = @"/usr/share/scratch/Media/Sounds/Electronic/Whoop.wav";
			var file = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			var player = new SoundPlayer (file);
			var isPlaying = false;
			driver.Allocate (ProcessorPin.Pin18, PinDirection.Input);
			driver.Allocate (led1.Pin, PinDirection.Output);
			driver.Allocate (led2.Pin, PinDirection.Output);


			bool toggle = true;
			while (true) {

				var pressed = !driver.Read (butPin);
				if (pressed) { 
					if (!isPlaying) {
						isPlaying= true;
						player.Stream.Position =0;
						player.Play ();
						//Task.Run (() => {
							//player.Stream.Position =0;
						//	player.PlayLooping ();
						//	isPlaying = false;
				//		});
					}
					driver.Write (led1.Pin, toggle);
					toggle = !toggle;
					driver.Write (led2.Pin, toggle);
					Console.WriteLine ("pushed");
				} else {
					if (isPlaying) {
						player.Stop ();
						isPlaying = false;
					}
					driver.Write (led1.Pin, false);
					driver.Write (led2.Pin, false);
				}
				System.Threading.Thread.Sleep (200);
			}
			driver.Release (ProcessorPin.Pin18);


		}

		private static void Main(string[] args)
		{
			Console.CursorVisible = false;

			const ConnectorPin triggerPin = ConnectorPin.P1Pin10;
			const ConnectorPin echoPin = ConnectorPin.P1Pin08;

			Console.WriteLine("HC-SR04 Sample: measure distance");
			Console.WriteLine();
			Console.WriteLine("\tTrigger: {0}", triggerPin);
			Console.WriteLine("\tEcho: {0}", echoPin);
			Console.WriteLine();

			var interval = GetInterval(args);
			var driver = GpioConnectionSettings.DefaultDriver;

			using (var connection = new HcSr04Connection(
				driver.Out(triggerPin.ToProcessor()), 
				driver.In(echoPin.ToProcessor())))
			{
				while (!Console.KeyAvailable)
				{
					try
					{
						var distance = connection.GetDistance().Centimeters;
						Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.0}cm", distance).PadRight(16));
						Console.CursorTop--;
					}
					catch (TimeoutException e)
					{
						Console.WriteLine("(Timeout): " + e.Message);
					}

					Timer.Sleep(interval);
				}
			}

			Console.CursorVisible = true;
		}

		#region Private Helpers

		private static TimeSpan GetInterval(IEnumerable<string> args)
		{
			return TimeSpan.FromMilliseconds(args
				.SkipWhile(a => a != "-interval")
				.Skip(1)
				.Select(int.Parse)
				.DefaultIfEmpty(100)
				.First());
		}

		#endregion

	}
}