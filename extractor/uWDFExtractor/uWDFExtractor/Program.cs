using System;
using System.IO;
using System.Linq;

namespace uWDFExtractor
{
    internal static class Program
    {
        private static void Main()
        {
			var configPackage = new WindsoulPackage("config");
			configPackage.ExtractFile("addons\\Configure", true);

			foreach (var file in File.ReadAllLines("luaFiles.txt").Select(line => line.Replace(@"\\", @"\")))
				configPackage.ExtractFile(file, true);

	        var uiPackage = new WindsoulPackage("ui");

			uiPackage.ExtractFile("icon02\\icon_active_label.dds", false);
			uiPackage.ExtractFile("wnd02\\WndActivePlayer.tga", false);
			uiPackage.ExtractFile("wnd01\\WndFamilyMedalEnable1.tga", false);
			uiPackage.ExtractFile("wnd02\\WndFamilyMedalEnable25.tga", false);
			uiPackage.ExtractFile("item01\\Itm_Mallfhbyzsuit.dds", false);

			var modelPackage = new WindsoulPackage("model");
			modelPackage.ExtractFile("item\\item_genmatsuitbox\\item_genmatsuitbox.mdl", false);
			modelPackage.ExtractFile("chr01\\mvr_female\\part\\suit\\100101\\part_fhbyzsuit.o3d", false);

			var monasteryOfBloodFile = new WindsoulFile("world\\monastery_of_blood.wdf");
			monasteryOfBloodFile.ExtractFile("monastery_of_blood.dyo", true);

            Console.ReadKey();	
        }
    }
}