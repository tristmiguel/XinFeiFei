using System.Collections.Generic;
using System.Linq;
using uXinEmu.Generic;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void WriteObjectData(Snapshot snapshot, Character character, bool full = false)
		{
			var checkValues = new Dictionary<string, int>
			                  {
				                  {"base", 0x77777777},
				                  {"extend", 0x11111111},
				                  {"inventory", 0x22222222},
				                  {"taskbar", 0x33333333},
				                  {"quest", 0x44444444},
				                  {"messenger", 0x55555555},
				                  {"skill", 0x66666666},
				                  {"tbag", 0x7777777a},
				                  {"credit", 0x7777777b},
				                  {"faction", 0x7777777c},
				                  {"lover", 0x77777790},
				                  {"closet", 0x77777791},
				                  {"vessel", 0x77777792},
				                  {"hotkey", 0x77777793}
			                  };

			snapshot.SetType(SnapshotType.ADDOBJ);
			snapshot.WriteUInt32(389); //85 01 00 00 (Object Type: Player)
			snapshot.WriteInt32(character.GetHashCode()); //2A 79 85 00 (Object Id) //TODO: Generate Id to support more entities.
			snapshot.WriteInt32(character.Id);
			snapshot.WriteByte(character.Gender);
			snapshot.WriteUInt32(character.Job);
			snapshot.WriteUInt32((uint) Authority.ADMINISTRATOR);
			snapshot.WriteUInt32(Constants.ServerId); //C0 0D 03 00 (Server Id)
			snapshot.WriteString(""); //00 00 (VIP Bar Name => ?)
			snapshot.WriteUInt32(12); //0C 00 00 00 (Index => ?)
			snapshot.WriteUInt32(0xFFFFFFFF); //FF FF FF FF (Link Id => ?)
			snapshot.WriteString(character.Name);
			snapshot.WriteInt16((short) character.PersonalInformation.ZodiacSign);
			snapshot.WriteInt16((short) character.PersonalInformation.City);
			snapshot.WriteByte((byte) character.Appearance.HairMesh);
			snapshot.WriteUInt32((uint) character.Appearance.HairColor);
			snapshot.WriteByte((byte) character.Appearance.HeadMesh);
			snapshot.WriteUInt32(0x7FFFFFFF); //FF FF FF 7F (Option => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Team Id)
			snapshot.WriteString(""); //00 00 (Vendor String => ?)
			snapshot.WriteInt32(character.PKInfo.Kills);
			snapshot.WriteInt32(character.PKInfo.Deaths);
			snapshot.WriteInt32(character.PKInfo.Fame);
			snapshot.WriteByte(character.PKInfo.PKMode);
			snapshot.WriteByte(character.PKInfo.PKName);
			snapshot.WriteInt32(character.PKInfo.GreyTime);
			snapshot.WriteUInt32((uint) character.PKInfo.ProtectionCooldownStart);
			snapshot.WriteUInt32(0); //00 00 00 00 (Is 'sf' => ?)
			snapshot.WritePosition(character.Position);
			snapshot.WriteUInt32(0x42A1C502); //02 C5 A1 42 (Angle => Yaw (Y)) -> float
			snapshot.WriteUInt32(0x00000000); //00 00 00 00 (Angle X => Pitch?) -> float
			snapshot.WriteUInt16((ushort) character.Scale); //64 00 (Player Scale)
			snapshot.WriteUInt16(9); //09 00 (Serialize State Length => ?)
			snapshot.WriteBytes(new byte[] {0x76, 0x31, 0x24, 0x32, 0x34, 0x2C, 0x24, 0x24, 0x24});
			//76 31 24 32 34 2C 24 24 24 (Serialize State Data => ?)

			var lowExperience = (int) (character.Experience << 31 >> 31);
			var highExperience = (int) (character.Experience >> 31);

			var streamData = new Dictionary<Define, int>
			                 {
				                 {Define.HP, 100}, //HP
				                 {Define.MP, 100}, //MP
				                 {Define.MAXMP, 100},
				                 {Define.MAXHP, 100},
				                 {Define.SGOLD, character.Savings}, //储蓄 (savings, NPCs only)
				                 {Define.EP, lowExperience}, //Experience (low part)
				                 {Define.EP2, highExperience}, //Experience (high part)
				                 {Define.LV, character.Level}, //character level
				                 {Define.GOLD, character.SilverCoins}, //银币 (silver coins, regular money)
				                 {Define.VIT, 99999}, //???
				                 {Define.FLV, character.FlyingStats.Level}, //Flying Level
								 {Define.FEP, character.FlyingStats.Experience},
				                 {Define.FHP, 600}, //Flying HP
				                 {Define.FMP, 450}, //Flying MP
				                 {Define.MOVE_SPEED, 6000}, //Movement speed (default: 6000)
				                 {Define.STR, character.Stats.STR}, //Strength
				                 {Define.STA, character.Stats.STA}, //Stamina
				                 {Define.INT, character.Stats.INT}, //Intelligence
				                 {Define.DEX, character.Stats.DEX}, //Dexterity
				                 {Define.SPI, character.Stats.SPI}, //Spirit
				                 {Define.GP, character.Stats.Available}, //Available stat points
				                 {Define.MONEY, User.GoldCoins}, //金币 (gold coins, credits)
			                 };

			snapshot.WriteInt32(streamData.Count);

			foreach (var pair in streamData)
			{
				snapshot.WriteInt16((short) pair.Key);
				snapshot.WriteX(pair.Value);
			}

			snapshot.WriteByte(0); //00 00 00 00 (Stream Buff Size => ?)
			snapshot.WriteUInt32(0); //06 00 00 00 (Equipment Count)
			snapshot.WriteByte(0); //0C 00 00 00 (Ext. (?) Parameter Count)

			snapshot.WriteUInt32(0); //00 00 00 00 (Player Title Count)
			snapshot.WriteUInt32(8); //07 00 00 00 (Player Title Flag => ?)
			snapshot.WriteByte(0); //00 (Player Title Count => ?)

			snapshot.WriteByte(0); //00 (Player Kingdom Job => ?)
			snapshot.WriteByte(0); //00 (Player Kingdom Sub Job => ?)

			if (character.Family != null)
			{
				snapshot.WriteUInt32((uint) character.Family.Id); //00 00 00 00 (Family Id)
				snapshot.WriteString(character.Family.Name); //00 00 (Family Name)
				snapshot.WriteByte(0); //00 (Family Job)
				snapshot.WriteByte(0); //00 (Title Id => ?)
				snapshot.WriteInt32(character.Family.Popularity); //00 00 00 00 (Family Popularity)
				snapshot.WriteInt32(character.Family.Rank); //00 00 00 00 (Family Rank)
				snapshot.WriteByte(character.Family.Icon); //00 (Family Icon Id)
			}
			else
			{
				snapshot.WriteUInt32(0); //00 00 00 00 (Family Id)
				snapshot.WriteString(""); //00 00 (Family Name)
				snapshot.WriteByte(0); //00 (Family Job)
				snapshot.WriteByte(0); //00 (Title Id => ?)
				snapshot.WriteUInt32(0); //00 00 00 00 (Family Popularity)
				snapshot.WriteUInt32(0); //00 00 00 00 (Family Rank)
				snapshot.WriteByte(0); //00 (Family Icon Id)
			}

			var masterRelationship = character.MasterRelationship;
			if (masterRelationship != null)
			{
				snapshot.WriteString(masterRelationship.Master.Name); //00 00 (Master Name)
				snapshot.WriteString("Master Faction Name"); //00 00 (Master Faction Name => ?)
				snapshot.WriteString("Faction Name A"); //00 00 (Faction Name => ?)
				snapshot.WriteInt32(masterRelationship.Points); //00 00 00 00 (Close Points Master)
			}
			else
			{
				snapshot.WriteString("Master Name"); //00 00 (Master Name)
				snapshot.WriteString("Master Faction Name"); //00 00 (Master Faction Name => ?)
				snapshot.WriteString("Faction Name A"); //00 00 (Faction Name => ?)
				snapshot.WriteUInt32(0); //00 00 00 00 (Close Points Master)
			}

			snapshot.WriteUInt32(character.Level); //0C 00 00 00 (Safety Immunity => ?)

			snapshot.WriteByte(0); //00 (Lover Count)

			snapshot.WriteBoolean(true); //01 (Marriage System Enabled)
			snapshot.WriteByte(0); //00 (Marriage System Kind => ?)

			var equippedFashionItems = character.Closet.Items.Where(item => item.Equipped).ToList();

			snapshot.WriteInt32(equippedFashionItems.Count()); //05 00 00 00 (Show Count => Fashion Items?)
			foreach (var item in equippedFashionItems)
				snapshot.WriteInt32(item.ItemId);

			snapshot.WriteUInt32(1); //?? ?? ?? ?? (Vessel Refine Level => ?)
			snapshot.WriteUInt32(0); //?? ?? ?? ?? (Vessel Equip Index => ?)
			snapshot.WriteBoolean(false); //?? (Vessel Equipped => ?)

			snapshot.WriteByte((byte) (full ? 1 : 0));
			if (!full)
				return;

			snapshot.WriteInt32(checkValues["base"]); //77 77 77 77 (Data Check => Constant)

			snapshot.WriteUInt32(0); //00 00 00 00 (Pet ID)
			snapshot.WriteUInt32(0); //00 00 00 00 (Title Count)

			snapshot.WriteUInt32(2); //03 00 00 00 (Renown Title Count =>)
			snapshot.WriteUInt32(24000); //C0 5D 00 00 (Renown Title => ?)
			snapshot.WriteUInt32(26000); //90 65 00 00 (Renown Title => ?)
			//snapshot.WriteUInt32(22005); //F5 55 00 00 (Renown Title => ?)
			snapshot.WriteByte(0); //00 (Renown Title Host Count => ?)

			//Bag:
			InventoryItems.WriteData(snapshot);

			snapshot.WriteInt32(checkValues["inventory"]);

			//Quest Inventory:
			snapshot.WriteByte((byte) Define.MAX_INVENTORY); //m_item_max
			snapshot.WriteByte((byte) Define.MAX_INVENTORY); //m_index_num
			snapshot.WriteBytes(new byte[]
			                    {
				                    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
				                    0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B,
				                    0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29
			                    }); //Slots?

			snapshot.WriteByte(0); //01  (Item Count)

			snapshot.WriteInt32(checkValues["tbag"]);

			WriteTaskBarItems(snapshot, character.TaskBarItems.Where(i => i.TaskBar == 1).ToList());
			WriteTaskBarItems(snapshot, character.TaskBarItems.Where(i => i.TaskBar == 3).ToList());
			snapshot.WriteInt32(0); //'right_ver' => Not used anymore?

			snapshot.WriteUInt32(4); //04 00 00 00 (Task Bar 'show_to_data' Count => ?)

			for (byte i = 1; i <= 4; i++)
				snapshot.WriteByte(i);

			snapshot.WriteUInt32(0); //00 00 00 00 (Task Bar Lock)

			snapshot.WriteInt32(checkValues["taskbar"]);

			snapshot.WriteUInt32(0); //03 00 00 00 (Skill Count)

			snapshot.WriteInt32(checkValues["skill"]);

			snapshot.WriteByte(1); //01 (Quest Change Flag => Always 1?)
			snapshot.WriteUInt32(0); //01 00 00 00 (Quest Count)
			snapshot.WriteInt32(0); //05 00 00 00 (Completed Quest Count)
			snapshot.WriteUInt16(0); //00 00 ('repeat_quest' => ?)

			snapshot.WriteInt32(checkValues["quest"]);

			snapshot.WriteUInt32(0); //00 00 00 00 ('my_state' => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 ('favor_value' => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 ('flower_count' => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Friend Count)
			snapshot.WriteUInt32(0); //00 00 00 00 (Blacklist Count)
			snapshot.WriteUInt32(0); //00 00 00 00 (Murderer Count)

			snapshot.WriteInt32(checkValues["messenger"]);

			snapshot.WriteBoolean(false); //00 (Faction Master Flag)
			snapshot.WriteUInt32(0); //00 00 00 00 (Faction Protege Count => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Faction Honor Points => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Faction Honor Points Total => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Faction Level Points => ?)
			snapshot.WriteBoolean(false); //00 (Faction Is Faction => ?)
			snapshot.WriteString("Faction Name B"); //00 00 (Faction Name)

			snapshot.WriteInt32(checkValues["faction"]);

			snapshot.WriteUInt32(0); //00 00 00 00 (Position Count => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Duels Won)
			snapshot.WriteUInt32(0); //00 00 00 00 (Duels Tied)
			snapshot.WriteUInt32(0); //00 00 00 00 (Duels Lost)
			snapshot.WriteUInt32(0); //00 00 00 00 (PKs Won)
			snapshot.WriteUInt32(0); //00 00 00 00 (Total PKs)
			snapshot.WriteUInt32(135); //87 00 00 00 ('adv' Stamina => ?)
			snapshot.WriteByte(1); //01 (Auto Assign => ?)

			snapshot.WriteByte(0); //00 (Credit Card Type => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Limit => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Current Limit => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Recharge => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card 'Pay Recharge' => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Preferential => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card One Day Consume => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Total Consume => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Last Consume Date => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card One Day Trade => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Last Trade Date => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Recharged Money => ?)
			snapshot.WriteUInt32(uint.MaxValue); //FF FF FF FF (Credit Card Trade Points => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Credit Card Recharge Reward => ?)

			snapshot.WriteInt32(checkValues["credit"]);

			snapshot.WriteByte(0); //00 (VIP Level Game => ?)
			snapshot.WriteByte(0); //00 (VIP Level GM => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (VIP Expiry Date => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Last Wage Time Game => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Last Wage Time GM => ?)

			snapshot.WriteByte(0); //00 (Equipment Attribute Stone STR Bonus => ?)
			snapshot.WriteByte(0); //00 (Equipment Attribute Stone DEX Bonus => ?)
			snapshot.WriteByte(0x2D); //2D (Equipment Attribute Stone STA Bonus => ?)
			snapshot.WriteByte(0x0A); //0A (Equipment Attribute Stone SPI Bonus => ?)
			snapshot.WriteByte(0x2D); //2D (Equipment Attribute Stone INT Bonus => ?)

			snapshot.WriteUInt32(0); //00 00 00 00 (Lovers Count)

			snapshot.WriteInt32(checkValues["lover"]);

			snapshot.WriteInt32(character.Closet.Capacity); //08 00 00 00 (Closet Capacity)
			snapshot.WriteInt32(character.Closet.Level); //01 00 00 00 (Closet Level)
			snapshot.WriteInt32(character.Closet.Items.Count); //00 00 00 00 (Fate Number => ?)

			foreach (var t in character.Closet.Items)
			{
				snapshot.WriteInt32(t.Index);
				snapshot.WriteInt32(t.ItemId);
				snapshot.WriteInt32(t.Level);
				snapshot.WriteInt32(t.Equipped ? 1 : 0);

				snapshot.WriteInt32(t.DateTime.GetUnixTimestamp());

				for (var j = 0; j < 15; j++)
					snapshot.WriteInt32(0);
			}

			snapshot.WriteInt32(checkValues["closet"]);

			snapshot.WriteUInt32(1); //01 00 00 00 (Vessel Level => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Vessel Equip Slot => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Vessel Spirit => ?)
			snapshot.WriteUInt32(0); //00 00 00 00 (Vessel Battle => ?
			snapshot.WriteUInt32(0); //00 00 00 00 (Vessel Slots => ?)

			snapshot.WriteInt32(checkValues["vessel"]);

			var hotkeys = character.GetHotkeys();

			foreach (var hotkey in hotkeys)
			{
				snapshot.WriteInt32(hotkey.Item1);
				snapshot.WriteInt32(hotkey.Item2);
			}

			snapshot.WriteInt32(checkValues["hotkey"]);

			snapshot.WriteUInt32(7621499);
		}
	}
}