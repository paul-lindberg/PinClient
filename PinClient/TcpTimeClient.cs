using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PinClient
{
    public class TcpTimeClient
    {
        private const int portNum = 13;
        private const string hostName = "192.168.1.53";//"host.contoso.com";

        public static int Init(String[] args)
        {
            try
            {
                var client = new TcpClient(hostName, portNum);

                NetworkStream ns = client.GetStream();

                byte[] bytes = new byte[1024];
                int bytesRead = ns.Read(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));

                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return 0;
        }
    }
}
