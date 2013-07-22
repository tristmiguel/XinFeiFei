using System;
using System.IO;
using System.Linq;

namespace uXinEmu.XinFeiFei
{
	internal class Snapshot : Packet
	{
		//TODO: Instead of sending them like normal packets, make use of the snapshots' purpose: multiple data updates per snapshot.

		public Snapshot() : base(Opcode.SNAPSHOT)
		{
			WriteUInt16(0); //Snapshot count placeholder
		}

		public ushort SnapshotCount;

		public void SetType(SnapshotType type)
		{
			if (type < (SnapshotType) 128)
				WriteByte((byte) type);
			else
				WriteBytes(BitConverter.GetBytes((ushort) type).Reverse().ToArray());

			SnapshotCount++;
		}

		public override byte[] GetBuffer()
		{
			var position = Writer.BaseStream.Position;
			Writer.BaseStream.Seek(6, SeekOrigin.Begin);
			WriteUInt16(SnapshotCount);
			Writer.BaseStream.Seek(position, SeekOrigin.Begin);

			return base.GetBuffer();
		}
	}
}