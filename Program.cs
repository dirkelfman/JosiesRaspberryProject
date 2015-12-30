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
		static SoundPlayer player;
		static OutputPinConfiguration led1;
		static OutputPinConfiguration led2;
		static ProcessorPin buttonPin;

		public static void Main3 (string[] args)
		{
		
			var driver = GpioConnectionSettings.DefaultDriver;
			led1 = ConnectorPin.P1Pin11.Output ();
			led2 = ConnectorPin.P1Pin13.Output ();
			buttonPin = ProcessorPin.Pin18;
			var filename = @"/usr/share/scratch/Media/Sounds/Electronic/Whoop.wav";
			var stream = new MemoryStream ();
			using (var file = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				file.CopyTo (stream);
			}
			//var player = new SoundPlayer (stream);
			var isPlaying = false;
			driver.Allocate (buttonPin, PinDirection.Input);
			driver.Allocate (led1.Pin, PinDirection.Output);
			driver.Allocate (led2.Pin, PinDirection.Output);


			bool toggle = true;
			while (true) {

				var pressed = !driver.Read (buttonPin);
				if (pressed) { 
					if (!isPlaying) {
						isPlaying = true;
						//stream.Position = 0;
						//player.Dispose ();
						//player.Stream.Seek (0, SeekOrigin.Begin);
						//player.Play ();
						Task.Run (() => {
							player.Stream.Position = 0;
							player.PlaySync ();
							//	isPlaying = false;
						});
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

		private static void TooClose ()
		{

		}

		private static void Main (string[] args)
		{
			Console.CursorVisible = false;


			var driver = GpioConnectionSettings.DefaultDriver;
			led1 = ConnectorPin.P1Pin11.Output ();
			led2 = ConnectorPin.P1Pin13.Output ();
			buttonPin = ProcessorPin.Pin18;
			var filename = @"/usr/share/scratch/Media/Sounds/Electronic/ComputerBeeps1.wav";
			var stream = new MemoryStream ();
			using (var file = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				file.CopyTo (stream);
			}
			stream.Position = 0;
			//player = new SoundPlayer (filename);
			var isPlaying = false;
			driver.Allocate (buttonPin, PinDirection.Input);
			driver.Allocate (led1.Pin, PinDirection.Output);
			driver.Allocate (led2.Pin, PinDirection.Output);




			const ConnectorPin triggerPin = ConnectorPin.P1Pin10;
			const ConnectorPin echoPin = ConnectorPin.P1Pin08;

			Console.WriteLine ("HC-SR04 Sample: measure distance");
			Console.WriteLine ();
			Console.WriteLine ("\tTrigger: {0}", triggerPin);
			Console.WriteLine ("\tEcho: {0}", echoPin);
			Console.WriteLine ();

			var interval = GetInterval (args);
			//var driver = GpioConnectionSettings.DefaultDriver;
			bool toggle = true;
			using (var connection = new HcSr04Connection (
				                        driver.Out (triggerPin.ToProcessor ()), 
				                        driver.In (echoPin.ToProcessor ()))) {
				while (true) {
					try {
						var distance = connection.GetDistance ().Centimeters;
						if (distance < 100) {
							if (!isPlaying) {
								isPlaying = true;
								Task.Run (() => {

								
									stream.Position = 0;

									//player = new SoundPlayer(
									lock (stream) {
										using (player = new SoundPlayer (stream)) {
											try {
												player.PlaySync ();
											} catch (Exception ex) {
												Console.WriteLine (ex.ToString ());
											}
											Timer.Sleep (TimeSpan.FromSeconds (3));
										}
									}
									isPlaying = false;
								});
							}
							driver.Write (led1.Pin, toggle);
							toggle = !toggle;
							driver.Write (led2.Pin, toggle);
						} else {
							if (isPlaying) {
								//lock (player) {
								//	player.Stop ();
								//}
								//player.Dispose();
								//	isPlaying = false;
							}
							driver.Write (led1.Pin, false);
							driver.Write (led2.Pin, false);
						}
						//Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.0}cm", distance).PadRight(16));
						//Console.CursorTop--;
					} catch (TimeoutException e) {
						Console.WriteLine ("(Timeout): " + e.Message);
					}

					Timer.Sleep (interval);
				}
			}

			Console.CursorVisible = true;
		}

		#region Private Helpers

		private static TimeSpan GetInterval (IEnumerable<string> args)
		{
			return TimeSpan.FromMilliseconds (args
				.SkipWhile (a => a != "-interval")
				.Skip (1)
				.Select (int.Parse)
				.DefaultIfEmpty (100)
				.First ());
		}

		#endregion

	}
}