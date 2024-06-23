using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;

namespace QMind
{
    public class QMindTrainerGrupoI : IQMindTrainer
    {

        #region Variables

        //EPISODIOS
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public bool episodeWorking = false;
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //RECOMPENSAS
        public float ReturnAveraged { get; private set; }
        public float Return { get; private set; }

        //POSICION DE PERSONAJES EN EL MUNDO
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }

        //PARAMETROS
        private QMindTrainerParams parametros;

        //MOVIMIENTO ENEMIGO
        private INavigationAlgorithm nav;

        //INFO DEL MUNDO
        private WorldInfo worldInfo;

        //OTRAS VARIABLES
        public float coefEpsilon;
        private TablaQ tablaq;
        private List<float> totalRewards;

        #endregion


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            //Debug.Log("QMindTrainerDummy: initialized");
            parametros = qMindTrainerParams;
            this.worldInfo = worldInfo;
            nav = navigationAlgorithm;
            nav.Initialize(worldInfo);
            tablaq = new TablaQ();
            parametros.epsilon = 1.0f;
            coefEpsilon = 0.0f;
        }

        public void DoStep(bool train)
        {

            //Debug.Log("QMindTrainerDummy: DoStep");
            //--------------------- INICIO EPISODIO -------------------
            if (!episodeWorking)
            {
                totalRewards = new List<float>();
                Return = 0;
                ReturnAveraged = 0;
                CurrentEpisode++;
                CurrentStep = 0;
                AgentPosition = worldInfo.RandomCell();
                OtherPosition = worldInfo.RandomCell();

                if (CurrentEpisode % parametros.episodesBetweenSaves == 0)
                {
                    Debug.Log("Guardando tabla");
                    tablaq.guardarCSV(tablaq.ruta);
                    coefEpsilon = (CurrentEpisode / (float)parametros.episodes) * 3.0f;
                    parametros.epsilon = Mathf.Exp(-coefEpsilon);
                    Debug.Log(parametros.epsilon);
                }

                episodeWorking = true;
                OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            }
            //--------------------------------------------------------

            //--------------------------- CONTINUACION EPISODIO ------------------------------------
            else
            {
                State playerState = getState(AgentPosition, OtherPosition, worldInfo, tablaq);     // Estado actual
                int indice = tablaq.buscaIndiceEstado(playerState);             // Indice del estado actual en la lista de estados
                float randomNumber = UnityEngine.Random.Range(0f, 1f);          // Numero aleatorio 0-1
                CellInfo[] path = nav.GetPath(OtherPosition, AgentPosition, 20);// Camino del enemigo hacia el jugador                

                // Se consideraba la posicion futura, esto podia hacer que al calcular 
                // las distancias y otros parametros se diera la misma ocasion y el 
                // agente no se diera cuenta de que se estaba alejando lo que es algo 
                // positivo, para ello se pone despues de todo el calculo que hay 
                // respecto el agente y se actualiza la posicion del perseguidor despues
                if (path != null && path.Length > 0)
                {
                    //OtherPosition = path[0]; // calcularlo antes - Cosas diferentes
                    int nextDirection; // Crear como seria la posicion de la mejor direccion
                    // Buscar la mejor direccion posible en base a los valores q del estado actual
                    if (randomNumber >= parametros.epsilon)
                    {
                        nextDirection = tablaq.buscaMejorDireccion(indice);
                    }
                    // Usa direccion aleatoria
                    else
                    {
                        nextDirection = UnityEngine.Random.Range(0, 4);
                    }
                    CellInfo nextPos = getNextPos(nextDirection, path[0], indice, playerState);
                    AgentPosition = nextPos;
                    OtherPosition = path[0]; // calcularlo despues - Cosas diferentes
                    CurrentStep++;
                }
                // Os dejo esto aqui pero deberiais de tener un metodo que os diga
                // si algo es terminal o no
                // Encapsulariais toda la logica para acabarlo facilmente ;)
                else
                {
                    OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                    episodeWorking = false;
                }
            }
        }


        #region Metodo obtener estados
        public State getState(CellInfo agent, CellInfo other, WorldInfo worldInfo, TablaQ tablaq)
        {
            Vector2 dif = new Vector2(other.x, other.y) - new Vector2(agent.x, agent.y);
            //float signedangle = Mathf.Atan2(other.y - agent.y, other.x - agent.x) * Mathf.Rad2Deg; // Calcular el angulo en grados hacia el oponente            
            float signedangle = Vector2.SignedAngle(new Vector2(1, 0), dif); // Calcular el angulo en grados hacia el oponente            

            signedangle = (signedangle - (tablaq.angCuadrantes / 2));    // Calcular el cuadrante del oponente en base al angulo en Cº
            if (signedangle < 0)
            {
                signedangle += 360;
            }
            int cuadrante = (int)(signedangle / tablaq.angCuadrantes); // Calcula el indice del cuadrante perteneciente

            //Debug.Log(cuadrante);

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=|x2-x1|+|y2-y1|
            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);

            // Calculo la franja de distancia (el min deberia seleccionar siempre al de la izq,
            // pero el caso de ser distancia 40 clavado podría salir 3 y el floor a 3 en vez de 2.9999 con floor a 2)
            //int cercano = (int)Math.Min(Math.Floor(dist / (40 / tablaq.numFranjasDist)), (tablaq.numFranjasDist - 1));
            int cercano = 0;
            if      (dist >= 0 && dist <= tablaq.franja1) { cercano = 0; }
            else if (dist > tablaq.franja1 && dist <= tablaq.franja2) { cercano = 1; }
            else if (dist > tablaq.franja2 && dist <= tablaq.franja3) { cercano = 2; }
            else if (dist > tablaq.franja3 && dist <= tablaq.franja4) { cercano = 3; }
            else
            {
                Debug.Log("Las franjas de distancia obtenida no esta en las franjas delimitadas, revise las franjas en TablaQ.cs");
                cercano = 3;
            }
            // Devuelve si arriba hay muro
            CellInfo up = worldInfo.NextCell(agent, Directions.Up);
            CellInfo right = worldInfo.NextCell(agent, Directions.Right);
            CellInfo down = worldInfo.NextCell(agent, Directions.Down);
            CellInfo left = worldInfo.NextCell(agent, Directions.Left);

            #region comprobar si hay muros
            //  1 si hay muro, 0 si no hay nada
            int upw = up.Walkable ? 0 : 1;
            int rightw = right.Walkable ? 0 : 1;
            int downw = down.Walkable ? 0 : 1;
            int leftw = left.Walkable ? 0 : 1;
            /*
            int upw;
            int rightw;
            int downw;
            int leftw;

            if (up.Walkable)
            {
                upw = 0;
            }
            else
            {
                upw = 1;
            }

            if (right.Walkable)
            {
                rightw = 0;
            }
            else
            {
                rightw = 1;
            }

            if (down.Walkable)
            {
                downw = 0;
            }
            else
            {
                downw = 1;
            }

            if (left.Walkable)
            {
                leftw = 0;
            }
            else
            {
                leftw = 1;
            }
            */
            #endregion

            // Escribir todos los datos en el estado actual del personaje
            State playerState = new State(upw, rightw, downw, leftw, cercano, cuadrante);

            return playerState;
        }
        #endregion

        #region Metodo para obtener siguiente posicion del personaje
        private CellInfo getNextPos(int direction, CellInfo otherFuturePosition, int index, State currentState)
        {
            CellInfo auxNextPos = new CellInfo(0, 0);

            //ESTABLECER NUEVA POSICION DEL PERSONAJE
            switch (direction)
            {
                case 0:
                    auxNextPos = worldInfo.NextCell(AgentPosition, Directions.Up);
                    break;
                case 1:
                    auxNextPos = worldInfo.NextCell(AgentPosition, Directions.Right);
                    break;
                case 2:
                    auxNextPos = worldInfo.NextCell(AgentPosition, Directions.Down);
                    break;
                case 3:
                    auxNextPos = worldInfo.NextCell(AgentPosition, Directions.Left);
                    break;
            }

            // Estado siguiente
            State nextState = getState(auxNextPos, otherFuturePosition, worldInfo, tablaq); // Obtengo el estado futuro
            int nextindice = tablaq.buscaIndiceEstado(nextState);
            // Recompensa
            float r;

            //--------------------- CALCULAR VALOR Q DEL ESTADO EN ESA ACCIoN -------------------
            // SI SIGUIENTE POSICION CAMINABLE
            if (auxNextPos.Walkable)
            {

                // Distancia entre la pos vieja del zombie y pos vieja del perseguidor // bien
                float distActual = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
                // Distancia entre la pos nueva del zombie y pos nueva del perseguidor // mal
                float distNew = auxNextPos.Distance(otherFuturePosition, CellInfo.DistanceType.Manhattan);
                // Distancia entre la pos nueva del zombie y pos vieja del perseguidor // bien
                float distNewCross = auxNextPos.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

                r = reward(distActual, distNew, distNewCross);

                update(index, direction, nextindice, r);
            }
            // SI SIGUIENTE POSICION NO CAMINABLE
            else
            {
                r = -100;
                update(index, direction, nextindice, r);
                episodeWorking = false;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            }
            // -------------------------------------------

            return auxNextPos;
        }
        #endregion
        #region gestion tabla corto
        private void update(int index, int direction, int nextindice, float r)
        {
            // Aplicar la formula
            tablaq.listValues[index][direction] = (1 - parametros.alpha) * (tablaq.listValues[index][direction]) +
            parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));

            // Para ver las estadisticas por pantalla
            totalRewards.Add(tablaq.listValues[index][direction]);
            Return = totalRewards.Sum();
            ReturnAveraged = totalRewards.Sum() / totalRewards.Count();
        }
        private float reward(float distActual, float distNew, float distNewCross)
        {
            float r;
            // Si el agente se ha alejado del enemigo
            //if (currentState.cercania <= nextState.cercania)

            // Si colisionan
            if (distNew == 0 || distNewCross == 0)
            {
                r = -100f;
                episodeWorking = false;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                //Debug.Log("Recompensa " + r);
            }
            // Si se aleja
            else if (distNew > distActual)
            {
                r = 0f;
            }
            // Si se ha acercado al enemigo
            else if (distNew < distActual)
            {
                r = -10f;
            }
            // Si se han mantenido las distancias
            else
            {
                r = 0f;
            }
            return r;
        }
        #endregion
    }
}
