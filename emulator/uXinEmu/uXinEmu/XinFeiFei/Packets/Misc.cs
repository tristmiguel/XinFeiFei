using System;
using System.Runtime.InteropServices;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		[DllImport("kernel32.dll")]
		private static extern uint GetTickCount();

		private void OnQueryTickCount(Packet packet)
		{
			//TODO: Find out what this really is (not in Lua files) => Sending GetTickCount() for now...

			var response = new Packet(Opcode.TICKCOUNT);
			response.WriteUInt32(GetTickCount());

			Send(response);
		}

		private void OnCollectClientLog(Packet packet)
		{
			var playerId = packet.ReadInt32();
			var systemId = packet.ReadByte();
			var logString = packet.ReadString();

			if (Character != null && Character.Id == playerId)
				Character.LogEntries.Add(new LogEntry {SystemId = systemId, LogString = logString, DateTime = DateTime.Now});
			else
				Disconnect();
		}
	}
}