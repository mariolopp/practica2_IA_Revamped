using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        QMindTrainerGrupoI trainer;
        #endregion

        public void Initialize(WorldInfo worldInfo)
        {
            //Debug.Log("QMindDummy: initialized");
            this.worldInfo = worldInfo;
            tablaq = new TablaQ();   
            nextPos = new CellInfo(0,0);
            trainer = new QMindTrainerGrupoI();


        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMindDummy: GetNextStep");
            State currentState = trainer.getState(currentPosition, otherPosition, worldInfo, tablaq);
            int indice = tablaq.buscaIndiceEstado(currentState);
            int bestDirection = tablaq.buscaMejorDireccion(indice);

            Debug.Log(currentState.up + " " + currentState.right + " " + currentState.down + " " + currentState.left + " " + currentState.cercania + " " + currentState.cuadrante);
            Debug.Log(tablaq.listValues[indice][0] + " " + tablaq.listValues[indice][1] + " " + tablaq.listValues[indice][2] + " " + tablaq.listValues[indice][3]);
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
    }
}
