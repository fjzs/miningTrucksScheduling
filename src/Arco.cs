using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class Arco
    {
        #region Atributos
        /// <summary>
        /// El largo del arco en km
        /// </summary>
        private double _largo_KM;
        /// <summary>
        /// Id nodo inicial
        /// </summary>
        private int _idNodo1;
        /// <summary>
        /// Id nodo final
        /// </summary>
        private int _idNodo2;
        /// <summary>
        /// True si es un arco de espera
        /// </summary>
        private bool _esDeEspera;
        /// <summary>
        /// True si es un arco de tránsito
        /// </summary>
        private bool _esDeTransito;
        /// <summary>
        /// True si es un arco donde una pala carga o se descarga en un destino
        /// </summary>
        private bool _esDeAtencion;
        /// <summary>
        /// La pendiente del arco. 1 si es cercana al 10%, 0 si es relativamente plano, -1 si es cercano al -10%
        /// </summary>
        private double _pendiente;
        /// <summary>
        /// La máxima velocidad entre todos los camiones para recorrer este arco
        /// </summary>
        private double _maximaVelocidadParaRecorrerlo_kph;
        #endregion

        #region Atributos dinámicos
        /// <summary>
        /// Lista de usos del arco. Cada trayectoria indica dos valores: t1 y t2, t1 cuando un camión empezó a recorrer el arco y t2 cuando salió de él.
        /// </summary
        private List<TimeWindow> _trayectorias;
        /// <summary>
        /// [Id Camión, Trayectoria]: Simulada en dispatch
        /// </summary>
        public Dictionary<int, TimeWindow> TrayectoriaPorCamion;

        #endregion

        #region Constructor
        /// <summary>
        /// El constructor del arco
        /// </summary>
        /// <param name="idNodo1">El id del nodo inicial del arco</param>
        /// <param name="idNodo2">El id del nodo final del arco</param>
        /// <param name="largoSubArco_km">El largo del arco en km</param>
        public Arco(int idNodo1, int idNodo2, double largo_km, double pendiente, bool esDeTransito, bool esDeAtencion, bool esDeEspera)
        {
            _idNodo1 = idNodo1;
            _idNodo2 = idNodo2;
            _largo_KM = largo_km;
            _esDeEspera = esDeEspera;
            _esDeTransito = esDeTransito;
            _esDeAtencion = esDeAtencion;
            _pendiente = pendiente;
        }
        #endregion

        #region Properties
        /// <summary>
        /// La pendiente del arco. 1 si es cercana al 10%, 0 si es relativamente plano, -1 si es cercano al -10%
        /// </summary>
        public double Pendiente
        {
            get { return _pendiente; }
        }
        
        /// <summary>
        /// El largo del arco en km
        /// </summary>
        public double Largo_KM
        {
            get { return _largo_KM; }
        }

        public List<TimeWindow> TrayectoriasConSimuladas()
        {

            List<TimeWindow> Simuladas = new List<TimeWindow>();
            foreach (int id in TrayectoriaPorCamion.Keys)
            {
                Simuladas.Add(TrayectoriaPorCamion[id]);
            }
            Simuladas.AddRange(_trayectorias);
            Simuladas.Sort(delegate(TimeWindow tw1, TimeWindow tw2)
                {
                    return tw1.Termino_min.CompareTo(tw2.Termino_min);
                });

            return Simuladas;

        }

        /// <summary>
        /// Lista de trayectorias pasadas que ocurren en este arco
        /// </summary>
        public List<TimeWindow> Trayectorias
        {
            get 
            {
                return _trayectorias; 
            }
            set
            {
                _trayectorias = value;
            }
        }
        /// <summary>
        /// El id del nodo inicial del arco. Parte desde 1
        /// </summary>
        public int Id_Nodo_Inicial
        {
            get { return _idNodo1; }
        }
        /// <summary>
        /// El id del nodo final del arco. Parte desde 1
        /// </summary>
        public int Id_Nodo_Final
        {
            get { return _idNodo2; }
        }
        /// <summary>
        /// True si es un arco donde una pala carga o se descarga en un destino
        /// </summary>
        public bool EsDeAtencion
        {
            get { return _esDeAtencion; }
        }
        /// <summary>
        /// True si es un arco de espera
        /// </summary>
        public bool EsDeEspera
        {
            get { return _esDeEspera; }
        }
        /// <summary>
        /// True si es un arco de tránsito
        /// </summary>
        public bool EsDeTransito
        {
            get { return _esDeTransito; }
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Resetea los atributos dinámicos del arco
        /// </summary>
        public void Reset()
        {
            _trayectorias = new List<TimeWindow>();
            TrayectoriaPorCamion = new Dictionary<int, TimeWindow>();
        }
        /// <summary>
        /// Actualiza la lista de trayectorias. Un camión pasó por este arco desde T1 a T2
        /// </summary>
        /// <param name="T1">Instante de entrada al arco</param>
        /// <param name="T2">Instante de salida del arco</param>
        /// <param name="instanteActual_min">El instante_minutos actual de la corrida</param>
        public void AddTrayectoria(double T1, double T2, int idCam, int numProblema)
        {
            TimeWindow TW = new TimeWindow(T1, T2, idCam, numProblema);
            _trayectorias.Add(TW);
        }

        public void SetMaxVelocidad_KPH(double v_kph)
        {
            _maximaVelocidadParaRecorrerlo_kph = v_kph;
        }

        public void OrdenarTrayectorias()
        {
            //Ordeno la nueva lista según el menor tiempo de salida.
            if (_trayectorias.Count > 1)
            {
                _trayectorias.Sort(delegate(TimeWindow tw1, TimeWindow tw2)
                {
                    return tw1.Termino_min.CompareTo(tw2.Termino_min);
                });
            }
        }
        /// <summary>
        /// Actualiza las trayectorias según el instante actual siguiente
        /// </summary>
        /// <param name="instanteActual_min"></param>
        public void CorregirTrayectoriasSegunInstanteActual(double instanteActual)
        {
            List<TimeWindow> NuevaLista = new List<TimeWindow>();
            foreach (TimeWindow TW in _trayectorias)
            {
                if (TW.Inicio_min < instanteActual)
                {
                    TW.Inicio_min = instanteActual;
                }


                if (TW.Termino_min >= instanteActual)
                {
                    NuevaLista.Add(TW);
                }
                else
                { 
                    //Se sacó la trayectoria, su información ya no importaba
                }
            }

            //La asigno
            _trayectorias = NuevaLista;
            OrdenarTrayectorias();

        }

        #endregion
    }
}
