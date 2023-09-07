using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class Ruta_Dispatch : Ruta
    {
        #region Atributos
        /// <summary>
        /// Indicador de Dispatch acerca del instante en que necesitará un camión
        /// </summary>
        private double _needTime;
        /// <summary>
        /// El flujo total de toneladas
        /// </summary>
        private double _flujoTotal_tph;        
        /// <summary>
        /// Como neediest path
        /// </summary>
        private int _numeroDeVecesSeleccionada;
        #endregion

        #region Propiedades
        /// <summary>
        /// Como neediest path
        /// </summary>
        public int NumeroDeVecesSeleccionada
        {
            get { return _numeroDeVecesSeleccionada; }
            set { _numeroDeVecesSeleccionada = value; }
        }
        /// <summary>
        /// El flujo total de toneladas
        /// </summary>
        public double FlujoTotal_tph
        {
            get { return _flujoTotal_tph; }
        }
        /// <summary>
        /// Is a measure indicating how the current production on a path is behind schedule comparatively to the flow rate indicated by the solution results obtained in the upper stage
        /// </summary>
        public double NeedTime
        {
            get { return _needTime; }
            set { _needTime = value; }
        }
        #endregion

        #region Métodos

        public Ruta_Dispatch(int id, bool _esRutaHaciaDestino, int _idPala, int _idDestino, List<Arco> _arcos)
            : base(id)
        {
            this._esRutaHaciaDestino = _esRutaHaciaDestino;
            this._idPala = _idPala;
            this._idDestino = _idDestino;
            this._arcos = _arcos;                    
        }
        /// <summary>
        /// Agrega un flujo de toneladas a una ruta
        /// </summary>
        /// <param name="Flujo_tph">El flujo de toneladas</param>
        public void AgregarFlujoTPH(double Flujo_tph)
        {
            _flujoTotal_tph += Flujo_tph;
        }
        
        #endregion
    }
}
