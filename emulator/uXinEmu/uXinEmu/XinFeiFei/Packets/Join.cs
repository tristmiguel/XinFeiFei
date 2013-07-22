using System;
using System.Linq;
using uXinEmu.Generic;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnJoin(Packet packet)
		{
			//TODO: Use all fields.

			var characterId = packet.ReadInt32();
			var patchVersion = packet.ReadByte();
			var hdInfo = packet.ReadString();

			Character = User.Characters.FirstOrDefault(c => c.Id == characterId);

			if (Character != null)
			{
				Console.WriteLine(Character.GetHashCode());

				InventoryItems = new ItemContainer<InventoryItem>(Character, Character.InventoryItems, (byte) Define.MAX_INVENTORY, (byte) Define.MAX_HUMAN_PARTS, Constants.InventorySlots);
				QuestItems = new ItemContainer<QuestItem>(Character, Character.QuestItems, (byte) Define.MAX_INVENTORY, 0, Constants.QuestSlots);

				StorageItems = new ItemContainer<StorageItem>(Character, Character.StorageItems, 0, 0, new byte[] { }); //TODO: Implement storage items.

				var response = new Packet(Opcode.JOIN_RIGHT);

				response.WriteInt32(characterId);

				response.WriteInt32(Character.WorldId);
				response.WritePosition(Character.Position);

				response.WriteBoolean(false); //Is festival
				response.WriteInt32(0); //Festival 'yday'

				Send(response);

				var joinSnapshot = new Snapshot();

				joinSnapshot.SetType(SnapshotType.UPDATE_SERVER_TIME);
				joinSnapshot.WriteInt32(DateTime.UtcNow.GetUnixTimestamp());

				WriteObjectData(joinSnapshot, Character, true);

				Send(joinSnapshot);

				var spawnNearPlayers = new Snapshot();
				foreach (var player in NearPlayers)
					WriteObjectData(spawnNearPlayers, player.Character);

				Send(spawnNearPlayers);

				var spawnNewPlayer = new Snapshot();
				WriteObjectData(spawnNewPlayer, Character);

				SendNearPlayers(spawnNewPlayer);
			}
			else
			{
				//NOTE: In theory, it's possible to send JOIN_ERROR and an error code, but the client doesn't seem to use it.

				Disconnect();
			}
		}
	}
}