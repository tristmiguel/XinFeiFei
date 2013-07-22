using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace uXinEmu.Generic
{
	internal sealed class TCPServer<T> where T : Connection<T>
	{
		public readonly List<T> Connections;

		public TCPServer()
		{
			Connections = new List<T>();
		}

		private void BeginAccept(Socket socket)
		{
			var result = socket.BeginAccept(ListenCallback, socket);

			result.AsyncWaitHandle.WaitOne();
		}

		public void Listen(string address, int port)
		{
			Connections.Clear();

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
			socket.Listen(100);

			BeginAccept(socket);
		}

		private void ListenCallback(IAsyncResult ar)
		{
			var socket = ((Socket) ar.AsyncState).EndAccept(ar);

			var connection = Activator.CreateInstance(typeof(T), socket, this) as T;

			Connections.Add(connection);

			//TODO: Use a thread pool.
			new Thread(connection.OnConnected) { Name = "Connection" }.Start();

			BeginAccept((Socket) ar.AsyncState);
		}
	}
}