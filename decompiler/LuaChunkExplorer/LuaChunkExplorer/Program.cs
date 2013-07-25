using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LuaChunkExplorer
{
	internal static class Program
	{
		private static void Main()
		{
			//***How to use:***
			//0. Change the paths in the hardcoded strings of the extractor application (uWDFExtractor) and this application to reflect the location of your game installation and the luadec.exe binary.
			//1. Use the extractor to extract 'addons\Configure' from the Config package.
			//2. Use (*) to decompile the file you have just extracted.
			//3. Use (**) to extract the filenames of the remaining scripts from the decompiled file.
			//4. Use the extractor to extract these files (using the luaFiles.txt file you have created in step 3).
			//5. Finally, use (*) to decompile these files.
			//6. (Nearly) all scripts should now be decompiled!

			//(*)
			SaveDecompiledFiles(@"P:\XinFeiFei\gameres\addons");

			//(**)
			var lines = File.ReadAllLines(@"P:\XinFeiFei\gameres\addons\Configure.lua");
			File.WriteAllText(@"P:\XinFeiFei\gameres\luaFiles.txt", (from line in lines where line.Contains("DoFile(") select new string(line.Substring(line.IndexOf("DoFile(\"", StringComparison.Ordinal) + 8).TakeWhile(c => c != '\"').ToArray())).Aggregate("", (current, filename) => current + filename + "\r\n"));

			Console.ReadKey();
		}

		private static void EncodeFile(string file)
		{
			var data = File.ReadAllBytes(file);

			for (var i = 0; i < data.Length; i++)
				data[i] = (byte) ((byte) (data[i] << 4 | data[i] >> 4) ^ 194);

			File.WriteAllBytes(file, data);
		}
		
		public static void DecodeFile(string file)
		{
			var data = File.ReadAllBytes(file);

			for (var i = 0; i < data.Length; i++)
			{
				data[i] ^= 194;
				data[i] = (byte) ((data[i] << 4) | (data[i] >> 4));
			}

			File.WriteAllBytes(file, data);
		}

		private static void SaveDecompiledFiles(string directory)
		{
			foreach (var path in Directory.EnumerateFiles(directory).Where(path => !path.EndsWith(".lua") && !File.Exists(path + ".lua")))
			{
				Console.WriteLine("{0} > {0}.lua", path);

				var info = new ProcessStartInfo("luadec.exe", path) { RedirectStandardOutput = true, UseShellExecute = false };

				var process = Process.Start(info);
				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();

				File.WriteAllText(path + ".lua", output);
			}
		}
	}
}