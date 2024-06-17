using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TablaQ
{
    public int hayMuro = 2;  // 2 posibles casos
    public int numFranjasDist = 4;  // 4 posibles franjas de distancia
    public int numCuadrantes = 4;   // 4 u 8 posibles cuadrantes enemigo respecto al agente
    
    // Posiciones de cada direccion en el array
    #region
    private int posNorth = 0;
    private int posSouth = 1;
    private int posWest = 2;
    private int posEast = 3;
    private int posDist = 4;
    private int posCuadrante = 5;
    #endregion

    // Lista de estados
    public List<State> listStates;
    //public Dictionary<int, int> stateIndexMap;

    // [0|1, 0|1, 0|1, 0|1, 0|1|2|3, 0|1|2|3 ]
    // Lista de objetos State que indican:
    // Si el personaje tiene muros a su alrededor (primeras 4 posiciones) up, left, right, down
    // si el enemigo esta a distancia baja, media o alta (posicion 5)
    // y el cuadrante en el que se situa el enemigo en base
    // a la posicion del personaje (posicion 6)
    // por ej si el personaje esta en <5,5> y el enemigo esta en <10,10>
    // se consideraría que el enemigo esta en el primer cuadrante
    // ejemplo: [left, up, right, down, distance, direction ], [  ]

    public TablaQ() {   // Constructor
        listStates = new List<State>();
        //stateIndexMap = new Dictionary<int, int>();
        // Montaña de for que inicializan los estados (codigo espaguetti pero funciona)
        for (int i0 = 0; i0 < hayMuro; i0++)
        {
            for (int i1 = 0; i1 < hayMuro; i1++)
            {
                for (int i2 = 0; i2 < hayMuro; i2++)
                {
                    for (int i3 = 0; i3 < hayMuro; i3++)
                    {
                        for (int i4 = 0; i4 < numFranjasDist; i4++)
                        {
                            for (int i5 = 0; i5 < numCuadrantes; i5++)
                            {
                                State addS = new State(i0, i1, i2, i3, i4, i5);
                                listStates.Add(addS);
                                Debug.Log(addS.up +", "+ addS.right + ", " + addS.down + ", " + addS.left + ", " + addS.cercania + ", " + addS.cuadrante);
                            }
                        }
                    }
                }
            }
        }

        //int[] estadoBuscar = { 0,0,0,0,0,4 };
        //buscaIndiceEstado(estadoBuscar);

    }
    public int buscaIndiceEstado(State state) {
        int index = listStates.IndexOf(state);
        return index;
    }
    //public int EncontrarIndiceEstado(int[] estado)
    //{
    //    //string key = string.Join(",", estado);
    //    //if (stateIndexMap.TryGetValue(key, out int index))
    //    //{
    //    //    return index;
    //    //}
    //    //return -1; // Si no se encuentra el estado
    //}

}
