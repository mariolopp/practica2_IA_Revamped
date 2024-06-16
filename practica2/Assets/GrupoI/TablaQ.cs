using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TablaQ
{
    // Lista de arrays de ints
    public List<int[]> statesArray;
    // [0|1, 0|1, 0|1, 0|1, 0|1|2, 1|2|3|4 ]
    // Lista de arrays de ints que indican:
    // Si el personaje tiene muros a su alrededor (primeras 4 posiciones),
    // si el enemigo esta a distancia baja, media o alta (posicion 5)
    // y el cuadrante en el que se situa el enemigo en base
    // a la posicion del personaje (posicion 6)
    // por ej si el personaje esta en <5,5> y el enemigo esta en <10,10>
    // se consideraría que el enemigo esta en el primer cuadrante
    // ejemplo: [left, up, right, down, distance, direction ], [  ]

    public void Main() {
        statesArray = new List<int[]>();

        for (int i0 = 0; i0 < 2; i0++)
        {
            for (int i1 = 0; i1 < 2; i1++)
            {
                for (int i2 = 0; i2 < 2; i2++)
                {
                    for (int i3 = 0; i3 < 2; i3++)
                    {
                        for (int i4 = 0; i4 < 3; i4++)
                        {
                            for (int i5 = 0; i5 < 4; i5++)
                            {
                                int[] array1 = new int[] { i0, i1,i2,i3,i4,i5 };
                                statesArray.Add(array1);
                                Debug.Log(array1);
                            }
                        }
                    }
                }
            }
        }
        
    }
    
}
