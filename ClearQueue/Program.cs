using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace ClearQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ingrese la Cadena de Conexion del Service Bus:");
            string sbSC = Console.ReadLine();
            Console.WriteLine("Ingrese el nombre de la cola:");
            string colaSC = Console.ReadLine();

            var queue = QueueClient.CreateFromConnectionString(sbSC,colaSC);
            int cont = 0;
            queue.OnMessage(message =>
            {
                Console.WriteLine(String.Format("Message body: {0}", message.GetBody<String>()));
                cont++;
                Console.WriteLine("Procesados {0} mensajes.", cont);
            });            
            Console.ReadLine();
            
        }
    }
}
