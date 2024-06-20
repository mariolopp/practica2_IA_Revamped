using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using UnityEngine;

using static UnityEngine.Rendering.DebugUI;

namespace QMind
{
    public class QMindTesterGrupoI : IQMind
    {
        #region Variables
        WorldInfo worldInfo;
        TablaQ tablaq;
        CellInfo nextPos;
        #endregion

        public void Initialize(WorldInfo worldInfo)
        {
            //Debug.Log("QMindDummy: initialized");
            this.worldInfo = worldInfo;
            tablaq = new TablaQ();   
            nextPos = new CellInfo(0,0);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMindDummy: GetNextStep");
            State currentState = getState(currentPosition, otherPosition);
            int indice = tablaq.buscaIndiceEstado(currentState);
            int bestDirection = tablaq.buscaMejorDireccion(indice);

            switch (bestDirection)
            {
                case 0:
                    nextPos = worldInfo.NextCell(currentPosition, Directions.Up);
                    break;
                case 1:
                    nextPos = worldInfo.NextCell(currentPosition, Directions.Right);
                    break;
                case 2:
                    nextPos = worldInfo.NextCell(currentPosition, Directions.Down);
                    break;
                case 3:
                    nextPos = worldInfo.NextCell(currentPosition, Directions.Left);
                    break;
            }
            return nextPos;
        }

        private State getState(CellInfo agent, CellInfo other)
        {

            float signedangle = Mathf.Atan2(other.y - agent.y, other.x - agent.x) * Mathf.Rad2Deg; // Calcular el angulo en grados hacia el oponente            
            signedangle = (signedangle + 360 - (tablaq.angCuadrantes / 2)) % 360;    // Calcular el cuadrante del oponente en base al angulo            
            int cuadrante = (int)(signedangle / tablaq.angCuadrantes);

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1
            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);
            int cercano = (int)Math.Floor(dist / tablaq.tamFranjaDist);

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
    }

}
