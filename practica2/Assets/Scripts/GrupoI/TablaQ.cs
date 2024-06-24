using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// ------------- CLASE PARA CREACIÓN Y MODIFICACIÓN DE LA TABLA Q --------------------
public class TablaQ
{
    //---------- INFORMACIÓN ------------------
    // [0|1, 0|1, 0|1, 0|1, 0|1|2|3, 0|1|2|3 ] → array de la lista de State
    // [up, right, down, left] → Uno por cada indice de la lista de 'State'
    // Indican los valores de la tablaq que el personaje puede tener en cuenta al moverse

    // Lista de objetos State, cada uno indica:
    // Si el personaje tiene muros a su alrededor up, right, down, left
    // si el enemigo esta a distancia baja, media o alta (cercania)
    // y el cuadrante en el que se situa el enemigo en base
    // a la posicion del personaje (8 cuadrantes)
    // ejemplo: [left, up, right, down, distance, direction ], [ ... ]
    //------------------------------------------

    #region Variables

    public int hayMuro = 2;  // 2 posibles casos (si hay o si no)
    public int numFranjasDist = 4;  // 4 posibles franjas de distancia    
    public int franja1=1, franja2=2, franja3 = 10, franja4=40;    // el maximo de distancia es 40, la primera franja es los personajes juntos
    public int numCuadrantes = 8;   // 8 cuadrantes enemigo respecto al agente
    public int angCuadrantes {get => 360/numCuadrantes;}    // Calcula el ángulo de cada cuadrante
    public string ruta = Application.dataPath + "/Scripts/GrupoI/" + "Qtable.csv";    //Ruta archivo CSV
    public List<State> listStates;  // Lista de estados   
    public List<float[]> listValues;    // tabla Q con los estados y sus acciones
    
    // ------------------- RESETEO DE LA TABLA -------------------
    bool resetTabla = false;    
    // -----------------------------------------------------------
    #endregion

    #region Constructor
    public TablaQ() {   
        listStates = new List<State>();
        listValues = new List<float[]>();

        // -------------- INICIALIZACIÓN ESTADOS ----------------------------
        int i = 0;
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
                                listValues.Add(new float[] { 0f, 0f, 0f, 0f });   // Añadir datos inicializados a 0 a la tabla q
                                i++;
                            }
                        }
                    }
                }
            }
        }
        // -------------------------------------------------------------------

        // Vuelve a poner los valores de la tabla a 0 (si se quiere reiniciar entrenamiento)
        if (resetTabla)
        {
            guardarCSV(ruta);
        }
        // Carga los valores del archivo CSV en la lista listValues
        cargarCSV();
    }

    #endregion

    #region Métodos de busqueda

    // --------------  MÉTODO BÚSQUEDA DEL ESTADO EN LA LISTA DE ESTADOS --------------------------
    // Devuelve el indice del estado de la lista de estados listStates
    public int buscaIndiceEstado(State state) {
        int index = listStates.IndexOf(state);
        return index;
    }

    // -------------- MÉTODO BÚSQUEDA DE LA ACCIÓN CON MAYOR VALOR Q DEL ESTADO --------------------------
    // Devuelve el indice de la mejor acción del estado de la lista listValues
    public int buscaMejorDireccion(int index) {
        float bestValue = listValues[index].Max();
        int bestIndex = Array.IndexOf(listValues[index], bestValue);

        // 0 = up, 1 = right, 2 = down, 3 = left
        return bestIndex;
    }
    #endregion

    #region Métodos de tratado de archivos

    // ---------------- MÉTODO GUARDADO DE VALORES DE LA TABLA Q DE ARCHIVO CSV A LISTA LISTVALUES -------------------------
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


    // ---------------- MÉTODO GUARDADO DE VALORES DE LA LISTA LISTVALUES AL ARCHIVO CSV -------------------------
    public void guardarCSV(string ruta)
    {
        // Crear un StreamWriter para escribir en el archivo CSV
        using (StreamWriter writer = new StreamWriter(ruta, false))
        {
            // Escribir cada fila de datos en el archivo CSV
            foreach (float[] fila in listValues)
            {
                writer.WriteLine(string.Join(";", fila));
            }
        }
    }

    #endregion
}