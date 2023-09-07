using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Clase estática que tiene ciertas propiedades fijas del programa
    /// </summary>
    public static class Configuracion
    {
        #region Atributos

        public static Boolean EscribirRespuestasDeInstancias;
        /// <summary>
        /// Indica los métodos que se deben usan para resolver el problema
        /// </summary>
        public static List<MetodoSolucion> MetodosDeSolucion;        
        /// <summary>
        /// La ruta del modelo
        /// </summary>
        private static Dictionary<MetodoSolucion, string> RutaMod;        
        /// <summary>
        /// Entrega el nombre de la formulación para ponerlo en la consola
        /// </summary>
        public static Dictionary<MetodoSolucion, string> NombreModelo;
        /// <summary>
        /// El gap relativo con se resuelve el modelo de optimización. Ej: 0.01 -> 1%
        /// </summary>
        public static double CPLEX_GapRelativo;
        /// <summary>
        /// Indica el número de threads utilizados para correr el modelo
        /// </summary>
        public static int CPLEX_NumeroThreads;
        /// <summary>
        /// Indica los minutos a considerar en el pasado para evaluar la media móvil
        /// </summary>
        public static double MinutosConsideradosMediaMovil;
        /// <summary>
        /// Intervalo de tiempo en que se analiza la ley
        /// </summary>
        public static double IntervaloTiempoAnalisisLey;
        public static bool ImprimirCosasInnecesarias;
        /// <summary>
        /// El tiempo total del turno [min]
        /// </summary>
        public static double TiempoTotalHorizonte;
        /// <summary>
        /// Multiplicador de la pendiente en el modelo. Mientras más alto, más simultáneo se cumple el PD
        /// </summary>
        public static double multiplicadorPendiente;
        public static double MaxTiempoVentana;
        /// <summary>
        /// Diferencia porcentual máxima permitida en el cumplimiento del plan día por REquerimiento antes de exigir que se despache al REQ más atrasado. 5 --> 5% (se pasa un numero entre 0 y 100)
        /// </summary>
        public static double MaxDeltaPorcentual_DISPATCH;
        public static bool SetearValoresSolInicial;
        public static bool HacerPolishing;
        public static double LeyDePlanta;

        #endregion

        /// <summary>
        /// Retorna la ruta del modelo
        /// </summary>
        /// <param name="MS"></param>
        /// <param name="numeroModelo"></param>
        /// <returns></returns>
        public static string GetRutaModelo(MetodoSolucion MS, int numeroModelo)
        {
            if (MS == MetodoSolucion.DISPATCH && numeroModelo == 1)
            {
                return "Modelo_Dispatch_LP_v5.mod";
            }
            else
            {
                return RutaMod[MS];
            }
        }

        public static void SetParameters(List<MetodoSolucion> _MetodosDeSolucion)
        {
            SetearValoresSolInicial = false;
            HacerPolishing = false;
            MaxTiempoVentana = 5000;
            MetodosDeSolucion = _MetodosDeSolucion;
            CPLEX_GapRelativo = 0.000001/100;
            CPLEX_NumeroThreads = 8;
            MinutosConsideradosMediaMovil = 30;
            EscribirRespuestasDeInstancias = false;
            ImprimirCosasInnecesarias = true;
            TiempoTotalHorizonte = 12 * 60;
            IntervaloTiempoAnalisisLey = 30;
            LeyDePlanta = 0.7;

            RutaMod = new Dictionary<MetodoSolucion, string>();
            RutaMod.Add(MetodoSolucion.MIP, "Modelo_Escoge_Rutas_v16.mod");
            RutaMod.Add(MetodoSolucion.DISPATCH, RutaMod[MetodoSolucion.MIP]);
            RutaMod.Add(MetodoSolucion.DRMA, RutaMod[MetodoSolucion.MIP]);

            NombreModelo = new Dictionary<MetodoSolucion, string>();
            NombreModelo.Add(MetodoSolucion.MIP, "MODELO ESCOGE RUTAS");
            NombreModelo.Add(MetodoSolucion.DISPATCH, "MODELO DISPATCH");
            NombreModelo.Add(MetodoSolucion.DRMA, "MODELO DESCARGA AL REQUERIMIENTO MÁS ATRASADO");
        }
    }
}
