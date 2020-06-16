using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServidorPingPongThrift
{
    class ImplServicioPingPong : ServicioPingPong.IAsync
    {

        public List<Jugador> jugadores = new List<Jugador>();
        private int puntJugador1 = 0;
        private int puntJugador2 = 0;
        private const int TAMAÑO_PALETA = 6;
        private const int ANCHO_ESCENARIO = 70;
        private const int ALTO_ESCENARIO = 20;
        private const int PUNTOS_PARTIDA = 2;
        private static bool pelotaSubiendo = true;
        private static bool pelotaAIzquierda = true;
        private static Posicion posicionPelota = new Posicion
        {
            X = ANCHO_ESCENARIO / 2,
            Y = ALTO_ESCENARIO / 2
        };

        public bool MoverPelota()
        {
            if (puntJugador1 == PUNTOS_PARTIDA || puntJugador2 == PUNTOS_PARTIDA)
            {
                return false;
            }
            if (posicionPelota.Y == 0)
            {
                pelotaSubiendo = false;
            }
            else if (posicionPelota.Y == ALTO_ESCENARIO - 1)
            {
                pelotaSubiendo = true;
            }
            if (posicionPelota.X == 0)
            {
                puntJugador2++;
                RegresarPelotaAlCentro();
            }
            else if (posicionPelota.X == ANCHO_ESCENARIO - 1)
            {
                puntJugador1++;
                RegresarPelotaAlCentro();
            }
            if (posicionPelota.X == jugadores[0].Posicion.X + 1 &&
                (posicionPelota.Y >= jugadores[0].Posicion.Y && posicionPelota.Y <= jugadores[0].Posicion.Y + (TAMAÑO_PALETA - 1)))
            {
                pelotaAIzquierda = false;
            }
            else if (posicionPelota.X == jugadores[1].Posicion.X - 1 &&
              (posicionPelota.Y >= jugadores[1].Posicion.Y && posicionPelota.Y <= jugadores[1].Posicion.Y + (TAMAÑO_PALETA - 1)))
            {
                pelotaAIzquierda = true;
            }
            if (pelotaAIzquierda)
            {
                posicionPelota.X--;
            }
            else
            {
                posicionPelota.X++;
            }
            if (pelotaSubiendo)
            {
                posicionPelota.Y--;
            }
            else
            {
                posicionPelota.Y++;
            }
            return true;
        }

        private void RegresarPelotaAlCentro()
        {
            posicionPelota.X = ANCHO_ESCENARIO / 2;
            posicionPelota.Y = ALTO_ESCENARIO / 2;
            pelotaSubiendo = true;
            pelotaAIzquierda = true;
        }

        public void BorrarPartidaAnterior()
        {
            jugadores.Clear();
            puntJugador1 = 0;
            puntJugador2 = 0;
        }

        //public Task<Position> GetBallPositionAsync(CancellationToken cancellationToken = default)
        //{
        //    return Task.FromResult(posicionPelota);
        //}

        //public Task<PlayerPadPosition> GetLatestPlayerPadPositionAsync(int playerId, CancellationToken cancellationToken = default)
        //{
        //    //if (jugadores.Find(j => j.PlayerId == playerId) == null)
        //    //{
        //    //    throw new PlayerNotFoundException
        //    //    {
        //    //        Message = "No hay ningun jugador con " + playerId + " como id "
        //    //    };
        //    //}
        //    //return Task.FromResult(jugadores[playerId]);
        //}

        //public Task<int> GetPlayerScoreAsync(int playerId, CancellationToken cancellationToken = default)
        //{
        //    //if (jugadores.Find(player => player.PlayerId == playerId) == null)
        //    //{
        //    //    throw new PlayerNotFoundException
        //    //    {
        //    //        Message = "No hay ningun jugador con " + playerId + " como id "
        //    //    };
        //    //}
        //    //if (playerId == 0)
        //    //{
        //    //    return Task.FromResult(puntJugador1);
        //    //}
        //    //else
        //    //{
        //    //    return Task.FromResult(puntJugador2);
        //    //}
        //}

        //public Task<int> JoinGameAsync(CancellationToken cancellationToken = default)
        //{
        //    var idNuevoJugador = jugadores.Count;
        //    jugadores.Add(new PlayerPadPosition
        //    {
        //        PlayerId = idNuevoJugador,
        //        Position = new Position
        //        {
        //            X = 0,
        //            Y = 0
        //        }
        //    });
        //    Console.WriteLine(" Jugador " + jugadores.Count + " listo");
        //    return Task.FromResult(idNuevoJugador);
        //}

        //public Task SendPlayerPadPositionAsync(PlayerPadPosition playerPadPosition, CancellationToken cancellationToken = default)
        //{
        //    jugadores[playerPadPosition.PlayerId] = playerPadPosition;
        //    return Task.CompletedTask;
        //}

        public Task<int> EntrarALaPartidaAsync(CancellationToken cancellationToken = default)
        {
            var idNuevoJugador = jugadores.Count;
            jugadores.Add(new Jugador
            {
                IdJugador = idNuevoJugador,
                Posicion = new Posicion
                {
                    X = 0,
                    Y = 0
                }
            });
            Console.WriteLine(" Jugador " + jugadores.Count + " listo");
            return Task.FromResult(idNuevoJugador);
        }

        public Task EnviarPosicionAsync(Jugador jugador, CancellationToken cancellationToken = default)
        {
            jugadores[jugador.IdJugador] = jugador;
            return Task.CompletedTask;
        }

        public Task<Posicion> ConsultarPosicionPelotaAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(posicionPelota);
        }

        public Task<Jugador> ConsultarPosicionJugadorAsync(int idJugador, CancellationToken cancellationToken = default)
        {
            if (jugadores.Find(j => j.IdJugador == idJugador) == null)
            {
                throw new JugadorNoEncontrado
                {
                    IdJugador = idJugador,
                    Mensaje = "No hay ningun jugador con ese id "
                };
            }
            return Task.FromResult(jugadores[idJugador]);
        }

        public Task<int> ConsultarPuntajeAsync(int idJugador, CancellationToken cancellationToken = default)
        {
            if (jugadores.Find(j => j.IdJugador == idJugador) == null)
            {
                throw new JugadorNoEncontrado
                {
                    IdJugador = idJugador,
                    Mensaje = "No hay ningun jugador con ese id "
                };
            }
            if (idJugador == 0)
            {
                return Task.FromResult(puntJugador1);
            }
            else
            {
                return Task.FromResult(puntJugador2);
            }
        }
    }
}
