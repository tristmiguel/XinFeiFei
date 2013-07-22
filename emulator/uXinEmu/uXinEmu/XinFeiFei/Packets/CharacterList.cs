using System;
using uXinEmu.Generic;
using System.Linq;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void CharacterList()
		{
			var packet = new Packet(Opcode.PLAYERLIST);

			packet.WriteInt32(DateTime.UtcNow.GetUnixTimestamp());
			packet.WriteByte((byte) (User.AcceptedAgreement ? 0 : 1));

			var characters = User.Characters.Where(c => !c.Deleted && !c.Blocked).ToList();
			packet.WriteByte((byte) characters.Count);
			foreach (var character in characters)
			{
				packet.WriteInt32(character.Slot);

				packet.WriteString(character.Name);
				packet.WriteInt32(character.Id);

				packet.WriteInt32(character.WorldId);
				packet.WriteByte(character.Gender);
				packet.WritePosition(character.Position);

				packet.WriteInt32(character.Level);
				packet.WriteInt32(character.Job);

				packet.WriteInt32(character.Stats.STR);
				packet.WriteInt32(character.Stats.STA);
				packet.WriteInt32(character.Stats.DEX);
				packet.WriteInt32(character.Stats.INT);
				packet.WriteInt32(character.Stats.SPI);

				packet.WriteInt32(character.Appearance.HairMesh);
				packet.WriteUInt32((uint) character.Appearance.HairColor);
				packet.WriteInt32(character.Appearance.HeadMesh);

				packet.WriteInt32(character.Blocked ? 1 : 0);
				packet.WriteInt32(character.BlockTime);

				//TODO: Clean this up.

				var container = new ItemContainer<InventoryItem>(character, character.InventoryItems, (byte) Define.MAX_INVENTORY, (byte) Define.MAX_HUMAN_PARTS, Constants.InventorySlots);

				var equipmentItems = container.EquippedItems.ToList();

				packet.WriteInt32(equipmentItems.Count); //TODO: Implement non-fashion items.

				//  for i = 1, count do
				//    parts = LAr:ReadByte(ar)
				//    item_id = LAr:ReadDword(ar)
				//    flag = LAr:ReadDword(ar)
				//    attr = LAr:ReadInt(ar)
				//  end

				for(var i = 1; i <= 5; i++)
				{
					var item = character.Closet.Items.FirstOrDefault(c => c.Equipped && c.Index == i);

					packet.WriteInt32(item != null ? item.ItemId : 0);
				}
			}

			packet.WriteInt32(User.Characters.Count(c => c.Deleted));

			//TODO: Implement city/province.
			packet.WriteInt32(0); //City Id
			packet.WriteInt32(0); //Province Id

			packet.WriteSByte(20); //TODO: Find out what this is.

			Send(packet);
		}

		private void OnCreatePlayer(Packet packet)
		{
			var result = new Packet(Opcode.CREATEPLAYERRESULT);

			var name = packet.ReadString();
			var hdHash = packet.ReadString();
			var slot = packet.ReadInt32();
			var job = packet.ReadByte();
			var gender = packet.ReadByte();
			var hairMesh = packet.ReadByte();
			var hairColor = packet.ReadUInt32();
			var headMesh = packet.ReadByte();
			var city = packet.ReadInt32();
			var zodiacSign = packet.ReadInt32();
			var country = packet.ReadByte();
			var snCard = packet.ReadString();
			var cardType = packet.ReadInt32();
			var hdSerialNumber = packet.ReadString();
			var binAccount = packet.ReadString();
			var clothList = new int[5];
			for (var i = 0; i < 5; i++)
				clothList[i] = packet.ReadInt32();

			if (!Constants.CharacterCreationEnabled)
			{
				result.WriteInt32((int) Error.ERR_NOCREATE_ALL);
			}
			else if (Database.Characters.Any(c => c.Name == name))
			{
				result.WriteInt32((int) Error.ERR_PLAYER_EXIST);
			}
			else if (User.Characters.Count(c => !c.Deleted && !c.Blocked) > 3 || User.Characters.Count > 10 || User.Characters.Any(c => c.Slot == slot)) //TODO: Check items.
			{
				result.WriteInt32((int) Error.ERR_NOCREATE);
			}
			else
			{
				var character = new Character
				{
					Name = name,
					Slot = slot,
					Job = job,
					Gender = gender,
					Appearance = new CharacterAppearance
					{
						HairMesh = hairMesh,
						HairColor = (int) hairColor,
						HeadMesh = headMesh
					},
					PersonalInformation = new PersonalInformation
					{
						City = city,
						ZodiacSign = zodiacSign
					},
					Closet = new Closet()
				};

				for (var i = 0; i < 5; i++)
				{
					if (clothList[i] == 0) continue;

					character.Closet.Items.Add(new ClosetItem
					                             {
						                             ItemId = clothList[i],
													 Index = i + 1,
													 DateTime = DateTime.UtcNow,
													 Equipped = true
					                             });
				}

				User.Characters.Add(character);

				User.AcceptedAgreement = true;

				Database.SaveChanges(); //TODO: Fix this, shouldn't do a full DB update for this...

				result.WriteInt32((int) Error.ERR_SUCCESS);

				Send(result);

				var playerSlot = new Packet(Opcode.PLAYERSLOT);

				playerSlot.WriteInt32(character.Slot);

				playerSlot.WriteString(character.Name);
				playerSlot.WriteInt32(character.Id);

				playerSlot.WriteInt32(character.WorldId);
				playerSlot.WriteByte(character.Gender);
				playerSlot.WritePosition(character.Position);

				playerSlot.WriteInt32(character.Level);
				playerSlot.WriteInt32(character.Job);

				playerSlot.WriteInt32(character.Stats.STR);
				playerSlot.WriteInt32(character.Stats.STA);
				playerSlot.WriteInt32(character.Stats.DEX);
				playerSlot.WriteInt32(character.Stats.INT);
				playerSlot.WriteInt32(character.Stats.SPI);

				playerSlot.WriteInt32(character.Appearance.HairMesh);
				playerSlot.WriteUInt32((uint) character.Appearance.HairColor);
				playerSlot.WriteInt32(character.Appearance.HeadMesh);

				playerSlot.WriteInt32(character.Blocked ? 1 : 0);
				playerSlot.WriteInt32(character.BlockTime);

				//TODO: Clean this up.

				var container = new ItemContainer<InventoryItem>(character, character.InventoryItems, (byte) Define.MAX_INVENTORY, (byte) Define.MAX_HUMAN_PARTS, Constants.InventorySlots);

				var equipmentItems = container.EquippedItems.ToList();

				playerSlot.WriteInt32(equipmentItems.Count); //TODO: Implement non-fashion items.

				//  for i = 1, count do
				//    parts = LAr:ReadByte(ar)
				//    item_id = LAr:ReadDword(ar)
				//    flag = LAr:ReadDword(ar)
				//    attr = LAr:ReadInt(ar)
				//  end

				foreach (var fashionItem in character.Closet.Items.Where(item => item.Equipped))
					playerSlot.WriteInt32(fashionItem.ItemId);

				playerSlot.WriteInt32(User.Characters.Count(c => c.Deleted));

				Send(playerSlot);

				return;
			}

			Send(result);
		}
	}
}