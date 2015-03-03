using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VersusKiosk.Domain.Network
{
	public class Comms
	{
		private ObservableCollection<QueuedMsg> outbound_msg_queue = new ObservableCollection<QueuedMsg>();
		private volatile bool _shouldStop;
		private bool udp_listening;
		private String network_name;		
		private UdpClient Listener;

		public delegate void MessageListenerDelegate(dynamic msg);
		public event MessageListenerDelegate MessageListener;

		public delegate void BroadcastListenerDelegate(dynamic msg);
		public event BroadcastListenerDelegate BroadcastListener;

		public StationClient StationClient { get; private set; }

		public Comms()
		{
			this.StationClient = new StationClient();
		}

		// Thread signal.
		public static ManualResetEvent allDone = new ManualResetEvent(false);

		/*
		public void setGame(Game gme)
		{
			game = gme;
		}
		 * */

		public void RequestStop()
		{
			_shouldStop = true;
			allDone.Set();
			if (this.Listener != null)
				this.Listener.Close();
			if (this.StationClient != null)
				this.StationClient.Dispose();

		}

		private static int udpThreads = 0;

		public void startUDPListener(String net_name)
		{

			try
			{
				network_name = net_name;

				Console.WriteLine("Network name=" + network_name);

				// Data buffer for incoming data.
				byte[] bytes = new Byte[4096];

				int listening_port = 9102;

				IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listening_port);
				this.Listener = new UdpClient();
				this.Listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				this.Listener.Client.Bind(groupEP);

				ThreadStart start = delegate()
				{
					if (udpThreads > 0)
						EventLog.WriteEntry("Versus Station", "Multiple UDP threads running!", EventLogEntryType.Error);
					udpThreads++;

					try
					{
						while (!_shouldStop)
						{
							bytes = this.Listener.Receive(ref groupEP);

							//Console.WriteLine("Received broadcast from " + groupEP.ToString() + ": " + Encoding.ASCII.GetString(bytes, 0, bytes.Length));

							string content = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

							//serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
							//dynamic msg = serializer.Deserialize(content, typeof(object));
							var msg = JsonConvert.DeserializeObject<dynamic>(content);

							bool ignore = false;

							// if a Network Name has been specified
							if (msg.network_name != null)
							{
								// ignore any messages not destined for this Versus network
								if (msg.network_name != network_name) ignore = true;
							}

							if (!ignore)
							{
								try
								{
									if (this.BroadcastListener != null)
										this.BroadcastListener(msg);
								}
								catch
								{
								}
							}
							else
							{
								/*
								log("Ignoring UDP msg for network=" + msg.network_name + ", msg=" + content);
								 * */
							}
						}

					}
					catch (Exception e)
					{
						this.Listener.Close();
						Console.WriteLine(e.ToString());
					}

					udpThreads--;
				};


				var thread = new Thread(start) { IsBackground = true };
				thread.Start();

				//new Thread(start).Start();

				udp_listening = true;
				Console.WriteLine("Server is started");
			}
			catch (Exception)
			{
				//game.log("Exception starting UDP Listener: " + ex.Message);
				udp_listening = false;
			}
		}

		public bool sendUDPMsg(dynamic msg, bool serialize = true)
		{
			string json;

			if (serialize)
			{
				json = JsonConvert.SerializeObject(msg, Formatting.Indented);
			}
			else
			{
				json = msg;
			}

			Console.WriteLine("Sending udp message: " + msg.cmd);

			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPAddress subnet = LocalIPAddress();

			string[] subnet_arr = subnet.ToString().Split((char)46);

			IPAddress broadcast = IPAddress.Parse(subnet_arr[0] + "." + subnet_arr[1] + "." + subnet_arr[2] + ".255");

			byte[] sendbuf = Encoding.ASCII.GetBytes(json);
			IPEndPoint ep = new IPEndPoint(broadcast, 9100);

			if (s.SendTo(sendbuf, ep) > 0)
			{
				Console.WriteLine("UDP message successfully sent");
				s.Close();
				return true;
			}
			else
			{
				Console.WriteLine("UDP message failed to send");
				s.Close();
				return false;
			}

			//return true;

		}

		public bool isRunning()
		{
			if (udp_listening)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int tcpThreads = 0;

		public void startTCPSender()
		{
			ThreadStart start = delegate()
			{
				if (tcpThreads > 0)
					EventLog.WriteEntry("Versus Station", "Multiple tcp threads running!", EventLogEntryType.Error);
				tcpThreads++;

				try
				{
					while (!_shouldStop)
					{
						sendOutboundMsgQueue();
					}

				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
				tcpThreads--;
			};


			Thread t = new Thread(start) { IsBackground = true };
			t.Start();
		}

		void StationClient_OnMessageReceived(string message)
		{
			// deserialize the data into a dynamic object
			//JavaScriptSerializer serializer = new JavaScriptSerializer();
			//serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
			//dynamic msg = serializer.Deserialize(message, typeof(object));
			//dynamic result = new System.Dynamic.ExpandoObject();
			var result = JsonConvert.DeserializeObject<dynamic>(message);

			if (this.MessageListener != null)
				this.MessageListener(result);

			/*
			result = game.processControllerCmd(message);
			*/

			/*
			// if the received msg has the result_required flag set to true then process it in realtime (blocking)
			if (msg.result_required != null)
			{
				result = game.processControllerCmd(message);
			}
			else
			{
				// if result_required is false then add to inbound msg queue for processing
				inbound_queue.Add(message);
				result.success = true;
			}

			string json = JsonConvert.SerializeObject(result, Formatting.Indented);

			// return result
			//this.StationClient.Send(json);
			 * */
		}

		public void startStationClient(IPAddress address)
		{
			this.StationClient.OnMessageReceived += StationClient_OnMessageReceived;
			this.StationClient.Start(address);
		}

		public bool sendOutboundMsgQueue()
		{

			try
			{
				ObservableCollection<QueuedMsg> thisQueue = new ObservableCollection<QueuedMsg>();
				lock (outbound_msg_queue)
				{
					foreach (var msg in outbound_msg_queue)
						thisQueue.Add(msg);
					outbound_msg_queue.Clear();
				}

				foreach (var queued_msg in thisQueue)
				{
					IPAddress ip = IPAddress.Parse((String)queued_msg.ip_address);
					String result = sendMsg(queued_msg.msg, ip, true);
				}

				/*
				lock (outbound_msg_queue)
				{
					ObservableCollection<QueuedMsg> delete_msgs = new ObservableCollection<QueuedMsg>();

					//log("Checking outbound message queue");

					//Console.WriteLine("sending outbound msg queue");

					if (outbound_msg_queue.Count > 0)
					{
						log("Sending Outbound Msg Queue - " + outbound_msg_queue.Count);
						foreach (QueuedMsg queued_msg in outbound_msg_queue)
						{
							bool send = false;
							bool expired = false;

							if (queued_msg.delay > 0)
							{
								if (queued_msg.submitted_at.AddMilliseconds(queued_msg.delay) >= DateTime.Now)
								{
									log("Delayed msg " + queued_msg.msg.cmd + " is being sent");
									send = true;
								}
							}
							else
							{
								send = true;
							}

							if (queued_msg.ttl > 0)
							{
								if (queued_msg.submitted_at.AddMilliseconds(queued_msg.ttl) > DateTime.Now)
								{
									send = true;
								}
								else
								{
									expired = true;
									log("Message " + queued_msg.msg.cmd + " with " + queued_msg.ttl + "ttl has expired");
								}
							}
							else
							{
								send = true;
							}

							if (!expired && send)
							{
								log("Sending queued msg: " + queued_msg.msg.cmd);
								IPAddress ip = IPAddress.Parse((String)queued_msg.ip_address);
								String result = sendMsg(queued_msg.msg, ip, true);
								if (result == "ACK")
								{
									log("Sent queued msg to " + queued_msg.ip_address);
									//outbound_msg_queue.Remove(queued_msg);

									delete_msgs.Add(queued_msg);
								}
								else
								{
									log("Failed to send queued msg to " + queued_msg.ip_address);
								}
							}
							else if (expired)
							{
								//outbound_msg_queue.Remove(queued_msg);
								delete_msgs.Add(queued_msg);
							}

						}

						// delete msgs that were sent or have been expired
						for (int x = 0; x < delete_msgs.Count; x++)
						{
							outbound_msg_queue.Remove(delete_msgs.ElementAt(x));
						}

						//outbound_msg_queue.Clear();
					}
                }
				*/

				return true;
			}
			catch (Exception ex)
			{
				log("Exception in SendOutboundMsgQueue: " + ex.Message);
				return false;
			}
		}

		public void stopAll()
		{
			Console.WriteLine("Stopping Comms servers");

			RequestStop();

		}

		public void disconnect()
		{
			Console.WriteLine("Forcing disconnect from server");

			this.StationClient.DisconnectFromServer();

		}

		/*
		public string getNextMsgInQueue()
		{
			try
			{
				lock (inbound_queue)
				{
					String msg;

					// if there are one or more msgs in the inbound queue
					if (inbound_queue.Count() > 0)
					{
						//Console.WriteLine(inbound_queue.Count() + " msgs in the queue");

						// retrieve the msg in the first position
						msg = inbound_queue.ElementAt(0);

						//Console.WriteLine("Msg=" + msg);

						// delete the msg in the first position
						inbound_queue.RemoveAt(0);
					}
					else
					{
						//Console.WriteLine("No msgs in inbound queue");
						msg = "";
					}

					return msg;
				}
			}
			catch (Exception ex)
			{
				log("Exception in getNextMsgInQueue: " + ex.Message);
				return "";
			}
		}
		*/

		private static void Send(Socket handler, String data)
		{
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), handler);
		}

		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket handler = (Socket)ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				//Console.WriteLine("Sent {0} bytes to client.", bytesSent);

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}


		public bool queueMsg(dynamic msg, IPAddress ip_address, int delay = 0, int ttl = 0)
		{
			try
			{
				if (ip_address != null)
				{
					QueuedMsg queued_msg = new QueuedMsg();
					queued_msg.msg = msg;
					queued_msg.ip_address = ip_address.ToString();
					queued_msg.delay = delay;
					queued_msg.ttl = ttl;
					lock (outbound_msg_queue)
						outbound_msg_queue.Add(queued_msg);
				}
				return true;
			}
			catch (Exception ex)
			{
				log("Exception in queueMsg: " + ex.Message);
				return false;
			}

		}

		public String sendMsg(dynamic msg, IPAddress ip_address, bool serialize = false)
		{
			try
			{
				string json;
				//game.log("Connected to Command Center on " + ip_address.ToString());
				if (serialize)
					json = JsonConvert.SerializeObject(msg, Formatting.Indented);
				else
					json = msg;
				if (this.StationClient != null)
					this.StationClient.Send(json);
				return "";
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.Message);
				return "Error";
			}
		}

		public IPAddress LocalIPAddress()
		{
			IPHostEntry host;
			IPAddress localIP = new IPAddress(0);
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip;
					break;
				}
			}
			return localIP;
		}


		public void log(String msg)
		{
			EventLog.WriteEntry("Versus Station", msg);
			//Console.WriteLine(DateTime.Now + ": " + msg);
		}

		////////////////////////////// NEW SOCKET-BASED CODE ///////////////////////////////

		public void StartClient()
		{
			// start searching for servers
			//this.ServerPoller = new ServerPoller();
		}

		public void StopClient()
		{
		}

	}
}
