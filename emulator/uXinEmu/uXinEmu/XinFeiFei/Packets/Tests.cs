using System;
using System.Diagnostics;
using System.Linq;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnNormalChat(Packet packet)
		{
			var chatFlag = packet.ReadByte();
			var chatString = packet.ReadString();
			var infoString = packet.ReadString();

			var splitString = chatString.Split(' ');

			var snapshot = new Snapshot();
			snapshot.SetType(SnapshotType.GET_CLIENT_INFO);

			snapshot.WriteUInt32(1337); //Source Player Id
			snapshot.WriteString(splitString[0]);
			snapshot.WriteString(splitString.Length > 1 ? splitString.Skip(1).Aggregate("", (current, s) => current + s) : "");

			Console.WriteLine("OnNormalChat() => chatstring: {0}", chatString);

			Send(snapshot);
		}

		private void OnGetClientInfo(Packet packet)
		{
			Debug.Assert(packet.ReadUInt32() == 1337);

			var command = packet.ReadString();
			var result = packet.ReadString();

			Console.WriteLine("OnGetClientInfo() => command: {0}, result: {1}", command, result);
		}
	}
}