using System;
using System.Security.Cryptography;
using System.Text;

namespace uXinEmu.Generic
{
	public static class Extensions
	{
		private static readonly MD5CryptoServiceProvider MD5CryptoService = new MD5CryptoServiceProvider();

		public static string GetMD5Hash(this string input)
		{
			var hashBytes = MD5CryptoService.ComputeHash(Encoding.GetEncoding("gb2312").GetBytes(input));

			var hashStringBuilder = new StringBuilder();

			foreach (var hashByte in hashBytes)
				hashStringBuilder.Append(hashByte.ToString("x2"));

			return hashStringBuilder.ToString();
		}

		public static int GetUnixTimestamp(this DateTime dateTime)
		{
			return (int) (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime()).TotalSeconds;
		}
	}
}
