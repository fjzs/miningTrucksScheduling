using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Contiene la información de la solicitud de despacho de un camión
    /// </summary>
    public class Asignacion
    {
        #region Atributos
        /// <summary>
        /// El tiempo que tomó en resolver en segundos
        /// </summary>
        private double _tiempoResolucion;
        /// <summary>
        /// El id del destino inicial
        /// </summary>
        private int _iDdestinoInicial;
        /// <summary>
        /// Instante en minutos de esta asignación
        /// </summary>
        private double _instante_min;
        /// <summary>
        /// Id del camión que solicitó despacho
        /// </summary>
        private int _idCamion;
        /// <summary>
        /// Id de la pala asigada
        /// </summary>
        private int _idPalaAsignada;
        /// <summary>
        /// Id del destino asignado
        /// </summary>
        private int _idDestinoAsignado;
        /// <summary>
        /// Tolva del camión
        /// </summary>
        private double _tolvaCamion;
        /// <summary>
        /// El tiempo de carga en la pala [min]
        /// </summary>
        private double _tiempoCargaEnPala_min;
        /// <summary>
        /// Costo de transporte [$]
        /// </summary>
        private double _costoTransporte;
        /// <summary>
        /// Tiempo de espera en el destino [min]
        /// </summary>
        private double _esperaEnDestino_min;
        /// <summary>
        /// Tiempo de espera en pala [min]
        /// </summary>
        private double _esperaEnPala_min;
        /// <summary>
        /// Tiempo de demora [min]
        /// </summary>
        private double _demora_min;
        /// <summary>
        /// El tiempo del ciclo en minutos
        /// </summary>
        private double _tiempoDeCiclo_min;
        /// <summary>
        /// Número de tuplas camión-ciclo
        /// </summary>
        private int _numCamionCiclo;

        public double InstanteDescarga;
        /// <summary>
        /// 1 si el tiempo que se demoró en resolver esta instancia alcanzó a responder antes de que empiece el siguiente despacho, 0 si no
        /// </summary>
        public int AlcanzaAResponder;

        #endregion

        #region Properties
        /// <summary>
        /// Número de tuplas camión-ciclo
        /// </summary>
        public int NumCamionCiclo
        {
            get { return _numCamionCiclo; }
            set { _numCamionCiclo = value; }
        }
        /// <summary>
        /// El tiempo del ciclo en minutos
        /// </summary>
        public double TiempoDeCiclo_min
        {
            get { return _tiempoDeCiclo_min; }
            set { _tiempoDeCiclo_min = value; }
        }
        /// <summary>
        /// El id del destino inicial
        /// </summary>
        public int IdDestinoInicial
        {
            get { return _iDdestinoInicial; }
            set { _iDdestinoInicial = value; }
        }
        /// <summary>
        /// El tiempo que tomó en resolver en segundos
        /// </summary>
        public double TiempoResolucion_s
        {
            get { return _tiempoResolucion; }
            set{_tiempoResolucion = value;}
        }
        /// <summary>
        /// Id del destino asignado
        /// </summary>
        public int IdDestinoAsignado
        {
            get { return _idDestinoAsignado; }
            set { _idDestinoAsignado = value; }
        }
        /// <summary>
        /// Tolva del camión
        /// </summary>
        public double TolvaCamion
        {
            get { return _tolvaCamion; }
            set { _tolvaCamion = value; }
        }
        /// <summary>
        /// El tiempo de carga en la pala [min]
        /// </summary>
        public double TiempoCargaEnPala_min
        {
            get { return _tiempoCargaEnPala_min; }
            set { _tiempoCargaEnPala_min = value; }
        }
        /// <summary>
        /// Id de la pala asigada
        /// </summary>
        public int IdPalaAsignada
        {
            get { return _idPalaAsignada; }
            set { _idPalaAsignada = value; }
        }
        /// <summary>
        /// Instante en minutos de esta asignación
        /// </summary>
        public double Instante_min
        {
            get { return _instante_min; }
        }
        /// <summary>
        /// Id del camión que solicitó despacho
        /// </summary>
        public int IdCamion
        {
            get { return _idCamion; }
        }
        /// <summary>
        /// Costo de transporte [$]
        /// </summary>
        public double CostoTransporte
        {
            get { return _costoTransporte; }
            set { _costoTransporte = value; }
        }
        /// <summary>
        /// Tiempo de espera en el destino [min]
        /// </summary>
        public double EsperaEnDestino_min
        {
            get { return _esperaEnDestino_min; }
            set { _esperaEnDestino_min = value; }
        }
        /// <summary>
        /// Tiempo de espera en pala [min]
        /// </summary>
        public double EsperaEnPala_min
        {
            get { return _esperaEnPala_min; }
            set { _esperaEnPala_min = value; }
        }
        /// <summary>
        /// Tiempo de demora [min]
        /// </summary>
        public double Demora_min
        {
            get { return _demora_min; }
            set { _demora_min = value; }
        }
        
        
        
        #endregion
        
        #region Métodos
        public Asignacion(double instante_min, int idCamion)
        {
            this._idCamion = idCamion;
            this._instante_min = instante_min;            
        }

        #endregion
    }
}
