using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{   
    /// <summary>
    /// Modela una ruta
    /// </summary>
    public class Ruta
    {
        #region Atributos
        /// <summary>
        /// Id de la ruta para el modelo. >= 1
        /// </summary>
        protected int _id;
        /// <summary>
        /// Lista de arcos que utiliza la ruta
        /// </summary>
        protected List<Arco> _arcos;
        /// <summary>
        /// Indica el largo de la ruta en KM
        /// </summary>
        protected double _largoKm;
        /// <summary>
        /// True si la ruta tiene camiones cargados hacia algún destino, false si van vacíos a una idPala
        /// </summary>
        protected bool _esRutaHaciaDestino;
        /// <summary>
        /// El id de la idPala
        /// </summary>
        protected int _idPala;
        /// <summary>
        /// El id del destino
        /// </summary>
        protected int _idDestino;
        /// <summary>
        /// Id del nodo de la pala
        /// </summary>
        protected int _idNodoPala;
        /// <summary>
        /// Id del nodo del destino
        /// </summary>
        protected int _idNodoDestino;
        /// <summary>
        /// [Id Tipo Camión , TIEMPO VIAJE]
        /// </summary>
        private Dictionary<int, double> _tiempoDeViajePorTipoCamion_min;
        #endregion

        #region Atributos dinámicos
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Lista de instantes considerados en la media móvil
        /// </summary>
        private Dictionary<int, List<double>> _instantesConsideradosMediaMovil;
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Demoras que se consideran en la media móvil
        /// </summary>
        private Dictionary<int, List<double>> _demorasConsideradasEnMediaMovil;
        /// <summary>
        /// Las toneladas enviadas por la ruta hasta el último instante de asignación
        /// </summary>
        private double _toneladasEnviadas;
        #endregion

        #region Propiedades
        /// <summary>
        /// Las toneladas enviadas por la ruta hasta el último instante de asignación
        /// </summary>
        public double ToneladasEnviadas
        {
            get { return _toneladasEnviadas; }
        }
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Lista de instantes considerados en la media móvil
        /// </summary>
        public Dictionary<int, List<double>> InstantesConsideradosMediaMovil
        {
            get { return _instantesConsideradosMediaMovil; }
        }
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Demoras que se consideran en la media móvil
        /// </summary>
        public Dictionary<int, List<double>> DemorasConsideradasEnMediaMovil
        {
            get { return _demorasConsideradasEnMediaMovil; }
        }
        /// <summary>
        /// El id de la idPala
        /// </summary>
        public int IdPala
        {
            get { return _idPala; }
        }
        /// <summary>
        /// El id del destino
        /// </summary>
        public int IdDestino
        {
            get { return _idDestino; }
        }
        /// <summary>
        /// True si la ruta tiene camiones cargados hacia algún destino, false si van vacíos hacia una pala
        /// </summary>
        public bool EsRutaHaciaDestino
        {
            get { return _esRutaHaciaDestino; }
            set { _esRutaHaciaDestino = value; }
        }
        /// <summary>
        /// Indica el largo de la ruta en KM
        /// </summary>
        public double Largo_Km
        {
            get { return _largoKm; }
        }
        /// <summary>
        /// Id de la ruta para el modelo. >= 1
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        /// <summary>
        /// Lista de arcos que utiliza la ruta
        /// </summary>
        public List<Arco> Arcos
        {
            get { return _arcos; }
        }
        #endregion

        public Ruta() { }
        public Ruta(int id)
        {
            this._id = id;
            this._arcos = new List<Arco>();
        }
        /// <summary>
        /// Agrega un arco a la ruta
        /// </summary>
        /// <param name="A_Actual"></param>
        public void AgregarArco(Arco A)
        {
            _arcos.Add(A);
        }
        /// <summary>
        /// Calcula el largo de la ruta
        /// </summary>
        public void CalcularLargo()
        {
            _largoKm = 0;
            foreach (Arco A in _arcos)
            {
                _largoKm += A.Largo_KM;
            }
        }
        public void Reset()
        {
            _instantesConsideradosMediaMovil = new Dictionary<int, List<double>>();
            _demorasConsideradasEnMediaMovil = new Dictionary<int, List<double>>();
            _toneladasEnviadas = 0;
        }
        /// <summary>
        /// Retorna la demora promedio de esta ruta en los últimos X minutos (parámetro de configuración)
        /// </summary>
        /// <returns></returns>
        public double GetDemoraPromedio_Minutos(double instanteActual)
        {
            double demora = 0;
            
            int n = 0;
            foreach (int k in _instantesConsideradosMediaMovil.Keys)
            {
                for (int i = 0; i < _instantesConsideradosMediaMovil[k].Count; i++)
                {
                    double tiempo = _instantesConsideradosMediaMovil[k][i];
                    if (tiempo >= instanteActual - Configuracion.MinutosConsideradosMediaMovil)
                    {
                        n++;
                        double demoraDeEsteInstante = _demorasConsideradasEnMediaMovil[k][i];
                        demora += demoraDeEsteInstante;
                    }
                }
            }
            demora = demora / Math.Max(1, n);

            return demora;
        }
        /// <summary>
        /// Agrega un dato de demora
        /// </summary>
        /// <param name="instanteMinutos">El instante</param>
        /// <param name="tipoCamion">El tipo de camión</param>
        /// <param name="demora_minutos">La demora en minutos</param>
        public void AgregarDemora(double instanteMinutos, int tipoCamion, double demora_minutos)
        {
            if (!_instantesConsideradosMediaMovil.ContainsKey(tipoCamion))
            {
                _instantesConsideradosMediaMovil.Add(tipoCamion, new List<double>());
                _demorasConsideradasEnMediaMovil.Add(tipoCamion, new List<double>());
            }

            _instantesConsideradosMediaMovil[tipoCamion].Add(instanteMinutos);
            _demorasConsideradasEnMediaMovil[tipoCamion].Add(demora_minutos);
        }
        /// <summary>
        /// Setea otros parámetros que habían faltado
        /// </summary>
        public void SetOtrosParametros(bool _esRutaHaciaDestino, int _idPala, int _idDestino, int idNodoPala, int idNodoDestino)
        {
            this._esRutaHaciaDestino = _esRutaHaciaDestino;
            this._idPala = _idPala;
            this._idDestino = _idDestino;
            _idNodoPala = idNodoPala;
            _idNodoDestino = idNodoDestino;
        }
        /// <summary>
        /// Retorna la lista de arcos ordenados según instante de uso
        /// </summary>
        /// <returns></returns>
        public void OrdenarArcos()
        {
            List<Arco> ArcosPorSeleccionar = new List<Arco>(_arcos);
            List<Arco> ArcosOrdenados = new List<Arco>();
            int idInicio = _esRutaHaciaDestino ? _idNodoPala : _idNodoDestino;

            while (ArcosPorSeleccionar.Count > 0)
            {
                bool foundit = false;
                for (int i = 0; i < ArcosPorSeleccionar.Count; i++)
                {
                    Arco A = ArcosPorSeleccionar[i];
                    if (A.Id_Nodo_Inicial == idInicio) //Si encontré el arco:
                    {
                        ArcosOrdenados.Add(A);
                        ArcosPorSeleccionar.Remove(A);
                        idInicio = A.Id_Nodo_Final;
                        foundit = true;
                        break;
                    }
                }
                if (!foundit)
                {
                    throw new Exception("No se encontró el arco!");
                }
            }

            _arcos = ArcosOrdenados;
        }
        /// <summary>
        /// Calcula el tiempo de viaje por tipo de camión para esta ruta
        /// </summary>
        /// <param name="tiempoViajeCamionArco_min">[TIPO CAMIÓN, STATUS, NODO, NODO]</param>
        public void SetTiemposDeViajePorTipoCamion(double[, , ,] tiempoViajeTipoCamionArco_min)
        {
            _tiempoDeViajePorTipoCamion_min = new Dictionary<int, double>();
            for (int k = 0; k < tiempoViajeTipoCamionArco_min.GetLength(0); k++)
            {
                double t = 0;
                foreach (Arco A in _arcos)
                {
                    int u = A.Id_Nodo_Inicial;
                    int v = A.Id_Nodo_Final;
                    double tiempo = 0;
                    if (_esRutaHaciaDestino)
                    {
                        tiempo = tiempoViajeTipoCamionArco_min[k, 1, u - 1, v - 1];                        
                    }
                    else //Es hacia la Pala (vacío)
                    {
                        tiempo = tiempoViajeTipoCamionArco_min[k, 0, u - 1, v - 1];               
                    }
                    t += tiempo;
                }
                _tiempoDeViajePorTipoCamion_min.Add(k + 1, t);
            }

        }
        /// <summary>
        /// Devuelve el tiempo de viaje del camión en esta ruta. NO INCLUYE LA ATENCIÓN
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(int idTipoCamion)
        {
            if (_tiempoDeViajePorTipoCamion_min.ContainsKey(idTipoCamion))
            {
                return _tiempoDeViajePorTipoCamion_min[idTipoCamion];
            }
            else
            {
                throw new Exception("No se encontró el tipo del camión");
            }
        }
        /// <summary>
        /// Regitro las toneladas entregadas recién
        /// </summary>
        /// <param name="ton"></param>
        public void AgregarToneladasEnviadas(double ton)
        {
            _toneladasEnviadas += ton;
        }
    }
}
