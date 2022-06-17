namespace StandardClient
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    public class Client
    {
        #region Public Properties
        private volatile bool _ExitSignal;
        public bool ExitSignal
        {
            get => this._ExitSignal;
            set => this._ExitSignal = value;
        }
        #endregion

        #region Public Delegates
        public delegate void ConnectionHandlerDelegate(NetworkStream connectedAutoDisposedNetStream);
        public delegate void MessageDelegate(string message);
        #endregion

        #region Variables
        #region Init/State
        protected readonly int ConnectionAttemptDelayInMS; //The client delay between failed attempts to connect to the server
        protected readonly string Host;
        protected readonly int Port;
        protected bool IsRunning;
        #endregion

        #region Callbacks
        protected readonly ConnectionHandlerDelegate OnHandleConnection; //the connection handler logic will be performed by the consumer of this class
        protected readonly MessageDelegate OnMessage;
        #endregion
        #endregion
        
        #region Constructor
        public Client(MessageDelegate onMessage, ConnectionHandlerDelegate connectionHandler, string host = "192.168.1.21", int port = 8080, int connectionAttemptDelayInMS = 2000)
        {//127.0.0.1
            this.OnMessage = onMessage ?? throw new ArgumentNullException(nameof(onMessage));
            this.OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.Port = port;
            this.ConnectionAttemptDelayInMS = connectionAttemptDelayInMS;
        }
        #endregion

        #region Public Functions
        public virtual void Run()
        {
            if (this.IsRunning)
                return; //Already running, only one running instance allowed.

            this.IsRunning = true;
            this.ExitSignal = false;

            while (!this.ExitSignal)
                this.ConnectionLooper();

            this.IsRunning = false;
        }
        #endregion

        #region Protected Functions
        protected virtual void ConnectionLooper()
        {
            this.OnMessage.Invoke("Attemping server connection... on Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
            using (var Client = new TcpClient())
            {
                try
                {
                    Client.Connect(this.Host, this.Port);
                }
                catch(SocketException ex)
                {
                    this.OnMessage.Invoke(ex.Message);
                    Thread.Sleep(this.ConnectionAttemptDelayInMS); //Server is unavailable, wait before re-trying
                    return;
                }

                if (!Client.Connected) //Abort if not connected
                    return;

                using (var netstream = Client.GetStream()) //Auto dispose of the netstream connection
                {
                    this.OnHandleConnection.Invoke(netstream);
                }
            }
        }
        #endregion
    }
}