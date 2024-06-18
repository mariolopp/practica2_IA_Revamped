using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Linq;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;

namespace QMind
{
    public class QMindTrainerGrupoI : IQMindTrainer
    {
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public bool episodeWorking;
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        QMindTrainerParams parametros;
        INavigationAlgorithm nav;
        public GameObject scenery;

        TablaQ tablaq;
        WorldInfo worldInfo;

        
        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            tablaq = new TablaQ();
            Debug.Log("QMindTrainerDummy: initialized");
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            parametros = qMindTrainerParams;
            nav = navigationAlgorithm;
            nav.Initialize(worldInfo);
            this.worldInfo = worldInfo;
            episodeWorking = false;
            
        }


        public void DoStep(bool train)
        {
            Debug.Log("QMindTrainerDummy: DoStep");
            if (!episodeWorking)
            {
                episodeWorking = true;
                AgentPosition = worldInfo.RandomCell();
                OtherPosition = worldInfo.RandomCell();
                OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Array de ints en el que se introducen los resultados (podría usarse tambien una clase state)
                //State playerState = new State(); // Estado del personaje


                State playerState = getState();

                // Buscar a que indice de la lista de estados corresponde el estado actual del personaje
                int indice = tablaq.buscaIndiceEstado(playerState);

                // Random para decidir si hará caso a la tabla o no
                float randomNumber = UnityEngine.Random.Range(0f, 1f);


                CellInfo[] path = nav.GetPath(OtherPosition, AgentPosition, 20);
                // Si es menor de 0.85 se hace caso a la tabla
                if (randomNumber < parametros.epsilon)
                {
                    // Buscar la mejor dirección posible en base a la lista de valores de un indice de estado
                    int bestDirection = tablaq.buscaMejorDireccion(indice);

                    // Crear como sería la posición de la mejor direccion
                    CellInfo nextPos = new CellInfo(0, 0);
                    switch (bestDirection)
                    {
                        case 0:
                            nextPos = worldInfo.NextCell(AgentPosition, Directions.Up);
                            break;
                        case 1:
                            nextPos = worldInfo.NextCell(AgentPosition, Directions.Right);
                            break;
                        case 2:
                            nextPos = worldInfo.NextCell(AgentPosition, Directions.Down);
                            break;
                        case 3:
                            nextPos = worldInfo.NextCell(AgentPosition, Directions.Left);
                            break;
                    }


                    // FORMULA
                    State nextState = getStateParam(nextPos, path[0]);
                    int nextindice = tablaq.buscaIndiceEstado(nextState);

                    // Calular recompensa

                    // Si el siguiente estado es eliminatorio damos la mayor penalización
                    float r = 0;
                    if (nextPos.Walkable)
                    {
                        // Si el agente se ha alejado del enemigo
                        tablaq.listValues[indice][bestDirection] = (1 - parametros.alpha) * (tablaq.listValues[indice][bestDirection]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                    }
                    else
                    {
                        // Si el estado al que ha caminado es eliminatorio
                        r = -100;
                        tablaq.listValues[indice][bestDirection] = (1 - parametros.alpha) * (tablaq.listValues[indice][bestDirection]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                        // Acabar episodio
                        episodeWorking = false;
                        OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                    }
                    // Si el siguiente estado es mal damos una pequeña penalización
                    // Si el siguiente estado es positivo damos una pequeña recompensa




                    // Si decide algo eliminatorio penalizar tablaQ y finalizar al juego

                    // Asignar la dirección al personaje

                }
                // Si es mayor de 0.85 se escoge una direccion aleatoria
                else
                {
                    int direccion = UnityEngine.Random.Range(0, 4);

                }


                // actualizar 
                Debug.Log("Indice del estado x: " + indice);


                OtherPosition = path[0];
            }
        }
        
        
        public State getState()
        {
            // Calcular el angulo en grados hacia el oponente
            float signedangle = Mathf.Atan2(OtherPosition.y - AgentPosition.y,
                OtherPosition.x - AgentPosition.x) * Mathf.Rad2Deg;

            // Calcular el cuadrante del oponente en base al angulo
            signedangle = (signedangle + 360) % 360;
            Debug.Log("Angulo enemigo signed: " + signedangle);

            int cuadrante = (int)(signedangle / 90);
            Debug.Log("Cuadrante obtenido: " + cuadrante);

            // Escribir el cuadrante en el estado
            //playerState.cuadrante = cuadrante;

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1∣

            float dist = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

            int cercano = (int)Math.Floor(dist / 10);

            Debug.Log("Cercania es: " + cercano);

            // Se utilizarán mas adelante para saber si hay muro con walkable
            CellInfo up = worldInfo.NextCell(AgentPosition, Directions.Up);
            CellInfo right = worldInfo.NextCell(AgentPosition, Directions.Right);
            CellInfo down = worldInfo.NextCell(AgentPosition, Directions.Down);
            CellInfo left = worldInfo.NextCell(AgentPosition, Directions.Left);

            int upw;
            int rightw;
            int downw;
            int leftw;

            // 1 si hay muro, 0 si no hay nada
            #region ifs
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
            #endregion

            // Escribir todos los datos en el estado actual del personaje
            State playerState = new State(upw, rightw, downw, leftw, cercano, cuadrante);

            return playerState;
        }
        private State getStateParam(CellInfo agent, CellInfo other)
        {
            // Calcular el angulo en grados hacia el oponente
            float signedangle = Mathf.Atan2(other.y - agent.y,
                other.x - agent.x) * Mathf.Rad2Deg;

            // Calcular el cuadrante del oponente en base al angulo
            signedangle = (signedangle + 360) % 360;
            Debug.Log("Angulo enemigo signed: " + signedangle);

            int cuadrante = (int)(signedangle / 90);
            Debug.Log("Cuadrante obtenido: " + cuadrante);

            // Escribir el cuadrante en el estado
            //playerState.cuadrante = cuadrante;

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1∣

            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);

            int cercano = (int)Math.Floor(dist / 10);

            Debug.Log("Cercania es: " + cercano);

            // Devuelve si arriba hay muro
            CellInfo up = worldInfo.NextCell(agent, Directions.Up);
            CellInfo right = worldInfo.NextCell(agent, Directions.Right);
            CellInfo down = worldInfo.NextCell(agent, Directions.Down);
            CellInfo left = worldInfo.NextCell(agent, Directions.Left);

            int upw;
            int rightw;
            int downw;
            int leftw;

            // 1 si hay muro, 0 si no hay nada
            #region ifs
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
            #endregion

            // Escribir todos los datos en el estado actual del personaje
            State playerState = new State(upw, rightw, downw, leftw, cercano, cuadrante);

            return playerState;
        }
    }
}
