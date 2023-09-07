using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Clase que modela un destino: Chancador, Stock o Botadero
    /// </summary>
    public class Destino
    {
        #region Atributos fijos
        /// <summary>
        /// El tipo de destino que es
        /// </summary>
        public TipoDestino TipoDestino;
        /// <summary>
        /// El id de este destino. Parte de 1
        /// </summary>
        private int _id;
        /// <summary>
        /// El id del nodo al que pertenece. Parte de 1
        /// </summary>
        private int _idNodo;
        /// <summary>
        /// La lista de frentes (sus ids) de los que puede recibir material
        /// </summary>
        private List<int> _idsPalasAceptados;
        /// <summary>
        /// Capacidad de procesamiento en toneladas por hora
        /// </summary>
        private double _capacidad_tph;
        /// <summary>
        /// Mínimo tiempo de descarga entre los camiones [min]
        /// </summary>
        private double _minimoTiempoDeDescarga;
        
        #endregion

        #region Atributos dinámicos
        
        /// <summary>
        /// Lista de ventanas de tiempo_min en que se puede entrar a este nodo
        /// </summary
        private List<TimeWindow> _ventanasDeTiempoDisponible;
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Lista de instantes considerados en la media móvil
        /// </summary>
        private Dictionary<int, List<double>> _instantesConsideradosMediaMovil;
        /// <summary>
        /// [IdTipoCamion, List[Minutos]]: Demoras que se consideran en la media móvil
        /// </summary>
        private Dictionary<int, List<double>> _demorasConsideradasEnMediaMovil;
        /// <summary>
        /// List[Instante, Ton Descargado]: Indica las toneladas programadas por descargar
        /// </summary>
        public List<double[]> ToneladasProgramadas;
        /// <summary>
        /// Las toneladas acumuladas hasta este instante que recibirá el destino
        /// </summary>
        public double TonsAcumuladas;

        /// <summary>
        /// En la simulación de DISPATCH
        /// </summary>
        public Dictionary<int, TimeWindow> VentanasDeAtencionUsadasPorCam_DISPATCH;

        #endregion

        #region Properties

        /// <summary>
        /// Ventanas en que se atendieron los camiones simulados por DISPATCH
        /// </summary>
        public List<TimeWindow> VentanasDeTiempoDisponibles_ConsideraSimulacionDispatch
        {
            get
            {
                List<TimeWindow> Atenciones = new List<TimeWindow>(); //Las atenciones que había simulado DISPATCH
                #region Genero las atenciones por orden de inicio
                foreach (int key in VentanasDeAtencionUsadasPorCam_DISPATCH.Keys)
                {
                    Atenciones.Add(VentanasDeAtencionUsadasPorCam_DISPATCH[key]);
                }
                Atenciones.Sort(delegate(TimeWindow t1, TimeWindow t2)
                {
                    return t1.Inicio_min.CompareTo(t2.Inicio_min);
                });
                #endregion

                List<TimeWindow> VentanasPosiblesFinales = new List<TimeWindow>();
                foreach (TimeWindow TW in _ventanasDeTiempoDisponible)
                {
                    List<TimeWindow> AtencionesEnEstaTW = new List<TimeWindow>();
                    #region Genero esta lista [ORDENADA]: Atenciones que DISPATCH realizó en esta TW
                    foreach (TimeWindow TWDisp in Atenciones)
                    {
                        if (TWDisp.Inicio_min >= TW.Inicio_min && TWDisp.Termino_min <= TW.Termino_min)
                        {
                            AtencionesEnEstaTW.Add(TWDisp);
                        }
                    }
                    #endregion

                    if (AtencionesEnEstaTW.Count > 0)
                    {
                        #region Divido TW según las atenciones que realizó DISPATCH
                        double T1 = TW.Inicio_min;
                        foreach (TimeWindow Atencion in AtencionesEnEstaTW)
                        {
                            TimeWindow TW1 = new TimeWindow(T1, Atencion.Inicio_min, Atencion.IdCamion, Atencion.NumProblema);
                            if (TW1.LargoIntervalo_Minutos > 0)
                            {
                                VentanasPosiblesFinales.Add(TW1);
                            }
                            T1 = Atencion.Termino_min;
                        }
                        TimeWindow TWFinal = new TimeWindow(T1, TW.Termino_min, -1, -1); //HARDCODE: NO SÉ QUÉ VALORES PONERLE 
                        if (TWFinal.LargoIntervalo_Minutos > 0)
                        {
                            VentanasPosiblesFinales.Add(TWFinal);
                        }
                        #endregion
                    }
                    else //Agrego la ventana TW tal como está
                    {
                        VentanasPosiblesFinales.Add(TW);
                    }
                }

                //Ordeno las ventanas
                VentanasPosiblesFinales.Sort(delegate(TimeWindow t1, TimeWindow t2)
                {
                    return t1.Inicio_min.CompareTo(t2.Inicio_min);
                });

                return VentanasPosiblesFinales;
            }
        }


        
        /// <summary>
        /// Lista de ventanas de tiempo_min en que este arco puede utilizarse
        /// </summary>
        public List<TimeWindow> VentanasDeTiempoDisponible
        {
            get { return _ventanasDeTiempoDisponible; }
        }
        /// <summary>
        /// Capacidad de procesamiento en toneladas por hora
        /// </summary>
        public double Capacidad_TPH
        {
            get { return _capacidad_tph; }
        }        
        /// <summary>
        /// La lista de palas (sus ids) de los que puede recibir material
        /// </summary>
        public List<int> IdsPalasAceptadas
        {
            get { return _idsPalasAceptados; }
        }
        /// <summary>
        /// El id del nodo al que pertenece. Parte de 1
        /// </summary>
        public int IdNodo
        {
            get { return _idNodo; }
        }
        /// <summary>
        /// El id de este destino. Parte de 1
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        #endregion

        #region Métodos
        public Destino(TipoDestino _tipoDestino, int _id, int _idNodo, double capacidad_tph, List<int> idsPalasAceptadas)
        {
            this._capacidad_tph = capacidad_tph;
            this.TipoDestino = _tipoDestino;
            this._id = _id;
            this._idNodo = _idNodo;
            _idsPalasAceptados = idsPalasAceptadas;
        }
        /// <summary>
        /// Resetea los atributos dinámicos del destino
        /// </summary>
        public void Reset()
        {
            _instantesConsideradosMediaMovil = new Dictionary<int, List<double>>();
            _demorasConsideradasEnMediaMovil = new Dictionary<int, List<double>>();
            ToneladasProgramadas = new List<double[]>();            
            TonsAcumuladas = 0;

            //Inicialmente todos los arcos están disponible en el periodo [0, 2T]. 2T para que no haya problemas al llegar a T
            _ventanasDeTiempoDisponible = new List<TimeWindow>();
            TimeWindow tw = new TimeWindow(0, Configuracion.MaxTiempoVentana, 0, 0);
            _ventanasDeTiempoDisponible.Add(tw);

            VentanasDeAtencionUsadasPorCam_DISPATCH = new Dictionary<int, TimeWindow>();

        }        
        /// <summary>
        /// Agrega un dato de demora
        /// </summary>
        /// <param name="instanteMinutos">El instante</param>
        /// <param name="tipoCamion">El tipo de camión</param>
        /// <param name="demora_minutos">La demora en minutos</param>
        public void AgregarEspera(double instanteMinutos, int tipoCamion, double demora_minutos)
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
        /// Retorna la demora promedio de esta ruta en los últimos X minutos (parámetro de configuración) para un cierto tipo de camión
        /// </summary>
        /// <param name="tipoCamion"></param>
        /// <returns></returns>
        public double GetEsperaEnColaPromedio_Minutos(double instanteActual)
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
        /// Agrega el instante en que el camión fue terminó la descarga y cuántas toneladas recibirá
        /// </summary>
        /// <param name="instanteCamionDespachado"></param>
        /// <param name="toneladasPorRecibir"></param>
        public void AgregarDescargaDeToneladas(double instanteCamionDespachado, double instanteTermino, double toneladas)
        {            
            double[] dato = new double[2];
            dato[0] = instanteTermino;
            dato[1] = toneladas;
            ToneladasProgramadas.Add(dato);
            TonsAcumuladas += toneladas;
        }
        /// <summary>
        /// Setea un intervalo de atención y actualiza los otros
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="numTW">El número de la TW. PARTE DE 1</param>
        public void SetNuevoIntervaloDeAtencion(double T1, double T2, int numTW, double instanteActual)
        {
            TimeWindow TWUsada = _ventanasDeTiempoDisponible[numTW - 1];
            _ventanasDeTiempoDisponible.Remove(TWUsada); //La saco
            if (T1 < TWUsada.Inicio_min || T2 > TWUsada.Termino_min)
            {
                throw new Exception("Uso de la tw erróneo");
            }

            //Agrego una nueva
            TimeWindow Nueva1 = new TimeWindow(Math.Max(TWUsada.Inicio_min, instanteActual), T1, -1,-1);
            TimeWindow Nueva2 = new TimeWindow(Math.Max(T2, instanteActual), TWUsada.Termino_min,-1,-1);

            //Verifico que sean válidas para el camión más rápido en cargar
            if (Nueva1.LargoIntervalo_Minutos >= _minimoTiempoDeDescarga)
            {
                _ventanasDeTiempoDisponible.Add(Nueva1);
            }
            if (Nueva2.LargoIntervalo_Minutos >= _minimoTiempoDeDescarga)
            {
                _ventanasDeTiempoDisponible.Add(Nueva2);
            }


            //Ordeno las ventanas
            _ventanasDeTiempoDisponible.Sort(delegate(TimeWindow t1, TimeWindow t2)
            {
                return t1.Inicio_min.CompareTo(t2.Inicio_min);
            });
        }
        /// <summary>
        /// Borra las ventanas de tiempo inutilizables
        /// </summary>
        public void LimpiarVentanasViejas(double instanteActual)
        {
            List<TimeWindow> TWNuevas = new List<TimeWindow>();
            foreach (TimeWindow TW in _ventanasDeTiempoDisponible)
            {
                if (TW.Termino_min > instanteActual)
                {
                    TWNuevas.Add(TW);
                }
            }

            _ventanasDeTiempoDisponible = TWNuevas;
            //Ordeno las ventanas
            _ventanasDeTiempoDisponible.Sort(delegate(TimeWindow t1, TimeWindow t2)
            {
                return t1.Inicio_min.CompareTo(t2.Inicio_min);
            });

            

        }
        public void SetTiempoDeDescargaMinimo_min(double T)
        {
            _minimoTiempoDeDescarga = T;
        }

        #endregion
    }
}