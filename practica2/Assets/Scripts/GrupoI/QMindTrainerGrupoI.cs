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
        public bool episodeWorking;
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //RECOMPENSAS
        public float ReturnAveraged { get; private set; }
        public float Return { get; private set; }

        //POSICION DE PERSONAJES EN EL MUNDO
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        
        //PARAMETROS
        QMindTrainerParams parametros;

        //MOVIMIENTO ENEMIGO
        INavigationAlgorithm nav;

        //INFO DEL MUNDO
        WorldInfo worldInfo;

        //OTRAS VARIABLES
        public float coefEpsilon;       
        TablaQ tablaq;
        List<float> totalRewards;
        #endregion


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            //Debug.Log("QMindTrainerDummy: initialized");
            parametros = qMindTrainerParams; 
            this.worldInfo = worldInfo;
            nav = navigationAlgorithm;
            nav.Initialize(worldInfo);     
            tablaq = new TablaQ();   
            
            coefEpsilon = 0.0f;       
            episodeWorking = false;            
        }

        public void DoStep(bool train)
        {

            //Debug.Log("QMindTrainerDummy: DoStep");
            //--------------------- INICIO EPISODIO-------------------
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
                    coefEpsilon = (CurrentEpisode / (float)parametros.episodes)*3.0f;
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
                State playerState = getState(AgentPosition,OtherPosition); //Estado actual
                int indice = tablaq.buscaIndiceEstado(playerState); //Indice del estado actual en la lista de estados
                float randomNumber = UnityEngine.Random.Range(0f, 1f);  //Numero aleatorio 0-1
                CellInfo[] path = nav.GetPath(OtherPosition, AgentPosition, 20);    //Camino del enemigo hacia el jugador                
                CellInfo nextPos = new CellInfo(0, 0);  // Crear como sería la posición de la mejor direccion

                if (path != null && path.Length > 0)
                {
                    // Buscar la mejor dirección posible en base a los valores q del estado actual
                    if (randomNumber >= parametros.epsilon)
                    {
                        int bestDirection = tablaq.buscaMejorDireccion(indice);
                        nextPos = getNextPos(bestDirection, path, indice);

                    }
                    // Usa direccion aleatoria
                    else
                    {
                        int direccionRandom = UnityEngine.Random.Range(0, 4);
                        nextPos = getNextPos(direccionRandom, path, indice);
                    }
                    AgentPosition = nextPos;
                    OtherPosition = path[0];
                    CurrentStep++;
                }
                else
                {
                    OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                    episodeWorking = false;
                }
            }
        }


        #region Método obtener estados
        private State getState(CellInfo agent, CellInfo other)
        {
            
            float signedangle = Mathf.Atan2(other.y - agent.y, other.x - agent.x) * Mathf.Rad2Deg; // Calcular el angulo en grados hacia el oponente            
            signedangle = (signedangle + 360 - (tablaq.angCuadrantes / 2)) % 360 ;    // Calcular el cuadrante del oponente en base al angulo            
            int cuadrante = (int)(signedangle / tablaq.angCuadrantes);
            Debug.Log(cuadrante);
            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1
            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);
            int cercano = (int)Math.Floor(dist / 10);

            // Devuelve si arriba hay muro
            CellInfo up = worldInfo.NextCell(agent, Directions.Up);
            CellInfo right = worldInfo.NextCell(agent, Directions.Right);
            CellInfo down = worldInfo.NextCell(agent, Directions.Down);
            CellInfo left = worldInfo.NextCell(agent, Directions.Left);

            int upw;
            int rightw;
            int downw;
            int leftw;

            #region comprobar si hay muros
            //  1 si hay muro, 0 si no hay nada
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
        #endregion

        #region Método para obtener siguiente posicion personaje
        private CellInfo getNextPos(int direction, CellInfo[] auxPath, int index)
        {
            CellInfo auxNextPos = new CellInfo(0,0);

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
            State nextState = getState(auxNextPos, auxPath[0]);
            int nextindice = tablaq.buscaIndiceEstado(nextState);
            // Recompensa
            float r;

            //--------------------- CALCULAR VALOR Q DEL ESTADO EN ESA ACCIÓN -------------------
            // SI SIGUIENTE POSICION CAMINABLE
            if (auxNextPos.Walkable)
            {
                float distActual = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
                float distNew = auxNextPos.Distance(auxPath[0], CellInfo.DistanceType.Manhattan);
                float distNewCross = auxNextPos.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

                // Si el agente se ha alejado del enemigo
                if (distNew > distActual)
                {
                    r = 1;
                }
                // Si se ha acercado al enemigo
                else if (distNew < distActual)
                {
                    r = -10;
                }
                // Si colisionan
                else if (distNew == 0 || distNewCross == 0)
                {
                    r = -100;
                    episodeWorking = false;
                    OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                }
                // Si se han mantenido las distancias
                else
                {                    
                    r = 0;
                }

                tablaq.listValues[index][direction] = (1 - parametros.alpha) * (tablaq.listValues[index][direction]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                totalRewards.Add(tablaq.listValues[index][direction]);
                Return = totalRewards.Sum();
                ReturnAveraged = totalRewards.Sum() / totalRewards.Count();
            }
            // Si SIGUIENTE POSICION NO CAMINABLE
            else if (!auxNextPos.Walkable)
            {
                r = -100;
                tablaq.listValues[index][direction] = (1 - parametros.alpha) * (tablaq.listValues[index][direction]) + parametros.alpha * (r + parametros.gamma * (tablaq.listValues[nextindice].Max()));
                totalRewards.Add(tablaq.listValues[index][direction]);
                Return = totalRewards.Sum();
                ReturnAveraged = totalRewards.Sum()/totalRewards.Count();
                episodeWorking = false;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                
            }
            // -------------------------------------------

            return auxNextPos;
        }
        #endregion
    }
}
