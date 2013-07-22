using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uWDFExtractor
{
	public sealed class WindsoulFile
	{
		private readonly Stream _file;

		public readonly List<FileEntry> FileEntries = new List<FileEntry>();

		public string FileFormat { get; private set; }
		public int FileCount { get { return FileEntries.Count; } }

		public string Path { get; private set; }

		public WindsoulFile(string path)
		{
			Path = path;

			_file = new FileStream(path, FileMode.Open);

			var reader = new BinaryReader(_file, Encoding.UTF8);

			FileFormat = new string(reader.ReadChars(4));

			var fileCount = reader.ReadInt32();

			var fileTableOffset = reader.ReadInt32();
			_file.Seek(fileTableOffset, SeekOrigin.Begin);

			for (var i = 0; i < fileCount; i++)
				FileEntries.Add(new FileEntry(id: reader.ReadInt32(), offset: reader.ReadInt32(), size: reader.ReadInt32(), space: reader.ReadInt32()));
		}

		public bool ExtractFile(string virtualPath, bool lua, bool package = false)
		{
			var data = ReadFile((int) virtualPath.Hash());

			if (data == null)
			{
				if (!package)
					Console.WriteLine("{0}: Not found.", virtualPath);

				return false;
			}

			new FileInfo(virtualPath).Directory.Create();

			if (lua)
			{
				for (var i = 0; i < data.Length; i++)
				{
					var j = (byte) (data[i] ^ 194);
					j = (byte) ((j << 4) | (j >> 4));

					data[i] = j;
				}

				//'Fix' header to be used with luadec
				var list = new List<byte>(data);
				list.Insert(0, 0);
				data = list.ToArray();
				data[0] = 0x1B;
				data[1] = 0x4C;
				data[2] = 0x75;
				data[3] = 0x61;
				data[4] = 0x51;
			}

			File.WriteAllBytes(virtualPath, data);

			Console.WriteLine("{0}: Extracted.", virtualPath);

			return true;
		}

		public byte[] ReadFile(int id)
		{
			var entry = FileEntries.FirstOrDefault(i => i.Id == id);

			if (entry == default(FileEntry)) return null;

			_file.Seek(entry.Offset, SeekOrigin.Begin);

			return new BinaryReader(_file, Encoding.UTF8).ReadBytes(entry.Size);
		}
	}
}
