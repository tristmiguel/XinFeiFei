using System.Collections.Generic;
using System.Linq;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		public void WriteTaskBarItems(Packet packet, List<TaskBarItem> items)
		{
			packet.WriteInt32(items.Count);

			foreach (var taskbarItem in items)
			{
				packet.WriteInt32(taskbarItem.Line);
				packet.WriteInt32(taskbarItem.Grid);

				packet.WriteInt32(taskbarItem.Shortcut);
				packet.WriteInt32(taskbarItem.ShortcutId);
				packet.WriteInt32(taskbarItem.Type);
				packet.WriteInt32(taskbarItem.Index);
				packet.WriteInt32(taskbarItem.UserId);
				packet.WriteInt32(taskbarItem.Data);
				packet.WriteString(taskbarItem.String);
			}
		}

		private void OnUpdateTaskbar(Packet packet)
		{
			var taskBar = packet.ReadInt32(); //ar:WriteInt(rgn)
			var line = packet.ReadInt32(); //ar:WriteInt(data_line)
			var grid = packet.ReadInt32(); //ar:WriteInt(grid)

			var shortcut = packet.ReadInt32(); //ar:WriteInt(self.m_short_cut)
			var id = packet.ReadInt32(); //ar:WriteInt(self.m_id)
			var type = packet.ReadInt32(); //ar:WriteInt(self.m_type)
			var index = packet.ReadInt32(); //ar:WriteInt(self.m_index)
			var userId = packet.ReadInt32(); //ar:WriteInt(self.m_user_id)
			var data = packet.ReadInt32(); //ar:WriteInt(self.m_data)
			var shortcutString = packet.ReadString(); //ar:WriteString(self.m_string)

			var oldTaskBarItem =
				Character.TaskBarItems.FirstOrDefault(i => i.TaskBar == taskBar && i.Line == line && i.Grid == grid);

			if (oldTaskBarItem != null)
			{
				if (shortcut == 0)
					Program.Database.DeleteObject(oldTaskBarItem);
				else
				{
					oldTaskBarItem.Shortcut = shortcut;
					oldTaskBarItem.ShortcutId = id;
					oldTaskBarItem.Type = type;
					oldTaskBarItem.Index = index;
					oldTaskBarItem.UserId = userId;
					oldTaskBarItem.Data = data;
					oldTaskBarItem.String = shortcutString;
				}
			}
			else
			{
				Character.TaskBarItems.Add(new TaskBarItem
				                           {
					                           TaskBar = taskBar,
					                           Line = line,
					                           Grid = grid,
					                           Shortcut = shortcut,
					                           ShortcutId = id,
					                           Type = type,
					                           Index = index,
					                           UserId = userId,
					                           Data = data,
					                           String = shortcutString
				                           });
			}
		}
	}
}