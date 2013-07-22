using System;
using System.Linq;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global
	// ReSharper disable RedundantIfElseBlock

	internal partial class FFConnection
	{
		private void OnMoveItem(Packet packet)
		{
			var bagId = packet.ReadByte();
			var sourceIndex = packet.ReadByte();
			var destinationIndex = packet.ReadByte();

			//TODO: Implement other bags.

			if (bagId != 0)
				throw new NotImplementedException();

			InventoryItems.Move(sourceIndex, destinationIndex);

			var snapshot = new Snapshot();
			snapshot.SetType(SnapshotType.MOVEITEM);
			snapshot.WriteInt32(Character.GetHashCode()); //Object Id
			snapshot.WriteByte(bagId);
			snapshot.WriteByte(sourceIndex);
			snapshot.WriteByte(destinationIndex);

			Send(snapshot);

			SendNearPlayers(snapshot);

			//TODO: Send item update to other players in range.
		}

		private void OnDropItem(Packet packet)
		{
			var bagId = packet.ReadByte();
			var index = packet.ReadByte();
			var count = packet.ReadUInt16();

			//TODO: Implement other bags.

			if (bagId != 0)
				throw new NotImplementedException();

			if (Constants.DeleteItemOnDrop)
				InventoryItems.DecreaseStack(index, count);
			else
				throw new NotImplementedException();

			var item = InventoryItems[index];
			var newSize = item != null ? item.StackSize : 0;

			var snapshot = new Snapshot();
			snapshot.SetType(SnapshotType.UPDATEITEM);
			snapshot.WriteInt32(Character.GetHashCode()); //Object Id
			snapshot.WriteByte(0); //Bag Id
			snapshot.WriteByte(index);
			snapshot.WriteInt32((int) UpdateItemType.NUM);
			snapshot.WriteInt32(newSize);

			Send(snapshot);
		}
	}
}