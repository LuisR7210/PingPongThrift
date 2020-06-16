using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using Thrift.Protocol;
using Thrift.Transport.Client;
using Thrift;
using Thrift.Transport;
using System.Collections.Generic;

namespace ClientePingPongThrift
{
    class Cliente
    {
        private static bool jugando = false;
        private static bool partidaFinalizada = false;
        private static bool partidaIniciada = false;
        private static int puntJugador1 = 0;
        private static int puntJugador2 = 0;
        private static string marcador = "";
        private static int miId = 0;
        private static int idRival = 0;
        private static ServicioPingPong.Client cliente;
        private static List<Jugador> jugadores = new List<Jugador>();
        private const int TAMAÑO_PALETA = 6;
        private const int ANCHO_ESCENARIO = 70;
        private const int ALTO_ESCENARIO = 20;
        private const int PUNTOS_PARTIDA = 2;
        private static Posicion posicionPelota;

        static async Task Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.SetWindowSize(ANCHO_ESCENARIO, ALTO_ESCENARIO);
                Console.BufferHeight = ALTO_ESCENARIO;
                Console.BufferWidth = ANCHO_ESCENARIO;
            }
            posicionPelota = new Posicion { X = ANCHO_ESCENARIO / 2, Y = ALTO_ESCENARIO / 2 };
            jugadores.Add(new Jugador
            {
                IdJugador = 0,
                Posicion = new Posicion { X = 0, Y = (ALTO_ESCENARIO / 2 - TAMAÑO_PALETA / 2) }
            });
            jugadores.Add(
            new Jugador
            {
                IdJugador = 1,
                Posicion = new Posicion { X = ANCHO_ESCENARIO - 1, Y = (ALTO_ESCENARIO / 2 - TAMAÑO_PALETA / 2) }
            });
            try
            {
                TTransport transporte = new TSocketTransport("localhost", 5000);
                TProtocol protocolo = new TBinaryProtocol(transporte);
                cliente = new ServicioPingPong.Client(protocolo);
                while (true)
                {
                    if (!jugando)
                    {
                        miId = await cliente.EntrarALaPartidaAsync();
                        idRival = miId % 2 == 0 ? miId + 1 : miId - 1;
                        await cliente.EnviarPosicionAsync(jugadores[miId]);
                        jugando = true;
                    }
                    if (!Console.KeyAvailable)
                    {
                        try
                        {
                            jugadores[idRival] = await cliente.ConsultarPosicionJugadorAsync(idRival);
                            partidaIniciada = true;
                        }
                        catch (JugadorNoEncontrado)
                        {
                            if (partidaIniciada)
                            {
                                partidaIniciada = false;
                            }
                            else
                            {
                                marcador = "Esperando un oponente";
                            }
                        }
                        InicializarEscenario();
                        if (partidaFinalizada)
                        {
                            break;
                        }
                        if (partidaIniciada)
                        {
                            marcador = puntJugador1 + "  vs  " + puntJugador2;
                            InicializarEscenario();
                            Console.SetCursorPosition(posicionPelota.X, posicionPelota.Y);
                            Console.Write("°");
                            MoverPelota();
                        }
                        Thread.Sleep(70);
                        continue;
                    }
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (jugadores[miId].Posicion.Y > 0)
                            {
                                jugadores[miId].Posicion.Y--;
                                await cliente.EnviarPosicionAsync(jugadores[miId]);
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            if (jugadores[miId].Posicion.Y < (ALTO_ESCENARIO - TAMAÑO_PALETA))
                            {
                                jugadores[miId].Posicion.Y++;
                                await cliente.EnviarPosicionAsync(jugadores[miId]);
                            }
                            break;
                    }
                }
                transporte.Close();
                Console.Read();
            }
            catch (TApplicationException tApplicationException)
            {
                Console.Clear();
                Console.WriteLine(tApplicationException.StackTrace);
                Console.Read();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine(" Error al conectar con el servidor");
                Console.Read();
            }
        }

        private static void InicializarEscenario()
        {
            Console.Clear();
            for (int i = 0; i < TAMAÑO_PALETA; i++)
            {
                Console.SetCursorPosition(jugadores[0].Posicion.X, jugadores[0].Posicion.Y + i);
                Console.Write("|");
                Console.SetCursorPosition(jugadores[1].Posicion.X, jugadores[1].Posicion.Y + i);
                Console.Write("|");
            }
            Console.SetCursorPosition((ANCHO_ESCENARIO / 2) - (marcador.Length / 2), 0);
            Console.Write(marcador);
        }

        private static async void MoverPelota()
        {
            posicionPelota = await cliente.ConsultarPosicionPelotaAsync();
            if (posicionPelota.X == (ANCHO_ESCENARIO / 2) - 1 && posicionPelota.Y == (ALTO_ESCENARIO / 2) - 1)
            {
                await VerMarcador();
            }
        }

        private static async Task VerMarcador()
        {
            int puntaje1 = await cliente.ConsultarPuntajeAsync(miId);
            int puntaje2 = await cliente.ConsultarPuntajeAsync(idRival);
            if (miId % 2 == 0)
            {
                puntJugador1 = puntaje1;
                puntJugador2 = puntaje2;
            }
            else
            {
                puntJugador1 = puntaje2;
                puntJugador2 = puntaje1;
            }
            if (puntaje1 == PUNTOS_PARTIDA)
            {
                marcador = "¡Felicidades ganaste!";
                partidaFinalizada = true;
            }
            else if (puntaje2 == PUNTOS_PARTIDA)
            {
                marcador = "Mejor suerte la próxima";
                partidaFinalizada = true;
            }
        }

        
    }
}
