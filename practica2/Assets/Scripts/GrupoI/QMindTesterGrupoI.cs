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

            Vector2 dif = new Vector2(other.x, other.y) - new Vector2(agent.x, agent.y);
            //float signedangle = Mathf.Atan2(other.y - agent.y, other.x - agent.x) * Mathf.Rad2Deg; // Calcular el angulo en grados hacia el oponente            
            float signedangle = Vector2.SignedAngle(new Vector2(1, 0), dif); // Calcular el angulo en grados hacia el oponente            

            signedangle = (signedangle - (tablaq.angCuadrantes / 2));    // Calcular el cuadrante del oponente en base al angulo en Cº
            if (signedangle < 0)
            {
                signedangle += 360;
            }
            int cuadrante = (int)(signedangle / tablaq.angCuadrantes); // Calcula el indice del cuadrante perteneciente
            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1
            float dist = agent.Distance(other, CellInfo.DistanceType.Manhattan);
            int cercano = (int)Math.Min(Math.Floor(dist / (40 / tablaq.numFranjasDist)), (tablaq.numFranjasDist - 1));

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
            Debug.Log(upw+" "+ rightw + " " + downw + " " + leftw + " " + cercano + " " + cuadrante);
            return playerState;
        }
    }

}
