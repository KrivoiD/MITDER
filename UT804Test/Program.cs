using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UsbRelayNet;
using UsbRelayNet.HidLib;

namespace UT804Test
{
	class Program
	{
		static void Main(string[] args)
		{
			var usbHid = new UsbRelayNet.HidLib.HidEnumerator();
			var result = usbHid.CollectInfo()
				.Where(x => x.VendorID == 0x1A86 && x.ProductId == 0xE008)
				.ToArray();
			var utInfo = result.First();
			Console.WriteLine($"{utInfo.Product}, {utInfo.ProductId}, {utInfo.Vendor}, {utInfo.VendorID}, {utInfo.Version}");

			var device = new HidDevice();
			device.Open(utInfo.Path);
			Console.WriteLine(device.GetVendorString());
			Console.WriteLine(device.GetProductString());
			Console.WriteLine(device.GetType());
			Console.WriteLine(device.GetAttributes().ToString());
			//Console.WriteLine(buffer.Count());
			for (int j = 0; j < 20; j++)
			{

				for(var i=0; i<10; i++)
				{
					byte[] buffer = new byte[64];
					device.GetFeature(j, out buffer);
					var rawData = new byte[64];
					Array.Copy(buffer, 0, rawData, 0, 64);
					Console.WriteLine(string.Join(" ", rawData.Select(x => x.ToString())));
				}
			}
			device.Close();

			Console.ReadKey();
		}
	}



}
