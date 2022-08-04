using System.Net;
using System.Net.Sockets;

namespace TradingAPI.MT4Server;

internal class SecureSocket
{
    private static object GetResponseLock = new();
    private readonly Logger Log;
    internal Socket Client;
    private Decoder Decoder;
    private Encoder Encoder;
    private readonly bool IsProxy;
    private HttpWebResponse webResponse;
    private Stream webStream;

    public SecureSocket()
    {
        Log = new Logger(this);
        IsProxy = false;
    }

    public SecureSocket(byte[] key)
    {
        Decoder = new Decoder(key);
        Encoder = new Encoder(key);
        Log = new Logger(this);
        IsProxy = false;
    }

    internal int Available => Client.Available;

    public void SetNewKey(byte[] key)
    {
        if (Decoder == null)
            Decoder = new Decoder(key);
        else
            Decoder.ChangeKey(key);
        if (Encoder == null)
            Encoder = new Encoder(key);
        else
            Encoder.ChangeKey(key);
    }

    public byte[] GetKey()
    {
        if (Decoder != null)
            return Decoder.GetKey();
        return Encoder != null ? Encoder.GetKey() : null;
    }

    public void ResetDecoder()
    {
        Decoder.Reset();
    }

    public void ResetEncoder()
    {
        Encoder.Reset();
    }

    public void Send(byte[] buf)
    {
        Client.SendBufferSize = buf.Length;
        Client.Send(buf);
    }

    public byte[] Receive(int count)
    {
        var buffer = new byte[count];
        int num;
        for (var length = buffer.Length; length > 0; length -= num)
        {
            num = Client.Receive(buffer, buffer.Length - length, length, SocketFlags.None);
            if (num == 0)
                throw new Exception("Server closed the socket");
        }

        return buffer;
    }

    public void SendEnrypt(byte[] buf)
    {
        byte num = 0;
        for (var index = 1; index < buf.Length; ++index)
        {
            num = (byte) ((num + (uint) MT4Crypt.CryptKey[(index - 1) & 15]) ^ buf[index]);
            buf[index] = num;
        }

        Send(buf);
    }

    public byte[] ReceiveDecrypt(int count)
    {
        var bytes = Receive(count);
        byte num1 = 0;
        for (var index = 0; index < bytes.Length; ++index)
        {
            var num2 = (byte) (num1 + (uint) MT4Crypt.CryptKey[index & 15]);
            num1 = bytes[index];
            bytes[index] ^= num2;
        }

        Log.trace($"RECV {ConvertBytes.toHex(bytes)}");
        return bytes;
    }

    public void SendEncode(byte[] buf)
    {
        Send(Encoder.Encode(buf));
    }

    public byte[] ReceiveDecode(int count) => Decoder.Decode(Receive(count));

    public byte[] ReceiveCopmressed()
    {
        var int32 = BitConverter.ToInt32(ReceiveDecode(4), 0);
        var src = Decompressor.decompress(ReceiveDecode(BitConverter.ToInt32(ReceiveDecode(4), 0)), int32);
        Decompressor.compress(src);
        return src;
    }

    internal void Connect(string host, int port, ref string reason)
    {
        if (Decoder != null)
            Decoder.Reset();
        if (Encoder != null)
            Encoder.Reset();
        reason = $"{reason}, connecting to {host}:{port}";
        Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
        Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
        Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, true);
        Client.Connect(host, port);
        reason = $"{reason}, connected to {host}:{port}";
    }

    internal void Connect(
        string targetHost,
        int targetPort,
        string proxyHost,
        int proxyPort,
        string proxyUser,
        string proxyPassword,
        ProxyTypes type)
    {
        if (Decoder != null)
            Decoder.Reset();
        if (Encoder != null)
            Encoder.Reset();
        var proxySocket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        proxySocket.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(proxyHost), proxyPort);
        proxySocket.ProxyUser = proxyUser;
        proxySocket.ProxyPass = proxyPassword;
        proxySocket.ProxyType = type;
        proxySocket.Connect(targetHost, targetPort);
        Client = proxySocket;
        Log.trace("Connected to proxy server");
    }

    internal void Close()
    {
        lock (this)
        {
            if (!IsProxy)
                return;
            webStream.Close();
            webResponse.Close();
        }
    }
}
//
//
// using System;
// using System.Collections.Generic;
// using System.Text;
// using System.Net.Sockets;
// using System.IO;
// using System.Reflection;
// using System.Net;
// using System.Threading;
// using TradingAPI.MT4Server;
//
// namespace TradingAPI.MT4Server
// {
// 	internal class SecureSocket
// 	{
// 		private Decoder Decoder;
// 		private Encoder Encoder;
// 		private readonly Logger Log;
//
// #if SOCK
// 		internal Socket Client;
// #else
// 		internal TcpClient Client;
// 		NetworkStream Stream;
// #endif
// 		//internal BReader Reader;
// 		//internal StreamWriter Writer;
//
// 		private bool IsProxy;
// 		private HttpWebResponse webResponse = null;
// 		private Stream webStream = null;
//
// 		public SecureSocket()
// 		{
// 			Log = new Logger(this);
// 			IsProxy = false;
// 		}
//
// 		public SecureSocket(byte[] key)
// 		{
// 			Decoder = new Decoder(key);
// 			Encoder = new Encoder(key);
// 			Log = new Logger(this);
// 			IsProxy = false;
// 		}
//
// 		public void SetNewKey(byte[] key)
// 		{
// 			if (Decoder == null)
// 				Decoder = new Decoder(key);
// 			else
// 				Decoder.ChangeKey(key);
// 			if (Encoder == null)
// 				Encoder = new Encoder(key);
// 			else
// 				Encoder.ChangeKey(key);
// 		}
//
// 		public byte[] GetKey()
// 		{
// 			if (Decoder != null)
// 				return Decoder.GetKey();
// 			if (Encoder != null)
// 				return Encoder.GetKey();
// 			return null;
// 		}
//
// 		public void ResetDecoder()
// 		{
// 			Decoder.Reset();
// 		}
//
// 		public void ResetEncoder()
// 		{
// 			Encoder.Reset();
// 		}
//
// 		public void Send(byte[] buf)
// 		{
// #if SOCK
// 			Client.SendBufferSize = buf.Length;
// 			Client.Send(buf);
// #else
// 			if (!Stream.WriteAsync(buf, 0, buf.Length).Wait(20000))
// 				throw new TimeoutException("Send timeout. Please check internet connection.");
// 			Stream.Flush();
// #endif
// 			//try
// 			//{
// 			//	return Sock.Send(buf);
// 			//}
// 			//catch (NullReferenceException)
// 			//{
// 			//	throw new Exception("Socket not connected");
// 			//}
// 			//catch (Exception ex)
// 			//{
// 			//	throw new ConnectException(ex.Message);
// 			//}
// 		}
//
// 		public byte[] Receive(int count)
// 		{
// 			byte[] buf = new byte[count];
// 			int rest = buf.Length;
// 			while (rest > 0)
// 			{
// 				int len;
// #if SOCK
// 				len = len = Client.Receive(buf, buf.Length - rest, rest, SocketFlags.None);
// #else
// 				var task = Stream.ReadAsync(buf, buf.Length - rest, rest);
// 				if (!task.Wait(30000))
// 					throw new TimeoutException("Recieve timeout. Please check internet connection.");
// 					len = task.Result;
// #endif
// 				if (len == 0)
// 					throw new Exception("Server closed the socket");
// 				else
// 					rest -= len;
// 			}
// 			return buf;
//
// 			//try
// 			//{
// 			//	byte[] buf = new byte[count];
// 			//	int rest = buf.Length;
// 			//	while (rest > 0)
// 			//	{
// 			//		int len = Sock.Receive(buf, buf.Length - rest, rest, SocketFlags.None);
// 			//		if (len == 0)
// 			//			throw new Exception("Server closed the socket");
// 			//		else
// 			//			rest -= len;
// 			//	}
// 			//	return buf;
// 			//}
// 			//catch (NullReferenceException)
// 			//{
// 			//	throw new Exception("Socket not connected");
// 			//}
// 			//catch (Exception ex)
// 			//{
// 			//	throw new ConnectException(ex.Message);
// 			//}
//
// 		}
//
// 		public void SendEnrypt(byte[] buf)
// 		{
// 			byte value = 0;
// 			for (int i = 1; i < buf.Length; i++)
// 			{
// 				value = (byte)((value + MT4Crypt.CryptKey[(i - 1) & 0xF]) ^ buf[i]);
// 				buf[i] = value;
// 			}
// 			Send(buf);
// 		}
//
// 		public byte[] ReceiveDecrypt(int count)
// 		{
// 			byte[] buf = Receive(count);
// 			byte prev = 0;
// 			for (int i = 0; i < buf.Length; i++)
// 			{
// 				byte value = (byte)(prev + MT4Crypt.CryptKey[i & 0xF]);
// 				prev = buf[i];
// 				buf[i] ^= value;
// 			}
// 			Log.trace("RECV " + ConvertBytes.toHex(buf));
// 			return buf;
// 		}
//
// 		public void SendEncode(byte[] buf)
// 		{
// 			Send(Encoder.Encode(buf));
// 		}
//
// 		public byte[] ReceiveDecode(int count)
// 		{
// 			byte[] buf = Receive(count);
// 			return Decoder.Decode(buf);
// 		}
//
// 		public byte[] ReceiveCopmressed()
// 		{
// 			byte[] buf = ReceiveDecode(4);
// 			int dstlen = BitConverter.ToInt32(buf, 0);
// 			buf = ReceiveDecode(4);
// 			int len = BitConverter.ToInt32(buf, 0);
// 			buf = ReceiveDecode(len);
// 			var a = Decompressor.decompress(buf, dstlen);
// 			var b = Decompressor.compress(a);
// 			return a;
// 		}
//
// 		internal void Connect(string host, int port, ref string reason)
// 		{
// 			if (Decoder != null)
// 				Decoder.Reset();
// 			if (Encoder != null)
// 				Encoder.Reset();
// 			reason += ", connecting to " + host + ":" + port;
// #if SOCK
//
// 			Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
// 			Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
// 			Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
// 			Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
// 			Client.Connect(host, port);
// #else
// 			Client = new TcpClient();
// 			var res = Client.BeginConnect(host, port, null, null);
// 			if (!res.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10000)))
// 				throw new TimeoutException("Socket connect timeout. Please check internet connection.");
// 			Client.EndConnect(res);
// 			//if(!Client.ConnectAsync(host, port).Wait(30000))
// 			//	throw new TimeoutException("Connect timeout");
// 			Stream = Client.GetStream();
// #endif
// 			reason += ", connected to " + host + ":" + port;
// 		}
//
// 		private static object GetResponseLock = new object();
//
// 		internal void Connect(string targetHost, int targetPort, string proxyHost, int proxyPort,
// 				string proxyUser, string proxyPassword, ProxyTypes type)
// 		{
// 			if (Decoder != null)
// 				Decoder.Reset();
// 			if (Encoder != null)
// 				Encoder.Reset();
// 			ProxySocket s = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
// 			s.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(proxyHost), proxyPort);
// 			s.ProxyUser = proxyUser;
// 			s.ProxyPass = proxyPassword;
// 			s.ProxyType = type;
// 			s.Connect(targetHost, targetPort);
// 			// Client = s;
// 			//Client = SocksProxy.ConnectToSocks5Proxy(proxyHost, (ushort)proxyPort, targetHost,  (ushort)targetPort, proxyUser, proxyPassword);
// 			//Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
// 			//Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
// 			//Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
// 			Log.trace("Connected to proxy server");
// 		}
//
// 		internal void Close()
// 		{
// 			lock (this)
// 			{
// 				if (IsProxy)
// 				{
// 					webStream.Close();
// 					webResponse.Close();
// 				}
// #if SOCK
// #else
// 				else if (Client != null)
// 					Client.Dispose();
// #endif
// 				//else if (Sock != null)
// 				//{
// 				//	try { Sock.Shutdown(SocketShutdown.Both); } catch (Exception) { }
// 				//	try { Sock.Disconnect(false); } catch (Exception) { }
// 				//	try { Sock.Close(); } catch (Exception) { }
// 				//	try { Sock.Dispose(); } catch (Exception) { }
// 				//}
// 				//Sock = null;
// 			}
// 		}
//
// 		internal int Available
// 		{
// 			get
// 			{
// 				return Client.Available;
// 			}
// 		}
// 	}
// }
