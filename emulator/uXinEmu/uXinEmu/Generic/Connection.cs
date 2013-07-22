using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace uXinEmu.Generic
{
	abstract class Connection<T> where T : Connection<T>
	{
		private readonly Socket _socket;
		protected readonly TCPServer<T> Server;

		public Connection(Socket socket, TCPServer<T> server)
		{
			_socket = socket;

			Server = server;
		}

		public abstract void OnConnected();
		protected virtual void OnDisconnected()
		{
			_socket.Disconnect(false);

			Server.Connections.Remove((T) this);
		}

		protected abstract void OnDataReceived(byte[] buffer, int offset, int size);
		protected virtual void OnDataSent(byte[] buffer, int offset, int size) { }

		protected void Send(byte[] buffer, int offset, int size)
		{
			try
			{
				_socket.BeginSend(buffer, offset, size, SocketFlags.None, ar => { _socket.EndSend(ar); OnDataSent(buffer, offset, size); }, null);
			}
			catch (SocketException exception)
			{
				Debug.Assert(exception.ErrorCode == 10058);

				OnDisconnected();
			}
		}

		protected void BeginReceive()
		{
			//TODO: Change buffer (new byte[8192]) to something more dynamic.

			var buffer = new byte[8192];

			_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ar =>
			                                                                 	{
			                                                                 		try
			                                                                 		{
			                                                                 			OnDataReceived((byte[]) ar.AsyncState, 0,
			                                                                 			               _socket.EndReceive(ar));

			                                                                 		}
																					catch (SocketException exception)
																					{
																						Debug.Assert(exception.ErrorCode == 10054);

																						OnDisconnected();
																					}
			                                                                 	}, buffer);
		}
	}
}
