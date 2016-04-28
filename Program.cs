using System;
using System.Net;
using System.Threading;
using Mina.Core.Future;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Codec.Serialization;
using Mina.Filter.Logging;
using Mina.Transport.Socket;
using Mina.Filter.Codec.TextLine;


namespace Client
{
	class MainClass
	{
		private static readonly int PORT = 9321;
		private static readonly long CONNECT_TIMEOUT = 30 * 1000L; // 30 seconds

		public static void Main (string[] args)
		{

			AsyncSocketConnector connector = new AsyncSocketConnector();

			// Configure the service.
			connector.ConnectTimeoutInMillis = CONNECT_TIMEOUT;

			connector.FilterChain.AddLast("codec",
				new ProtocolCodecFilter( new TextLineCodecFactory( System.Text.Encoding.UTF8,
					LineDelimiter.Unix, LineDelimiter.Unix)));
			
			connector.FilterChain.AddLast("logger", new LoggingFilter());

			connector.SessionOpened += (s, e) =>
			{
				string message = "{\"type\":\"A\",\"name\":\"B\"}";
				e.Session.Write(message);
			};

			connector.ExceptionCaught += (s, e) =>
			{
				Console.WriteLine(e.Exception);
				e.Session.Close(true);
			};

			connector.MessageReceived += (s, e) =>
			{
				string message = (string)e.Message;

				System.Console.WriteLine(message);
			
			};

			IoSession session;
			while (true)
			{
				try
				{
					IConnectFuture future = connector.Connect(new IPEndPoint(IPAddress.Loopback, PORT));
					future.Await();
					session = future.Session;
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					Thread.Sleep(3000);
				}
			}

			// wait until the summation is done
			session.CloseFuture.Await();
			Console.WriteLine("Press any key to exit");
			Console.Read();
		}
	}
}
