using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class Pala
    {
        #region Atributos estáticos
        /// <summary>
        /// El id del nodo asociado
        /// </summary>
        private int _idNodo;
        /// <summary>
        /// El identificador de la idPala. Parte desde 1
        /// </summary>
        private int _id;        
        /// <summary>
        /// Lista de Ids de tipo camión que la idPala acepta
        /// </summary>
        private List<int> _idsTipoCamionAcepta;
        /// <summary>
        /// Capacidad de excavación en Ton por hora
        /// </summary>
        private double _capacidad_TPH;
        /// <summary>
        /// La tasas de excavación asignada por el modelo LP de DISPATCH
        /// </summary>
        private double _tasaExcavacionSegun_DispatchLP_tph;
        /// <summary>
        /// El tiempo de carga más rápido entre todos los camiones
        /// </summary>
        private double _minimoTiempoDeCarga;
        /// <summary>
        /// Lista con los ids de los destinos a los que se puede descargar
        /// </summary>
        private List<int> _idsDestinosPosibles;
        /// <summary>
        /// La ley de cobre. Ej: 0.9 ==> 0.9%
        /// </summary>
        public double Ley;

        #endregion

        #region Atributos dinámicos
        /// <summary>
        /// Último instante en que un camión solicitó despacho y fue asignado a este destino
        /// </summary>
        private double _ultimoInstanteAsignacion;
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
        /// [Tiempo, Tons]: Las toneladas programadas para remover por la pala en cada instante de asignación
        /// </summary>
        private List<DoubleDouble> _tiempoTonsProgradamas;
        /// <summary>
        /// En la simulación de DISPATCH
        /// </summary>
        public Dictionary<int, TimeWindow> VentanasDeAtencionUsadasPorCam_DISPATCH;
        
        #endregion

        #region Properties
        /// <summary>
        /// Último instante en que un camión solicitó despacho y fue asignado a esta pala
        /// </summary>
        public double UltimoInstanteAsignacion
        {
            get { return _ultimoInstanteAsignacion; }
        }

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
        /// Lista con los ids de los destinos a los que se puede descargar
        /// </summary>
        public List<int> IdsDestinosPosibles
        {
            get { return _idsDestinosPosibles; }
        }
        /// <summary>
        /// Lista de ventanas de tiempo en que esta pala puede utilizarse
        /// </summary>
        public List<TimeWindow> VentanasDeTiempoDisponible
        {
            get { return _ventanasDeTiempoDisponible; }
        }
        /// <summary>
        /// El identificador del nodo
        /// </summary>
        public int IdNodo
        {
            get { return _idNodo; }
        }
        /// <summary>
        /// La tasas de excavación asignada por el modelo LP de DISPATCH
        /// </summary>
        public double TasaExcavacionSegun_DispatchLP_tph
        {
            get { return _tasaExcavacionSegun_DispatchLP_tph; }
        }
        /// <summary>
        /// Capacidad de excavación en Ton por hora
        /// </summary>
        public double Capacidad_TPH
        {
            get { return _capacidad_TPH; }
        }
        /// <summary>
        /// Lista de Ids de tipo camión que la idPala acepta
        /// </summary>
        public List<int> IdsTipoCamionAcepta
        {
            get { return _idsTipoCamionAcepta; }
        }
        /// <summary>
        /// El identificador de la idPala. Parte desde 1
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        
        
        #endregion

        #region Métodos
        /// <summary>
        /// Constructor de una idPala
        /// </summary>
        /// <param name="_id">El identificador de la idPala</param>
        /// <param name="_aceptaAlTipoCamion">Los tipos de camiones que pueden cargar en esta idPala</param>
        public Pala(int _id, List<int> _listaIdsTipoCamionAceptados, double capacidad_tph, int idNodo, List<int> _idsDestinosPosibles, double ley)
        {
            _capacidad_TPH = capacidad_tph;
            this._id = _id;
            this._idsTipoCamionAcepta = _listaIdsTipoCamionAceptados;
            _idNodo = idNodo;
            this._idsDestinosPosibles = _idsDestinosPosibles;
            Ley = ley;
        }
        /// <summary>
        /// Asigna los resultados de la fase LP de DISPATCH
        /// </summary>
        /// <param name="tasaAsignada"></param>
        public void SetResultadoDispatchLP(double tasaAsignada)
        {
            _tasaExcavacionSegun_DispatchLP_tph = tasaAsignada;
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

            //Ahora veo las ventanas de tiempo: Analizo si se debe corregir el T_inicio al instante actual de la PRIMERA ventana
            for (int t = 0; t < _ventanasDeTiempoDisponible.Count; t++)
            {
                TimeWindow TW = _ventanasDeTiempoDisponible[t];
                if (TW.Inicio_min < instanteActual)
                {
                    TW.Inicio_min = instanteActual;
                    break;
                }
            }


        }
        /// <summary>
        /// Resetea los atributos dinámicos
        /// </summary>
        public void Reset()
        {
            _tiempoTonsProgradamas = new List<DoubleDouble>();
            _instantesConsideradosMediaMovil = new Dictionary<int, List<double>>();
            _demorasConsideradasEnMediaMovil = new Dictionary<int, List<double>>();
            _ultimoInstanteAsignacion = 0;

            //Inicialmente todos los arcos están disponible en el periodo [0, 2T]. 2T para que no haya problemas al llegar a T
            _ventanasDeTiempoDisponible = new List<TimeWindow>();
            TimeWindow tw = new TimeWindow(0, Configuracion.MaxTiempoVentana, 0, 0);
            _ventanasDeTiempoDisponible.Add(tw);
            VentanasDeAtencionUsadasPorCam_DISPATCH = new Dictionary<int, TimeWindow>();
        }
        /// <summary>
        /// Agrega un dato de espera
        /// </summary>
        /// <param name="instanteMinutos">El instante</param>
        /// <param name="tipoCamion">El tipo de camión</param>
        /// <param name="demora_minutos">La demora en minutos</param>
        public void AgregarEspera(double instanteMinutos, int tipoCamion, double espera)
        {
            if (!_instantesConsideradosMediaMovil.ContainsKey(tipoCamion))
            {
                _instantesConsideradosMediaMovil.Add(tipoCamion, new List<double>());
                _demorasConsideradasEnMediaMovil.Add(tipoCamion, new List<double>());
            }
            _instantesConsideradosMediaMovil[tipoCamion].Add(instanteMinutos);
            _demorasConsideradasEnMediaMovil[tipoCamion].Add(espera);
        }
        /// <summary>
        /// El tiempo más rápido en cargar a un camión [minutos]
        /// </summary>
        /// <param name="T_minutos"></param>
        public void SetTiempoMasRapidoDeCarga(double T_minutos)
        {
            _minimoTiempoDeCarga = T_minutos;
        }
        /// <summary>
        /// Retorna la demora promedio de esta ruta en los últimos X minutos (parámetro de configuración)
        /// </summary>
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
        /// Setea un intervalo de atención
        /// </summary>
        /// <param name="T1"></param>
        /// <param name="T2"></param>
        /// <param name="numTW"></param>
        /// <param name="instanteActual"></param>
        public void SetNuevoIntervaloDeAtencion(double T1, double T2, int numTW, double instanteActual)
        {
            _ultimoInstanteAsignacion = instanteActual;
            TimeWindow TWUsada = _ventanasDeTiempoDisponible[numTW-1];
            _ventanasDeTiempoDisponible.Remove(TWUsada); //La saco
            if (T1 < TWUsada.Inicio_min || T2 > TWUsada.Termino_min)
            {
                throw new Exception("Uso de la tw erróneo");
            }

            //Agrego una nueva
            TimeWindow Nueva1 = new TimeWindow(Math.Max(TWUsada.Inicio_min, instanteActual), T1,-1,-1);
            TimeWindow Nueva2 = new TimeWindow(Math.Max(T2, instanteActual), TWUsada.Termino_min,-1,-1);

            //Verifico que sean válidas para el camión más rápido en cargar
            if (Nueva1.LargoIntervalo_Minutos >= _minimoTiempoDeCarga)
            {
                _ventanasDeTiempoDisponible.Add(Nueva1);
            }
            if (Nueva2.LargoIntervalo_Minutos >= _minimoTiempoDeCarga)
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
        /// Calcula el tiempo de ocio esperado si el camión llega en este instante. >= 0
        /// </summary>
        /// <param name="llegada_min"></param>
        /// <returns></returns>
        public double TiempoDeOcioEsperado_MIN(double llegada_min, double tiempoCargaRequerido_min)
        {
            double t_ocio = 0;
            foreach (TimeWindow TW in _ventanasDeTiempoDisponible)
            {
                // Veo si me puedo atender:
                double T_InicioCarga = Math.Max(llegada_min, TW.Inicio_min);
                double T_CargaDisponible = TW.Termino_min - T_InicioCarga;
                if (T_CargaDisponible >= tiempoCargaRequerido_min) //Si me puedo atender
                {
                    double ocio = llegada_min - TW.Inicio_min;
                    t_ocio += ocio;
                    return t_ocio;
                }
                else // Perdí la ventana de tiempo, la pala estuvo ociosa en este rato
                {
                    t_ocio += TW.LargoIntervalo_Minutos;
                }
            }
            throw new Exception("No se pudo atender el camión");
        }

        #endregion
    }
}
