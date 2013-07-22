using System;
using System.Diagnostics;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnStateMessage(Packet packet)
		{
			var msg = (StateMessage) packet.ReadByte();
			var state = (StateType) packet.ReadUInt16();
			var group = packet.ReadByte();

			switch (msg)
			{
				case StateMessage.SYS_SET_STATE:
					switch (state)
					{
						case StateType.STATE_MOVE_TO:
							var unknown = packet.ReadInt32(); //TODO: Figure out what this is.

							var x = packet.ReadInt32();
							var y = packet.ReadInt32();
							var z = packet.ReadInt32();

							//TODO: Add some basic checking.
							Character.Position.X = x / 1000f;
							Character.Position.Y = y / 1000f;
							Character.Position.Z = z / 1000f;

							var update = new Snapshot();
							update.SetType((SnapshotType) 23);
							update.WriteInt32(Character.GetHashCode());
							update.WriteBytes(new byte[] { 0x00, 0x0E, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); //TODO: Figure out how this works.
							update.WriteInt32(x);
							update.WriteInt32(y);
							update.WriteInt32(z);

							SendNearPlayers(update);
							break;

						default:
							Console.WriteLine("OnStateMessage() => StateType not implemented yet! (msg: {0}, state: {1})", msg, state);
							return;
					}
					break;

				default:
					Console.WriteLine("OnStateMessage() => Message not implemented yet! (msg: {0}, state: {1})", msg, state);
					return;
			}
		}
	}
}