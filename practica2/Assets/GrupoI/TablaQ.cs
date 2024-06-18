using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TablaQ
{
    public int hayMuro = 2;  // 2 posibles casos
    public int numFranjasDist = 4;  // 4 posibles franjas de distancia
    public int numCuadrantes = 4;   // 4 u 8 posibles cuadrantes enemigo respecto al agente
    

    // Lista de estados
    public List<State> listStates;
    // Lista de valorea para cada indice de estado
    public List<float[]> listValues;

    // [0|1, 0|1, 0|1, 0|1, 0|1|2|3, 0|1|2|3 ] → array de la lista de State
    // [up, right, down, left] → Uno por cada indice de la lista de 'State'
    // Indican los valores de la tablaq que el personaje puede tener en cuenta al moverse
    
    // Lista de objetos State, cada uno indica:
    // Si el personaje tiene muros a su alrededor up, left, right, down
    // si el enemigo esta a distancia baja, media o alta (cercania)
    // y el cuadrante en el que se situa el enemigo en base
    // a la posicion del personaje (cuadrante)
    // por ej si el personaje esta en <5,5> y el enemigo esta en <10,10>
    // se consideraría que el enemigo esta en el cuadrante 0 
    //  1 | 0
    // -------  (por si olvidaste dibujo tecnico ;) )
    //  2 | 3
    // ejemplo: [left, up, right, down, distance, direction ], [ ... ]

    public TablaQ() {   // Constructor
        listStates = new List<State>();
        listValues = new List<float[]>();
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
                                // Añadir datos inicializados a 0 a la tabla (si se quiere mejorar los datos ya disponibles en la tabla, habría que cambiarlo)
                                listValues.Add(new float[] { 0f, 0f, 0f, 0f });   
                                
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
    public int buscaMejorDireccion(int index) {
        float bestValue = listValues[index].Max();
        int bestIndex = Array.IndexOf(listValues[index], bestValue);

        // 0 = up, 1 = right, 2 = down, 3 = left
        return bestIndex;
    }

}
