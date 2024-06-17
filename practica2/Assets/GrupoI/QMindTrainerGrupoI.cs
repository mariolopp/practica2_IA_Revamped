﻿using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using UnityEngine;

namespace QMind
{
    public class QMindTrainerGrupoI : IQMindTrainer
    {
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;
        TablaQ tablaq;
        WorldInfo worldInfo;
        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            tablaq = new TablaQ();
            Debug.Log("QMindTrainerDummy: initialized");
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        public void DoStep(bool train)
        {
            Debug.Log("QMindTrainerDummy: DoStep");

            // Array de ints en el que se introducen los resultados (podría usarse tambien una clase state)
            //State playerState = new State(); // Estado del personaje
          
            // Calcular el angulo en grados hacia el oponente
            float signedangle = Mathf.Atan2(OtherPosition.y - AgentPosition.y, 
                OtherPosition.x - AgentPosition.x) * Mathf.Rad2Deg;

            // Calcular el cuadrante del oponente en base al angulo
            signedangle = (signedangle + 360) % 360;
            Debug.Log("Angulo enemigo signed: "+ signedangle);

            int cuadrante = (int)(signedangle / 90);
            Debug.Log("Cuadrante obtenido: " + cuadrante);

            // Escribir el cuadrante en el estado
            //playerState.cuadrante = cuadrante;

            //// Calcular distancia del agente a su oponente
            // distancia_Manhattan=∣x2−x1∣+∣y2−y1∣

            float dist = Math.Abs(OtherPosition.x - AgentPosition.x) + Math.Abs(OtherPosition.y - AgentPosition.y);

            int cercano = (int)Math.Floor(dist / 10);

            Debug.Log("Cercania es: " + cercano);

            // Escribir todos los datos en el estado actual
            State playerState = new State(); // Estado del personaje

            // Devuelve si arriba hay muro
            //bool up = worldInfo.NextCell(AgentPosition, Directions.Up);







            int indice = tablaq.buscaIndiceEstado(new State(1,0,0,0,0,3));
            Debug.Log("Indice del estado x: " + indice);
        }
    }
}
