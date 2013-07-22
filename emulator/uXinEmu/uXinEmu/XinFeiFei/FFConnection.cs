using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using FFEncryptionLibrary;
using uXinEmu.Generic;
using uXinEmu.XinFeiFei.Data_Model;

namespace uXinEmu.XinFeiFei
{
    // ReSharper disable ClassNeverInstantiated.Global

    internal partial class FFConnection : Connection<FFConnection>
    {
        private bool _connected;

	    public KeyPair KeyPair { get; private set; }

        #region Overrides of Connection

        private DateTime _lastPacketReceived = DateTime.Now;

        public override void OnConnected()
        {
            _connected = true;

            Console.WriteLine("Client connected!");

			KeyPair = new KeyPair(new Random().Next());

			var writer = new BinaryWriter(new MemoryStream(8));
			writer.Write((uint)8);
			writer.Write(KeyPair.Seed);

			var keyPacket = ((MemoryStream)writer.BaseStream).GetBuffer();
			Send(keyPacket, 0, keyPacket.Length);

            BeginReceive();

            while (_connected)
            {
                if (DateTime.Now - _lastPacketReceived > TimeSpan.FromSeconds(30 + Constants.TimeoutSeconds))
                {
                    Console.WriteLine("Client timed out!");

                    Disconnect();
                }

                Thread.Sleep(100);
            }
        }

        protected override void OnDisconnected()
        {
            _connected = false;

            Console.WriteLine("Client disconnected!");

            User = null;
            Character = null;

            base.OnDisconnected();
        }

        protected override void OnDataReceived(byte[] buffer, int offset, int size)
        {
			KeyPair.Decrypt(ref buffer, offset, size);

            if (size < 2)
            {
                OnDisconnected();

                return;
            }

            _lastPacketReceived = DateTime.Now;

            var packets = new List<Packet>();

            var bytesUsed = 0;

            while (bytesUsed < size)
            {
                var packetSize = BitConverter.ToUInt16(buffer, offset + bytesUsed);

                packets.Add(new Packet(buffer, offset + bytesUsed, packetSize));

                bytesUsed += packetSize;
            }

            foreach (var packet in packets)
            {
				if (Constants.LogLevel >= 2)
					Console.WriteLine("C> {0}", packet.Opcode);

                switch (packet.Opcode)
                {
                    case Opcode.CERTIFY:
                        OnCertify(packet);
                        break;

                    case Opcode.PING:
                        //Ignore.
                        break;
                    case Opcode.QUERYTICKCOUNT:
                        OnQueryTickCount(packet);
                        break;
                    case Opcode.COLLECT_CLIENT_LOG:
                        OnCollectClientLog(packet);
                        break;
					case Opcode.LEAVE:	
		                Disconnect(); //TODO: Handle this correctly.
						Database.SaveChanges();
						Database.DetectChanges();
		                break;
					case Opcode.HOTKEY_CHANGE:
						OnHotkeyChange(packet);
		                break;
					case Opcode.UPDATE_TASKBAR:
						OnUpdateTaskbar(packet);
		                break;

                    case Opcode.CREATEPLAYER:
                        OnCreatePlayer(packet);
                        break;
                    case Opcode.JOIN:
                        OnJoin(packet);
                        break;

					case Opcode.STATE_MSG:
		                OnStateMessage(packet);
		                break;

					case Opcode.MOVEITEM:
		                OnMoveItem(packet);
		                break;
					case Opcode.DROPITEM:
						OnDropItem(packet);
		                break;

					case Opcode.NORMALCHAT:
						OnNormalChat(packet);
		                break;

					case Opcode.GMCMD:
						OnCommand(packet);
		                break;
					case Opcode.GET_CLIENT_INFO:
		                OnGetClientInfo(packet);
		                break;

	                default:
						if (Constants.LogLevel >= 1)
							Console.WriteLine("Packet not handled! ({0})", packet.Opcode);
                        break;
                }
            }

            if (_connected)
                BeginReceive();
        }

        #endregion

        public FFConnection(Socket socket, TCPServer<FFConnection> server)
            : base(socket, server)
        {
        }

        public User User { get; private set; }
        public Character Character { get; private set; }

        public ItemContainer<InventoryItem> InventoryItems { get; private set; }
		public ItemContainer<QuestItem> QuestItems { get; private set; }
		public ItemContainer<StorageItem> StorageItems { get; private set; }

        private static FFModelContainer Database
        {
            get { return Program.Database; }
        }

        public void Disconnect()
        {
            Send(new Packet(Opcode.DISCONNECT));

            OnDisconnected();
        }

        public void Send(Packet packet)
		{
			if (Constants.LogLevel >= 2)
				Console.WriteLine("S> {0}", packet.Opcode);

			var buffer = packet.GetBuffer();
			KeyPair.Encrypt(ref buffer, 0, packet.Size);

			Send(buffer, 0, packet.Size);
        }

	    public IEnumerable<FFConnection> NearPlayers
	    {
		    get
		    {
			    return Server.Connections.Where(c => c != this && c.Character != null && c.Character.Position.DistanceTo(Character.Position) <= 100);
		    }
	    }

		public void SendNearPlayers(Packet packet)
		{
			foreach (var connection in NearPlayers)
				connection.Send(packet);
		}
    }
}