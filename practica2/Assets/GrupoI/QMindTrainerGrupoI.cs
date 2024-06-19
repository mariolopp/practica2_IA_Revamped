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
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
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
            Debug.Log("QMindTrainerDummy: initialized");
            parametros = qMindTrainerParams; 
            this.worldInfo = worldInfo;
            nav = navigationAlgorithm;
            nav.Initialize(worldInfo);     
            tablaq = new TablaQ();            
            //AgentPosition = worldInfo.RandomCell();
            //OtherPosition = worldInfo.RandomCell();           
            episodeWorking = false;            
        }

        public void DoStep(bool train)
        {

            //Debug.Log("QMindTrainerDummy: DoStep");
            if (!episodeWorking)
            {                
                AgentPosition = worldInfo.RandomCell();
                OtherPosition = worldInfo.RandomCell();
                CurrentEpisode++;
                tablaq.guardarCSV();
                CurrentStep = 0;
                episodeWorking = true;
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


                // Crear como sería la posición de la mejor direccion
                CellInfo nextPos = new CellInfo(0, 0);

                bool puede = true;
                if (randomNumber < parametros.epsilon)
                {
                    // Buscar la mejor dirección posible en base a la lista de valores de un indice de estado
                    int bestDirection = tablaq.buscaMejorDireccion(indice);
                    
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

                    if (path != null && path.Length > 0)
                    {
                        // FORMULA
                        State nextState = getStateParam(nextPos, path[0]);
                        int nextindice = tablaq.buscaIndiceEstado(nextState);

                        // Calular recompensa

                        // Si el siguiente estado es eliminatorio damos la mayor penalización
                        float r = 0;
                        if (nextPos.Walkable)
                        {
                            float distActual = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
                            float distNew = nextPos.Distance(path[0], CellInfo.DistanceType.Manhattan);

                            if (distNew > distActual)
                            {
                                // Si el agente se ha alejado del enemigo
                                r = 1;
                            }
                            else if (distNew < distActual)
                            {
                                // Si se ha acercado al enemigo
                                r = -10;
                            }
                            else if (distNew==0) { // Si la nueva acción provocaría que colisionaran...
                                r = -100;
                                puede = false;
                            }
                            else
                            {
                                // Si se ha mantenido
                                r = 0;
                            }

                            tablaq.listValues[indice][bestDirection] = (1 - parametros.alpha) * (tablaq.listValues[indice][bestDirection]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                        }
                        else if (!nextPos.Walkable || (nextPos.x == path[0].x && nextPos.y == path[0].y)) //|| nextPos == path[0]
                        {
                            episodeWorking = false;
                            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                            // Si el estado al que ha caminado es eliminatorio
                            r = -100;
                            tablaq.listValues[indice][bestDirection] = (1 - parametros.alpha) * (tablaq.listValues[indice][bestDirection]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));                        // Acabar episodio

                        }
                    }
                    else {
                        Debug.Log("El if no se ha cumplido");
                        puede = false;
                    }
                               

                }
                // Si es mayor de 0.85 se escoge una direccion aleatoria
                else
                {
                    int direccionRandom = UnityEngine.Random.Range(0, 4);
                    switch (direccionRandom)
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

                    if (path != null && path.Length > 0) {
                        // FORMULA
                        State nextState = getStateParam(nextPos, path[0]);
                        int nextindice = tablaq.buscaIndiceEstado(nextState);

                        // Calular recompensa

                        // Si el siguiente estado es eliminatorio damos la mayor penalización
                        float r = 0;
                        if (nextPos.Walkable)
                        {
                            float distActual = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
                            float distNew = nextPos.Distance(path[0], CellInfo.DistanceType.Manhattan);

                            if (distNew > distActual)
                            {
                                // Si el agente se ha alejado del enemigo
                                r = 1;
                            }
                            else if (distNew < distActual)
                            {
                                // Si se ha acercado al enemigo
                                r = -10;
                            }
                            else if (distNew == 0)
                            { // Si la nueva acción provocaría que colisionaran...
                                r = -100;
                                puede = false;
                            }
                            else
                            {
                                // Si se ha mantenido
                                r = 0;
                            }

                            tablaq.listValues[indice][direccionRandom] = (1 - parametros.alpha) * (tablaq.listValues[indice][direccionRandom]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                        }
                        else if (!nextPos.Walkable || (nextPos.x == path[0].x && nextPos.y == path[0].y)) //|| nextPos == path[0]
                        {
                            
                            episodeWorking = false;
                            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                            // Si el estado al que ha caminado es eliminatorio
                            r = -100;
                            tablaq.listValues[indice][direccionRandom] = (1 - parametros.alpha) * (tablaq.listValues[indice][direccionRandom]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));                        // Acabar episodio

                        }
                    }else {
                        Debug.Log("El if no se ha cumplido");
                        puede = false;
                    }
                }


                // actualizar 
                //Debug.Log("Indice del estado x: " + indice);
                if (puede)
                {
                    AgentPosition = nextPos;
                    OtherPosition = path[0];
                    CurrentStep++;
                }
                else {
                    OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                    episodeWorking = false;
                }
            }
        }
        
        
        public State getState()
        {
            // Calcular el angulo en grados hacia el oponente
            float signedangle = Mathf.Atan2(OtherPosition.y - AgentPosition.y, OtherPosition.x - AgentPosition.x) * Mathf.Rad2Deg;

            // Calcular el cuadrante del oponente en base al angulo
            signedangle = (signedangle + 360) % 360;
            //Debug.Log("Angulo enemigo signed: " + signedangle);

            int cuadrante = (int)(signedangle / 90);
            //Debug.Log("Cuadrante obtenido: " + cuadrante);

            // Escribir el cuadrante en el estado
            //playerState.cuadrante = cuadrante;

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1∣

            float dist = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

            int cercano = (int)Math.Floor(dist / 10);

            //Debug.Log("Cercania es: " + cercano);

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
            //Debug.Log("Angulo enemigo signed: " + signedangle);

            int cuadrante = (int)(signedangle / 90);
            //Debug.Log("Cuadrante obtenido: " + cuadrante);

            // Escribir el cuadrante en el estado
            //playerState.cuadrante = cuadrante;

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1∣

            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);

            int cercano = (int)Math.Floor(dist / 10);

            //Debug.Log("Cercania es: " + cercano);

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
