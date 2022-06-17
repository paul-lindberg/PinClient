namespace ConsoleClient
{
    using System;
    public static class ClientProgram
    {
        public static void Main()
        {
            Console.WriteLine("Press esc key to stop");

            int i = 0;
            void PeriodicallyClearScreen()
            {
                i++;
                if (i > 15)
                {
                    Console.Clear();
                    Console.WriteLine("Press esc key to stop");
                    i = 0;
                }
            }

            //Write the host messages to the console
            void OnHostMessage(string input)
            {
                PeriodicallyClearScreen();
                Console.WriteLine(input);
            }

            var BLL = new ClientHost.Host(OnHostMessage);
            BLL.RunClientThread(); //Client runs in a dedicated thread seperate from mains thread

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.Clear();
                Console.WriteLine("Press esc key to stop");
            }

            Console.WriteLine("Attempting clean exit");
            BLL.WaitForClientThreadToStop();

            Console.WriteLine("Exiting console Main.");
        }
    }
}