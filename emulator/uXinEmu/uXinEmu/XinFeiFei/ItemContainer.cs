using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using LuaInterface;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	internal class ItemContainer<T> where T : Item
	{
		public readonly Character Owner;
		private readonly EntityCollection<T> _items;

		public ItemContainer(Character owner, EntityCollection<T> items, byte size, byte extraSize, byte[] slots)
		{
			Owner = owner;

			_items = items;

			Size = size;
			ExtraSize = extraSize;

			Slots = slots;

			Debug.Assert(Slots.Length == FullSize);
		}

		public byte Size { get; private set; }
		public byte ExtraSize { get; private set; }
		private byte FullSize
		{
			get { return (byte) (Size + ExtraSize); }
		}

		public byte[] Slots { get; private set; }

		public IEnumerable<Item> EquippedItems
		{
			get { return _items.Where(item => item.IsEquipped()); }
		}

		public T this[byte slot]
		{
			get { return _items.FirstOrDefault(item => item.Slot == slot); }
		}

		public void Add(T item, byte? slot = null)
		{
			if (slot == null)
				slot = GetEmptySlot();

			if (slot == null)
				return;

			item.Slot = (byte) slot;

			_items.Add(item);
		}

		public byte? GetEmptySlot()
		{
			byte? slot = null;

			for (var i = 0; i < Size; i++)
			{
				if (_items.Any(item => item.Slot == Slots[i]))
					continue;

				slot = Slots[i];

				break;
			}

			return slot;
		}

		public void WriteData(Packet packet)
		{
			packet.WriteByte(FullSize); //m_item_max
			packet.WriteByte(Size); //m_index_num

			packet.WriteBytes(Slots); //Slots?

			var itemList = _items.ToList();

			packet.WriteByte((byte) itemList.Count);

			for (byte i = 0; i < itemList.Count; i++)
			{
				var item = itemList[i];

				item.Serialize(packet);

				packet.WriteByte((byte) Array.IndexOf(Slots, item.Slot)); //00  (Object Index => ?)
			}
		}

		public void Remove(T item)
		{
			if (item == null)
				return;

			Program.Database.DeleteObject(item);

			_items.Remove(item);
		}

		public void Move(byte sourceIndex, byte destinationIndex)
		{
			//TODO: Check if the destination slot is valid.

			var sourceItem = this[Slots[sourceIndex]];
			var destinationItem = this[Slots[destinationIndex]];

			if (sourceItem == null)
			{
				Owner.LogEntries.Add(new LogEntry
				                     {
					                     SystemId = byte.MaxValue,
					                     LogString = "Character trying to move unexisting item!",
					                     DateTime = DateTime.Now
				                     });

				return;
			}

			if (destinationItem == null)
				sourceItem.Slot = Slots[destinationIndex];
			else if (sourceItem.ItemId == destinationItem.ItemId)
			{
				var maximumStackSize = (int) ((double) sourceItem.GetMetadata()["PackMax"]);

				var destinationStackSize = destinationItem.StackSize + sourceItem.StackSize;
				var sourceStackSize = destinationStackSize - maximumStackSize;

				if (destinationStackSize > maximumStackSize)
					destinationStackSize -= sourceStackSize;

				destinationItem.StackSize = destinationStackSize;
				sourceItem.StackSize = sourceStackSize;

				if (sourceStackSize <= 0)
					Remove(sourceItem);
			}
			else
			{
				sourceItem.Slot = Slots[destinationIndex];
				destinationItem.Slot = Slots[sourceIndex];
			}
		}

		public void DecreaseStack(byte index, int count)
		{
			var item = this[index];

			if (item == null || count > item.StackSize)
				return;

			item.StackSize -= count;

			if (item.StackSize == 0)
				Remove(item);
		}
	}

	internal static class ItemExtensions
	{
		public static LuaTable GetMetadata(this Item item)
		{
			return (LuaTable) Program.ItemData[item.ItemId];
		}

		public static bool IsEquipped(this Item item)
		{
			if (!item.IsEquippable())
				return false;

			var itemData = item.GetMetadata();

			return (int) ((double) itemData["Parts"]) == item.Slot;
		}

		public static bool IsEquippable(this Item item)
		{
			var itemData = item.GetMetadata();

			return itemData.Keys.OfType<string>().Contains("Parts");
		}

		public static void Serialize(this Item item, Packet packet)
		{
			packet.WriteByte(item.Slot);
			packet.WriteInt32(item.ItemId);
			packet.WriteUInt32(0x00000000); //EC D9 14 91  (Serial Number)
			packet.WriteX(item.StackSize);
			packet.WriteX(0); //CF 46 00 00  (Hitpoints => Current Durability?)
			packet.WriteX(0); //18 47 00 00  (Maximum Hitpoints => Maximum Durability?)
			packet.WriteUInt32(0x00000000); //00 00 00 00  (Word => ?)
			packet.WriteByte(0x00); //00  (Ability Option => ?)
			packet.WriteByte(0x00); //00  (Item Resistance => ?)
			packet.WriteByte(0x00); //00  (Resistance Ability Option => ?)
			packet.WriteX(0x00000000); //00 00 00 00  (Keep Time)
			packet.WriteByte(0x00); //00  (Item Lock)
			packet.WriteUInt32(0x00000000); //00 00 00 00  (Bind End Time)
			packet.WriteByte(0x00); //00  (Stability => ?)
			packet.WriteByte(0x00); //00  (Quality => ?)
			packet.WriteByte(0x00); //00  (Ability Rate => ?)
			packet.WriteX(0); //00 00 00 00  (Use Time => ?)
			packet.WriteX(0); //00 00 00 00  (Buy 'tm' => Buy Time?)
			packet.WriteX(0); //00 00 00 00  (Price => Sell/Buy?)
			packet.WriteX(0); //00 00 00 00  (Pay 银币 => ?)
			packet.WriteX(0); //00 00 00 00  (Free 银币 => ?)
			packet.WriteUInt32(Constants.ServerId);
			packet.WriteByte(0); //Attributes
		}
	}
}