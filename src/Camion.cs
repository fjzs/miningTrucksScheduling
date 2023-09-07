using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class Camion
    {
        #region Atributos Fijos
        /// <summary>
        /// Lista de los ids de los destinos a los que se puede asignar este camión
        /// </summary>
        private List<int> _idsDestinosAsignables;
        /// <summary>
        /// Lista de los ids de las palas a los que se puede asignar este camión
        /// </summary>
        private List<int> _idsPalasAsignables;
        /// <summary>
        /// El id del camión. Parte desde 1
        /// </summary>
        private int _id;
        /// <summary>
        /// El tipo de camión
        /// </summary>
        private TipoCamion _tipoCamion;
        /// <summary>
        /// El id del nodo de dónde inicia
        /// </summary>
        private int _idNodoInicial;
        /// <summary>
        /// Instante medido en minutos en el que el camión estará disponible por primera vez
        /// </summary>
        private double _instanteDisponibleInicial_minutos;
        
        #endregion

        #region Atributos Dinámicos
        /// <summary>
        /// El id del nodo en que estará el camión cuando quede disponible
        /// </summary>
        private int _idNodoDisponible;
        /// <summary>
        /// Instante de la simulación en que este camión estará disponible (terminará de descargar)
        /// </summary>
        private double _instanteSiguienteDisponible_minutos;
               
        
        #endregion

        #region Properties
        /// <summary>
        /// Lista de los ids de los destinos a los que se puede asignar este camión
        /// </summary>
        public List<int> IdsDestinosAsignables
        {
            get { return _idsDestinosAsignables; }
        }
        /// <summary>
        /// Lista de los ids de las palas a los que se puede asignar este camión
        /// </summary>
        public List<int> IdsPalasAsignables
        {
            get { return _idsPalasAsignables; }
        }        
        /// <summary>
        /// El tipo de camión
        /// </summary>
        public TipoCamion TipoCamion
        {
            get { return _tipoCamion; }
        }
        /// <summary>
        /// El id del camión. Parte desde 1
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        /// <summary>
        /// Instante medido en minutos en el que el camión estará disponible
        /// </summary>
        public double InstanteSiguienteDisponible_Minutos
        {
            get { return _instanteSiguienteDisponible_minutos; }
            set { _instanteSiguienteDisponible_minutos = value; }
        }
        /// <summary>
        /// El id del nodo en que estará el camión cuando quede disponible
        /// </summary>
        public int IdNodoDisponible
        {
            get { return _idNodoDisponible; }
            set { _idNodoDisponible = value; }
        }
        /// <summary>
        /// La capacidad en toneladas de la tolva del camión
        /// </summary>
        public double Capacidad_Ton
        {
            get { return _tipoCamion.Capacidad_Ton; }
        }

        #endregion

        #region Métodos públicos
        /// <summary>
        /// El constructor de un camión
        /// </summary>
        /// <param name="_id">El id del camión</param>
        /// <param name="_tipoCamion">El tipo de camión</param>
        /// <param name="_idNodoInicial">El id del nodo de dónde inicia</param>
        /// <param name="_minInicial">Instante medido en minutos en el que el camión está disponible</param>
        public Camion(int _id, TipoCamion _tipoCamion, int _idNodoInicial, double _minInicial)
        {
            this._id = _id;
            this._tipoCamion = _tipoCamion;
            this._idNodoInicial = _idNodoInicial;
            this._instanteDisponibleInicial_minutos = _minInicial;
            _idNodoDisponible = _idNodoInicial;            
        }
        /// <summary>
        /// Reinicia los datos que van cambiando en la simulación
        /// </summary>
        public void Reset()
        {
            _instanteSiguienteDisponible_minutos = _instanteDisponibleInicial_minutos;
            _idNodoDisponible = _idNodoInicial;            
        }
        /// <summary>
        /// Asigna las palas y destinos a los que se puede asignar el camión
        /// </summary>
        /// <param name="palas"></param>
        /// <param name="destinos"></param>
        public void SetPalasyDestinosCompatibles(List<int> palas, List<int> destinos)
        {
            _idsPalasAsignables = palas;
            _idsDestinosAsignables = destinos;
        }
        
        #endregion
    }
}
