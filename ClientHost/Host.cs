namespace ClientHost
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    public class Host
    {
        #region Public Delegates
        public delegate void HostMessagesDelegate(string message);
        #endregion

        #region Variables
        private readonly StandardClient.Client Client;
        private readonly Thread ClientThread;

        #region Callbacks
        protected readonly HostMessagesDelegate OnHostMessages; //the connection handler logic will be performed by the consumer of this class
        #endregion
        #endregion

        #region Constructor
        public Host(HostMessagesDelegate onHostMessages)
        {
            this.OnHostMessages = onHostMessages ?? throw new ArgumentNullException(nameof(onHostMessages));
            this.Client = new StandardClient.Client(this.OnMessage, this.ConnectionHandler); //Uses default host and port and timeouts
            this.ClientThread = new Thread(this.Client.Run);
        }
        #endregion

        #region Public Functions
        public virtual void RunClientThread()
        {
            this.ClientThread.Start();
            this.OnHostMessages.Invoke("Client started");
        }

        public virtual void WaitForClientThreadToStop()
        {
            this.Client.ExitSignal = true; //Signal that the client connection-loop should stop gracefully
            this.OnHostMessages.Invoke("Exit Signal sent to Client thread");
            this.OnHostMessages.Invoke("Joining Client thread");
            this.ClientThread.Join();
            this.OnHostMessages.Invoke("Client thread has exited gracefully");
        }
        #endregion

        #region Protected Functions
        protected virtual void ConnectionHandler(NetworkStream connectedAutoDisposedNetStream)
        {
            if (!connectedAutoDisposedNetStream.CanRead && !connectedAutoDisposedNetStream.CanWrite)
                return; //We need to be able to read and write

            var writer = new StreamWriter(connectedAutoDisposedNetStream) { AutoFlush = true };
            var reader = new StreamReader(connectedAutoDisposedNetStream);

            var StartTime = DateTime.Now;
            int i = 0;
            while (!this.Client.ExitSignal) //Tight network message-loop (optional)
            {
                var JSON_Helper = new Helper.JSON();
                string JSON = JSON_Helper.JSONstring();

                string Response;
                try
                {
                    //Synchronously send some JSON to the connected server
                    writer.WriteLine(JSON);
                    //Synchronously wait for a response from the connected server
                    Response = reader.ReadLine();
                }
                catch (IOException ex)
                {
                    _ = ex;
                    return; //Abort on network error
                }

                //Put breakpoint here to inspec the JSON string return by the connected server
                Helper.SomeDataObject Data = JSON_Helper.DeserializeFromJSON(Response);
                _ = Data;

                i++;
                var ElapsedTime = DateTime.Now - StartTime;
                if (ElapsedTime.TotalMilliseconds >= 1000)
                {
                    this.OnHostMessages.Invoke("Thread: " + Thread.CurrentThread.ManagedThreadId.ToString() + " Messages per second: " + i);
                    i = 0;
                    StartTime = DateTime.Now;
                }
            }
        }

        protected virtual void OnMessage(string message)
        {
            this.OnHostMessages.Invoke(message);
        }
        #endregion 
    }
}