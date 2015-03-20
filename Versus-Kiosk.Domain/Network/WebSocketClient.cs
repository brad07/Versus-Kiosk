using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace VersusKiosk.Domain.Network
{
	// this class is responsible for maintaining a permanent websocket connection to the control center 
	public class StationClient : IDisposable
	{
		//private WebSocketClient Client;
		private WebSocket Client;
		private string Address;
		private volatile bool Connected = false;

		public delegate void ServerMessageReceiveHandler(string message);
		public event ServerMessageReceiveHandler OnMessageReceived;

		public delegate void ConnectedToServerHandler(object context);
		public delegate void DisconnectedFromServerHandler(object context);
		public event ConnectedToServerHandler OnConnectedToServer;
		public event DisconnectedFromServerHandler OnDisconnectedFromServer;

		public StationClient()
		{
		}

		public void Start(IPAddress address)
		{
			this.Address = address.ToString();
			ConnectToServer();
		}

		public void Dispose()
		{
			DisconnectFromServer();
		}

		Mutex mutex = new Mutex();

		private void ConnectToServer()
		{
			// make sure we're not already trying to connect
			Task.Factory.StartNew(() =>
			{
				mutex.WaitOne();
				try
				{
					if (this.Client != null)
					{
						this.Client.Opened -= OnOpened;
						this.Client.Closed -= OnClosed;
						this.Client.Error -= OnClosed;
						this.Client.MessageReceived -= OnMessage;
						this.Client.Dispose();
					}

					this.Client = new WebSocket("ws://" + this.Address + ":81/");
					this.Client.EnableAutoSendPing = true;
					this.Client.NoDelay = true;
					this.Client.Opened += OnOpened;
					this.Client.Closed += OnClosed;
					this.Client.Error += OnClosed;
					this.Client.MessageReceived += OnMessage;
					this.Client.Open();
				}
				catch
				{
				}
				mutex.ReleaseMutex();
			});
		}

		public void DisconnectFromServer()
		{
			mutex.WaitOne();
			try
			{
				if (this.Client != null)
					this.Client.Close();
			}
			catch
			{
			}
			finally
			{
				this.Connected = false;
				this.Client = null;
			}
			mutex.ReleaseMutex();
		}

		private void OnOpened(object sender, EventArgs e)
		{
			Console.WriteLine("*** CONNECTED TO SERVER ***");
			Debug.Assert(this.Connected == false);
			this.Connected = true;
			if (this.OnConnectedToServer != null)
				this.OnConnectedToServer(sender);
		}

		private void OnClosed(object sender, EventArgs e)
		{
			Console.WriteLine("*** DISCONNECTED FROM SERVER ***");
			mutex.WaitOne();
			try
			{
				if ((this.Client != null) && (this.Client.State==WebSocketState.Open))
					this.Client.Close();
			}
			catch
			{
			}
			finally
			{
				this.Connected = false;
				this.Client = null;
			}
			mutex.ReleaseMutex();
			if (this.OnDisconnectedFromServer != null)
				this.OnDisconnectedFromServer(sender);
			Thread.Sleep(1000);
			//ConnectToServer();
		}

		private void OnMessage(object sender, MessageReceivedEventArgs args)
		{
			try
			{
				var message = args.Message;
#if LOG_TRAFFIC
                Console.WriteLine("Received data from server : {0}", message);
#endif
				if (this.OnMessageReceived != null)
					this.OnMessageReceived(message);
			}
			catch (Exception e)
			{
				Console.WriteLine("Websocket exception: " + e.Message);
			}
		}

		public void Send(string msg)
		{
			if (!mutex.WaitOne(TimeSpan.Zero))
				return;
			try
			{
				if (this.Connected)
				{
#if LOG_TRAFFIC
					Console.WriteLine("Sending data to server : {0}", msg);
#endif
					this.Client.Send(msg);
				}
			}
			catch
			{
				// todo: report error - MJF
			}
			mutex.ReleaseMutex();
		}
	}
}
