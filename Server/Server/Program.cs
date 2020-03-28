using System;


namespace DedicatedServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.Title = "Dedicated Server";

            Server.Start(8, 26950);
            Console.ReadKey();
        }
    }

 
}
