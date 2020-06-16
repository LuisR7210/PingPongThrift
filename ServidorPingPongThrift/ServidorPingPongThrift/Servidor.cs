using System;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Transport;
using Thrift.Transport.Server;

namespace ServidorPingPongThrift
{
    class Servidor
    {
        static void Main(string[] args)
        {
            var servicioPP = new ImplServicioPingPong();
            var procesador = new ServicioPingPong.AsyncProcessor(servicioPP);
            TServerTransport transporte = new TServerSocketTransport(5000);
            TServer server = new TThreadPoolAsyncServer(procesador, transporte);
            Console.WriteLine(" Servidor de Ping Pong Thrift iniciado\n Para salir presione enter");
            server.ServeAsync(new CancellationToken());
            Task.Run(() => {
                while (true)
                {
                    if (servicioPP.jugadores.Count == 2)
                    {
                        if (!servicioPP.MoverPelota())
                        {
                            Thread.Sleep(1000);
                            servicioPP.BorrarPartidaAnterior();
                            Console.WriteLine("Juego terminado\n Vuelva a ejecutar los jugadores para otra partida");
                        }
                        Thread.Sleep(70);
                    }
                }
            });
            while (true)
            {
                if (Console.Read() >= 0)
                {
                    break;
                }
            }
            server.Stop();
        }
    }
}
