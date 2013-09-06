using System;
using MonoTouch.UIKit;
using System.Globalization;

namespace Wave.iOS.Extensions
{
	public static class UIColorExtension
	{
		// Ported from UIColor+ColorWithHex by Angelo Villegas
		public static UIColor FromHex(string hex, float alpha)
		{
			int rgb = Int32.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);

			// Bit shift right the hexadecimal's first 2 values
			int red = (rgb >> 16) & 0xFF;
			// Bit shift right the hexadecimal's 2 middle values
			int green = (rgb >> 8) & 0xFF;
			// Bit shift right the hexadecimal's last 2 values
			int blue = rgb & 0xFF;

			return UIColor.FromRGBA(red, green, blue, (int)(255f * alpha));
		}
	}
}

