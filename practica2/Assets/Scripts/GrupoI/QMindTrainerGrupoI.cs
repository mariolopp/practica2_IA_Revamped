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

// ------------- CLASE DE ENTRENO DE LA TABLA Q --------------------
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

        #region Métodos de la Interfaz
        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            parametros = qMindTrainerParams;
            this.worldInfo = worldInfo;
            nav = navigationAlgorithm;
            nav.Initialize(worldInfo);
            tablaq = new TablaQ();
            parametros.epsilon = 1.0f;      // Parametro epsilo a 1 por defecto
            coefEpsilon = 0.0f;             // coeficiente de calculo del descenso del parametro epsilon
        }

        // ------------------- MÉTODO PRINCIPAL -----------------------
        public void DoStep(bool train)
        {

            //--------------------- INICIO EPISODIO -------------------
            //Al inicio del episodio
            if (!episodeWorking)
            {
                // Resetear todos los valores y cambiar posición personajes aleatoriamente
                totalRewards = new List<float>();
                Return = 0;
                ReturnAveraged = 0;
                CurrentStep = 0;                    
                AgentPosition = worldInfo.RandomCell();
                OtherPosition = worldInfo.RandomCell();
                CurrentEpisode++;   // aumentar número episodio

                // Por cada número establecido de episodios, actualizar tabla q
                if (CurrentEpisode % parametros.episodesBetweenSaves == 0)
                {
                    tablaq.guardarCSV(tablaq.ruta);
                    coefEpsilon = (CurrentEpisode / (float)parametros.episodes) * 3.0f; // Coeficiente de función e^-(coefEpsilon)
                    parametros.epsilon = Mathf.Exp(-coefEpsilon);   // Función e^-(coefEpsilon) (Se reducé gradualmente el porcentaje de entrenos aleatorios)
                }

                // Activación del episodio
                episodeWorking = true;
                OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            }
            //-------------------- FIN INICIO EPISODIO --------------------

            //--------------------------- CONTINUAR EPISODIO ------------------------------------
            else
            {
                State playerState = getState(AgentPosition, OtherPosition, worldInfo, tablaq);     // Estado actual
                int indice = tablaq.buscaIndiceEstado(playerState);             // Indice del estado actual en la lista de estados
                float randomNumber = UnityEngine.Random.Range(0f, 1f);          // Numero aleatorio 0-1
                CellInfo[] path = nav.GetPath(OtherPosition, AgentPosition, 20);// Camino del enemigo hacia el jugador                

                // Cuando el perseguidor no está junto al perseguido (evitar Error index out of range)
                if (path != null && path.Length > 0)
                {
                    int nextDirection; // Crear como seria la posicion de la mejor direccion

                    // Busca la mejor direccion posible en base a los valores q del estado actual
                    if (randomNumber >= parametros.epsilon)
                    {
                        nextDirection = tablaq.buscaMejorDireccion(indice);
                    }
                    // Usa direccion aleatoria
                    else
                    {
                        nextDirection = UnityEngine.Random.Range(0, 4);
                    }

                    CellInfo nextPos = getNextPos(nextDirection, path[0], indice);  // Obtener la siguiente posición del perseguido
                    // Nuevas posiciones de los personajes
                    AgentPosition = nextPos;
                    OtherPosition = path[0];
                    CurrentStep++;  // siguiente paso sin ser perseguido o salirse de posiciones caminables
                }
                // Si el perseguidor ha colisionado con el perseguido, se acaba el episodio
                else
                {
                    OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                    episodeWorking = false;
                }
            }
            //-------------------- FIN CONTINUAR EPISODIO --------------------
        }

        #endregion

        #region Metodo Secundarios

        // ------------------- MÉTODO PARA OBTENER ESTADOS -----------------------
        public State getState(CellInfo agent, CellInfo other, WorldInfo worldInfo, TablaQ tablaq)
        {
            // ------------ CÁLCULO CUADRANTE ---------------
            Vector2 dif = new Vector2(other.x, other.y) - new Vector2(agent.x, agent.y);   // Diferencia entre ambos personajes      
            float signedangle = Vector2.SignedAngle(new Vector2(1, 0), dif); // Calcular el angulo con signo en grados hacia el oponente            
            signedangle = (signedangle - (tablaq.angCuadrantes / 2));    // Calcular el cuadrante del oponente en base al angulo en Cº
            // Cambiar a grado equivalente positivo
            if (signedangle < 0)
            {
                signedangle += 360;
            }
            int cuadrante = (int)(signedangle / tablaq.angCuadrantes); // Calcula el indice del cuadrante perteneciente
            // ------------ FIN CÁLCULO CUADRANTE ---------------

            // ------------ CÁLCULO RANGO DE DISTANCIA ---------------
            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=|x2-x1|+|y2-y1|
            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);

            // Calculo la franja de distancia
            int cercano;
            if      (dist >= 0 && dist <= tablaq.franja1) { cercano = 0; }
            else if (dist > tablaq.franja1 && dist <= tablaq.franja2) { cercano = 1; }
            else if (dist > tablaq.franja2 && dist <= tablaq.franja3) { cercano = 2; }
            else if (dist > tablaq.franja3 && dist <= tablaq.franja4) { cercano = 3; }
            else
            {
                Debug.Log("Las franjas de distancia obtenida no esta en las franjas delimitadas, revise las franjas en TablaQ.cs");
                cercano = 3;
            }
            // ------------ FIN CÁLCULO RANGO DE DISTANCIA ---------------

            // ------------ CÁLCULO DE PAREDES ---------------
            CellInfo up = worldInfo.NextCell(agent, Directions.Up);
            CellInfo right = worldInfo.NextCell(agent, Directions.Right);
            CellInfo down = worldInfo.NextCell(agent, Directions.Down);
            CellInfo left = worldInfo.NextCell(agent, Directions.Left);

            //  1 si hay muro, 0 si no hay nada
            int upw = up.Walkable ? 0 : 1;
            int rightw = right.Walkable ? 0 : 1;
            int downw = down.Walkable ? 0 : 1;
            int leftw = left.Walkable ? 0 : 1;
            // ------------ FIN CÁLCULO DE PAREDES ---------------

            // Escribir todos los datos en el estado actual del personaje
            State playerState = new State(upw, rightw, downw, leftw, cercano, cuadrante);

            return playerState;
        }
 

        // ------------------- MÉTODO PARA OBTENER SIGUIENTE POSICIÓN -----------------------
        private CellInfo getNextPos(int direction, CellInfo otherFuturePosition, int index)
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

            State nextState = getState(auxNextPos, otherFuturePosition, worldInfo, tablaq); // Estado futuro
            int nextindice = tablaq.buscaIndiceEstado(nextState);   // indice del Estado futuro           
            float r;    // Recompensa

            //--------------------- CALCULAR VALOR Q DEL ESTADO EN ESA ACCIÓN -------------------
            // SI SIGUIENTE POSICION CAMINABLE
            if (auxNextPos.Walkable)
            {
                // Distancia entre la pos vieja del zombie y pos vieja del perseguidor
                float distActual = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
                // Distancia entre la pos nueva del zombie y pos nueva del perseguidor
                float distNew = auxNextPos.Distance(otherFuturePosition, CellInfo.DistanceType.Manhattan);
                // Distancia entre la pos nueva del zombie y pos vieja del perseguidor
                float distNewCross = auxNextPos.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

                r = Reward(distActual, distNew, distNewCross);  // Establecer la recompensa
                UpdateQ(index, direction, nextindice, r);   // Cálcular nuevo valor q de la acción del estado
            }
            // SI SIGUIENTE POSICION NO CAMINABLE
            else
            {
                r = -100f;  //Penalización
                UpdateQ(index, direction, nextindice, r);   // Cálcular nuevo valor q de la acción del estado
                // Fin del episodio
                episodeWorking = false;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            }
            // -------------------------------------------

            return auxNextPos;
        }

        // ------------------- MÉTODO PARA ACTUALIZAR VALOR Q |ESTADO - ACCIÓN ELEGIDA| -----------------------
        private void UpdateQ(int index, int direction, int nextindice, float reward)
        {
            // Aplicar la formula Q(S,A) = (1 - alpha)*Q(S,A) + alpha*(recompensa + (gamma * Max(Q(S',A')) 
            tablaq.listValues[index][direction] = (1 - parametros.alpha) * (tablaq.listValues[index][direction]) +
            parametros.alpha * (reward + parametros.gamma * (tablaq.listValues[nextindice].Max()));

            // Para ver las estadisticas por pantalla
            totalRewards.Add(tablaq.listValues[index][direction]);
            Return = totalRewards.Sum();
            ReturnAveraged = totalRewards.Sum() / totalRewards.Count();
        }

        // ------------------- MÉTODO PARA SELECCIONAR RECOMPENSA |ESTADO - ACCIÓN ELEGIDA| -----------------------
        private float Reward(float distActual, float distNew, float distNewCross)
        {
            float auxR; // recompensa seleccionada

            // Si colisionan
            if (distNew == 0 || distNewCross == 0)
            {
                auxR = -100f;
                episodeWorking = false;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                //Debug.Log("Recompensa " + r);
            }
            // Si se aleja
            else if (distNew > distActual)
            {
                auxR = 0f;
            }
            // Si se ha acercado al enemigo
            else if (distNew < distActual)
            {
                auxR = -10f;
            }
            // Si se han mantenido las distancias
            else
            {
                auxR = 0f;
            }

            return auxR;
        }
        #endregion
    }
}
