using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace uWDFExtractor
{
	class WindsoulPackage
	{
		private readonly List<WindsoulFile> _files = new List<WindsoulFile>();

		public WindsoulPackage(string path)
		{
			var directory = path.Contains('\\') ? new string(path.Take(path.LastIndexOf('\\') + 1).ToArray()) : ".\\";
			var name = path.Contains('\\') ? new string(path.Skip(path.LastIndexOf('\\') + 1).ToArray()) : path;

			var files = Directory.EnumerateFiles(directory);

			var enumerable = files as string[] ?? files.ToArray();

			if (!enumerable.Contains(directory + name + ".wdf"))
				return;

			_files.Add(new WindsoulFile(path + ".wdf"));

			for (var i = 1; ; i++)
			{
				var iFile = string.Format("{0}{1}.wd{2}", directory, name, i);

				if (!enumerable.Contains(iFile))
					return;

				_files.Add(new WindsoulFile(directory + iFile));
			}
		}

		public void ExtractFile(string virtualPath, bool lua)
		{
			_files.Any(file => file.ExtractFile(virtualPath, lua, true)); //Hackish way to stop trying to extract when the file is found in one file that's part of the package.
		}

		public byte[] ReadFile(int id)
		{
			return _files.Select(file => file.ReadFile(id)).FirstOrDefault(data => data != null);
		}
	}
}
