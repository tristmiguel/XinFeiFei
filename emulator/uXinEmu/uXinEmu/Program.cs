using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using LuaInterface;
using uXinEmu.Generic;
using uXinEmu.XinFeiFei;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu
{
	internal static class Program
	{
		//TODO: Refactor static fields (make server context class, if possible generic)!
		
		public static FFModelContainer Database;

		public static readonly Lua Lua = new Lua();
		public static LuaTable ItemData;

		public static List<object> Entities = new List<object>();

		[MTAThread]
		private static void Main()
		{
			Lua.DoFile(@"P:\XinFeiFei\gameres\addons\DefineText.lua");
			Lua.DoFile(@"P:\XinFeiFei\gameres\addons\jobs.lua");
			Lua.DoFile(@"P:\XinFeiFei\gameres\addons\propItem.lua");
			ItemData = Lua.GetTable("propItem");

			Console.WriteLine("Data loaded!");

			Database = new FFModelContainer(ConfigurationManager.ConnectionStrings["FFModelContainer"].ConnectionString);

			AppDomain.CurrentDomain.ProcessExit += (EventHandler)((sender, args) => Database.SaveChanges());

			new Thread(() =>
					   {
						   while (Database != null)
						   {
							   Database.SaveChanges();

							   Thread.Sleep(Constants.DatabaseUpdateInterval);
						   }
					   }).Start();

			var server = new TCPServer<FFConnection>();
			server.Listen("127.0.0.1", 9111);
		}
	}
}