using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TablaQ
{
    public int hayMuro = 2;  // 2 posibles casos
    public int numFranjasDist = 4;  // 4 posibles franjas de distancia
    public int numCuadrantes = 4;   // 4 u 8 posibles cuadrantes enemigo respecto al agente
    //public string ruta = Application.dataPath + "/GrupoI/";
    public string ruta = "Qtable.csv";

    // Constructor

    // Metodos de gestion

    // Otros metodos que querais

    // Escritura lectura


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
    // ---+---  (por si olvidaste dibujo tecnico ;) )
    //  2 | 3
    // ejemplo: [left, up, right, down, distance, direction ], [ ... ]
    
    // Constructor
    public TablaQ() {   
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

        guardarCSV();
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
    /// Lee csv ya existente
    //public List<List<float>> cargarCSV()
    //{
    //    List<List<float>> result = new List<List<float>>();
    //    //Carga la tabla desde un archivo .csv.
    //    try
    //    {
    //        using (StreamReader reader = new StreamReader(ruta))
    //        {
    //            while (!reader.EndOfStream)
    //            {
    //                // Read current line from the file
    //                string line = reader.ReadLine();

    //                // Split the line using semicolon as the separator
    //                string[] fields = line.Split(';');

    //                // Convert string array to List<float>
    //                List<float> row = new List<float>();

    //                foreach (string field in fields)
    //                {
    //                    if (float.TryParse(field, out float value))
    //                    {
    //                        row.Add(value);
    //                    }
    //                    else
    //                    {
    //                        // Handle the case where conversion to float fails
    //                        Console.WriteLine($"Warning: Unable to parse '{field}' as float.");
    //                    }
    //                }

    //                // Add the row to the result
    //                result.Add(row);
    //            }
    //        }
    //    }
    //}
    /// Guardar archivo
    public void guardarCSV()
    {
        // Crear un StreamWriter para escribir en el archivo CSV
        using (StreamWriter writer = new StreamWriter(ruta, false))
        {
            // Escribir cada fila de datos en el archivo CSV
            foreach (float[] fila in listValues)
            {
                writer.WriteLine(string.Join(";", fila));
                //foreach (float value in fila) { 
                //    // Convertir cada valor float a string y escribirlos en el archivo, separados por comas
                //    writer.Write(value + ";"); // value + ";"
                //}
                //writer.WriteLine("");
            }
        }
    }
}