using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Clase que modela un tipo de camión
    /// </summary>
    public class TipoCamion
    {
        #region Atributos
        /// <summary>
        /// El id (en este caso, nombre) del tipo
        /// </summary>
        private int _id;
        /// <summary>
        /// El nombre de este tipo
        /// </summary>
        private string _nombre;
        /// <summary>
        /// La capacidad de la tolva, en toneladas
        /// </summary>
        private double _capacidad_ton;
        /// <summary>
        /// La velocidad del camión cuando va bajando cargado, en kph
        /// </summary>
        private double _velocidadCargado_Bajando_kph;
        /// <summary>
        /// La velocidad del camión cuando va en plano cargado, en kph
        /// </summary>
        private double _velocidadCargado_Plano_kph;
        /// <summary>
        /// La velocidad del camión cuando va subiendo cargado, en kph
        /// </summary>
        private double _velocidadCargado_Subiendo_kph;
        /// <summary>
        /// La velocidad del camión cuando va bajando vacío, en kph
        /// </summary>
        private double _velocidadVacio_Bajando_kph;
        /// <summary>
        /// La velocidad del camión cuando va en plano vacío, en kph
        /// </summary>
        private double _velocidadVacio_Plano_kph;
        /// <summary>
        /// La velocidad del camión cuando va subiendo vacío, en kph
        /// </summary>
        private double _velocidadVacio_Subiendo_kph;
        /// <summary>
        /// [IdPala , Tiempo]: Los tiempos de aculatamiento en cada pala
        /// </summary>
        private Dictionary<int, double> _tiempoAculatamientoPalas_min;
        /// <summary>
        /// [IdDestino , Tiempo]: Los tiempos de aculatamiento en cada destino
        /// </summary>
        private Dictionary<int, double> _tiempoAculatamientoDestinos_min;
        /// <summary>
        /// [IdPala , Tiempo]: Los tiempos de carga en cada pala
        /// </summary>
        private Dictionary<int, double> _tiempoCargaPalas_min;
        /// <summary>
        /// [IdDestino , Tiempo]: Los tiempos de descarga en cada destino
        /// </summary>
        private Dictionary<int, double> _tiempoDescargaDestinos_min;
        /// <summary>
        /// Rendimiento promedio del camión en $/km según status
        /// </summary>
        private Dictionary<StatusCamion,double> _rendimiento_pesosPorKm;
        

        #endregion

        #region Properties
        /// <summary>
        /// El nombre de este tipo
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
        }
        /// <summary>
        /// El id (en este caso, nombre) del tipo
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        /// <summary>
        /// [IdPala , Tiempo]: Los tiempos de aculatamiento en cada pala
        /// </summary>
        public Dictionary<int, double> TiempoAculatamientoPalas_min
        {
            get { return _tiempoAculatamientoPalas_min; }
        }
        /// <summary>
        /// [IdDestino , Tiempo]: Los tiempos de aculatamiento en cada destino
        /// </summary>
        public Dictionary<int, double> TiempoAculatamientoDestinos_min
        {
            get { return _tiempoAculatamientoDestinos_min; }
        }
        /// <summary>
        /// [IdPala , Tiempo]: Los tiempos de carga en cada pala
        /// </summary>
        public Dictionary<int, double> TiempoCargaPalas_min
        {
            get { return _tiempoCargaPalas_min; }
        }
        /// <summary>
        /// [IdDestino , Tiempo]: Los tiempos de descarga en cada destino
        /// </summary>
        public Dictionary<int, double> TiempoDescargaDestinos_min
        {
            get { return _tiempoDescargaDestinos_min; }
        }
        /// <summary>
        /// La capacidad de la tolva, en toneladas
        /// </summary>
        public double Capacidad_Ton
        {
            get { return _capacidad_ton; }
        }
        /// <summary>
        /// Rendimiento promedio del camión en $/km según status
        /// </summary>
        public Dictionary<StatusCamion, double> Rendimiento_pesosPorKm
        {
            get { return _rendimiento_pesosPorKm; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="id"></param>
        /// <param name="_nombre"></param>
        /// <param name="_capacidad_ton"></param>
        /// <param name="rendimientoCargado_pesosPorKm"></param>
        /// <param name="rendimientoVacio_pesosPorKm"></param>
        /// <param name="_velocidadCargado_Bajando_kph"></param>
        /// <param name="_velocidadCargado_Plano_kph"></param>
        /// <param name="_velocidadCargado_Subiendo_kph"></param>
        /// <param name="_velocidadVacio_Bajando_kph"></param>
        /// <param name="_velocidadVacio_Plano_kph"></param>
        /// <param name="_velocidadVacio_Subiendo_kph"></param>
        public TipoCamion(int id, string _nombre, double _capacidad_ton, double rendimientoCargado_pesosPorKm, double rendimientoVacio_pesosPorKm,
            double _velocidadCargado_Bajando_kph, double _velocidadCargado_Plano_kph, double _velocidadCargado_Subiendo_kph,
            double _velocidadVacio_Bajando_kph, double _velocidadVacio_Plano_kph, double _velocidadVacio_Subiendo_kph)
        {
            this._id = id;
            this._nombre = _nombre;
            this._capacidad_ton = _capacidad_ton;
            this._velocidadCargado_Bajando_kph = _velocidadCargado_Bajando_kph;
            this._velocidadCargado_Plano_kph = _velocidadCargado_Plano_kph;
            this._velocidadCargado_Subiendo_kph = _velocidadCargado_Subiendo_kph;
            this._velocidadVacio_Bajando_kph = _velocidadVacio_Bajando_kph;
            this._velocidadVacio_Plano_kph = _velocidadVacio_Plano_kph;
            this._velocidadVacio_Subiendo_kph = _velocidadVacio_Subiendo_kph;

            _rendimiento_pesosPorKm = new Dictionary<StatusCamion, double>();
            _rendimiento_pesosPorKm.Add(StatusCamion.Cargado, rendimientoCargado_pesosPorKm);
            _rendimiento_pesosPorKm.Add(StatusCamion.Vacio, rendimientoVacio_pesosPorKm);
        }
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Tiempos de aculatamiento, carga y descarga para este tipo de camión
        /// </summary>
        /// <param name="_tiempoAculatamientoFrentes_min">Los tiempos de aculatamiento en cada pala</param>
        /// <param name="_tiempoAculatamientoDestinos_min">Los tiempos de aculatamiento en cada destino</param>
        /// <param name="_tiempoCargaFrentes_min">Los tiempos de carga en cada pala</param>
        /// <param name="_tiempoDescargaDestinos_min">Los tiempos de descarga en cada destino</param>
        public void SetTiempos(Dictionary<int, double> _tiempoAculatamientoPalas_min, Dictionary<int, double> _tiempoAculatamientoDestinos_min,
            Dictionary<int, double> _tiempoCargaPalas_min, Dictionary<int, double> _tiempoDescargaDestinos_min)
        {
            this._tiempoAculatamientoPalas_min = _tiempoAculatamientoPalas_min;
            this._tiempoAculatamientoDestinos_min = _tiempoAculatamientoDestinos_min;
            this._tiempoCargaPalas_min = _tiempoCargaPalas_min;
            this._tiempoDescargaDestinos_min = _tiempoDescargaDestinos_min;
        }
        /// <summary>
        /// Devuelve la velocidad según el status y la pendiente
        /// </summary>
        /// <param name="S"></param>
        /// <param name="pendiente"></param>
        /// <returns></returns>
        public double GetVelocidadSegunStatusYPendiente_Kph(StatusCamion S, double pendiente)
        {
            if (S == StatusCamion.Cargado)
            {
                if (pendiente == -1)
                {
                    return _velocidadCargado_Bajando_kph;
                }
                else if (pendiente == 0)
                {
                    return _velocidadCargado_Plano_kph;
                }
                else //pendiente +1
                {
                    return _velocidadCargado_Subiendo_kph;
                }
            }
            else //Vacío
            {
                if (pendiente == -1)
                {
                    return _velocidadVacio_Bajando_kph;
                }
                else if (pendiente == 0)
                {
                    return _velocidadVacio_Plano_kph;
                }
                else //pendiente +1
                {
                    return _velocidadVacio_Subiendo_kph;
                }
            }
        }
        
        #endregion
    }
}
