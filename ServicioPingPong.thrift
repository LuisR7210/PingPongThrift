
struct Posicion {
    1: i32 x;
    2: i32 y;
}

struct Jugador {
    1: i32 idJugador;
    2: Posicion posicion;
}

exception JugadorNoEncontrado {
	1: i32 idJugador;
    2: string mensaje;
}

service ServicioPingPong {

	i32 EntrarALaPartida();
    
    void EnviarPosicion(1: Jugador jugador);

    Posicion ConsultarPosicionPelota();

    Jugador ConsultarPosicionJugador(1: i32 idJugador) throws (1: JugadorNoEncontrado excepcionJugadorNoEncontrado);

    i32 ConsultarPuntaje(1: i32 idJugador);
    
}
