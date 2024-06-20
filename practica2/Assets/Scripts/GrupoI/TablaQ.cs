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
    //---------- INFORMACIÓN ------------------
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
    //------------------------------------------
    
    #region Variables

    public int hayMuro = 2;  // 2 posibles casos
    public int numFranjasDist = 4;  // 4 posibles franjas de distancia
    public int numCuadrantes = 4;   // 4 u 8 posibles cuadrantes enemigo respecto al agente
    public int angCuadrantes = 90;
    public string ruta = Application.dataPath + "/Scripts/GrupoI/" + "Qtable.csv";    //Ruta archivo CSV
    public List<State> listStates;  // Lista de estados   
    public List<float[]> listValues;    // valores Q de las acciones de cada estado

    #endregion

    #region Constructor
    public TablaQ() {   
        listStates = new List<State>();
        listValues = new List<float[]>();

        //INICIALIZACIÓN ESTADOS

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
                                listValues.Add(new float[] { 0f, 0f, 0f, 0f });   // Añadir datos inicializados a 0 a la tabla

                                //Debug.Log(addS.up +", "+ addS.right + ", " + addS.down + ", " + addS.left + ", " + addS.cercania + ", " + addS.cuadrante);
                            }
                        }
                    }
                }
            }
        }



        // Si descomentas aquí te va a guardar una tabla todo a 0 (que son los valores que tiene listValues por el for)
        //guardarCSV(ruta);   

        cargarCSV();
        //guardarCSV("Qbackup.csv");
    }

    #endregion

    #region Métodos de busqueda
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

    #endregion

    #region Métodos de tratado de archivos

    // SOBREESCRIBE LISTA DE VALORES Q A PARTIR DE UN ARCHIVO CSV
    public void cargarCSV()
    {
       
        try
        {
            using (StreamReader reader = new StreamReader(ruta))
            {
                int i = 0;  // indice de la lista listValues (estado)
                while (!reader.EndOfStream)
                {
                    // Toma cada fila de la tabla q, la separa por las acciones de cada estado y lo almacena en listValues
                    string line = reader.ReadLine();

                    string[] fields = line.Split(';');

                    List<float> row = new List<float>();

                    foreach (string field in fields)
                    {
                        if (float.TryParse(field, out float value))
                        {
                            row.Add(value);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Unable to parse '{field}' as float.");
                        }
                    }

                    // guardado valores de cada fila del csv en listValues
                    for (int j = 0;  j < 4; j++)
                    {
                        listValues[i][j] = row[j];
                    }
                    i++;
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }


    /// GUARDAR LISTA DE VALORES Q EN UN ARCHIVO CSV
    public void guardarCSV(string ruta)
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

    #endregion
}