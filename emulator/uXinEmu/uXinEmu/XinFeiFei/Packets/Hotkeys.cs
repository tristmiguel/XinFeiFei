using System;
using System.Collections.Generic;
using System.Linq;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	static class Hotkeys
	{
		public static readonly List<Tuple<int, int>> Default = new List<Tuple<int, int>>
		{
				new Tuple<int, int>(0x00000043, 0x00000000),
				new Tuple<int, int>(0x00000056, 0x00000000), 
				new Tuple<int, int>(0x00000055, 0x00000000), 
				new Tuple<int, int>(0x0000004E, 0x00000000), 
				new Tuple<int, int>(0x00000049, 0x00000000), 
				new Tuple<int, int>(0x00000042, 0x00000000), 
				new Tuple<int, int>(0x0000004C, 0x00000000), 
				new Tuple<int, int>(0x00000045, 0x00000000), 
				new Tuple<int, int>(0x00000050, 0x00000000), 
				new Tuple<int, int>(0x00000031, 0x00000000), 
				new Tuple<int, int>(0x00000032, 0x00000000), 
				new Tuple<int, int>(0x00000033, 0x00000000), 
				new Tuple<int, int>(0x00000034, 0x00000000), 
				new Tuple<int, int>(0x00000035, 0x00000000), 
				new Tuple<int, int>(0x00000036, 0x00000000), 
				new Tuple<int, int>(0x00000037, 0x00000000), 
				new Tuple<int, int>(0x00000038, 0x00000000), 
				new Tuple<int, int>(0x00000039, 0x00000000), 
				new Tuple<int, int>(0x00000030, 0x00000000), 
				new Tuple<int, int>(0x00000070, 0x00000000), 
				new Tuple<int, int>(0x00000071, 0x00000000), 
				new Tuple<int, int>(0x00000072, 0x00000000), 
				new Tuple<int, int>(0x00000073, 0x00000000), 
				new Tuple<int, int>(0x00000074, 0x00000000), 
				new Tuple<int, int>(0x00000075, 0x00000000), 
				new Tuple<int, int>(0x00000076, 0x00000000), 
				new Tuple<int, int>(0x00000077, 0x00000000), 
				new Tuple<int, int>(0x00000078, 0x00000000), 
				new Tuple<int, int>(0x00000079, 0x00000000), 
				new Tuple<int, int>(0x00000054, 0x00000000), 
				new Tuple<int, int>(0x00000057, 0x00000000), 
				new Tuple<int, int>(0x00000053, 0x00000000), 
				new Tuple<int, int>(0x00000041, 0x00000000), 
				new Tuple<int, int>(0x00000044, 0x00000000), 
				new Tuple<int, int>(0x00000046, 0x00000000), 
				new Tuple<int, int>(0x0000005A, 0x00000011), 
				new Tuple<int, int>(0x00000047, 0x00000011), 
				new Tuple<int, int>(0x00000058, 0x00000011), 
				new Tuple<int, int>(0x00000031, 0x00000012), 
				new Tuple<int, int>(0x00000032, 0x00000012), 
				new Tuple<int, int>(0x00000033, 0x00000012), 
				new Tuple<int, int>(0x00000034, 0x00000012), 
				new Tuple<int, int>(0x00000035, 0x00000012), 
				new Tuple<int, int>(0x00000036, 0x00000012), 
				new Tuple<int, int>(0x00000037, 0x00000012), 
				new Tuple<int, int>(0x00000038, 0x00000012), 
				new Tuple<int, int>(0x00000039, 0x00000012)
		};

		
		public static IEnumerable<Tuple<int, int>> GetHotkeys(this Character character)
		{
			var hotkeys = new List<Tuple<int, int>>(Default);

			foreach (var hotkey in character.Hotkeys)
				hotkeys[hotkey.Index - 1] = new Tuple<int, int>(hotkey.Key, hotkey.Modifier);

			return hotkeys;
		}

		//TODO: Convert to DB function or add this to Character class directly.
		public static void SetHotkey(this Character character, int index, int key, int modifier)
		{
			var defaultHotkey = Default[index - 1];

			if (defaultHotkey.Item1 == key && defaultHotkey.Item2 == modifier)
			{
				var oldEntry = character.Hotkeys.FirstOrDefault(h => h.Index == index);

				if (oldEntry != null)
					Program.Database.DeleteObject(oldEntry);

				return;
			}

			var hotkeyEntry = character.Hotkeys.FirstOrDefault(h => h.Index == index);

			if (hotkeyEntry == null)
				character.Hotkeys.Add(new Hotkey { Index = index, Key = key, Modifier = modifier });	
			else
			{
				hotkeyEntry.Key = key;
				hotkeyEntry.Modifier = modifier;
			}
		}
	}

	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnHotkeyChange(Packet packet)
		{
			var count = packet.ReadInt32();

			for (var i = 0; i < count; i++)
				Character.SetHotkey(packet.ReadInt32(), packet.ReadInt32(), packet.ReadInt32());
		}
	}
}
