using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnCommand(Packet packet)
		{
			var commandString = packet.ReadString();

			var tokens = commandString.Split(' ');

			switch (tokens[0])
			{
				case "createitem":
					CreateItem(tokens);
					break;

				default:
					Console.WriteLine("OnCommand() => Command '{0}' not implemented yet!", tokens[0]);
					break;
			}
		}

		private void CreateItem(IList<string> tokens)
		{
			var item = new InventoryItem {Character = Character, ItemId = int.Parse(tokens[1]), StackSize = int.Parse(tokens[2])};

			InventoryItems.Add(item);

			//TODO: Check if inventory is full.

			var snapshot = new Snapshot();
			snapshot.SetType(SnapshotType.CREATEITEM);

			snapshot.WriteInt32(Character.GetHashCode()); //TODO: Change to generated id.
			snapshot.WriteByte(0); //Bag Id
			item.Serialize(snapshot);
			snapshot.WriteByte(1); //Item Count
			snapshot.WriteByte(item.Slot); //Item Slot
			snapshot.WriteInt16((short) item.StackSize); //Item Stack Size

			Send(snapshot);
		}
	}
}