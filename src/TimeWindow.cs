using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Clase que modela una ventana de tiempo en un cierto arco
    /// </summary>
    public class TimeWindow
    {
        #region Atributos
        /// <summary>
        /// El instante_minutos inicial en minutos de la ventana de tiempo
        /// </summary>
        private double _instanteInicial_min;
        /// <summary>
        /// El instante_minutos final en minutos de la ventana de tiempo
        /// </summary>
        private double _instanteFinal_min;

        public int IdCamion;
        //Número de instancia del problema en que se asignó esta ventana de tiempo
        public int NumProblema;

        #endregion
        /// <summary>
        /// El constructor de una ventana de tiempo
        /// </summary>
        /// <param name="_instanteInicial_min">El instante_minutos inicial en minutos de la ventana de tiempo_min</param>
        /// <param name="_instanteFinal_min">El instante_minutos final en minutos de la ventana de tiempo_min</param>
        public TimeWindow(double _instanteInicial_min, double _instanteFinal_min, int idCam, int numP)
        {
            this._instanteInicial_min = _instanteInicial_min;
            this._instanteFinal_min = _instanteFinal_min;
            IdCamion = idCam;
            NumProblema = numP;
        }

        #region Properties
        /// <summary>
        /// Devuelve el largo del intervalo de esta TW en minutos
        /// </summary>
        public double LargoIntervalo_Minutos
        {
            get
            {
                double largo = this._instanteFinal_min - this._instanteInicial_min;
                return largo;
            }
        }
        /// <summary>
        /// El instante_minutos inicial en minutos de la ventana de tiempo_min
        /// </summary>
        public double Inicio_min
        {
            get { return _instanteInicial_min; }
            set { _instanteInicial_min = value; }
        }
        /// <summary>
        /// El instante_minutos final en minutos de la ventana de tiempo_min
        /// </summary>
        public double Termino_min
        {
            get { return _instanteFinal_min; }
            set { _instanteFinal_min = value; }
        }
        #endregion

    }
}
