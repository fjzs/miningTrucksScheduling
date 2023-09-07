using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ShiftUC.Solver.Modelacion;
using ShiftUC.Solver;
using System.Configuration;
using System.Threading;

namespace SolverDemo
{
    /// <summary>
    /// Clase que organiza todo el despacho de los camiones durante el día
    /// </summary>
    public class MinaCieloAbierto
    {
        //Propios de la clase
        #region Atributos fijos
        /// <summary>
        /// La lista de camiones con que se contará en este día
        /// </summary>
        private List<Camion> _camiones;
        /// <summary>
        /// La lista de los camiones ordenada según el tiempo de inicio.
        /// </summary>
        private List<Camion> _camionPorTiempoDisponible;
        /// <summary>
        /// La lista de palas con que se contará en este día
        /// </summary>
        private List<Pala> _palas;
        /// <summary>
        /// El plan día asociado
        /// </summary>
        private PlanDia planDia;
        /// <summary>
        /// El grafo de la red de transporte
        /// </summary>
        private Grafo _grafo;
        /// <summary>
        /// Los destinos posibles de la MCA
        /// </summary>
        private List<Destino> _destinos;
        /// <summary>
        /// [IdTipo ; TipoCamión]: Diccionario que contiene los tipos de camiones y sus características
        /// </summary>
        private Dictionary<int, TipoCamion> _tipoCamiones;
        /// <summary>
        /// Lista de los status posibles de los camiones
        /// </summary>
        private List<StatusCamion> _statusCamiones;
        /// <summary>
        /// Conjunto de rutas de la mina
        /// </summary>
        private List<Ruta> _rutas;
        /// <summary>
        /// [#Ciclos, [Id D0 ; Lista de secuencia de ids de rutas]]
        /// </summary>
        private Dictionary<int,Dictionary<int, List<List<int>>>> SecuenciasDeRutasSegunNumCiclosDInicio;
        /// <summary>
        /// [IdPala , IdDest, HaciaDestino] --> 1 si la ruta hace eso
        /// </summary>
        private Dictionary<bool, int[,]> IdRutaSegun_HaciaDestino_Pala_Dest;

        #endregion
        
        //Con respecto al modelo de despacho
        #region SETS
        private Conjunto CAMIONES;
        private Conjunto PALAS;
        private Conjunto DESTINOS;
        private Conjunto NODOS;
        private Conjunto STATUS_CAMIONES;
        private Conjunto RUTAS;
        private Conjunto TIPO_CAMIONES;
        private Conjunto REQUERIMIENTOS;
        #endregion
        #region PARÁMETROS ESTÁTICOS
        /// <summary>
        /// [CAMIONES]
        /// </summary>
        private int[] tipoCamion;
        /// <summary>
        /// [NODO,NODO]: 1 si existe un arco entre estos nodos
        /// </summary>
        private int[,] existeArco;
        /// <summary>
        /// [NODOS,NODOS]: 1 si el arco es para esperar en cola
        /// </summary>
        private int[,] esArcoDeEsperaEnCola;
        /// <summary>
        /// [NODOS, NODOS, RUTAS]: 1 si el arco (u,v) pertenece a la ruta r
        /// </summary>
        private int[, ,] arcosPorRuta;
        /// <summary>
        /// [RUTAS, DESTINOS, PALAS]: 1 si la ruta r se puede utilizar para ir desde d a j
        /// </summary>
        private int[, ,] esRutaDestinoPala;
        /// <summary>
        /// [RUTAS, PALAS, DESTINOS]: 1 si la ruta r se puede utilizar para ir desde j a d
        /// </summary>
        private int[, ,] esRutaPalaDestino;        
        /// <summary>
        /// [DESTINOS]
        /// </summary>
        private int[] nodoDestino;
        /// <summary>
        /// [PALAS]
        /// </summary>
        private int[] nodoPala;
        /// <summary>
        /// [PALAS]: Id del nodo donde inicia la carga
        /// </summary>
        private int[] nodoInicioCarga;
        /// <summary>
        /// [DESTINOS]: Id del nodo donde inicia la descarga
        /// </summary>
        private int[] nodoInicioDescarga;
        /// <summary>
        /// [PALAS]: Id del nodo donde inicia la cola
        /// </summary>
        private int[] nodoLlegadaColaPala;
        /// <summary>
        /// [DESTINOS]: Id del nodo donde inicia la cola
        /// </summary>
        private int[] nodoLlegadaColaDestino;
        /// <summary>
        /// [TIPO CAMIONES]
        /// </summary>
        private double[] capacidadCamion_ton;
        /// <summary>
        /// [TIPO CAMION, STATUS, NODO, NODO]
        /// </summary>
        private double[, , ,] tiempoViajeTipoCamionArco_min;
        /// <summary>
        /// [TIPO CAMION, STATUS, NODO, NODO]
        /// </summary>
        private double[, , ,] costoViaje_pesos;
        /// <summary>
        /// [TIPO CAMIÓN ; RUTA]: El costo de viaje de la ruta por tipo de camión
        /// </summary>
        private double[,] costoRutaPorTipoCamionStatus_pesos;
        /// <summary>
        /// [CAMIONES , PALAS]
        /// </summary>
        private int[,] isCamionAsignablePala;
        /// <summary>
        /// [TIPO CAMIÓN , DESTINOS]
        /// </summary>
        private double[,] tiempoDescargaTipoCamionDestino_min;
        /// <summary>
        /// [TIPO CAMIÓN , PALA]
        /// </summary>
        private double[,] tiempoCargaTipoCamionPala_min;
        /// <summary>
        /// Indica el valor en pesos de una tonelada promedio excavada
        /// </summary>
        private double pesosPorTonelada;
        /// <summary>
        /// Indica cuántas toneladas debieran excavar como mínimo por minuto para alcanzar el plan día
        /// </summary>
        private double toneladasPorMinuto;
        /// <summary>
        /// [IdTipoCamión, Destino Inicial]: Tiempo de ciclo suponiendo flujo libre máximo en la red para este tipo de camión
        /// </summary>
        private double[,] tiempoDeCicloFlujoLibreMax_min;
        /// <summary>
        /// [IdTipoCamión]: Tiempo de ciclo a flujo libre mínimo de este tipo de camión independiente del destino de partida
        /// </summary>
        private double[] tiempoDeCicloFlujoLibreMin_min;
        /// <summary>
        /// [DInicial , Id TipoCam]: Tiempo de ciclo a flujo libre mín según estos dos parámetros
        /// </summary>
        private double[,] tiempoDeCicloFlujoLibreMinSegunDInicioTipoCam_min;
        /// <summary>
        /// [Id-TipoCam, Id-Destino Inicial, Id-Pala, Id-Destino Final]
        /// </summary>
        private double[, , ,] tiempoDeCicloFlujoLibre_KD0PD;

        private double capacidadPromedio;

        //Parámetros del plan diario
        /// <summary>
        /// Intervalo para el que cambia la penalidad del tonelaje en el plan día
        /// </summary>
        private double intervaloTonelaje;
        /// <summary>
        /// [REQUERIMIENTOS]: Toneladas totales que debe procesar cada requerimiento
        /// </summary>
        private double[] toneladasTotalesSegunPlanDiario;
        /// <summary>
        /// [PALAS , DESTINOS]
        /// </summary>
        private int[,] isMaterialPalaDescargableEnDestino;
        /// <summary>
        /// [PALA, DESTINO, REQUERIMIENTO]: 1 si desde la pala j se descarga a d en el req r
        /// </summary>
        private int[,,] isPalaDestinoEnReq;
        /// <summary>
        /// Penalización asociada a la mantención de la ley
        /// </summary>
        private double coefMantenerLey;

        #endregion
        #region PARÁMETROS DE ESTADO
        private Conjunto CAMIONES_CONSIDERADOS;
        private int numProblema;
        private double demoraMaximaPorCiclo_min;
        private double demoraMaximaPorArco_min;
        private double esperaEnColaMax_min;
        private double instanteActual_min;
        /// <summary>
        /// [Camión]: Destino inicial del camión
        /// </summary>
        private int[] destinoInicialCamion;
        private int idCamionSolicitaDespacho;
        /// <summary>
        /// Horizonte [min] para el cual se planifica el despacho
        /// </summary>
        private double horizontePlanificacion_min;
        /// <summary>
        /// [Tipo Camión ; Destino Inicial]
        /// </summary>
        private double[,] tiempoDeCicloAVGPor_TipoCamionDestinoInicial_min;
        /// <summary>
        /// [Tipo Camión]
        /// </summary>
        private double[] tiempoDeCicloAVGPor_TipoCamion_min;
        /// <summary>
        /// El número de ciclos que realizará cada camión
        /// </summary>
        private int[] numCiclosPorCamion;
        private List<int> idsCamionesConsiderados;
        private int[] nodoInicioCamion;
        private double[] instanteCamionDisponible_min;
        /// <summary>
        /// [PALAS]
        /// </summary>
        private int[] numTimeWindowsPorPala;
        /// <summary>
        /// [DESTINOS]
        /// </summary>
        private int[] numTimeWindowsPorDestino;
        /// <summary>
        /// [PALAS, TW]
        /// </summary>
        private double[,] instanteInicioTW_Pala_min;
        /// <summary>
        /// [PALAS, TW]
        /// </summary>
        private double[,] instanteTerminoTW_Pala_min;
        /// <summary>
        /// [DESTINOS, TW]
        /// </summary>
        private double[,] instanteInicioTW_Dest_min;
        /// <summary>
        /// [DESTINOS, TW]
        /// </summary>
        private double[,] instanteTerminoTW_Dest_min;
        /// <summary>
        /// [NODOS, NODOS]
        /// </summary>
        private double[,] numTrayectoriasPorArco;
        /// <summary>
        /// [NODOS, NODOS, MáxNUM]
        /// </summary>
        private double[, ,] instanteInicioTrayectoria_min;
        /// <summary>
        /// [NODOS, NODOS, MáxNUM]
        /// </summary>
        private double[, ,] instanteTerminoTrayectoria_min;
        /// <summary>
        /// [REQUERIMIENTOS]
        /// </summary>
        private double[] toneladasRecibidas;
        /// <summary>
        /// 1 si estoy en DISPATCH,0 si no.
        /// </summary>
        private int esModelo_Dispatch;
        /// <summary>
        /// 1 si estoy en el modelo de descarga más temprana, 0 si no
        /// </summary>
        private int esModelo_DescargaMasTemprana;
        /// <summary>
        /// [CAMIONES_CONSIDERADOS, STATUS, RUTAS]: 1 si el camión hace la ruta r con status s
        /// </summary>
        private int[, ,] heuristica_Dispatch;
        /// <summary>
        /// [CAMIONES_CONSIDERADOS, STATUS, RUTAS]: 1 si el camión hace la ruta r con status s
        /// </summary>
        private int[, ,] heuristica_DescargaAlDestinoMasAtrasado;
        /// <summary>
        /// [CAMIONES, CICLOS, STATUS, NODOS]: Tiempo de viaje a flujo libre saliendo desde un destino específico
        /// </summary>
        private double[, , ,] tiempoViajeMinimoFlujoLibreEnLlegarANodo;
        /// <summary>
        /// [CAMIONES, CICLOS, STATUS, NODOS]: Tiempo de viaje a flujo libre saliendo desde un destino específico
        /// </summary>
        private double[, , ,] tiempoViajeMaximoFlujoLibreEnLlegarANodo;  //CREAR
        /// <summary>
        /// [CAMIONES ; CICLOS]
        /// </summary>
        private double[,] instanteInicioAproxCamionCiclo;
        /// <summary>
        /// AsignaCamionPalaDestino [i,j,d,c]: Solución pasada. Con esto las instancias no parten desde 0
        /// </summary>
        private SolucionAnterior solucionAnterior;

        #endregion
        
        #region ESTADÍSTICA
        Dictionary<MetodoSolucion, List<Asignacion>> AsignacionesSegunMS;
        List<ResultadoCorrida> ResultadoCorridas;
        double TCicloAVGInicial;
        bool continuarInstancia;
        #endregion

        //Con respecto al modelo LP de DISPATCH
        #region PARÁMETROS
        /// <summary>
        /// Intervalo para evaluar la penalidad asociada a la desutilización de las palas
        /// </summary>
        private double intervaloPala;
        /// <summary>
        /// [PALAS]
        /// </summary>
        private double[] capacidadPala_tph;
        /// <summary>
        /// [DESTINOS]
        /// </summary>
        private double[] capacidadDestino_tph;
        /// <summary>
        /// [TIPO_CAMIONES , PALAS]
        /// </summary>
        private double[,] tiempoCargaTipoCamionPala_hra;
        /// <summary>
        /// [TIPO_CAMIONES , DESTINOS]
        /// </summary>
        private double[,] tiempoDescargaTipoCamionDestino_hra;
        /// <summary>
        /// [TIPO_CAMIONES , PALAS, DESTINOS]: Viaje cargado
        /// </summary>
        private double[,,] tiempoViajePorTipo_PD_h;
        /// <summary>
        /// [TIPO_CAMIONES , DESTINOS, PALAS]: Viaje vacío
        /// </summary>
        private double[, ,] tiempoViajePorTipo_DP_h;
        /// <summary>
        /// [TIPO_CAMIONES , PALAS]
        /// </summary>
        private int[,] isTipoAsignableAPala;

        // *** Parámetros del plan diario
        /// <summary>
        /// [REQUERIMIENTOS]
        /// </summary>
        private double[] toneladasPorHoraSegunPlanDiario;
        #endregion
        #region RESULTADO FASE LP
        /// <summary>
        /// Contiene las rutas usadas por Dispatch en que los camiones van a las palas
        /// </summary>
        private List<Ruta_Dispatch> _RutasDispatch_Vacias;
        /// <summary>
        /// Contiene las rutas usadas por Dispatch en que los camiones van a destinos
        /// </summary>
        private List<Ruta_Dispatch> _RutasDispatch_Cargadas;
        /// <summary>
        /// Suma de las tasas de excavación de las palas [tph]
        /// </summary>
        private double _tasaDeExcavacionTotal;
        /// <summary>
        /// Total de camiones requeridos por el plan estacionario
        /// </summary>
        private double _totalDeCamionesRequeridos;
        #endregion

        #region MÉTODOS

        #region Métodos típicos
        public MinaCieloAbierto()
        {            
            #region Cosas de texto
            Utiles.EscribeMensajeConsola("Leyendo datos de entrada...", CategoriaMensaje.Categoria_1, 0, 1);
            string[] texto;
            char[] tab = { '\t' };
            char[] coma = { ',' };
            #endregion
            #region Inicializo datos del formulario y configuraciones
            //HARDCODE Instancia 2P2D
            TCicloAVGInicial = 20;
            demoraMaximaPorCiclo_min = 25;
            esperaEnColaMax_min = 15;
            demoraMaximaPorArco_min = 10;           

            List<MetodoSolucion> metodosSolucion = new List<MetodoSolucion>();
            metodosSolucion.Add(MetodoSolucion.MIP);
            metodosSolucion.Add(MetodoSolucion.DISPATCH);
            metodosSolucion.Add(MetodoSolucion.DRMA);
            Configuracion.SetParameters(metodosSolucion);
            CategoriaMensaje categoriaMensaje = CategoriaMensaje.Categoria_4;
            Utiles.EscribeMensajeConsola("Configuración inicializada", categoriaMensaje, 0, 0);
            #endregion            
            #region Lectura de datos
            try
            {
                int numeroColumnas;
                List<int> nodosDestinos = new List<int>();
                #region Inicializo el Plan Día
                texto = File.ReadAllLines("Input/PlanDia.txt");
                List<Requerimiento> requerimientos = new List<Requerimiento>();
                numeroColumnas = 4;
                double totalTons = 0;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura del plan día falló");
                    }                    
                    int id = Int32.Parse(datos[0]);
                    int idD = Int32.Parse(datos[1]);
                    int idP = Int32.Parse(datos[2]);
                    double toneladas = double.Parse(datos[3], System.Globalization.CultureInfo.InvariantCulture);
                    totalTons += toneladas;
                    Requerimiento R = new Requerimiento(idD, toneladas, i, idP);
                    requerimientos.Add(R);
                }
                foreach (Requerimiento R in requerimientos)
                {
                    double porcen = R.ToneladasNecesita / totalTons;
                    R.PorcentajeDelPlan = porcen;
                }
                planDia = new PlanDia(requerimientos);
                Utiles.EscribeMensajeConsola("Plan diario inicializado", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo Destinos
                _destinos = new List<Destino>();
                texto = File.ReadAllLines("Input/Destinos.txt");
                numeroColumnas = 4;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de los destino falló");
                    }
                    int id_destino = Int32.Parse(datos[0]);
                    int id_nodo = Int32.Parse(datos[1]);
                    string tipo = datos[2];
                    TipoDestino tipoDestino = TipoDestino.Botadero;
                    if (tipo == "Chancador")
                    {
                        tipoDestino = TipoDestino.Chancador;
                    }
                    else if (tipo == "Stock")
                    {
                        tipoDestino = TipoDestino.Stock;
                    }
                    double capacidad = Double.Parse(datos[3]);

                    List<int> idsPalasAceptadas = new List<int>();
                    foreach (Requerimiento R in planDia.Requerimientos)
                    {
                        if (R.IdDestino == id_destino)
                        {
                            idsPalasAceptadas.Add(R.IdPala);
                        }
                    }

                    Destino D = new Destino(tipoDestino, id_destino, id_nodo, capacidad, idsPalasAceptadas);
                    _destinos.Add(D);
                    nodosDestinos.Add(id_nodo);
                }
                Utiles.EscribeMensajeConsola("Destinos inicializados", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo Tipos de camiones
                _tipoCamiones = new Dictionary<int, TipoCamion>();
                texto = File.ReadAllLines("Input/TiposCamiones.txt");
                numeroColumnas = 11;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de los tipos de camiones falló");
                    }
                    int id = Int32.Parse(datos[0]);
                    string nombre = datos[1];
                    double capacidadTolva = double.Parse(datos[2], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadCargado_Bajando_kph = double.Parse(datos[3], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadCargado_Plano_kph = double.Parse(datos[4], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadCargado_Subiendo_kph = double.Parse(datos[5], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadVacio_Bajando_kph = double.Parse(datos[6], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadVacio_Plano_kph = double.Parse(datos[7], System.Globalization.CultureInfo.InvariantCulture);
                    double velocidadVacio_Subiendo_kph = double.Parse(datos[8], System.Globalization.CultureInfo.InvariantCulture);
                    double rendimientoCargado_pesosPorKm = double.Parse(datos[9], System.Globalization.CultureInfo.InvariantCulture);
                    double rendimientoVacio_pesosPorKm = double.Parse(datos[10], System.Globalization.CultureInfo.InvariantCulture);

                    TipoCamion tc = new TipoCamion(id, nombre, capacidadTolva, rendimientoCargado_pesosPorKm, rendimientoVacio_pesosPorKm,
                        velocidadCargado_Bajando_kph, velocidadCargado_Plano_kph, velocidadCargado_Subiendo_kph, velocidadVacio_Bajando_kph,
                        velocidadVacio_Plano_kph, velocidadVacio_Subiendo_kph);
                    _tipoCamiones.Add(id, tc);                    
                }
                Utiles.EscribeMensajeConsola("Tipos de camiones inicializados", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo Camiones
                _camiones = new List<Camion>();
                _camionPorTiempoDisponible = new List<Camion>();
                texto = File.ReadAllLines("Input/Camiones.txt");
                numeroColumnas = 4;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de los camiones falló");
                    }
                    int id = Int32.Parse(datos[0]);
                    int id_tipoCamion = Int32.Parse(datos[1]);
                    TipoCamion TC = _tipoCamiones[id_tipoCamion];
                    int idNodoInicio = Int32.Parse(datos[2]);
                    double min_inicio = double.Parse(datos[3], System.Globalization.CultureInfo.InvariantCulture);
                    Camion C = new Camion(id, TC, idNodoInicio, min_inicio);
                    C.Reset();
                    _camiones.Add(C);
                    _camionPorTiempoDisponible.Add(C);
                }
                Utiles.EscribeMensajeConsola("Camiones inicializados", categoriaMensaje, 0, 0);
                #endregion                
                #region Inicializo Palas
                _palas = new List<Pala>();
                texto = File.ReadAllLines("Input/Palas.txt");
                numeroColumnas = 5;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de las palas falló");
                    }
                    int id = Int32.Parse(datos[0]);
                    List<int> _idsTipoCamionAcepta = new List<int>();
                    List<string> ids_permitidos = datos[1].Split(coma).ToList();
                    foreach (string id_permitido in ids_permitidos)
                    {
                        int id_int = Int32.Parse(id_permitido);
                        _idsTipoCamionAcepta.Add(id_int);
                    }
                    double capacidad = Double.Parse(datos[2]);
                    int idNodo = Int32.Parse(datos[3]);
                    double ley = Double.Parse(datos[4])/100;

                    List<int> idsDestinosPosibles = new List<int>();
                    foreach (Requerimiento R in planDia.Requerimientos)
                    {
                        if(R.IdPala == id)
                        {
                            idsDestinosPosibles.Add(R.IdDestino);
                        }
                    }

                    Pala P = new Pala(id, _idsTipoCamionAcepta, capacidad, idNodo, idsDestinosPosibles, ley);
                    _palas.Add(P);
                }
                Utiles.EscribeMensajeConsola("Palas inicializadas", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo los tiempos de aculatamiento, carga y descarga por tipo de camión
                //Estas variables son diccionarios para cada tipo de camión en que se indica el tiempo de aculatamiento
                //tiempo de descarga y carga en cada pala y destino
                Dictionary<int, Dictionary<int, double>> _tiempoAculatamientoPalas_min_PorTipoCamion = new Dictionary<int, Dictionary<int, double>>();
                Dictionary<int, Dictionary<int, double>> _tiempoAculatamientoDestinos_min_PorTipoCamion = new Dictionary<int, Dictionary<int, double>>();
                Dictionary<int, Dictionary<int, double>> _tiempoCargaPalas_min_PorTipoCamion = new Dictionary<int, Dictionary<int, double>>();
                Dictionary<int, Dictionary<int, double>> _tiempoDescargaDestinos_min_PorTipoCamion = new Dictionary<int, Dictionary<int, double>>();
                texto = File.ReadAllLines("Input/Tiempos.txt");
                numeroColumnas = 5;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de los tiempos falló");
                    }
                    int id_tipoCamion = Int32.Parse(datos[0]);
                    //Creo el diccionario de este tipo de camión si no está
                    if (!_tiempoAculatamientoPalas_min_PorTipoCamion.ContainsKey(id_tipoCamion))
                    {
                        _tiempoAculatamientoPalas_min_PorTipoCamion.Add(id_tipoCamion, new Dictionary<int, double>());
                    }
                    if (!_tiempoAculatamientoDestinos_min_PorTipoCamion.ContainsKey(id_tipoCamion))
                    {
                        _tiempoAculatamientoDestinos_min_PorTipoCamion.Add(id_tipoCamion, new Dictionary<int, double>());
                    }
                    if (!_tiempoCargaPalas_min_PorTipoCamion.ContainsKey(id_tipoCamion))
                    {
                        _tiempoCargaPalas_min_PorTipoCamion.Add(id_tipoCamion, new Dictionary<int, double>());
                    }
                    if (!_tiempoDescargaDestinos_min_PorTipoCamion.ContainsKey(id_tipoCamion))
                    {
                        _tiempoDescargaDestinos_min_PorTipoCamion.Add(id_tipoCamion, new Dictionary<int, double>());
                    }

                    string tipo_workplace = datos[1];
                    int id_lugar = Int32.Parse(datos[2]);
                    double tiempo_acul = double.Parse(datos[3], System.Globalization.CultureInfo.InvariantCulture);
                    double tiempo_carga_o_descarga = double.Parse(datos[4], System.Globalization.CultureInfo.InvariantCulture);

                    if (tipo_workplace == "Pala")
                    {
                        _tiempoAculatamientoPalas_min_PorTipoCamion[id_tipoCamion].Add(id_lugar, tiempo_acul);
                        _tiempoCargaPalas_min_PorTipoCamion[id_tipoCamion].Add(id_lugar, tiempo_carga_o_descarga);
                    }
                    else //Es un destino
                    {
                        _tiempoAculatamientoDestinos_min_PorTipoCamion[id_tipoCamion].Add(id_lugar, tiempo_acul);
                        _tiempoDescargaDestinos_min_PorTipoCamion[id_tipoCamion].Add(id_lugar, tiempo_carga_o_descarga);
                    }

                    //Agrego estos diccionarios
                    _tipoCamiones[id_tipoCamion].SetTiempos
                        (
                        _tiempoAculatamientoPalas_min_PorTipoCamion[id_tipoCamion],
                        _tiempoAculatamientoDestinos_min_PorTipoCamion[id_tipoCamion],
                        _tiempoCargaPalas_min_PorTipoCamion[id_tipoCamion],
                        _tiempoDescargaDestinos_min_PorTipoCamion[id_tipoCamion]
                        );
                }
                Utiles.EscribeMensajeConsola("Tiempos de carga y descarga inicializados", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo el Grafo
                List<Nodo> nodos = new List<Nodo>();
                List<int> ids_nodos = new List<int>();
                List<Arco> arcos = new List<Arco>();
                _rutas = new List<Ruta>();
                List<int> idsRutas = new List<int>();

                texto = File.ReadAllLines("Input/Grafo.txt");
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    #region PARA CADA ARCO
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    int idNodo1 = Int32.Parse(datos[0]);
                    int idNodo2 = Int32.Parse(datos[1]);
                    double distancia_km = double.Parse(datos[2], System.Globalization.CultureInfo.InvariantCulture)/1000;
                    bool esArcoDeEspera = datos[3] == "1";
                    bool esArcoDeAtencion = datos[4] == "1";
                    bool esArcoDeTransito = (esArcoDeEspera == false) && (esArcoDeAtencion == false);
                    double pendiente = double.Parse(datos[5],System.Globalization.CultureInfo.InvariantCulture);
                    int n = 6;
                    //Creo el nodo si no está creado aún
                    if (!ids_nodos.Contains(idNodo1))
                    {
                        Nodo N = new Nodo(idNodo1);
                        nodos.Add(N);
                        ids_nodos.Add(idNodo1);
                    }
                    if (!ids_nodos.Contains(idNodo2))
                    {
                        Nodo N = new Nodo(idNodo2);
                        nodos.Add(N);
                        ids_nodos.Add(idNodo2);
                    }

                    Arco A = new Arco(idNodo1, idNodo2, distancia_km, pendiente, esArcoDeTransito, esArcoDeAtencion, esArcoDeEspera);
                    arcos.Add(A);

                    List<int> rutasPertenece = new List<int>();
                    #region Genero la lista de rutas a las que pertenece este arco
                    for (int j = n; j < datos.Length; j++)
                    {
                        int idruta = j - (n - 1);
                        if (datos[j] != "")
                        {
                            int r = Int32.Parse(datos[j]);
                            if (r > 0)
                            {
                                rutasPertenece.Add(idruta);
                            }
                        }
                    }
                    #endregion


                    //Lo asigno a la ruta
                    foreach (int id in rutasPertenece)
                    {
                        if (idsRutas.Contains(id))
                        {
                            //busco la ruta
                            foreach (Ruta R1 in _rutas)
                            {
                                if (R1.Id == id) //Encontré la ruta
                                {
                                    R1.AgregarArco(A);
                                    break;
                                }
                            }
                        }
                        else
                        { 
                            //No estaba la ruta creada, la creo ahora
                            Ruta R = new Ruta(id);
                            R.AgregarArco(A);
                            idsRutas.Add(id);
                            _rutas.Add(R);
                        }
                    }
                    #endregion
                }
                
                //Calculo el largo de las rutas y las ordeno
                foreach (Ruta R in _rutas)
                {
                    R.CalcularLargo();
                }

                //Creo el grafo
                _grafo = new Grafo(nodos, arcos);

                //Ordeno las rutas por su id
                _rutas.Sort(delegate(Ruta r1, Ruta r2)
                {
                    return r1.Id.CompareTo(r2.Id);
                });
                Utiles.EscribeMensajeConsola("Grafo inicializado", categoriaMensaje, 0, 0);
                #endregion
                #region Inicializo las Rutas
                esRutaDestinoPala = new int[_rutas.Count, _destinos.Count, _palas.Count];
                esRutaPalaDestino = new int[_rutas.Count, _palas.Count, _destinos.Count];
                texto = File.ReadAllLines("Input/Rutas.txt");
                numeroColumnas = 6;
                for (int i = 1; i < texto.Length; i++)  //La primera línea es el encabezado
                {
                    string linea = texto[i];
                    string[] datos = linea.Split(tab);
                    if (datos.Length != numeroColumnas)
                    {
                        throw new Exception("La lectura de la rutas falló");
                    }

                    //Rutas que parten de un destino y van a una Pala
                    int idD1 = Int32.Parse(datos[0]);
                    int idP1 = Int32.Parse(datos[1]);
                    int idNodoD1 = _destinos[idD1 - 1].IdNodo;
                    int idNodoP1 = _palas[idP1 - 1].IdNodo;
                    List<string> idRutas_D1P1 = datos[2].Split(coma).ToList();
                    foreach (string nombreID in idRutas_D1P1)
                    {
                        int idRuta = Int32.Parse(nombreID);
                        esRutaDestinoPala[idRuta - 1, idD1 - 1, idP1 - 1] = 1;
                        _rutas[idRuta - 1].SetOtrosParametros(false, idP1, idD1, idNodoP1, idNodoD1);
                    }

                    //Rutas que parten de una Pala y van a un destino
                    int idP2 = Int32.Parse(datos[3]);
                    int idD2 = Int32.Parse(datos[4]);
                    int idNodoD2 = _destinos[idD2 - 1].IdNodo;
                    int idNodoP2 = _palas[idP2 - 1].IdNodo;
                    List<string> idRutas_P2D2 = datos[5].Split(coma).ToList();
                    foreach (string nombreID in idRutas_P2D2)
                    {
                        int idRuta = Int32.Parse(nombreID);
                        esRutaPalaDestino[idRuta - 1, idP2 - 1, idD2 - 1] = 1;
                        _rutas[idRuta - 1].SetOtrosParametros(true, idP2, idD2, idNodoP2, idNodoD2);
                    }
                }

                IdRutaSegun_HaciaDestino_Pala_Dest = new Dictionary<bool, int[,]>();
                IdRutaSegun_HaciaDestino_Pala_Dest.Add(true, new int[_palas.Count, _destinos.Count]);
                IdRutaSegun_HaciaDestino_Pala_Dest.Add(false, new int[_palas.Count, _destinos.Count]);
                foreach (Ruta R in _rutas)
                {
                    IdRutaSegun_HaciaDestino_Pala_Dest[R.EsRutaHaciaDestino][R.IdPala - 1, R.IdDestino - 1] = R.Id;
                }

                Utiles.EscribeMensajeConsola("Rutas inicializadas", categoriaMensaje, 0, 0);
                #endregion                
                #region Inicializo Datos económicos
                double precioCobre_dolaresPorLibra = 3;
                double kilosEn1Libra = 0.45359237;
                double dolar_pesos = 565;
                double precio_pesosPorKiloCobre = (precioCobre_dolaresPorLibra / kilosEn1Libra) * dolar_pesos;
                double leyMina = 0.007;
                double kilosDeCobreEn1Ton = leyMina * 1000;
                pesosPorTonelada = Math.Truncate(precio_pesosPorKiloCobre * kilosDeCobreEn1Ton);
                toneladasPorMinuto = 0;
                foreach (Requerimiento R in requerimientos)
                {
                    toneladasPorMinuto += R.ToneladasNecesita / (24 * 60);
                }
                Utiles.EscribeMensajeConsola("Datos económicos inicializados", categoriaMensaje, 0, 0);
                Utiles.EscribeMensajeConsola("Valor promedio de la tonelada de tierra: " + Math.Round(pesosPorTonelada,1) + "[$/ton]", categoriaMensaje, 0, 0);
                Utiles.EscribeMensajeConsola("Toneladas promedio que procesa la planta por minuto: " + Math.Round(toneladasPorMinuto,1) + "[ton/min]", categoriaMensaje, 0, 0);
                Utiles.EscribeMensajeConsola("Lucro cesante : " + Math.Round(pesosPorTonelada * toneladasPorMinuto, 0) + "[$/min]", categoriaMensaje, 0, 0);

                #endregion
                #region Inicializo Status camiones
                _statusCamiones = new List<StatusCamion>();
                _statusCamiones.Add(StatusCamion.Vacio);
                _statusCamiones.Add(StatusCamion.Cargado);
                #endregion
            }
            catch (Exception e)
            { throw e; }
            #endregion            

            InicializaDiccionarioDeSecuenciamiento();
            #region Calculo las velocidades máximas para recorrer cada arco
            foreach (Arco A in _grafo.Arcos)
            {
                double v_max = VelocidadMasRapidaParaArco(A);
                A.SetMaxVelocidad_KPH(v_max);
            }
            #endregion            
            #region Con respecto al modelo Escoge Rutas
            #region SETS
            #region Camiones
            List<Indice> camiones = new List<Indice>();
            for (int i = 0; i < _camiones.Count; i++)
            {
                Indice I = (i + 1);
                camiones.Add(I);
            }
            CAMIONES = new Conjunto("CAMIONES", camiones);
            #endregion
            #region Palas
            List<Indice> palillas = new List<Indice>();
            for (int i = 0; i < _palas.Count; i++)
            {
                Indice I = (i + 1);
                palillas.Add(I);
            }
            PALAS = new Conjunto("PALAS", palillas);
            #endregion
            #region Destinos
            List<Indice> dest = new List<Indice>();
            for (int i = 0; i < _destinos.Count; i++)
            {
                Indice I = (i + 1);
                dest.Add(I);
            }
            DESTINOS = new Conjunto("DESTINOS", dest);
            #endregion
            #region Nodos
            List<Indice> nodos1 = new List<Indice>();
            for (int i = 0; i < _grafo.Nodos.Count; i++)
            {
                Indice I = (i + 1);
                nodos1.Add(I);
            }
            NODOS = new Conjunto("NODOS", nodos1);
            #endregion
            #region Status
            List<Indice> status = new List<Indice>();
            for (int i = 0; i < _statusCamiones.Count; i++)
            {
                Indice I = (i + 1);
                status.Add(I);
            }
            STATUS_CAMIONES = new Conjunto("STATUS_CAMIONES", status);
            #endregion
            #region Rutas
            List<Indice> rutas = new List<Indice>();
            for (int i = 0; i < _rutas.Count; i++)
            {
                Indice I = (i + 1);
                rutas.Add(I);
            }
            RUTAS = new Conjunto("RUTAS", rutas);
            #endregion
            #region Requerimientos
            List<Indice> reqs = new List<Indice>();
            for (int i = 0; i < planDia.Requerimientos.Count; i++)
            {
                Indice I = (i + 1);
                reqs.Add(I);
            }
            REQUERIMIENTOS = new Conjunto("REQUERIMIENTOS", reqs);
            #endregion
            #endregion
            #region Datos de camiones, palas y grafo
            existeArco = new int[_grafo.Nodos.Count, _grafo.Nodos.Count];
            esArcoDeEsperaEnCola = new int[_grafo.Nodos.Count, _grafo.Nodos.Count];
            foreach(Arco A in _grafo.Arcos)
            {
                int n1 = A.Id_Nodo_Inicial - 1;
                int n2 = A.Id_Nodo_Final - 1;
                existeArco[n1,n2] = 1;
                esArcoDeEsperaEnCola[n1, n2] = A.EsDeEspera ? 1 : 0;                
            }
            #region Con respecto a PALAS
            nodoPala = new int[_palas.Count];
            nodoInicioCarga = new int[_palas.Count];
            nodoLlegadaColaPala = new int[_palas.Count];
            foreach (Pala P in _palas)
            {
                nodoPala[P.Id - 1] = P.IdNodo;
                int idNodoInicioCarga = -1;
                foreach (Arco A in _grafo.Arcos)
                {
                    if (A.Id_Nodo_Final == P.IdNodo)
                    {
                        nodoInicioCarga[P.Id - 1] = A.Id_Nodo_Inicial;
                        idNodoInicioCarga = A.Id_Nodo_Inicial;
                        break;
                    }
                }
                foreach (Arco A in _grafo.Arcos)
                {
                    if (A.Id_Nodo_Final == idNodoInicioCarga && A.Id_Nodo_Inicial != P.IdNodo)
                    {
                        nodoLlegadaColaPala[P.Id - 1] = A.Id_Nodo_Inicial;
                        break;
                    }
                }
            }
            #endregion
            #region Con respecto a los DESTINOS
            nodoDestino = new int[_destinos.Count];
            nodoInicioDescarga = new int[_destinos.Count];
            nodoLlegadaColaDestino = new int[_destinos.Count];
            foreach (Destino D in _destinos)
            {
                int indexDestino = D.Id - 1;
                int nodo = D.IdNodo;
                nodoDestino[indexDestino] = nodo;
                int idNodoInicioDescarga = -1;
                foreach (Arco A in _grafo.Arcos)
                {
                    if (A.Id_Nodo_Final == D.IdNodo)
                    {
                        idNodoInicioDescarga = A.Id_Nodo_Inicial;
                        nodoInicioDescarga[indexDestino] = idNodoInicioDescarga;
                        break;
                    }
                }
                foreach (Arco A in _grafo.Arcos)
                {
                    if (A.Id_Nodo_Final == idNodoInicioDescarga && D.IdNodo != A.Id_Nodo_Inicial)
                    {
                        int idNodoIniciaCola = A.Id_Nodo_Inicial;
                        nodoLlegadaColaDestino[indexDestino] = idNodoIniciaCola;
                        break;
                    }
                }

            }
            #endregion
            #region Capacidad y tipos de camiones
            capacidadCamion_ton = new double[_tipoCamiones.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                capacidadCamion_ton[TC.Id - 1] = TC.Capacidad_Ton;
            }
            tipoCamion = new int[_camiones.Count];
            foreach (Camion C in _camiones)
            {
                int indexC = C.Id-1;
                tipoCamion[indexC] = C.TipoCamion.Id;
            }
            #endregion
            #region Tiempo de viaje por camión arco status
            tiempoViajeTipoCamionArco_min = new double[_tipoCamiones.Count, _statusCamiones.Count, _grafo.Nodos.Count, _grafo.Nodos.Count];
            foreach (StatusCamion S in _statusCamiones)
            {
                foreach (TipoCamion TC in _tipoCamiones.Values)
                {
                    foreach (Arco A in _grafo.Arcos)
                    {
                        double v_kph = TC.GetVelocidadSegunStatusYPendiente_Kph(S, A.Pendiente);
                        double tiempo_min = 60*A.Largo_KM / v_kph;
                        tiempoViajeTipoCamionArco_min[TC.Id - 1, (int)S - 1, A.Id_Nodo_Inicial - 1, A.Id_Nodo_Final - 1] = tiempo_min;
                    }
                }
            }
            foreach (Ruta R in _rutas)
            {
                R.SetTiemposDeViajePorTipoCamion(tiempoViajeTipoCamionArco_min);
                R.OrdenarArcos();
            }
            #endregion
            #region Tiempos de carga / descarga
            tiempoDescargaTipoCamionDestino_min = new double[_tipoCamiones.Count, _destinos.Count];
            tiempoCargaTipoCamionPala_min = new double[_tipoCamiones.Count, _palas.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                foreach (Destino D in _destinos)
                {
                    double tiempo = TC.TiempoDescargaDestinos_min[D.Id] + TC.TiempoAculatamientoDestinos_min[D.Id];
                    tiempoDescargaTipoCamionDestino_min[TC.Id - 1, D.Id - 1] = tiempo;
                }
                foreach (Pala P in _palas)
                {
                    double tiempo = TC.TiempoCargaPalas_min[P.Id] + TC.TiempoAculatamientoPalas_min[P.Id];
                    tiempoCargaTipoCamionPala_min[TC.Id - 1, P.Id - 1] = tiempo;
                }
            }
            #endregion
            #region Compatibilidad de asignación
            isCamionAsignablePala = new int[_camiones.Count, _palas.Count];
            foreach (Pala P in _palas)
            {
                foreach (Camion C in _camiones)
                {
                    if (P.IdsTipoCamionAcepta.Contains(C.TipoCamion.Id))
                    {
                        isCamionAsignablePala[C.Id - 1, P.Id - 1] = 1;
                    }
                }
            }
            foreach (Camion C in _camiones)
            {
                List<int> idsPalas = new List<int>();
                foreach (Pala P in _palas)
                {
                    if (isCamionAsignablePala[C.Id - 1, P.Id - 1] == 1)
                    {
                        idsPalas.Add(P.Id);
                    }
                }

                List<int> idsDestinos = new List<int>();
                foreach (Destino D in _destinos)
                {
                    foreach (Pala P in _palas)
                    {
                        if (D.IdsPalasAceptadas.Contains(P.Id))
                        {
                            idsDestinos.Add(D.Id);
                            break;
                        }
                    }
                }

                C.SetPalasyDestinosCompatibles(idsPalas, idsDestinos);
            }
            #endregion
            #region Datos del plan día
            isMaterialPalaDescargableEnDestino = new int[_palas.Count, _destinos.Count];
            foreach (Destino D in _destinos)
            {
                foreach (int idPala in D.IdsPalasAceptadas)
                {
                    isMaterialPalaDescargableEnDestino[idPala - 1, D.Id - 1] = 1;
                }
            }
            toneladasTotalesSegunPlanDiario = new double[planDia.Requerimientos.Count];            
            foreach (Requerimiento R in planDia.Requerimientos)
            {
                toneladasTotalesSegunPlanDiario[R.Id-1] = R.ToneladasNecesita;                
            }
            isPalaDestinoEnReq = new int[_palas.Count, _destinos.Count, planDia.Requerimientos.Count];
            foreach (Requerimiento R in planDia.Requerimientos)
            {
                isPalaDestinoEnReq[R.IdPala - 1, R.IdDestino - 1, R.Id - 1] = 1;                
            }

            #endregion
            #region Arcos por ruta
            arcosPorRuta = new int[_grafo.Nodos.Count, _grafo.Nodos.Count, _rutas.Count];
            foreach (Ruta R in _rutas)
            {
                int idR = R.Id;
                foreach (Arco A in R.Arcos)
                {
                    arcosPorRuta[A.Id_Nodo_Inicial - 1, A.Id_Nodo_Final - 1, idR - 1] = 1;
                }
            }
            #endregion
            #region Costo de viaje
            costoViaje_pesos = new double[_tipoCamiones.Count, _statusCamiones.Count, _grafo.Nodos.Count, _grafo.Nodos.Count];
            foreach (StatusCamion S in _statusCamiones)
            {
                foreach (TipoCamion TC in _tipoCamiones.Values)
                {
                    double rendimiento_pesosPorKm = TC.Rendimiento_pesosPorKm[S];
                    foreach (Arco A in _grafo.Arcos)
                    {
                        double km = A.Largo_KM;
                        double costo = rendimiento_pesosPorKm * km;
                        costoViaje_pesos[TC.Id - 1, (int)S - 1, A.Id_Nodo_Inicial - 1, A.Id_Nodo_Final - 1] = costo;
                    }
                }
            }
            #endregion
            #region Costo de la ruta
            costoRutaPorTipoCamionStatus_pesos = new double[_tipoCamiones.Count,_rutas.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                foreach (Ruta R in _rutas)
                {
                    double costoFinal = 0;
                    int S = R.EsRutaHaciaDestino ? 1:0;
                    foreach (Arco A in R.Arcos)
                    {
                        double costoArco = costoViaje_pesos[TC.Id - 1, S, A.Id_Nodo_Inicial - 1, A.Id_Nodo_Final - 1];
                        costoFinal += costoArco;
                    }
                    costoRutaPorTipoCamionStatus_pesos[TC.Id - 1, R.Id - 1] = costoFinal;
                }
            }
            #endregion

            #endregion
            #endregion
            #region Con respecto al modelo LP de DISPATCH
            #region SETS
            List<Indice> tipoCamiones = new List<Indice>();
            for (int i = 0; i < _tipoCamiones.Count; i++)
            {
                Indice I = (i + 1);
                tipoCamiones.Add(I);
            }
            TIPO_CAMIONES = new Conjunto("TIPO_CAMIONES", tipoCamiones);
            // LOS OTROS SETS, PALAS Y DESTINOS, YA SE CREARON ARRIBA
            #endregion
            #region PARÁMETROS
            #region Capacidades de palas, destinos y tipos de camiones
            capacidadPala_tph = new double[_palas.Count];
            for (int i = 0; i < _palas.Count; i++)
            {
                capacidadPala_tph[i] = _palas[i].Capacidad_TPH;
            }
            capacidadDestino_tph = new double[_destinos.Count];
            for (int i = 0; i < _destinos.Count; i++)
            {
                capacidadDestino_tph[i] = _destinos[i].Capacidad_TPH;
            }
            #endregion
            #region Tiempos de carga y descarga según tipo de camión
            tiempoCargaTipoCamionPala_hra = new double[_tipoCamiones.Count, _palas.Count];
            tiempoDescargaTipoCamionDestino_hra = new double[_tipoCamiones.Count, _destinos.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                foreach(Pala P in _palas)
                {
                    int index_pala = P.Id - 1;
                    double tiempoCarga_hra = (TC.TiempoAculatamientoPalas_min[P.Id] + TC.TiempoCargaPalas_min[P.Id]) / 60;
                    tiempoCargaTipoCamionPala_hra[TC.Id-1, index_pala] = tiempoCarga_hra;
                }
                for (int d = 0; d < _destinos.Count; d++)
                {
                    Destino D = _destinos[d];
                    double tiempoDescarga_hra = (TC.TiempoAculatamientoDestinos_min[D.Id] + TC.TiempoDescargaDestinos_min[D.Id]) / 60;
                    tiempoDescargaTipoCamionDestino_hra[TC.Id - 1, d] = tiempoDescarga_hra;
                }
            }


            #endregion
            #region Tiempos de viaje
            tiempoViajePorTipo_PD_h = new double[_tipoCamiones.Count, _palas.Count, _destinos.Count];
            tiempoViajePorTipo_DP_h = new double[_tipoCamiones.Count, _destinos.Count, _palas.Count];
            _RutasDispatch_Cargadas = new List<Ruta_Dispatch>();
            _RutasDispatch_Vacias = new List<Ruta_Dispatch>();
            //Estos tiempos de viaje corresponden al tiempo de viaje a flujo libre de la ruta más corta entre esos pares de nodo
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                foreach (Pala P in _palas)
                {
                    foreach (Destino D in _destinos)
                    {
                        double largoRuta_PalaDestino_km = 99999999;
                        double largoRuta_DestinoPala_km = 99999999;
                        Ruta R_PD = null;
                        Ruta R_DP = null;
                        #region Busco en las rutas que tengo las más cortas
                        foreach (Ruta R in _rutas) //Busco en todas las rutas posibles
                        {
                            if (esRutaPalaDestino[R.Id - 1, P.Id - 1, D.Id - 1] == 1)
                            {
                                if (R.Largo_Km <= largoRuta_PalaDestino_km)
                                {
                                    largoRuta_PalaDestino_km = R.Largo_Km;
                                    R_PD = R;                                    
                                }
                            }
                            if (esRutaDestinoPala[R.Id - 1, D.Id - 1, P.Id - 1] == 1)
                            {
                                if (R.Largo_Km <= largoRuta_DestinoPala_km)
                                {
                                    largoRuta_DestinoPala_km = R.Largo_Km;
                                    R_DP = R;
                                }
                            }
                        }
                        Ruta_Dispatch RDP = new Ruta_Dispatch(R_DP.Id, false, R_DP.IdPala, R_DP.IdDestino, new List<Arco>(R_DP.Arcos));
                        RDP.EsRutaHaciaDestino = false;
                        _RutasDispatch_Vacias.Add(RDP);
                        Ruta_Dispatch RPD = new Ruta_Dispatch(R_PD.Id, true, R_PD.IdPala, R_PD.IdDestino, new List<Arco>(R_PD.Arcos));
                        RPD.EsRutaHaciaDestino = true;
                        _RutasDispatch_Cargadas.Add(RPD);

                        #endregion

                        //Ahora calculo el tiempo en horas que toma recorrerla
                        double tiempo_PD_h = 0;
                        double tiempo_DP_h = 0;
                        foreach (Arco A in R_PD.Arcos) //Pala -> Destino: Cargado
                        {
                            double V_kph = TC.GetVelocidadSegunStatusYPendiente_Kph(StatusCamion.Cargado, A.Pendiente);
                            double tiempo = A.Largo_KM / V_kph;
                            tiempo_PD_h += tiempo;
                        }
                        foreach (Arco A in R_DP.Arcos) //Destino -> Pala: Cargado
                        {
                            double V_kph = TC.GetVelocidadSegunStatusYPendiente_Kph(StatusCamion.Vacio, A.Pendiente);
                            double tiempo = A.Largo_KM / V_kph;
                            tiempo_DP_h += tiempo;
                        }

                        //Asigno
                        tiempoViajePorTipo_PD_h[TC.Id - 1, P.Id - 1, D.Id - 1] = tiempo_PD_h;
                        tiempoViajePorTipo_DP_h[TC.Id - 1, D.Id - 1, P.Id - 1] = tiempo_DP_h;
                    }
                }
            }
            #endregion
            #region Tipos de camiones que acepta la idPala
            isTipoAsignableAPala = new int[_tipoCamiones.Count, _palas.Count];
            foreach (Pala P in _palas)
            {
                foreach (int idTC in P.IdsTipoCamionAcepta)
                {
                    isTipoAsignableAPala[idTC - 1, P.Id - 1] = 1;
                }
            }
            #endregion
            #region Parámetros del Plan diario
            toneladasPorHoraSegunPlanDiario = new double[planDia.Requerimientos.Count];
            foreach(Requerimiento R in planDia.Requerimientos)
            {
                toneladasPorHoraSegunPlanDiario[R.Id - 1] = R.ToneladasNecesita / (Configuracion.TiempoTotalHorizonte/60);
            }
            #endregion
            #endregion
            #endregion            
            #region Tiempos de des/carga mínimos
            foreach (Pala P in _palas)
            {
                double tmin = 99999;
                foreach (TipoCamion k in _tipoCamiones.Values)
                {
                    double tcarga = tiempoCargaTipoCamionPala_min[k.Id - 1, P.Id - 1];
                    if (tcarga < tmin)
                    {
                        tmin = tcarga;
                    }
                }
                P.SetTiempoMasRapidoDeCarga(tmin);
            }
            foreach (Destino D in _destinos)
            {
                double tmin = 99999;
                foreach (TipoCamion k in _tipoCamiones.Values)
                {
                    double tdescarga = tiempoDescargaTipoCamionDestino_min[k.Id - 1, D.Id - 1];
                    if (tdescarga < tmin)
                    {
                        tmin = tdescarga;
                    }
                }
                D.SetTiempoDeDescargaMinimo_min(tmin);
            }
            #endregion
            #region Tiempos de ciclo mínimos y máximos
            #region tiempoDeCicloFlujoLibreMax_min
            tiempoDeCicloFlujoLibreMax_min = new double[_tipoCamiones.Count, _destinos.Count];
            foreach (int k in _tipoCamiones.Keys)
            {
                foreach (Destino D1 in _destinos)
                {
                    double tciclo = 0;
                    foreach (Pala P in _palas)
                    {
                        double tViajeAPala = GetRuta(P.Id, D1.Id, false).GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                        double tCarga = tiempoCargaTipoCamionPala_min[k - 1,P.Id-1];
                        foreach (int idD2 in P.IdsDestinosPosibles)
                        {
                            double tViajeADest = GetRuta(P.Id, idD2, true).GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                            double tDescarga = tiempoDescargaTipoCamionDestino_min[k - 1, idD2 - 1];
                            tciclo = tViajeAPala + tCarga + tViajeADest + tDescarga;
                            tiempoDeCicloFlujoLibreMax_min[k - 1, D1.Id - 1] = Math.Max(tciclo, tiempoDeCicloFlujoLibreMax_min[k - 1, D1.Id - 1]);
                        }                        
                    }
                }
            }
            #endregion
            #region tiempoDeCicloFlujoLibreMin_min
            tiempoDeCicloFlujoLibreMin_min = new double[_tipoCamiones.Count];
            tiempoDeCicloFlujoLibreMinSegunDInicioTipoCam_min = new double[_destinos.Count, _tipoCamiones.Count];
            tiempoDeCicloFlujoLibre_KD0PD = new double[_tipoCamiones.Count, _destinos.Count, _palas.Count, _destinos.Count];
            foreach (int k in _tipoCamiones.Keys)
            {
                double TCicloPorTCMin = 99999999;
                foreach (Destino D1 in _destinos)
                {
                    double tCicloMin = 9999999;
                    // D1 --> P --> D2
                    double tCiclo = 0;
                    foreach (Pala P in _palas)
                    {
                        double tViajeAPala = GetRuta(P.Id, D1.Id, false).GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                        double tCarga = tiempoCargaTipoCamionPala_min[k - 1, P.Id - 1];
                        foreach (int idD2 in P.IdsDestinosPosibles)
                        {
                            double tViajeADest = GetRuta(P.Id, idD2, true).GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                            double tDescarga = tiempoDescargaTipoCamionDestino_min[k - 1, idD2 - 1];
                            tCiclo = tViajeAPala + tCarga + tViajeADest + tDescarga;
                            tCicloMin = Math.Min(tCiclo, tCicloMin);
                            tiempoDeCicloFlujoLibre_KD0PD[k - 1, D1.Id - 1, P.Id - 1, idD2 - 1] = tCiclo;
                        }
                    }
                    tiempoDeCicloFlujoLibreMinSegunDInicioTipoCam_min[D1.Id - 1, k - 1] = tCicloMin;
                    TCicloPorTCMin = Math.Min(TCicloPorTCMin, tCicloMin);
                }
                tiempoDeCicloFlujoLibreMin_min[k - 1] = TCicloPorTCMin;
            }
            #endregion
            #endregion
            #region Capacidad promedio
            capacidadPromedio = 0;
            foreach (Camion C in _camiones)
            {
                capacidadPromedio += C.TipoCamion.Capacidad_Ton;
            }
            capacidadPromedio = capacidadPromedio / _camiones.Count;
            #endregion
            Resolucion();
        }
        private void Resolucion()
        {
            ResultadoCorridas = new List<ResultadoCorrida>();
            AsignacionesSegunMS = new Dictionary<MetodoSolucion, List<Asignacion>>();
            Variable.InicializaVariablesPorMetodo(Configuracion.MetodosDeSolucion);

            //////////////////////////////////////////

            bool AplicarHeuristicas = false;
            bool AplicarProgEntera = true;
            
            /////////////////////////////////////////            

            continuarInstancia = true;
            foreach (MetodoSolucion MS in Configuracion.MetodosDeSolucion)
            {
                if (MS != MetodoSolucion.MIP && AplicarHeuristicas)
                {
                    intervaloTonelaje = 50;
                    Configuracion.MaxDeltaPorcentual_DISPATCH = 1000;
                    AsignacionesSegunMS.Add(MS, new List<Asignacion>());
                    SolucionarDespacho(MS);                    
                }
            }

            //Resuelvo los métodos de escoge rutas
            AsignacionesSegunMS.Add(MetodoSolucion.MIP, new List<Asignacion>());
            
            if (Configuracion.MetodosDeSolucion.Contains(MetodoSolucion.MIP) && AplicarProgEntera)
            {
                for (int h = 4; h <= 4; h++)
                {
                    coefMantenerLey = 1;
                    intervaloTonelaje = 50;
                    Configuracion.multiplicadorPendiente = 1;
                    horizontePlanificacion_min = h;
                    AsignacionesSegunMS[MetodoSolucion.MIP] = new List<Asignacion>();
                    SolucionarDespacho(MetodoSolucion.MIP);

                    if (!continuarInstancia)
                    {
                        break;
                    }
                }
            }
             
            ImprimirArchivoComparativoFinal();            
        }
        /// <summary>
        /// Crea el objeto SecuenciasDeRutasSegunNumCiclos
        /// </summary>
        private void InicializaDiccionarioDeSecuenciamiento()
        {
            SecuenciasDeRutasSegunNumCiclosDInicio = new Dictionary<int, Dictionary<int, List<List<int>>>>();
            
            //Creo el caso de 1 solo ciclo
            Dictionary<int, List<List<int>>> Secuencias_1_Ciclo = new Dictionary<int, List<List<int>>>();
            foreach (Destino D0 in _destinos)
            {
                List<List<int>> SecuenciaRutas = new List<List<int>>();
                foreach (Pala P in _palas)
                {                    
                    int idR1 = IdRutaSegun_HaciaDestino_Pala_Dest[false][P.Id - 1, D0.Id - 1];
                    foreach (int IdD1 in P.IdsDestinosPosibles)
                    {
                        int idR2 = IdRutaSegun_HaciaDestino_Pala_Dest[true][P.Id - 1, IdD1 - 1];
                        List<int> Secuencia = new List<int>();
                        Secuencia.Add(idR1);
                        Secuencia.Add(idR2);
                        SecuenciaRutas.Add(Secuencia);
                    }                    
                }
                Secuencias_1_Ciclo.Add(D0.Id, SecuenciaRutas);
            }
            SecuenciasDeRutasSegunNumCiclosDInicio.Add(1, Secuencias_1_Ciclo);            
        }
        /// <summary>
        /// Genera 1 ciclo más de secuencias posibles
        /// </summary>
        private void AgregarUnCicloDeSecuencias()
        {
            int maxCiclos = SecuenciasDeRutasSegunNumCiclosDInicio.Keys.Max();
            Dictionary<int, List<List<int>>> SecuenciasPorDInicial = new Dictionary<int, List<List<int>>>();         
 
            //A estas secuencias hay que agregarle las posibilidades de 1 ciclo
            foreach (Destino D0 in _destinos)
            {
                List<List<int>> SecuenciasActuales = SecuenciasDeRutasSegunNumCiclosDInicio[maxCiclos][D0.Id];
                List<List<int>> SecuenciasNuevas = new List<List<int>>();
                foreach (List<int> Secuencia in SecuenciasActuales)
                {
                    int IdUltimaRuta = Secuencia[Secuencia.Count - 1];
                    Ruta R = _rutas[IdUltimaRuta - 1];
                    int idDest = R.IdDestino;

                    List<List<int>> SecuenciasPartenDeEsteDestino = SecuenciasDeRutasSegunNumCiclosDInicio[1][idDest];
                    foreach (List<int> SecuenciaDesdeDestinoFinal in SecuenciasPartenDeEsteDestino)
                    {
                        List<int> NuevaSecuencia = new List<int>();
                        foreach (int r1 in Secuencia)
                        {
                            NuevaSecuencia.Add(r1);
                        }
                        foreach (int r2 in SecuenciaDesdeDestinoFinal)
                        {
                            NuevaSecuencia.Add(r2);
                        }
                        SecuenciasNuevas.Add(NuevaSecuencia);
                    }
                }
                SecuenciasPorDInicial.Add(D0.Id, SecuenciasNuevas);
            }
            SecuenciasDeRutasSegunNumCiclosDInicio.Add(maxCiclos + 1, SecuenciasPorDInicial);
        }
        /// <summary>
        /// Imprime un archivo que resume los principales resultados de los métodos utilizados
        /// </summary>
        private void ImprimirArchivoComparativoFinal()
        {
            #region Escritura del resumen del método
            string SL = Environment.NewLine; //Salto de Línea
            string T = "\t";
            string contenido = "";

            contenido += "Método" + T;
            contenido += "Horizonte" + T;
            contenido += "CamionesCiclosAVG" + T;
            contenido += "Coef Ley" + T;
            contenido += "Tiempo requerido [min]" + T;

            contenido += "Intervalo Tonelaje [ton]" + T;
            contenido += "Pesos por ton [$/ton]" + T;
            contenido += "Ton por min [ton/min]" + T;
            contenido += "Costo Total Tpte" + T;
            contenido += "Ton Transportadas" + T;
            contenido += "Costo por ton" + T;
            contenido += "Tiempo Ciclo AVG" + T;
            contenido += "Tiempo Solucion AVG" + T;

            contenido += "Espera en Palas" + T;
            contenido += "Espera en Destinos" + T;
            contenido += "Demoras en arcos" + T;
            contenido += "Retraso total" + T;
            contenido += "Minutos totales" + T;
            contenido += "Retraso [ton*min]" + T;
            contenido += "Max Prod [ton/min]" + T;
            contenido += "Multip Pendiente" + T;
            contenido += "Cumplimiento Ley" + T;
            contenido += "% Alcanza Respuesta" + T;

            foreach(Requerimiento R in planDia.Requerimientos)
            {
                contenido += "Tons CRPD R" + R.Id.ToString() + T;
            }
            contenido += SL;

            foreach (ResultadoCorrida RC in ResultadoCorridas)
            {
                contenido += RC.Metodo + T;
                contenido += RC.Horizonte + T;
                contenido += Math.Round(RC.CamionesCicloAVG,3).ToString().Replace(',', '.') + T;
                contenido += RC.CoeficienteLey + T;
                contenido += RC.TiempoRequerido.ToString().Replace(',', '.') + T;
                
                contenido += RC.IntervaloTonelaje + T;
                contenido += RC.PesosPorTonelada.ToString().Replace(',', '.') + T;
                contenido += RC.ToneladasPorMinuto.ToString().Replace(',', '.') + T;
                contenido += RC.CostoTotalTransporte.ToString().Replace(',', '.') + T;
                contenido += RC.ToneladasTransportadas.ToString().Replace(',', '.') + T;
                contenido += RC.CostoPorTonelada.ToString().Replace(',', '.') + T;
                contenido += RC.TiempoCicloAVG.ToString().Replace(',', '.') + T;
                contenido += RC.TiempoSolucionAVG.ToString().Replace(',', '.') + T;
                contenido += RC.EsperaPalas.ToString().Replace(',', '.') + T;
                contenido += RC.EsperaDestinos.ToString().Replace(',', '.') + T;
                contenido += RC.Demoras.ToString().Replace(',', '.') + T;
                contenido += RC.RetrasoTotal.ToString().Replace(',', '.') + T;
                contenido += RC.MinutosTotales.ToString().Replace(',', '.') + T;
                contenido += RC.Retraso_TonMin.ToString().Replace(',', '.') + T;
                contenido += RC.MaximaProductividadAlcanzada.ToString().Replace(',', '.') + T;
                contenido += RC.MultiplicadorPendiente.ToString().Replace(',', '.') + T;
                contenido += RC.PorcentajeCumplimientoLey.ToString().Replace(',', '.') + T;
                contenido += RC.PorcentajeAlcanzaAResolver.ToString().Replace(',', '.') + T;


                foreach(Requerimiento R in planDia.Requerimientos)
                {
                    contenido += RC.EntregadoCRPD[R.Id-1].ToString().Replace(',', '.') + T;
                }
                contenido += SL;
            }
            #endregion

            string rutaCarpeta = "Resultados";
            string fecha = DateTime.Now.ToString().Replace(":", ".");
            string nombreArchivo = "DeltaMetodos " + fecha + ".txt";
            Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta,nombreArchivo,contenido);

            #region Escritura de las leyes
            contenido = "";
            contenido += "T1" + T;
            contenido += "T2" + T;
            foreach (ResultadoCorrida RC in ResultadoCorridas)
            {
                string Nombre = RC.IdentificadorMetodo;
                foreach (Destino D in _destinos)
                {
                    string ND = "Ley_"+Nombre + "-D" + D.Id;
                    contenido += ND + T;
                }
            }
            contenido += SL;

            //Lleno la info
            for(int k=0; k<ResultadoCorridas[0].IntervalosLeyes.Count; k++)
            {
                contenido += ResultadoCorridas[0].IntervalosLeyes[k].T1_min + T;
                contenido += ResultadoCorridas[0].IntervalosLeyes[k].T2_min + T;
                foreach (ResultadoCorrida RC in ResultadoCorridas)
                {
                    IntervaloLey IL = RC.IntervalosLeyes[k];
                    foreach (int idD in IL.LeyEfectivaPorDestino.Keys)
                    {
                        string leyEf = IL.LeyEfectivaPorDestino[idD].ToString().Replace(',', '.');
                        contenido += leyEf + T;
                    }
                }
                contenido += SL;
            }

            rutaCarpeta = "Resultados";
            fecha = DateTime.Now.ToString().Replace(":", ".");
            nombreArchivo = "DeltaMetodos_Ley " + fecha + ".txt";
            Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta, nombreArchivo, contenido);

            #endregion


        }
        /// <summary>
        /// Escribe en consola los camiones considerados
        /// </summary>
        private void DisplayCamionesConsiderados()
        {
            Utiles.EscribeMensajeConsola("Camión\tHora Inicial\t# Ciclos", CategoriaMensaje.Categoria_3, 0, 0);
            foreach (int id in idsCamionesConsiderados)
            {
                Utiles.EscribeMensajeConsola(id.ToString() + "\t" + 
                    Utiles.FormatHora(_camiones[id - 1].InstanteSiguienteDisponible_Minutos)+ "\t   "+
                    numCiclosPorCamion[id-1], CategoriaMensaje.Categoria_3, 0, 0);
            }
        }        
        /// <summary>
        /// Resuelve la fase de LP de Dispatch
        /// </summary>
        /// <returns></returns>
        private void SolucionarFaseLP_Dispatch()
        {
            #region Inicialización
            Resultado _ResultadoDispatchLP = null;
            string ruta_dat = GenerarDatFaseLPDispatch();
            string ruta_mod = Configuracion.GetRutaModelo(MetodoSolucion.DISPATCH, 1);
            Utiles.EscribeMensajeConsola("RESOLUCIÓN DE LA FASE DE PROGRAMACIÓN LINEAL", CategoriaMensaje.Categoria_1, 1, 0);
            #endregion

            #region Resolución
            SolverFrontend solver = new SolverFrontend("SolverBackend_Cplex.dll");
            ProblemaSolver problema = new ProblemaSolver(ruta_mod, ruta_dat);
            
            //solver.AsignaParametro("EpGap", Configuracion.CPLEX_GapRelativo);
            //solver.AsignaParametro("EpGap", 0.00001);
            //solver.AsignaParametro("Threads", Configuracion.CPLEX_NumeroThreads);
            //solver.AsignaParametro("ParallelMode", (int)ParamCplex_ParallelMode._1_Deterministic); //Deterministico para que pueda debugear
            //solver.AsignaParametro("MIPDisplay", (int)ParamCPLEX_MIPDisplay._3_SameAs2_WithInfoAboutNodeCuts);
            //solver.EscribeParametros("parametrosSolver.txt");
            DateTime T1 = DateTime.Now;
            ResultadoSolver resultadoSolver = solver.ResolverProblema(problema);      //Intento resolver el problema
            if (resultadoSolver == ResultadoSolver.Optimo)
            {
                // Problema resuelto
            }
            else
            {
                throw new Exception("LP no se pudo resolver");
            }

            //Si el problema está resuelto:
            double tiempoResolucion_s = Math.Round((DateTime.Now - T1).TotalSeconds, 2);
            Utiles.EscribeMensajeConsola("Problema resuelto en " + tiempoResolucion_s + " segundos.", CategoriaMensaje.Categoria_1, 0, 0);

            #region Variables del modelo
            List<NombreVariablesIndexadas> VIndex = new List<NombreVariablesIndexadas>();
            VIndex.Add(NombreVariablesIndexadas.TasaExcavacion_tph);
            VIndex.Add(NombreVariablesIndexadas.FlujoToneladas_PD_tph);
            VIndex.Add(NombreVariablesIndexadas.PenalidadFlujoFaltante);
            
            List<NombreVariablesEscalares> VEsc = new List<NombreVariablesEscalares>();
            VEsc.Add(NombreVariablesEscalares.FO_Productividad);
            VEsc.Add(NombreVariablesEscalares.FO_PenalidadFlujoTonFaltante);
            VEsc.Add(NombreVariablesEscalares.FO_PenalidadNoUtilizacionPalas);
            VEsc.Add(NombreVariablesEscalares.NumCamionesUsados);

            #endregion
            _ResultadoDispatchLP = new Resultado(problema,tiempoResolucion_s, VIndex, VEsc);
            
            #endregion

            #region Muestro y guardo la solución
            #region FOB
            Utiles.EscribeMensajeConsola("RESULTADO:", CategoriaMensaje.Categoria_2, 1, 0);
            Utiles.EscribeMensajeConsola("FO_Productividad:\t\t" + Math.Round(_ResultadoDispatchLP.GetValorVariableEscalar(NombreVariablesEscalares.FO_Productividad), 2), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FO_PenalidadFlujoTonFaltante:\t\t\t" + Math.Round(_ResultadoDispatchLP.GetValorVariableEscalar(NombreVariablesEscalares.FO_PenalidadFlujoTonFaltante), 2), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FO_PenalidadNoUtilizacionPalas:\t\t\t" + Math.Round(_ResultadoDispatchLP.GetValorVariableEscalar(NombreVariablesEscalares.FO_PenalidadNoUtilizacionPalas), 2), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FOB:\t\t\t\t\t\t" + Math.Round(problema.Solucion.ValorObjetivo, 2), CategoriaMensaje.Categoria_4, 0, 0);
            #endregion
            #region Número de camiones usados
            _totalDeCamionesRequeridos = _ResultadoDispatchLP.GetValorVariableEscalar(NombreVariablesEscalares.NumCamionesUsados);
            Utiles.EscribeMensajeConsola("# Camiones usados:\t\t\t\t\t\t" + Math.Round(_totalDeCamionesRequeridos, 2), CategoriaMensaje.Categoria_4, 0, 0);
            #endregion
            #region Muestro las tasas de excavacion
            Utiles.EscribeMensajeConsola("Tasas de excavación:", CategoriaMensaje.Categoria_2, 1, 0);
            Utiles.EscribeMensajeConsola("Pala\tTasa[ton/hra]", CategoriaMensaje.Categoria_3, 0, 0);
            Dictionary<Tupla, double> TasaExcavacion_tph = _ResultadoDispatchLP.GetValoresVariableIndexada(NombreVariablesIndexadas.TasaExcavacion_tph, TIPO_VARIABLE.DECIMAL);
            foreach (Tupla T in TasaExcavacion_tph.Keys)
            {
                int idPala = (int)T[0];
                double valor = Math.Round(TasaExcavacion_tph[T], 2);
                _tasaDeExcavacionTotal += valor;
                _palas[idPala - 1].SetResultadoDispatchLP(TasaExcavacion_tph[T]);
                if (TasaExcavacion_tph[T] > 0)
                {
                    Utiles.EscribeMensajeConsola(idPala + "\t" + valor, CategoriaMensaje.Categoria_4, 0, 0);
                }
            }
            #endregion
            #region FlujoToneladas_PD_tph
            Utiles.EscribeMensajeConsola("Flujo de Toneladas [tph]:", CategoriaMensaje.Categoria_2, 1, 0);
            Utiles.EscribeMensajeConsola("Pala\tDestino\tFlujo[ton/hra]", CategoriaMensaje.Categoria_3, 0, 0);
            Dictionary<Tupla, double> FlujoToneladas_PD_tph = _ResultadoDispatchLP.GetValoresVariableIndexada(NombreVariablesIndexadas.FlujoToneladas_PD_tph, TIPO_VARIABLE.DECIMAL);
            foreach (Tupla T in FlujoToneladas_PD_tph.Keys)
            {
                int pala = (int)T[0];
                int dest = (int)T[1];
                double flujo = Math.Round(FlujoToneladas_PD_tph[T], 2);
                if (FlujoToneladas_PD_tph[T] > 0)
                {
                    Utiles.EscribeMensajeConsola(pala + "\t" + dest + "\t" + flujo, CategoriaMensaje.Categoria_4, 0, 0);
                    //Agrego esta info a la ruta DISPATCH asociada
                    foreach (Ruta_Dispatch RD in _RutasDispatch_Cargadas)
                    {
                        if (RD.IdPala == pala && RD.IdDestino == dest)
                        {
                            RD.AgregarFlujoTPH(FlujoToneladas_PD_tph[T]);
                            break;
                        }
                    }
                }
            }

            //Remuevo las rutas no usadas
            List<Ruta_Dispatch> Usadas = new List<Ruta_Dispatch>();
            foreach (Ruta_Dispatch RDD in _RutasDispatch_Cargadas)
            {
                if (RDD.FlujoTotal_tph > 0)
                {
                    Usadas.Add(RDD);
                }
            }
            _RutasDispatch_Cargadas = Usadas;
            #endregion            
            #endregion
        }
        /// <summary>
        /// Resuelve el problema
        /// </summary>
        /// <param name="MS">El método de solución que se debe usar</param>
        private void SolucionarDespacho(MetodoSolucion MS)
        {
            #region Inicialización
            Reset(MS);
            Utiles.EscribeMensajeConsola("#-------------------------------------------#", CategoriaMensaje.Categoria_1, 1, 0);
            Utiles.EscribeMensajeConsola("#                                           #", CategoriaMensaje.Categoria_1, 0, 0);
            Utiles.EscribeMensajeConsola("#  COMIENZO RESOLUCIÓN: " + Configuracion.NombreModelo[MS] + "  #", CategoriaMensaje.Categoria_1, 0, 0);
            Utiles.EscribeMensajeConsola("#                                           #", CategoriaMensaje.Categoria_1, 0, 0);
            Utiles.EscribeMensajeConsola("#-------------------------------------------#", CategoriaMensaje.Categoria_1, 0, 0);
            string ruta_dat = "";
            string ruta_mod = Configuracion.GetRutaModelo(MS, 2);
            DateTime TNow = DateTime.Now;
            #endregion

            if (MS == MetodoSolucion.DISPATCH) //Resuelvo fase de LP de DISPATCH
            {
                SolucionarFaseLP_Dispatch();
            }            
            idCamionSolicitaDespacho = idsCamionesConsiderados[0];
            
            while (instanteActual_min < Configuracion.TiempoTotalHorizonte && continuarInstancia)
            {
                //Genero el .dat
                DesplegarInfoDeLaInstancia(numProblema); 
                ruta_dat = GenerarDatDespacho(MS);
                
                //Obtengo el resultado
                Resultado resultado = ResolverDespacho(ruta_dat, ruta_mod, MS);
                ProcesarResultado(resultado, MS);
                numProblema++;
                Utiles.EscribeMensajeConsola("----------------------------------------------------------------------------------------------", CategoriaMensaje.Categoria_4, 1, 0);
            }

            double TotalMinutesCorrida = Math.Round((DateTime.Now - TNow).TotalMinutes, 2);
            GenerarArchivoRespuesta(MS, TotalMinutesCorrida);
        }
        private void DesplegarInfoDeLaInstancia(int numProblema)
        {
            Utiles.EscribeMensajeConsola("Despacho #" + numProblema.ToString() + ": Camión #" + idCamionSolicitaDespacho + " solicita despacho a las " + Utiles.FormatHora(instanteActual_min), CategoriaMensaje.Categoria_1, 1, 0);
            Utiles.EscribeMensajeConsola("Fin horizonte: " + Utiles.FormatHora(instanteActual_min + horizontePlanificacion_min), CategoriaMensaje.Categoria_1, 0, 0);

            if (Configuracion.ImprimirCosasInnecesarias)
            {
                double porcentajeCumplido = Math.Round(100 * planDia.PorcentajeCumplido, 3);
                string porcentajeCumplido_s = Math.Round(100 * planDia.PorcentajeCumplido, 3).ToString() + "%";
                Utiles.EscribeMensajeConsola("Porcentaje cumplimiento: " + porcentajeCumplido_s, CategoriaMensaje.Categoria_1, 0, 0);
                double numBarras = 100;
                double numX = Math.Floor(porcentajeCumplido / (100 / numBarras));
                string barra = "[";
                for (int i = 0; i < numX; i++)
                {
                    barra += "x";
                }
                for (int j = (int)numX; j < numBarras; j++)
                {
                    barra += "_";
                }
                barra += "]";
                Utiles.EscribeMensajeConsola(barra, CategoriaMensaje.Categoria_1, 0, 1);
            }
            DisplayCamionesConsiderados();    
        }
        /// <summary>
        /// Genera los archivos de respuesta de la corrida
        /// </summary>
        /// <param name="MS"></param>
        private void GenerarArchivoRespuesta(MetodoSolucion MS, double MinutosTotalesCorrida)
        {
            string SL = Environment.NewLine; //Salto de Línea
            string T = "\t";
            string contenido = "";
            
            //Datos para el resultado de la corrida completa
            List<DoubleDouble> TiempoDescarga_TonDescargadas = new List<DoubleDouble>();
            ResultadoCorrida RC = new ResultadoCorrida(planDia.Requerimientos.Count);
            RC.CoeficienteLey = coefMantenerLey;
            RC.IdentificadorMetodo = GetNombreIdentificador(MS);
            RC.MultiplicadorPendiente = Configuracion.multiplicadorPendiente;
            RC.Metodo = MS;
            RC.Horizonte = horizontePlanificacion_min;
            RC.IntervaloTonelaje = intervaloTonelaje;
            RC.PesosPorTonelada = pesosPorTonelada;
            RC.ToneladasPorMinuto = toneladasPorMinuto;
            RC.MinutosTotales = MinutosTotalesCorrida;
            double costoTotalTransporte = 0;
            double transporteTotalToneladas = 0;
            double totalTiemposCiclo = 0;
            double totalTiempoSolucion = 0;
            double camionesCiclo = 0;
            double EsperaPalas = 0;
            double EsperaDestinos = 0;
            double Demoras = 0;
            double totalRetraso_tonMin = 0;
            foreach(Requerimiento R in planDia.Requerimientos)
			{
                RC.EntregadoCRPD[R.Id - 1] = -toneladasTotalesSegunPlanDiario[R.Id - 1];
			} 
            

            contenido += "Tiempo [s]" + T;
            contenido += "Tiempo de ciclo [min]" + T;
            contenido += "Num CamionesCiclo" + T;
            contenido += "Destino Inicial" + T;
            contenido += "Pala" + T;
            contenido += "Destino" + T;
            contenido += "Camión" + T;
            contenido += "Instante [min]" + T;
            contenido += "Tolva [ton]" + T;
            contenido += "Costo Transporte [$]" + T;
            contenido += "Espera Pala [min]" + T;
            contenido += "Espera Destino [min]" + T;
            contenido += "Demora [min]" + T;
            contenido += "Alcanza a responder" + T;
            foreach (Pala P in _palas)
            {
                contenido += "Tiempo carga por P" + P.Id + T;
            }
            foreach (Requerimiento R in planDia.Requerimientos)
            {
                contenido += "Tons recibidas por Req" + R.Id + T;
            }

            contenido += SL;

            double numAlcanzaResponder = 0;
            foreach (Asignacion A in AsignacionesSegunMS[MS])
            {
                //Para RC:
                DoubleDouble DD = new DoubleDouble(A.InstanteDescarga, A.TolvaCamion);
                TiempoDescarga_TonDescargadas.Add(DD);

                costoTotalTransporte += A.CostoTransporte;
                transporteTotalToneladas += A.TolvaCamion;
                totalTiemposCiclo += A.TiempoDeCiclo_min;
                totalTiempoSolucion += A.TiempoResolucion_s;
                camionesCiclo += A.NumCamionCiclo;
                EsperaPalas += A.EsperaEnPala_min;
                EsperaDestinos += A.EsperaEnDestino_min;
                Demoras += A.Demora_min;
                
                double retrasoTonMin = (A.EsperaEnPala_min + A.EsperaEnDestino_min + A.Demora_min) * A.TolvaCamion;
                totalRetraso_tonMin += retrasoTonMin;

                contenido += A.TiempoResolucion_s.ToString().Replace(',', '.') + T;
                contenido += A.TiempoDeCiclo_min.ToString().Replace(',', '.') + T;
                contenido += A.NumCamionCiclo + T;
                contenido += A.IdDestinoInicial + T;
                contenido += A.IdPalaAsignada + T;
                contenido += A.IdDestinoAsignado + T;
                contenido += A.IdCamion + T;
                contenido += A.Instante_min.ToString().Replace(',', '.') + T;
                contenido += A.TolvaCamion.ToString().Replace(',', '.') + T;
                contenido += A.CostoTransporte.ToString().Replace(',', '.') + T;
                contenido += A.EsperaEnPala_min.ToString().Replace(',', '.') + T;
                contenido += A.EsperaEnDestino_min.ToString().Replace(',', '.') + T;
                contenido += A.Demora_min.ToString().Replace(',', '.') + T;
                contenido += A.AlcanzaAResponder.ToString().Replace(',', '.') + T;
                numAlcanzaResponder += A.AlcanzaAResponder;
                foreach (Pala P in _palas)
                {
                    if (P.Id == A.IdPalaAsignada)
                    {
                        contenido += A.TiempoCargaEnPala_min.ToString().Replace(',', '.') + T;
                    }
                    else
                    {
                        contenido += 0 + T;
                    }                    
                }
                foreach (Requerimiento R in planDia.Requerimientos)
                {
                    if (R.IdDestino == A.IdDestinoAsignado && R.IdPala == A.IdPalaAsignada)
                    {
                        contenido += A.TolvaCamion.ToString().Replace(',', '.') + T;
                        RC.EntregadoCRPD[R.Id - 1] += A.TolvaCamion;
                    }
                    else
                    {
                        contenido += 0 + T;
                    }
                }
                contenido += SL;
            }
            
            string rutaCarpeta = "Resultados";
            string nombreArchivo = GetNombreIdentificador(MS) + ".txt";
            Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta, nombreArchivo, contenido);


            //Actualizo las cosas para RC
            double n = AsignacionesSegunMS[MS].Count;
            RC.PorcentajeAlcanzaAResolver = numAlcanzaResponder / n;
            RC.CamionesCicloAVG = camionesCiclo / n;
            RC.CostoTotalTransporte = costoTotalTransporte;
            RC.ToneladasTransportadas = transporteTotalToneladas;
            RC.CostoPorTonelada = costoTotalTransporte / transporteTotalToneladas;
            RC.TiempoCicloAVG = totalTiemposCiclo / n;
            RC.TiempoSolucionAVG = totalTiempoSolucion / n;
            RC.EsperaDestinos += EsperaDestinos;
            RC.EsperaPalas += EsperaPalas;
            RC.Demoras += Demoras;
            RC.RetrasoTotal += EsperaDestinos + EsperaPalas + Demoras;
            RC.Retraso_TonMin = totalRetraso_tonMin;

            double tiempoTotal = 0;
            foreach (Requerimiento R in planDia.Requerimientos)
            {
                tiempoTotal = Math.Max(tiempoTotal, R.UltimoInstanteDescarga);
            }
            RC.TiempoRequerido = tiempoTotal;

            
            //Obtengo la productividad máxima
            TiempoDescarga_TonDescargadas.Sort(delegate(DoubleDouble dd1, DoubleDouble dd2)
            {
                return dd1.Double1.CompareTo(dd2.Double1);
            });
            List<double> ProductividadAcum = new List<double>();
            double tonAcum = 0;
            double ProdMax = -1;
            foreach (DoubleDouble DD in TiempoDescarga_TonDescargadas)
            {
                tonAcum += DD.Double2;
                double ProdAcum = tonAcum / DD.Double1;
                ProductividadAcum.Add(ProdAcum);
                ProdMax = Math.Max(ProdMax, ProdAcum);
            }
            RC.MaximaProductividadAlcanzada = ProdMax;

            #region Veo el cumplimiento de la ley en intervalos de tiempo
            List<IntervaloLey> Intervalos = new List<IntervaloLey>();
            #region Genero los intervalos
            for (double t1 = 0; t1 <= Configuracion.TiempoTotalHorizonte - Configuracion.IntervaloTiempoAnalisisLey; t1 += Configuracion.IntervaloTiempoAnalisisLey)
            {
                double t2 = t1 + Configuracion.IntervaloTiempoAnalisisLey;
                IntervaloLey IL = new IntervaloLey(t1, t2, _palas.Count, _destinos.Count);
                Intervalos.Add(IL);
            }
            #endregion
            
            //Ahora ordeno las asignaciones por tiempo ascendente de descarga
            List<Asignacion> Asignaciones = AsignacionesSegunMS[MS];
            Asignaciones.Sort(delegate(Asignacion a1, Asignacion a2)
            {
                return a1.InstanteDescarga.CompareTo(a2.InstanteDescarga);
            });

            //Ahora veo las asignaciones y las voy asignando a los intervalos correspondientes
            int r = 0;
            foreach (Asignacion A in Asignaciones)
            { 
                //Busco el intervalo
                while(!Intervalos[r].Contiene(A.InstanteDescarga)) //Si no estoy dentro del intervalo
                {
                    r++; //debo avanzar al siguiente
                    if (r >= Intervalos.Count)
                    {
                        break;
                    }
                }
                if (r >= Intervalos.Count)
                {
                    break;
                }

                //Este es el intervalo
                IntervaloLey IL = Intervalos[r];
                IL.AgregarTonelaje(A.IdDestinoAsignado, A.IdPalaAsignada, A.TolvaCamion);
            }

            //Calculo la ley efectiva y obtengo el porcentaje de cumplimiento
            double sumPorcentajeCumplimiento = 0;            
            for (int z=0; z<Intervalos.Count; z++)
            {
                IntervaloLey il = Intervalos[z];
                il.CalcularLeyEfectiva(_palas);
                if (z > 0)
                {
                    sumPorcentajeCumplimiento += il.SumPorcentajesCumplimiento(_destinos);                    
                }
            }
            //No considero el primer intervalo pq se está en estado transiente
            int numDestinosQueTienenLeyEntrada = 0;
            foreach (Destino D in _destinos)
            {
                if (D.TipoDestino != TipoDestino.Botadero)
                {
                    numDestinosQueTienenLeyEntrada++;
                }
            }

            double porcentajeCumplimiento = sumPorcentajeCumplimiento / ((Intervalos.Count - 1) * numDestinosQueTienenLeyEntrada);
            RC.PorcentajeCumplimientoLey = porcentajeCumplimiento;
            RC.IntervalosLeyes = Intervalos;
            #endregion

            ResultadoCorridas.Add(RC);
        }
        /// <summary>
        /// Genera un nombre para este método y número de camiones considerados para guardar las instancias de corridas (los .dat)
        /// </summary>
        /// <param name="MS"></param>
        /// <returns></returns>
        private string GetNombreIdentificador(MetodoSolucion MS)
        {
            string nombre = MS.ToString();
            if (MS == MetodoSolucion.DISPATCH)
            {
                nombre += "_" + Configuracion.MaxDeltaPorcentual_DISPATCH;
            }
            if (MS == MetodoSolucion.MIP)
            {
                nombre += "_H" + horizontePlanificacion_min + "_T" + intervaloTonelaje + "_L"+coefMantenerLey.ToString().Replace(',', '.');
            }
            return nombre;
        }
        /// <summary>
        /// Procesa el resultado. Lee las variables, las guarda, las imprime y actualiza los parámetros.
        /// </summary>
        /// <param name="R">El objeto resultado</param>
        /// <param name="MS">El método de solución actual</param>
        private void ProcesarResultado(Resultado R, MetodoSolucion MS)
        {
            #region VARIABLES USADAS:
            Dictionary<Tupla, double> AsignaCamionPalaDestino = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionPalaDestino, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> InstanteSalidaDestino = R.GetValoresVariableIndexada(NombreVariablesIndexadas.InstanteSalidaDestino, TIPO_VARIABLE.DECIMAL);
            Dictionary<Tupla, double> AsignaCamionStatusArco = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionStatusArco, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> AsignaCamionTWPala = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionTWPala, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> AsignaCamionTWDestino = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionTWDestino, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> AsignaCamionAntesTwArco = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionAntesTwArco, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> AsignaCamionDespuesTwArco = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionDespuesTwArco, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> InstanteCamionStatusNodo = R.GetValoresVariableIndexada(NombreVariablesIndexadas.InstanteCamionStatusNodo, TIPO_VARIABLE.DECIMAL);
            Dictionary<Tupla, double> DemoraEnArco_Minutos = R.GetValoresVariableIndexada(NombreVariablesIndexadas.DemoraEnArco_Minutos, TIPO_VARIABLE.DECIMAL);
            Dictionary<Tupla, double> EsperaEnColaPala_Minutos = R.GetValoresVariableIndexada(NombreVariablesIndexadas.EsperaEnColaPala_Minutos, TIPO_VARIABLE.DECIMAL);
            Dictionary<Tupla, double> AsignaCamionStatusRuta = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionStatusRuta, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> EsperaEnColaDestino_Minutos = R.GetValoresVariableIndexada(NombreVariablesIndexadas.EsperaEnColaDestino_Minutos, TIPO_VARIABLE.DECIMAL);
            Dictionary<Tupla, double> CamionCargaAntesEnPala = R.GetValoresVariableIndexada(NombreVariablesIndexadas.CamionCargaAntesEnPala, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> CamionDescargaAntesEnDestino = R.GetValoresVariableIndexada(NombreVariablesIndexadas.CamionDescargaAntesEnDestino, TIPO_VARIABLE.ENTERA);
            Dictionary<Tupla, double> AsignaCamionAntesArco = R.GetValoresVariableIndexada(NombreVariablesIndexadas.AsignaCamionAntesArco, TIPO_VARIABLE.ENTERA);   
            Dictionary<Tupla, double> TiempoDeCiclo = R.GetValoresVariableIndexada(NombreVariablesIndexadas.TiempoDeCiclo, TIPO_VARIABLE.DECIMAL);
            #endregion

            #region Escribo los valores de las variables en un archivo
            if (Configuracion.EscribirRespuestasDeInstancias)
            {
                
                string info = "";
                string nl = Environment.NewLine;
                string tab = "\t";
                List<int> IdsNodosUsados = new List<int>();                
                #region Escribo AsignaCamionPalaDestino
                Dictionary<int, int[]> camionCicloDestinoAsignado = new Dictionary<int, int[]>();
                Dictionary<int, int[]> camionCicloPalaAsignado = new Dictionary<int, int[]>();
                foreach (Camion C in _camiones)
                { 
                    camionCicloDestinoAsignado.Add(C.Id, new int[numCiclosPorCamion[C.Id-1]]);
                    camionCicloPalaAsignado.Add(C.Id, new int[numCiclosPorCamion[C.Id - 1]]);
                }
                info += "AsignaCamionPalaDestino[i,j,d,c]" + nl;
                foreach (Tupla t in AsignaCamionPalaDestino.Keys)
                {
                    double valor = AsignaCamionPalaDestino[t];
                    if (valor == 1)
                    {
                        int i = (int)t[0];
                        int j = (int)t[1];
                        int d = (int)t[2];
                        int c = (int)t[3];
                        camionCicloPalaAsignado[i][c - 1] = j;
                        camionCicloDestinoAsignado[i][c-1] = d;
                        for (int k = 0; k < t.Rango; k++)
                        {
                            info += t[k] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }
                #endregion
                #region Escribo AsignaCamionStatusRuta
                info += "AsignaCamionStatusRuta[i,c,s,r]" + nl;
                if (AsignaCamionStatusRuta != null)
                {
                    foreach (Tupla t in AsignaCamionStatusRuta.Keys)
                    {
                        double valor = AsignaCamionStatusRuta[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo AsignaCamionStatusArco
                info += "AsignaCamionStatusArco[i,c,s,u,v]" + nl;
                if (AsignaCamionStatusArco != null)
                {
                    foreach (Tupla t in AsignaCamionStatusArco.Keys)
                    {
                        double valor = AsignaCamionStatusArco[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;

                            int nodo1 = (int)t[2];
                            int nodo2 = (int)t[3];
                            if (!IdsNodosUsados.Contains(nodo1))
                            {
                                IdsNodosUsados.Add(nodo1);
                            }
                            if (!IdsNodosUsados.Contains(nodo2))
                            {
                                IdsNodosUsados.Add(nodo2);
                            }
                        }
                    }
                }
                #endregion
                #region Escribo AsignaCamionAntesArco
                if (AsignaCamionAntesArco != null)
                {
                    info += "AsignaCamionAntesArco[i1,c1,s1,i2,c2,s2,u,v]" + nl;
                    foreach (Tupla t in AsignaCamionAntesArco.Keys)
                    {
                        double valor = AsignaCamionAntesArco[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo AsignaCamionAntesTwArco
                info += "AsignaCamionAntesTwArco[i,c,s,u,v,tw]" + nl;
                if (AsignaCamionAntesTwArco != null)
                {
                    foreach (Tupla t in AsignaCamionAntesTwArco.Keys)
                    {
                        double valor = AsignaCamionAntesTwArco[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo AsignaCamionDespuesTwArco
                info += "AsignaCamionDespuesTwArco[i,c,s,u,v,tw]" + nl;
                if (AsignaCamionDespuesTwArco != null)
                {
                    foreach (Tupla t in AsignaCamionDespuesTwArco.Keys)
                    {
                        double valor = AsignaCamionDespuesTwArco[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo CamionCargaAntesEnPala
                info += "CamionCargaAntesEnPala[i1,c1,i2,c2,j]" + nl;
                if (CamionCargaAntesEnPala != null)
                {
                    foreach (Tupla t in CamionCargaAntesEnPala.Keys)
                    {
                        double valor = CamionCargaAntesEnPala[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo CamionCargaAntesDestino
                info += "CamionDescargaAntesEnDestino[i1,c1,i2,c2,d]" + nl;
                if (CamionDescargaAntesEnDestino != null)
                {
                    foreach (Tupla t in CamionDescargaAntesEnDestino.Keys)
                    {
                        double valor = CamionDescargaAntesEnDestino[t];
                        if (valor == 1)
                        {
                            for (int i = 0; i < t.Rango; i++)
                            {
                                info += t[i] + tab;
                            }
                            info += ":" + tab + valor + nl;
                        }
                    }
                }
                #endregion
                #region Escribo InstanteCamionStatusNodo
                info += "InstanteCamionStatusNodo[i,c,s,u]" + nl;
                foreach (Tupla t in InstanteCamionStatusNodo.Keys)
                {
                    int nodo = (int)t[3];
                    if (IdsNodosUsados.Contains(nodo))
                    {
                        string valor = InstanteCamionStatusNodo[t].ToString().Replace(',', '.');
                        for (int i = 0; i < t.Rango; i++)
                        {
                            info += t[i] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }

                #endregion
                #region Escribo DemoraEnArco_Minutos
                info += "DemoraEnArco_Minutos[i,c,s,u,v]" + nl;
                foreach (Tupla t in DemoraEnArco_Minutos.Keys)
                {
                    double valor = DemoraEnArco_Minutos[t];
                    if (valor > 0)
                    {
                        for (int i = 0; i < t.Rango; i++)
                        {
                            info += t[i] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }

                #endregion
                #region Escribo InstanteSalidaDestino
                info += "InstanteSalidaDestino[i,c,d]" + nl;
                foreach (Tupla t in InstanteSalidaDestino.Keys)
                {
                    int idC = (int)t[0];
                    int idD = (int)t[2];
                    int c = (int)t[1];
                    if (camionCicloDestinoAsignado.ContainsKey(idC) && camionCicloDestinoAsignado[idC][c-1] == idD)
                    {
                        double valor = InstanteSalidaDestino[t];
                        for (int i = 0; i < t.Rango; i++)
                        {
                            info += t[i] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }

                #endregion
                #region Escribo EsperaEnColaPala_Minutos
                info += "EsperaEnColaPala_Minutos[i,c,j]" + nl;
                foreach (Tupla t in EsperaEnColaPala_Minutos.Keys)
                {
                    int idC = (int)t[0];
                    int idP = (int)t[2];
                    int c = (int)t[1];
                    double valor = EsperaEnColaPala_Minutos[t];
                    if (valor > 0 && camionCicloPalaAsignado.ContainsKey(idC) && camionCicloPalaAsignado[idC][c-1] == idP)
                    {
                        for (int i = 0; i < t.Rango; i++)
                        {
                            info += t[i] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }

                #endregion
                #region Escribo EsperaEnColaDestino_Minutos
                info += "EsperaEnColaDestino_Minutos[i,c,d]" + nl;
                foreach (Tupla t in EsperaEnColaDestino_Minutos.Keys)
                {
                    int idC = (int)t[0];
                    int idD = (int)t[2];
                    int c = (int)t[1];
                    double valor = EsperaEnColaDestino_Minutos[t];
                    if (valor > 0 && camionCicloDestinoAsignado.ContainsKey(idC) && camionCicloDestinoAsignado[idC][c-1] == idD)
                    {
                        for (int i = 0; i < t.Rango; i++)
                        {
                            info += t[i] + tab;
                        }
                        info += ":" + tab + valor + nl;
                    }
                }

                #endregion                
                #region Escribo TiempoDeCiclo
                info += "TiempoDeCiclo[i,c]" + nl;
                foreach (Tupla t in TiempoDeCiclo.Keys)
                {
                    double valor = TiempoDeCiclo[t];
                    for (int i = 0; i < t.Rango; i++)
                    {
                        info += t[i] + tab;
                    }
                    info += ":" + tab + valor + nl;
                }
                #endregion

                
                string rutaCarpeta = "Datos_" + GetNombreIdentificador(MS);
                string nombreArchivo = "Respuesta_" + numProblema + ".txt";
                Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta, nombreArchivo, info);
            }
            #endregion

            #region Guardo esta solución para usarla como pie inicial a la siguiente iteración
            solucionAnterior = new SolucionAnterior(AsignaCamionPalaDestino, InstanteCamionStatusNodo, 
                CamionCargaAntesEnPala, CamionDescargaAntesEnDestino, AsignaCamionAntesArco);
            #endregion

            //Asignación que se realizó. Se deben llenar sus valores
            Asignacion A_Actual = new Asignacion(instanteActual_min, idCamionSolicitaDespacho);
            A_Actual.TiempoResolucion_s = R.TiempoResolucion;
            AsignacionesSegunMS[MS].Add(A_Actual);

            #region Identifico cuántas tuplas Camión-Ciclo se consideraron
            A_Actual.NumCamionCiclo = TiempoDeCiclo.Keys.Count;
            #endregion

            //Estas variables se llenarán ahora
            Ruta RutaCargado = null;
            Ruta RutaVacio = null;
            Camion RecienDespachado = _camiones[idCamionSolicitaDespacho - 1];
            int idDestinoCamionAsignado = -1;
            int idPalaAsignada = -1;
            #region Identifico las rutas usadas y le asigno el tonelaje entregado a la ruta cargada
            if (MS != MetodoSolucion.GeneraRuta)
            {
                List<Indice> indexs = new List<Indice>();
                //[i,c,s,r]
                indexs.Add(idCamionSolicitaDespacho); indexs.Add(1); indexs.Add(1); indexs.Add(0);
                foreach (Ruta RR in _rutas)
                {
                    indexs[3] = RR.Id;
                    Tupla T = new Tupla(indexs);
                    if (AsignaCamionStatusRuta.ContainsKey(T) && AsignaCamionStatusRuta[T] == 1)
                    {
                        RutaVacio = RR;
                        break;
                    }
                }
                if (RutaVacio == null)
                {
                    throw new Exception("No se encontró la ruta vacía usada");
                }
                //Ahora busco la ruta cargada
                indexs[2] = 2;
                foreach (Ruta RR in _rutas)
                {
                    indexs[3] = RR.Id;
                    Tupla T = new Tupla(indexs);
                    if (AsignaCamionStatusRuta.ContainsKey(T) && AsignaCamionStatusRuta[T] == 1)
                    {
                        RutaCargado = RR;
                        RutaCargado.AgregarToneladasEnviadas(RecienDespachado.Capacidad_Ton);
                        break;
                    }
                }
                if (RutaCargado == null)
                {
                    throw new Exception("No se encontró la ruta cargada usada");
                }
            }
            #endregion
            #region Busco las asignaciones planificadas y actualizo las toneladas entregadas en la pala y el destino
            // AsignaCamionPalaDestino [i,j,d,c]
            foreach (Tupla T in AsignaCamionPalaDestino.Keys)
            {
                double valor = AsignaCamionPalaDestino[T];
                if (valor == 1)
                {
                    int i = (int)T[0];
                    int j = (int)T[1];
                    int d = (int)T[2];
                    int c = (int)T[3];
                    if (i == idCamionSolicitaDespacho && c==1)
                    {
                        idDestinoCamionAsignado = d;
                        idPalaAsignada = j;
                    }                    
                }
            }
            #endregion
            Pala P_Asignada = _palas[idPalaAsignada - 1]; //[i,j,tw]
            Destino D_Asignado = _destinos[idDestinoCamionAsignado - 1];
            
            #region Guardo la demora que presentó el camión en las rutas usadas
            //Primero para las rutas cargadas
            double demoraVacío = 0; //[i,c,s,u,v]
            double demoraCargado = 0;
            double esperaEnColaPala = 0;
            double esperaEnColaDestino = 0;
            #region Demora vacío: Traslado y espera en la cola de la pala
            List<Indice> indices = new List<Indice>();
            indices.Add(idCamionSolicitaDespacho); indices.Add(1); indices.Add(1); indices.Add(0); indices.Add(0);
            foreach (Arco A in RutaVacio.Arcos)
            {
                if (A.EsDeTransito)
                {
                    indices[3] = A.Id_Nodo_Inicial;
                    indices[4] = A.Id_Nodo_Final;
                    Tupla T = new Tupla(indices);
                    if (DemoraEnArco_Minutos.ContainsKey(T))
                    {
                        double demora = DemoraEnArco_Minutos[T];
                        demoraVacío += demora;
                    }
                    else
                    {
                        throw new Exception("Raro: el arco de la ruta no estaba en la variable de demoras");
                    }
                }
            }
            //Espera en cola pala: EsperaEnColaPala_Minutos[i,c,j]
            indices = new List<Indice>();
            indices.Add(idCamionSolicitaDespacho); indices.Add(1); indices.Add(idPalaAsignada);
            Tupla T2 = new Tupla(indices);
            if (EsperaEnColaPala_Minutos.ContainsKey(T2))
            {
                double espera = EsperaEnColaPala_Minutos[T2];
                esperaEnColaPala = espera;
            }
            else
            {
                throw new Exception("Raro: La variable de espera en cola no encontró la tupla");
            }
            #endregion
            #region Demora cargado: Traslado y espera en destino 
            indices = new List<Indice>(); //[i,c,s,u,v]
            indices.Add(idCamionSolicitaDespacho); indices.Add(1); indices.Add(2); indices.Add(0); indices.Add(0);
            foreach (Arco A in RutaCargado.Arcos)
            {
                if (A.EsDeTransito)
                {
                    indices[3] = A.Id_Nodo_Inicial;
                    indices[4] = A.Id_Nodo_Final;
                    Tupla T = new Tupla(indices);
                    if (DemoraEnArco_Minutos.ContainsKey(T))
                    {
                        double demora = DemoraEnArco_Minutos[T];
                        demoraCargado += demora;
                    }
                    else
                    {
                        throw new Exception("Raro: el arco de la ruta no estaba en la variable de demoras");
                    }
                }
            }
            //Espera en cola destino: EsperaEnColaDestino_Minutos[i,c,d]
            indices = new List<Indice>();
            indices.Add(idCamionSolicitaDespacho); indices.Add(1); indices.Add(idDestinoCamionAsignado);
            T2 = new Tupla(indices);
            if (EsperaEnColaDestino_Minutos.ContainsKey(T2))
            {
                double espera = EsperaEnColaDestino_Minutos[T2];
                esperaEnColaDestino = espera;
            }
            else
            {
                throw new Exception("Raro: La variable de espera en cola destino no encontró la tupla");
            }
            #endregion
            RutaCargado.AgregarDemora(instanteActual_min, RecienDespachado.TipoCamion.Id, demoraCargado);
            RutaVacio.AgregarDemora(instanteActual_min, RecienDespachado.TipoCamion.Id, demoraVacío);
            P_Asignada.AgregarEspera(instanteActual_min, RecienDespachado.TipoCamion.Id, esperaEnColaPala);
            D_Asignado.AgregarEspera(instanteActual_min, RecienDespachado.TipoCamion.Id, esperaEnColaDestino);

            #region Analizo si están cambiando y las updeteo si aumentan mucho
            if (Math.Max(demoraVacío, demoraCargado) >= demoraMaximaPorArco_min / 2)
            {
                demoraMaximaPorArco_min *= 2;
            }
            if (Math.Max(esperaEnColaPala, esperaEnColaDestino) >= esperaEnColaMax_min / 2)
            {
                esperaEnColaMax_min *= 2;
            }
            if (demoraVacío + demoraCargado + esperaEnColaPala + esperaEnColaDestino >= demoraMaximaPorCiclo_min / 2)
            {
                demoraMaximaPorCiclo_min *= 2;
            }
            #endregion

            #endregion

            #region Registro los instantes de atención en la Pala y el Destino y Actualizo las toneladas descargadas
            double t_inicio_carga_min, t_fin_carga_min;
            #region Busco estas variables
            Tupla T_InicioAtencion, T_FinAtencion;
            indices = new List<Indice>(); //[i,c,s,u]
            indices.Add(RecienDespachado.Id);                   // i
            indices.Add(1);                                     // c
            indices.Add(1);                                     // s
            indices.Add(nodoInicioCarga[idPalaAsignada - 1]);   // u
            T_InicioAtencion = new Tupla(indices);
            if (InstanteCamionStatusNodo.ContainsKey(T_InicioAtencion))
            {
                t_inicio_carga_min = InstanteCamionStatusNodo[T_InicioAtencion];
            }
            else
            {
                throw new Exception("No se encontró la tupla de inicio de carga en la pala");
            }
            
            //Ahora tengo que ver cuándo salió
            indices[2] = 2;
            indices[3] = nodoPala[idPalaAsignada - 1];
            T_FinAtencion = new Tupla(indices);
            if (InstanteCamionStatusNodo.ContainsKey(T_FinAtencion))
            {
                t_fin_carga_min = InstanteCamionStatusNodo[T_FinAtencion];
            }
            else
            {
                throw new Exception("No se encontró la tupla de fin de carga en la pala");
            }
            #endregion
            int twPalaUsada = -1;
            #region Busco en qué TW se asignó el camión
            indices = new List<Indice>(); //[i,j,c,tw]
            indices.Add(RecienDespachado.Id); indices.Add(P_Asignada.Id); indices.Add(1); indices.Add(-1);            
            for (int i = 0; i < P_Asignada.VentanasDeTiempoDisponible.Count; i++)
            {
                indices[3] = (i + 1);
                Tupla T = new Tupla(indices);
                if (AsignaCamionTWPala.ContainsKey(T) && AsignaCamionTWPala[T] == 1)
                {
                    twPalaUsada = i + 1;
                    break;
                }             
            }
            if (twPalaUsada == -1)
            {
                throw new Exception("No se encontró la tw usada en la pala");
            }
            #endregion
            P_Asignada.SetNuevoIntervaloDeAtencion(t_inicio_carga_min, t_fin_carga_min, twPalaUsada, instanteActual_min);

            double t_inicio_descarga_min, t_fin_descarga_min;
            #region Busco estas variables
            Tupla T_InicioDescarga, T_FinDescarga;
            indices = new List<Indice>(); //[i,c,s,u]
            indices.Add(RecienDespachado.Id); indices.Add(1); indices.Add(2); indices.Add(nodoInicioDescarga[D_Asignado.Id - 1]);     
            T_InicioDescarga = new Tupla(indices);
            if (InstanteCamionStatusNodo.ContainsKey(T_InicioDescarga))
            {
                t_inicio_descarga_min = InstanteCamionStatusNodo[T_InicioDescarga];
            }
            else
            {
                throw new Exception("No se encontró la tupla de inicio de descarga en el destino");
            }

            //Ahora tengo que ver cuándo salió
            indices = new List<Indice>(); //[i,c,d]
            indices.Add(RecienDespachado.Id); indices.Add(1); indices.Add(D_Asignado.Id);
            T_FinDescarga = new Tupla(indices);
            if (InstanteSalidaDestino.ContainsKey(T_FinDescarga))
            {
                t_fin_descarga_min = InstanteSalidaDestino[T_FinDescarga];
            }
            else
            {
                throw new Exception("No se encontró la tupla de fin de descarga en el destino");
            }
            #endregion
            A_Actual.InstanteDescarga = t_fin_descarga_min;

            int twDestinoUsado = -1;
            #region Busco en qué TW se asignó el camión 
            indices = new List<Indice>(); //[i,d,c,tw]
            indices.Add(RecienDespachado.Id); indices.Add(D_Asignado.Id); indices.Add(1); indices.Add(-1);
            for (int i = 0; i < D_Asignado.VentanasDeTiempoDisponible.Count; i++)
            {
                indices[3] = (i + 1);
                Tupla T = new Tupla(indices);
                if (AsignaCamionTWDestino.ContainsKey(T) && AsignaCamionTWDestino[T] == 1)
                {
                    twDestinoUsado = i + 1;
                    break;
                }
            }
            if (twDestinoUsado == -1)
            {
                throw new Exception("No se encontró la tw usada en la pala");
            }
            #endregion
            D_Asignado.SetNuevoIntervaloDeAtencion(t_inicio_descarga_min, t_fin_descarga_min, twDestinoUsado, instanteActual_min);
            
            //Actualizo toneladas y veo si se cumplió el plan día
            D_Asignado.AgregarDescargaDeToneladas(instanteActual_min, t_fin_descarga_min, RecienDespachado.Capacidad_Ton);
            planDia.AgregarToneladas(P_Asignada.Id, D_Asignado.Id, RecienDespachado.Capacidad_Ton, t_fin_descarga_min);
            #endregion

            #region Actualizo el nuevo instante y nodo disponible en que estará el camión recién despachado
            // InstanteSalidaDestino [i,c,d]
            // El destino al que el camión fue enviado es: idDestinoCamionSolicitado
            indices = new List<Indice>();
            indices.Add(idCamionSolicitaDespacho); indices.Add(1); indices.Add(idDestinoCamionAsignado);
            Tupla T1 = new Tupla(indices);
            if (InstanteSalidaDestino.ContainsKey(T1))
            {
                double valor = InstanteSalidaDestino[T1];
                //Actualizo el tiempo disponible
                if (_camionPorTiempoDisponible[0].Id != RecienDespachado.Id)
                {
                    throw new Exception("Error! el camión aludido no es el mismo que el de la lista");
                }
                RecienDespachado.InstanteSiguienteDisponible_Minutos = valor;
                RecienDespachado.IdNodoDisponible = D_Asignado.IdNodo;
            }
            else
            {
                throw new Exception("No se encontró en la variable InstanteSalidaDestino la tupla asociada al destino que fue enviado el camión que solicitó despacho");
            }
            #endregion
            
            //Se actualiza el siguiente instante de despacho
            double t0 = instanteActual_min;
            ActualizarCamionesConsiderados_Y_CiclosPorCamion(MS);
            #region Analizo si pude responder antes del nuevo instante de despacho
            double terminoRespuesta = t0 + R.TiempoResolucion / 60;
            A_Actual.AlcanzaAResponder = terminoRespuesta <= instanteActual_min? 1:0;
            #endregion
            

            #region Registro la trayectoria utilizada por el camión

            //Debo llenar esta lista. Un stuct Status Arco indica el arco usado por el camión junto con el status en que se usó
            List<StatusArco> StatusUV = new List<StatusArco>();
            
            #region (1) Busco los arcos usados en cada ruta
            foreach (Arco A in RutaVacio.Arcos)
            {
                StatusArco SA = new StatusArco(A, 1);
                StatusUV.Add(SA);
            }
            foreach (Arco A in RutaCargado.Arcos)
            {
                StatusArco SA = new StatusArco(A, 2);
                StatusUV.Add(SA);
            }            
            #endregion

            #region(2) Ahora guardo la trayectoria establecida por el camión
            indices = new List<Indice> { RecienDespachado.Id, 1, -1, -1}; //[i,c,s,u]
            foreach (StatusArco statusUV in StatusUV)
            {
                #region Para cada Arco - Status
                if (statusUV.arco.EsDeTransito) //Guardo las trayectorias de los arcos de tránsito utilizados
                {
                    double LlegadaU = 0;
                    #region Llegada a (u)
                    indices[2] = statusUV.Status;
                    indices[3] = statusUV.arco.Id_Nodo_Inicial;
                    Tupla T_u = new Tupla(indices);
                    if (InstanteCamionStatusNodo.ContainsKey(T_u))
                    {
                        LlegadaU = InstanteCamionStatusNodo[T_u]; ;
                    }
                    else
                    {
                        throw new Exception("Error al obtener el instante de llegada al nodo " + statusUV.arco.Id_Nodo_Inicial);
                    }
                    #endregion

                    double LlegadaV = 0;
                    #region Llegada a (v)
                    indices[3] = statusUV.arco.Id_Nodo_Final;
                    Tupla T_v = new Tupla(indices);
                    if (InstanteCamionStatusNodo.ContainsKey(T_v))
                    {
                        LlegadaV = InstanteCamionStatusNodo[T_v]; ;
                    }
                    else
                    {
                        throw new Exception("Error al obtener el instante de llegada al nodo " + statusUV.arco.Id_Nodo_Final);
                    }
                    #endregion

                    //Agrego esta trayectoria al arco
                    statusUV.arco.AddTrayectoria(LlegadaU, LlegadaV, RecienDespachado.Id, numProblema);
                }
                #endregion
            }
            
            #endregion            

            #endregion

            #region Actualizo las trayectorias según el nuevo instante actual
            if (MS == MetodoSolucion.DISPATCH)
            {
                foreach (Arco A in _grafo.Arcos)
                {
                    List<TimeWindow> TWS = new List<TimeWindow>();
                    foreach (TimeWindow TW in A.Trayectorias)
                    {
                        if (TW.Termino_min > instanteActual_min)
                        {
                            TWS.Add(TW);
                        }
                    }
                    A.Trayectorias = TWS;
                    if (TWS.Count > 0)
                    {
                        
                        A.OrdenarTrayectorias();
                        /*
                        Utiles.EscribeMensajeConsola("Arco (" + A.Id_Nodo_Inicial + "," + A.Id_Nodo_Final + "):", CategoriaMensaje.Categoria_4, 1, 0);
                        string linea = "";
                        string tab = "\t";
                        foreach (TimeWindow TW in A.Trayectorias)
                        {
                            double t1 = Math.Round(TW.Inicio_min, 2);
                            double t2 = Math.Round(TW.Termino_min, 2);
                            linea += "[" + t1 + " ; " + t2 + "]" + tab;
                        }
                        Utiles.EscribeMensajeConsola(linea, CategoriaMensaje.Categoria_4, 0, 0);
                         */ 
                    }
                }
            }
            else
            {
                foreach (Arco A in _grafo.Arcos)
                {
                    A.CorregirTrayectoriasSegunInstanteActual(instanteActual_min);
                }
            }            
            #endregion

            #region Actualizo el objeto de Asignación
            A_Actual.IdDestinoInicial = RutaVacio.IdDestino;
            A_Actual.IdPalaAsignada = idPalaAsignada;
            A_Actual.IdDestinoAsignado = idDestinoCamionAsignado;
            A_Actual.TolvaCamion = RecienDespachado.TipoCamion.Capacidad_Ton;
            double tiempoCarga = tiempoCargaTipoCamionPala_min[RecienDespachado.TipoCamion.Id-1, P_Asignada.Id - 1];
            A_Actual.TiempoCargaEnPala_min = tiempoCarga;

            //Tiempo de ciclo
            indices = new List<Indice> { RecienDespachado.Id, 1 }; //[i,c]
            Tupla TCiclo = new Tupla(indices);
            double tCiclo_min = TiempoDeCiclo[TCiclo];
            A_Actual.TiempoDeCiclo_min = tCiclo_min;

            //Costo de transporte
            double costoTransporteCargado = costoRutaPorTipoCamionStatus_pesos[RecienDespachado.TipoCamion.Id - 1, RutaCargado.Id - 1];
            double costoTransporteVacio = costoRutaPorTipoCamionStatus_pesos[RecienDespachado.TipoCamion.Id - 1, RutaVacio.Id - 1];
            double costoTransporte = costoTransporteCargado + costoTransporteVacio;
            A_Actual.CostoTransporte = costoTransporte;

            //Espera en el destino: EsperaEnColaDestino_Minutos[i,d]
            A_Actual.EsperaEnDestino_min = esperaEnColaDestino;

            //Espera en la pala: EsperaEnColaPala_Minutos[i,j]
            A_Actual.EsperaEnPala_min = esperaEnColaPala;
            
            //Demora [min]
            double demoraTotal = demoraCargado + demoraVacío;
            A_Actual.Demora_min = demoraTotal;

            
            #endregion

            #region Actualizo las TW de las palas y destinos según el nuevo instante actual
            foreach (Pala P in _palas)
            {
                P.LimpiarVentanasViejas(instanteActual_min);
            }
            foreach (Destino D in _destinos)
            {
                D.LimpiarVentanasViejas(instanteActual_min);
            }
            #endregion
        }
        /// <summary>
        /// Setea algunos valores de ciertas variables para obtener una solución inicial más rápido
        /// </summary>
        /// <param name="PS">La instancia del problema</param>
        private void SetearValoresParaSolucionInicial(ProblemaSolver PS)
        {
            if (Configuracion.SetearValoresSolInicial)
            {
                //Esta heurística asigna los camiones a la ruta
                if (solucionAnterior == null)
                {
                    #region Caso: Primera resolución del problema
                    //Asigno el primer camión al primer requerimiento, el segundo cam al segundo req y así sucesivamente

                    List<CamionCiclo> CamionesCiclo = new List<CamionCiclo>();
                    #region Genero esta lista y la ordeno por orden de aparción
                    foreach (int id in idsCamionesConsiderados)
                    {
                        Camion C = _camiones[id - 1];
                        for (int c = 0; c < numCiclosPorCamion[id - 1]; c++)
                        {
                            double T_ini = instanteInicioAproxCamionCiclo[id - 1, c];
                            CamionCiclo CC = new CamionCiclo(id, c + 1, T_ini);
                            CamionesCiclo.Add(CC);
                        }
                    }
                    CamionesCiclo.Sort(delegate(CamionCiclo cc1, CamionCiclo cc2)
                    {
                        return cc1.InstanteInicio.CompareTo(cc2.InstanteInicio);
                    });
                    #endregion

                    //Realizo la asignación
                    VariableIndexada AsignaCamionPalaDestino = PS.ObtenerVariableIndexada("AsignaCamionPalaDestino");
                    int numReq = 1;
                    foreach (CamionCiclo CC in CamionesCiclo)
                    {
                        Requerimiento R = planDia.Requerimientos[numReq - 1];
                        List<Indice> I = new List<Indice>() { CC.IdCamion, R.IdPala, R.IdDestino, CC.Ciclo };
                        Tupla T = new Tupla(I);
                        if (AsignaCamionPalaDestino.Claves.Contains(T))
                        {
                            AsignaCamionPalaDestino[T].ValorInicial = 1;
                        }
                        else
                        {
                            throw new Exception("Raro: No está la tupla asociada");
                        }

                        //Actualizo el requerimiento
                        numReq = (numReq == planDia.Requerimientos.Count) ? 1 : numReq + 1;
                    }
                    #endregion
                }
                else
                {
                    //Ya se ha resuelto el problema
                    if (true)
                    {
                        #region AsignaCamionPalaDestino
                        VariableIndexada AsignaCamionPalaDestino = PS.ObtenerVariableIndexada("AsignaCamionPalaDestino");
                        foreach (Tupla T in solucionAnterior.AsignaCamionPalaDestino.Keys)
                        {
                            int i = (int)T[0];
                            int j = (int)T[1];
                            int d = (int)T[2];
                            int c = (int)T[3];

                            bool bi = idsCamionesConsiderados.Contains(i);
                            bool bc = c <= numCiclosPorCamion[i - 1];
                            if (bi && bc)
                            {
                                if (solucionAnterior.AsignaCamionPalaDestino[T] == 1)
                                {
                                    if (AsignaCamionPalaDestino.Claves.Contains(T))
                                    {
                                        AsignaCamionPalaDestino[T].ValorInicial = 1;
                                    }
                                    else
                                    {
                                        throw new Exception("Raro no tiene la clave");
                                    }
                                }
                            }
                        }
                        #endregion
                        #region CamionCargaAntesEnPala
                        try
                        {
                            VariableIndexada CamionCargaAntesEnPala = PS.ObtenerVariableIndexada("CamionCargaAntesEnPala");
                            if (solucionAnterior.CamionCargaAntesEnPala != null)
                            {
                                foreach (Tupla T in solucionAnterior.CamionCargaAntesEnPala.Keys)
                                {
                                    int i1 = (int)T[0];
                                    int c1 = (int)T[1];
                                    int i2 = (int)T[2];
                                    int c2 = (int)T[3];
                                    int j = (int)T[4];

                                    bool bi = idsCamionesConsiderados.Contains(i1) && idsCamionesConsiderados.Contains(i2);
                                    bool bc = c1 <= numCiclosPorCamion[i1 - 1] && c2 <= numCiclosPorCamion[i2 - 1];

                                    if (bi && bc)
                                    {
                                        if (solucionAnterior.CamionCargaAntesEnPala[T] == 1)
                                        {
                                            if (CamionCargaAntesEnPala.Claves.Contains(T))
                                            {
                                                CamionCargaAntesEnPala[T].ValorInicial = 1;
                                            }
                                            else
                                            {
                                                throw new Exception("Raro no tiene la clave");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                        #endregion
                        #region CamionDescargaAntesEnDestino
                        try
                        {
                            VariableIndexada CamionDescargaAntesEnDestino = PS.ObtenerVariableIndexada("CamionDescargaAntesEnDestino");
                            if (solucionAnterior.CamionDescargaAntesEnDestino != null)
                            {
                                foreach (Tupla T in solucionAnterior.CamionDescargaAntesEnDestino.Keys)
                                {
                                    int i1 = (int)T[0];
                                    int c1 = (int)T[1];
                                    int i2 = (int)T[2];
                                    int c2 = (int)T[3];
                                    int d = (int)T[4];

                                    bool bi = idsCamionesConsiderados.Contains(i1) && idsCamionesConsiderados.Contains(i2);
                                    bool bc = c1 <= numCiclosPorCamion[i1 - 1] && c2 <= numCiclosPorCamion[i2 - 1];
                                    if (bi && bc)
                                    {
                                        if (solucionAnterior.CamionDescargaAntesEnDestino[T] == 1)
                                        {
                                            if (CamionDescargaAntesEnDestino.Claves.Contains(T))
                                            {
                                                CamionDescargaAntesEnDestino[T].ValorInicial = 1;
                                            }
                                            else
                                            {
                                                throw new Exception("Raro no tiene la clave");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                        #endregion
                        #region AsignaCamionAntesArco
                        try
                        {
                            VariableIndexada AsignaCamionAntesArco = PS.ObtenerVariableIndexada("AsignaCamionAntesArco");
                            if (solucionAnterior.AsignaCamionAntesArco != null)
                            {
                                foreach (Tupla T in solucionAnterior.AsignaCamionAntesArco.Keys)
                                {
                                    int i1 = (int)T[0];
                                    int c1 = (int)T[1];
                                    int s1 = (int)T[2];
                                    int i2 = (int)T[3];
                                    int c2 = (int)T[4];
                                    int s2 = (int)T[5];
                                    int u = (int)T[6];
                                    int v = (int)T[7];

                                    bool bi = idsCamionesConsiderados.Contains(i1) && idsCamionesConsiderados.Contains(i2);
                                    bool bc = c1 <= numCiclosPorCamion[i1 - 1] && c2 <= numCiclosPorCamion[i2 - 1];

                                    if (bi && bc)
                                    {
                                        if (solucionAnterior.AsignaCamionAntesArco[T] == 1)
                                        {
                                            if (AsignaCamionAntesArco.Claves.Contains(T))
                                            {
                                                AsignaCamionAntesArco[T].ValorInicial = 1;
                                            }
                                            else
                                            {
                                                throw new Exception("Raro no tiene la clave");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                        #endregion
                    }
                }
            }
        }
        /// <summary>
        /// Resuelve el problema a partir del dat y un mod
        /// </summary>
        /// <param name="ruta_dat"></param>
        /// <param name="ruta_mod"></param>
        private Resultado ResolverDespacho(string ruta_dat, string ruta_mod, MetodoSolucion MS)
        {
            Resultado R = null;
            #region Inicialización (Independiente del método)
            SolverFrontend solver = null;
            ProblemaSolver problema = null;
            try
            {
                solver = new SolverFrontend("SolverBackend_Cplex.dll");
                problema = new ProblemaSolver(ruta_mod, ruta_dat);
            }
            catch (Exception e)
            {
                Utiles.EscribeMensajeConsola(e.ToString(), CategoriaMensaje.Categoria_4, 1, 1);
            }
            
            //Parámetros de CPLEX independientes del método
            solver.AsignaParametro("EpGap", Configuracion.CPLEX_GapRelativo);
            solver.AsignaParametro("Threads", Configuracion.CPLEX_NumeroThreads);
            solver.AsignaParametro("MIPOrdType", (int)ParamCPLEX_MIPOrderType._1_DecreasingCost);
            solver.AsignaParametro("ParallelMode", (int)ParamCplex_ParallelMode._1_Deterministic); //Deterministico para que pueda debugear

            #endregion

            if (MS == MetodoSolucion.MIP)
            {
                #region Modelo: Escoge Rutas
                Utiles.EscribeMensajeConsola("Comienzo resolución:", CategoriaMensaje.Categoria_2, 1, 0);
                
                #region Seteo Parámetros y Cortes
                    //Seteo valores para obtener una solución inicial más rápido

                    SetearValoresParaSolucionInicial(problema);

                    double tiempoBase = 10; //Tiempo base para resolver, en segundos
                    solver.AsignaParametro("TiLim", tiempoBase);
                    solver.AsignaParametro("ClockType", (int)ParamCPLEX_ClockType._2_Wall_Clock_Time);
                    solver.AsignaParametro("AdvInd", (int)ParamCplex_AdvancedStart._1_Default_UsarAdvancedBasis);
                    solver.AsignaParametro("MIPEmphasis", (int)ParamCPLEX_MIPEmphasis._0_Balancea_Optimalidad_y_Factibilidad);
                    //solver.AsignaParametro("IntSolLim", 1); //Cantidad de soluciones: Solo quiero una solución
                    solver.AsignaParametro("MIPDisplay", (int)ParamCPLEX_MIPDisplay._3_SameAs2_WithInfoAboutNodeCuts);
                    solver.AsignaParametro("NodeSel", (int)ParamCPLEX_NodeSelection._0_DepthFirst);

                    #region Cortes
                    solver.AsignaParametro("MIRCuts", (int)ParamCplex_Cuts_MIR._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("DisjCuts", (int)ParamCplex_Cuts_Disjuntive._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("Cliques", (int)ParamCplex_Cuts_Cliques._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("Covers", (int)ParamCplex_Cuts_Covers._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("FlowCovers", (int)ParamCplex_Cuts_FlowCovers._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("FlowPaths", (int)ParamCplex_Cuts_FlowPaths._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("FracCuts", (int)ParamCplex_Cuts_Gomory._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("GUBCovers", (int)ParamCplex_Cuts_GUB._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("ImplBd", (int)ParamCplex_Cuts_ImpliedBound._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("MCFCuts", (int)ParamCplex_Cuts_MultiCommodityFlow._0_Default_QueCPLEXDecida);
                    solver.AsignaParametro("ZeroHalfCuts", (int)ParamCplex_Cuts_ZeroHalf._0_Default_QueCPLEXDecida);
                    #endregion

                    //Verifico que mis parámetros hayan sido seteados
                    //solver.EscribeParametros("parametrosSolver.txt");
                    #endregion                
                
                #region Ciclo de solución
                DateTime T1 = DateTime.Now;
                ResultadoSolver RS = solver.ResolverProblema(problema);      //Intento resolver el problema                
                if (!(RS == ResultadoSolver.Optimo || RS == ResultadoSolver.Factible))
                {
                    string error = "Ver qué pasó";
                }

                int etapa = 1;
                DateTime T0 = DateTime.Now;
                while (RS != ResultadoSolver.Optimo)
                { 
                    Utiles.EscribeMensajeConsola("No pudo resolverse", CategoriaMensaje.Categoria_2, 1, 1);
                    if (etapa == 1 && Configuracion.HacerPolishing) //Hago Polishing
                    {
                        Utiles.EscribeMensajeConsola("Aplicando POLISHING", CategoriaMensaje.Categoria_2, 0, 0);
                        solver.AsignaParametro("PolishAfterEpGap", 0.5);
                        solver.AsignaParametro("TiLim", 5); //Tiempo de polishing
                        etapa = 2;                        
                        RS = solver.ResolverProblema(problema);      //Intento resolver el problema
                        if (RS == ResultadoSolver.Infactible)
                        {
                            throw new Exception("Problema infactible");
                        }
                    }
                    else //Optimizo
                    {
                        Utiles.EscribeMensajeConsola("Aplicando Branch & Cut", CategoriaMensaje.Categoria_2, 0, 0);
                        solver.AsignaParametro("PolishAfterEpGap", 0.0000000000000000000001);
                        solver.AsignaParametro("TiLim", 10);            //Tiempo de b&c
                        RS = solver.ResolverProblema(problema);         //Intento resolver el problema
                        if (RS == ResultadoSolver.Infactible)
                        {
                            throw new Exception("Problema infactible");
                        }
                        if (Configuracion.HacerPolishing)
                        {
                            etapa = 1;
                        }
                    }

                    //Condición de término de esta instancia
                    DateTime Tx = DateTime.Now;
                    if ((Tx - T0).TotalMinutes >= 30)
                    {
                        continuarInstancia = false;
                        break;
                    }
                }
                
                //Si el problema está resuelto:
                double tiempoResolucion_s = Math.Round((DateTime.Now - T1).TotalSeconds, 2);
                double gapRelFinal = Math.Round(Math.Abs(problema.Solucion.GapRelativo)*100,2);
                Utiles.EscribeMensajeConsola("Problema resuelto en " + tiempoResolucion_s + " segundos.", CategoriaMensaje.Categoria_1, 1, 0);
                Utiles.EscribeMensajeConsola("Gap relativo obtenido: " + gapRelFinal + "%", CategoriaMensaje.Categoria_1, 0, 0);
                R = new Resultado(problema, tiempoResolucion_s, Variable.NombresVariablesIndexadas_SegunMS[MS], Variable.NombresVariablesEscalares_SegunMS[MS]);
                MostrarFuncionObjetivo(R, MS, problema.Solucion.ValorObjetivo); 
                #endregion

                #endregion
            }
            else if (MS == MetodoSolucion.DISPATCH || MS == MetodoSolucion.DRMA)
            {
                #region Metodología de Heurísticas
                Utiles.EscribeMensajeConsola("Comienzo resolución:", CategoriaMensaje.Categoria_2, 1, 0);
                Destino DInicial = GetDestinoSegunNodo(_camiones[idCamionSolicitaDespacho - 1].IdNodoDisponible);
                
                #region Muestro la heurística
                int[, ,] H = heuristica_Dispatch;
                if (MS == MetodoSolucion.DRMA)
                {
                    H = heuristica_DescargaAlDestinoMasAtrasado;
                }
                Utiles.EscribeMensajeConsola("*** HEURÍSTICA:", CategoriaMensaje.Categoria_2, 0, 0);
                for (int s = 0; s < _statusCamiones.Count; s++)
                {
                    for (int r = 0; r < _rutas.Count; r++)
                    {
                        Ruta elegida = _rutas[r];
                        if (H[idCamionSolicitaDespacho - 1, s, r] == 1)
                        {
                            if (s == 0)
                            {
                                Utiles.EscribeMensajeConsola("Camión " + idCamionSolicitaDespacho.ToString() + " parte en D" + DInicial.Id.ToString() + " y carga en P" + elegida.IdPala.ToString() + " a través de la ruta " + (r + 1).ToString(), CategoriaMensaje.Categoria_3, 0, 0);
                            }
                            else
                            {
                                Utiles.EscribeMensajeConsola("Luego descarga en el D" + elegida.IdDestino.ToString() + " a través de la ruta " + (r + 1).ToString(), CategoriaMensaje.Categoria_3, 0, 0);
                            }
                        }
                    }
                }
                #endregion

                #region Seteo Parámetros y Cortes
                double tiempoBase = 10; //Tiempo base para resolver, en segundos
                solver.AsignaParametro("TiLim", tiempoBase);
                solver.AsignaParametro("MIPEmphasis", (int)ParamCPLEX_MIPEmphasis._1_Enfatiza_Factibilidad);
                //solver.AsignaParametro("IntSolLim", 1); //Cantidad de soluciones: Solo quiero una solución
                solver.AsignaParametro("MIPDisplay", (int)ParamCPLEX_MIPDisplay._3_SameAs2_WithInfoAboutNodeCuts);
                solver.AsignaParametro("NodeSel", (int)ParamCPLEX_NodeSelection._0_DepthFirst);
                //solver.AsignaParametro("PolishTime", 5); //Cantidad de tiempo para hacer polishing en segundos
                //solver.AsignaParametro("PolishAfterIntSol", 1);
                //Cortes
                solver.AsignaParametro("MIRCuts", (int)ParamCplex_Cuts_MIR._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("DisjCuts", (int)ParamCplex_Cuts_Disjuntive.__1_NoGenerar);
                solver.AsignaParametro("Cliques", (int)ParamCplex_Cuts_Cliques._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("Covers", (int)ParamCplex_Cuts_Covers._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("FlowCovers", (int)ParamCplex_Cuts_FlowCovers._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("FlowPaths", (int)ParamCplex_Cuts_FlowPaths._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("FracCuts", (int)ParamCplex_Cuts_Gomory._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("GUBCovers", (int)ParamCplex_Cuts_GUB._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("ImplBd", (int)ParamCplex_Cuts_ImpliedBound._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("MCFCuts", (int)ParamCplex_Cuts_MultiCommodityFlow._0_Default_QueCPLEXDecida);
                solver.AsignaParametro("ZeroHalfCuts", (int)ParamCplex_Cuts_ZeroHalf._0_Default_QueCPLEXDecida);

                //Verifico que mis parámetros hayan sido seteados
                //solver.EscribeParametros("parametrosSolver.txt");
                #endregion

                #region Ciclo de solución
                DateTime T1 = DateTime.Now;
                solver.ResolverProblema(problema);      //Intento resolver el problema
                bool problemaResuelto = false;
                if (problema.Solucion != null && problema.Solucion.GapRelativo <= Configuracion.CPLEX_GapRelativo)
                {
                    problemaResuelto = true;
                }
                
                while (!problemaResuelto)
                {
                    //Empiezo a iterar haciendo polishing en cada iteración hasta llegar al gap relativo, que va subiendo de a poco;
                    solver.ResolverProblema(problema);      //Intento resolver el problema
                    problemaResuelto = (problema.Solucion != null && problema.Solucion.GapRelativo <= Configuracion.CPLEX_GapRelativo);
                }

                //Si el problema está resuelto:
                double tiempoResolucion_s = Math.Round((DateTime.Now - T1).TotalSeconds, 2);
                Utiles.EscribeMensajeConsola("Problema resuelto en " + tiempoResolucion_s + " segundos.", CategoriaMensaje.Categoria_1, 0, 0);
                R = new Resultado(problema,tiempoResolucion_s, Variable.NombresVariablesIndexadas_SegunMS[MS], Variable.NombresVariablesEscalares_SegunMS[MS]);
                MostrarFuncionObjetivo(R, MS, problema.Solucion.ValorObjetivo);
                #endregion

                #endregion
            }

            try
            {
                solver.Dispose();

                string archivo = problema.RutaMPS.Remove(problema.RutaMPS.Length - 4);
                string archivoMPS = problema.RutaMPS;
                string archivoCOL = archivo + ".col";
                string archivoROW = archivo + ".row";
                
                if(System.IO.File.Exists(archivoMPS))
                {
                    System.IO.File.Delete(archivoMPS);
                }
                if(System.IO.File.Exists(archivoCOL))
                {
                    System.IO.File.Delete(archivoCOL);
                }
                if(System.IO.File.Exists(archivoROW))
                {
                    System.IO.File.Delete(archivoROW);
                }                
            }
            catch (Exception e)
            {
                throw e;
            }

            return R;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="R"></param>
        private void MostrarFuncionObjetivo(Resultado R, MetodoSolucion MS, double ValorFOB)
        {
            Utiles.EscribeMensajeConsola("Función Objetivo: \t\t\t " + Utiles.FormatDoubleAPesos(ValorFOB), CategoriaMensaje.Categoria_2, 1, 0);
            double FO_CostosPorTiempoDeCiclo = Math.Round(R.GetValorVariableEscalar(NombreVariablesEscalares.FO_CostosPorTiempoDeCiclo), 0);
            double FO_CostosDeTransporte = Math.Round(R.GetValorVariableEscalar(NombreVariablesEscalares.FO_CostosDeTransporte), 0);
            double FO_CostosPorDemora = Math.Round(R.GetValorVariableEscalar(NombreVariablesEscalares.FO_CostosPorDemora), 0);
            double FO_CostosPorToneladasFaltantes = Math.Round(R.GetValorVariableEscalar(NombreVariablesEscalares.FO_CostosPorToneladasFaltantes), 0);

            Utiles.EscribeMensajeConsola("FO_CostosPorTiempoDeCiclo: \t\t " + Utiles.FormatDoubleAPesos(FO_CostosPorTiempoDeCiclo), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FO_CostosDeTransporte: \t\t\t " + Utiles.FormatDoubleAPesos(FO_CostosDeTransporte), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FO_CostosPorDemora: \t\t\t " + Utiles.FormatDoubleAPesos(FO_CostosPorDemora), CategoriaMensaje.Categoria_4, 0, 0);
            Utiles.EscribeMensajeConsola("FO_CostosPorToneladasFaltantes: \t " + Utiles.FormatDoubleAPesos(FO_CostosPorToneladasFaltantes), CategoriaMensaje.Categoria_4, 0, 0);
            
        }
        /// <summary>
        /// Setea el número de ciclos por camión para el modelo Escoge Rutas
        /// </summary>
        private void CalcularNumeroDeCiclosPorCamionE_InstanteDeInicio()
        {
            numCiclosPorCamion = new int[_camiones.Count];
            int maxNum = 1;
            foreach (Camion C in _camiones)
            {
                #region Cálculo
                // Num Ciclos
                // Se pone como mínimo 0 de tiempo productivo para que haya al menos 1 ciclo
                double tiempoProductivo = Math.Max(0, instanteActual_min + horizontePlanificacion_min - C.InstanteSiguienteDisponible_Minutos);
                int idD = GetDestinoSegunNodo(C.IdNodoDisponible).Id;
                int numC = 0;
                while (tiempoProductivo >= 0)
                {
                    if (numC == 0)
                    {
                        tiempoProductivo -= tiempoDeCicloFlujoLibreMinSegunDInicioTipoCam_min[idD - 1, C.TipoCamion.Id - 1];
                    }
                    else
                    {
                        tiempoProductivo -= tiempoDeCicloFlujoLibreMin_min[C.TipoCamion.Id - 1];
                    }
                    numC++;
                }
                numCiclosPorCamion[C.Id - 1] = numC;
                maxNum = Math.Max(maxNum, numC);
                if (numC <= 0)
                {
                    throw new Exception("El número de ciclos no puede ser menor o igual a 0!");
                }
                #endregion
            }

            instanteInicioAproxCamionCiclo = new double[_camiones.Count, maxNum];
            foreach (int id in idsCamionesConsiderados)
            { 
                Camion C = _camiones[id-1];
                int d = GetDestinoSegunNodo(C.IdNodoDisponible).Id;
                int k = C.TipoCamion.Id;
                instanteInicioAproxCamionCiclo[id - 1, 0] = C.InstanteSiguienteDisponible_Minutos;
                int numC = numCiclosPorCamion[id - 1];
                if (numC > 1)
                {
                    double TCicloAVG = TCicloAVGInicial;
                    if (numProblema > 1)
                    { 
                        TCicloAVG = tiempoDeCicloAVGPor_TipoCamionDestinoInicial_min[k - 1, d - 1];
                    }
                    instanteInicioAproxCamionCiclo[id - 1, 1] = instanteInicioAproxCamionCiclo[id - 1, 0] + TCicloAVG;
                }
                for (int i = 2; i < numC; i++)
                {
                    double TCicloAVG = TCicloAVGInicial;
                    if (numProblema > 1)
                    {
                        TCicloAVG = tiempoDeCicloAVGPor_TipoCamion_min[k - 1];
                    }
                    instanteInicioAproxCamionCiclo[id - 1, i] = instanteInicioAproxCamionCiclo[id - 1, i - 1] + TCicloAVG;
                }
            }
        }
        /// <summary>
        /// Este método genera el .DAT y devuelve la ruta del archivo
        /// </summary>
        /// <param name="MS">El método de solución</param>
        private string GenerarDatDespacho(MetodoSolucion MS)
        {

            StringBuilder output = new StringBuilder();

            #region Desactivo los métodos
            esModelo_Dispatch = 0;
            esModelo_DescargaMasTemprana = 0;
            #endregion
            #region Creo Parámetros que van independiente del método de solución
            #region Cosas del Plan día
            toneladasRecibidas = new double[planDia.Requerimientos.Count];
            foreach (Requerimiento R in planDia.Requerimientos)
            {
                toneladasRecibidas[R.Id - 1] = R.ToneladasEntregadas;
            }
            #endregion
            #region Ventanas de tiempo de palas
            numTimeWindowsPorPala = new int[_palas.Count];
            int maxNum = 0;
            foreach (Pala P in _palas)
            {
                int n = P.VentanasDeTiempoDisponible.Count;
                if (n > maxNum)
                {
                    maxNum = n;
                }
            }
            instanteInicioTW_Pala_min = new double[_palas.Count, maxNum];
            instanteTerminoTW_Pala_min = new double[_palas.Count, maxNum];
            foreach (Pala P in _palas)
            {
                numTimeWindowsPorPala[P.Id - 1] = P.VentanasDeTiempoDisponible.Count;
                for (int i = 0; i < P.VentanasDeTiempoDisponible.Count; i++)
                {
                    TimeWindow TW = P.VentanasDeTiempoDisponible[i];
                    double t1 = TW.Inicio_min;
                    double t2 = TW.Termino_min;
                    instanteInicioTW_Pala_min[P.Id - 1, i] = t1;
                    instanteTerminoTW_Pala_min[P.Id - 1, i] = t2;
                }
            }
            #endregion
            #region Ventana de tiempo destinos
            numTimeWindowsPorDestino = new int[_destinos.Count];
            maxNum = 0;
            foreach (Destino D in _destinos)
            {
                int n = D.VentanasDeTiempoDisponible.Count;
                if (n > maxNum)
                {
                    maxNum = n;
                }
            }
            instanteInicioTW_Dest_min = new double[_destinos.Count, maxNum];
            instanteTerminoTW_Dest_min = new double[_destinos.Count, maxNum];
            foreach (Destino D in _destinos)
            {
                numTimeWindowsPorDestino[D.Id - 1] = D.VentanasDeTiempoDisponible.Count;
                for (int i = 0; i < D.VentanasDeTiempoDisponible.Count; i++)
                {
                    TimeWindow TW = D.VentanasDeTiempoDisponible[i];
                    double t1 = TW.Inicio_min;
                    double t2 = TW.Termino_min;
                    instanteInicioTW_Dest_min[D.Id - 1, i] = t1;
                    instanteTerminoTW_Dest_min[D.Id - 1, i] = t2;
                }
            }



            #endregion
            #region Atributos de camiones
            nodoInicioCamion = new int[_camiones.Count];
            instanteCamionDisponible_min = new double[_camiones.Count];
            destinoInicialCamion = new int[_camiones.Count];
            foreach (Camion C in _camiones)
            {
                nodoInicioCamion[C.Id - 1] = C.IdNodoDisponible;
                instanteCamionDisponible_min[C.Id - 1] = C.InstanteSiguienteDisponible_Minutos;
                int idD = GetDestinoSegunNodo(C.IdNodoDisponible).Id;
                destinoInicialCamion[C.Id - 1] = idD;
            }
            #endregion
            #region Heurísticas
            heuristica_Dispatch = new int[_camiones.Count, _statusCamiones.Count, _rutas.Count];
            heuristica_DescargaAlDestinoMasAtrasado = new int[_camiones.Count, _statusCamiones.Count, _rutas.Count];
            #endregion
            #region tiempoDeCicloAVG_min
            tiempoDeCicloAVGPor_TipoCamion_min = new double[_tipoCamiones.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                double sumTC = 0;
                int n = 0;
                foreach (Asignacion A in AsignacionesSegunMS[MS])
                {
                    if (_camiones[A.IdCamion - 1].TipoCamion.Id == TC.Id)
                    {
                        sumTC += A.TiempoDeCiclo_min;
                        n++;
                    }
                }
                double avg = TCicloAVGInicial;
                if (n > 0)
                {
                    avg = sumTC / n;
                }
                tiempoDeCicloAVGPor_TipoCamion_min[TC.Id - 1] = avg;
            }

            tiempoDeCicloAVGPor_TipoCamionDestinoInicial_min = new double[_tipoCamiones.Count, _destinos.Count];
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                foreach (Destino D in _destinos)
                {
                    double sumTC = 0;
                    int n = 0;
                    foreach (Asignacion A in AsignacionesSegunMS[MS])
                    {
                        if (A.IdDestinoInicial == D.Id && _camiones[A.IdCamion - 1].TipoCamion.Id == TC.Id)
                        {
                            sumTC += A.TiempoDeCiclo_min;
                            n++;
                        }
                    }
                    double avg = TCicloAVGInicial;
                    if (n > 0)
                    {
                        avg = sumTC / n;
                    }
                    tiempoDeCicloAVGPor_TipoCamionDestinoInicial_min[TC.Id - 1, D.Id - 1] = avg;
                }
            }
            #endregion
            #region Trayectorias de los arcos
            numTrayectoriasPorArco = new double[_grafo.Nodos.Count, _grafo.Nodos.Count];
            int maxNumTrayectorias = 0;
            foreach (Arco A in _grafo.Arcos)
            {
                if (A.Trayectorias.Count > maxNumTrayectorias)
                {
                    maxNumTrayectorias = A.Trayectorias.Count;
                }
            }
            instanteInicioTrayectoria_min = new double[_grafo.Nodos.Count, _grafo.Nodos.Count, maxNumTrayectorias];
            instanteTerminoTrayectoria_min = new double[_grafo.Nodos.Count, _grafo.Nodos.Count, maxNumTrayectorias];
            foreach (Arco A in _grafo.Arcos)
            {
                int u = A.Id_Nodo_Inicial;
                int v = A.Id_Nodo_Final;
                for (int i = 0; i < A.Trayectorias.Count; i++)
                {
                    TimeWindow Trayectoria = A.Trayectorias[i];
                    double ini = Trayectoria.Inicio_min;
                    double fin = Trayectoria.Termino_min;
                    instanteInicioTrayectoria_min[u - 1, v - 1, i] = ini;
                    instanteTerminoTrayectoria_min[u - 1, v - 1, i] = fin;
                }
                numTrayectoriasPorArco[u - 1, v - 1] = A.Trayectorias.Count;
            }


            #endregion
            #endregion

            if (MS == MetodoSolucion.MIP)
            {

            }
            else if (MS == MetodoSolucion.DISPATCH)
            {
                esModelo_Dispatch = 1;
                heuristica_Dispatch = HeuristicaDispatch();
            }
            else if (MS == MetodoSolucion.DRMA)
            {
                esModelo_DescargaMasTemprana = 1;
                heuristica_DescargaAlDestinoMasAtrasado = Heuristica_DescargaAlRequerimientoMasAtrasado();
            }

            CalcularTiemposDeViajeMinYMaxANodos();

            #region Escribo SETS
            output.Append(CAMIONES);
            output.Append(PALAS);
            output.Append(DESTINOS);
            output.Append(NODOS);
            output.Append(STATUS_CAMIONES);
            output.Append(TIPO_CAMIONES);
            output.Append(RUTAS);
            output.Append(REQUERIMIENTOS);
            List<Indice> camiones = new List<Indice>();
            foreach (int id in idsCamionesConsiderados)
            {
                camiones.Add(id);
            }
            CAMIONES_CONSIDERADOS = new Conjunto("CAMIONES_CONSIDERADOS", camiones);
            output.Append(CAMIONES_CONSIDERADOS);
            #endregion
            #region Escribo PARÁMETROS

            output.Append("param instanteActual_min:=" + instanteActual_min.ToString().Replace(',', '.') + ";\n");
            output.Append("param tiempoTotalHorizonte_min:=" + Configuracion.TiempoTotalHorizonte + ";\n");
            output.Append("param pesosPorTonelada:=" + pesosPorTonelada.ToString().Replace(',', '.') + ";\n");
            output.Append("param horizontePlanificacion_min:=" + horizontePlanificacion_min.ToString().Replace(',', '.') + ";\n");
            output.Append("param esModelo_Dispatch:=" + esModelo_Dispatch.ToString() + ";\n");
            output.Append("param esModelo_DescargaMasTemprana:=" + esModelo_DescargaMasTemprana.ToString() + ";\n");
            output.Append("param intervaloTonelaje:=" + intervaloTonelaje.ToString().Replace(',', '.') + ";\n");
            output.Append("param demoraMaximaPorArco_min:=" + demoraMaximaPorArco_min.ToString() + ";\n");
            output.Append("param demoraMaximaPorCiclo_min:=" + demoraMaximaPorCiclo_min.ToString() + ";\n");
            output.Append("param esperaEnColaMax_min:=" + esperaEnColaMax_min.ToString().Replace(',', '.') + ";\n");
            output.Append("param multiplicadorPendiente:=" + Configuracion.multiplicadorPendiente.ToString().Replace(',', '.') + ";\n");
            output.Append("param coefMantenerLey:=" + coefMantenerLey.ToString().Replace(',', '.') + ";\n");
            

            output.Append(Variable.EscribeArregloEnString(isPalaDestinoEnReq, "isPalaDestinoEnReq"));
            output.Append(Variable.EscribeArregloEnString(arcosPorRuta, "arcosPorRuta"));
            output.Append(Variable.EscribeArregloEnString(esRutaDestinoPala, "esRutaDestinoPala"));
            output.Append(Variable.EscribeArregloEnString(esRutaPalaDestino, "esRutaPalaDestino"));
            output.Append(Variable.EscribeArregloEnString(destinoInicialCamion, "destinoInicialCamion"));
            output.Append(Variable.EscribeArregloEnString(tiempoDeCicloFlujoLibreMax_min, "tiempoDeCicloFlujoLibreMax_min"));
            output.Append(Variable.EscribeArregloEnString(tipoCamion, "tipoCamion"));
            output.Append(Variable.EscribeArregloEnString(numCiclosPorCamion, "numCiclosPorCamion"));
            output.Append(Variable.EscribeArregloEnString(existeArco, "existeArco"));
            output.Append(Variable.EscribeArregloEnString(esArcoDeEsperaEnCola, "esArcoDeEsperaEnCola"));
            output.Append(Variable.EscribeArregloEnString(nodoDestino, "nodoDestino"));
            output.Append(Variable.EscribeArregloEnString(nodoPala, "nodoPala"));
            output.Append(Variable.EscribeArregloEnString(capacidadCamion_ton, "capacidadCamion_ton"));
            output.Append(Variable.EscribeArregloEnString(tiempoViajeTipoCamionArco_min, "tiempoViajeTipoCamionArco_min"));
            output.Append(Variable.EscribeArregloEnString(costoViaje_pesos, "costoViaje_pesos"));
            output.Append(Variable.EscribeArregloEnString(isCamionAsignablePala, "isCamionAsignablePala"));
            output.Append(Variable.EscribeArregloEnString(isMaterialPalaDescargableEnDestino, "isMaterialPalaDescargableEnDestino"));
            output.Append(Variable.EscribeArregloEnString(tiempoDescargaTipoCamionDestino_min, "tiempoDescargaTipoCamionDestino_min"));
            output.Append(Variable.EscribeArregloEnString(tiempoCargaTipoCamionPala_min, "tiempoCargaTipoCamionPala_min"));
            output.Append(Variable.EscribeArregloEnString(toneladasTotalesSegunPlanDiario, "toneladasTotalesSegunPlanDiario"));
            output.Append(Variable.EscribeArregloEnString(nodoInicioCamion, "nodoInicioCamion"));
            output.Append(Variable.EscribeArregloEnString(instanteCamionDisponible_min, "instanteCamionDisponible_min"));
            output.Append(Variable.EscribeArregloEnString(numTimeWindowsPorPala, "numTimeWindowsPorPala"));
            output.Append(Variable.EscribeArregloEnString(instanteInicioTW_Pala_min, "instanteInicioTW_Pala_min"));
            output.Append(Variable.EscribeArregloEnString(instanteTerminoTW_Pala_min, "instanteTerminoTW_Pala_min"));
            output.Append(Variable.EscribeArregloEnString(numTimeWindowsPorDestino, "numTimeWindowsPorDestino"));
            output.Append(Variable.EscribeArregloEnString(instanteInicioTW_Dest_min, "instanteInicioTW_Dest_min"));
            output.Append(Variable.EscribeArregloEnString(instanteTerminoTW_Dest_min, "instanteTerminoTW_Dest_min"));
            output.Append(Variable.EscribeArregloEnString(numTrayectoriasPorArco, "numTrayectoriasPorArco"));
            output.Append(Variable.EscribeArregloEnString(instanteInicioTrayectoria_min, "instanteInicioTrayectoria_min"));
            output.Append(Variable.EscribeArregloEnString(instanteTerminoTrayectoria_min, "instanteTerminoTrayectoria_min"));
            output.Append(Variable.EscribeArregloEnString(toneladasRecibidas, "toneladasRecibidas"));
            output.Append(Variable.EscribeArregloEnString(nodoInicioCarga, "nodoInicioCarga"));
            output.Append(Variable.EscribeArregloEnString(nodoInicioDescarga, "nodoInicioDescarga"));
            output.Append(Variable.EscribeArregloEnString(nodoLlegadaColaPala, "nodoLlegadaColaPala"));
            output.Append(Variable.EscribeArregloEnString(nodoLlegadaColaDestino, "nodoLlegadaColaDestino"));
            output.Append(Variable.EscribeArregloEnString(tiempoViajeMinimoFlujoLibreEnLlegarANodo, "tiempoViajeMinimoFlujoLibreEnLlegarANodo"));
            output.Append(Variable.EscribeArregloEnString(tiempoViajeMaximoFlujoLibreEnLlegarANodo, "tiempoViajeMaximoFlujoLibreEnLlegarANodo"));
            output.Append(Variable.EscribeArregloEnString(heuristica_Dispatch, "heuristica_Dispatch"));
            output.Append(Variable.EscribeArregloEnString(heuristica_DescargaAlDestinoMasAtrasado, "heuristica_DescargaAlDestinoMasAtrasado"));
            output.Append(Variable.EscribeArregloEnString(instanteInicioAproxCamionCiclo, "instanteInicioAproxCamionCiclo"));


            

            #endregion

            string rutaCarpeta = "Datos/";
            string nombreArchivo = "distancias"+".dat";
            Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta, nombreArchivo, output.ToString());
            string ruta = rutaCarpeta + "/" + nombreArchivo;
            return ruta;

        }        
        /// <summary>
        /// Genera el .dat de la fase de LP de Dispatch
        /// </summary>
        /// <returns></returns>
        private string GenerarDatFaseLPDispatch()
        {
            try
            {
                StringBuilder output = new StringBuilder();

                #region SETS
                output.Append(PALAS);
                output.Append(DESTINOS);
                output.Append(REQUERIMIENTOS);
                #endregion
                #region PARÁMETROS
                intervaloPala = 50;
                int numeroCamiones = _camiones.Count;
                double[] tiempoCarga_hra = new double[_palas.Count];
                double[] tiempoDescarga_hra = new double[_destinos.Count];
                double[,] tiempoViajeCargado_h = new double[_palas.Count, _destinos.Count];
                double[,] tiempoViajeVacio_h = new double[_destinos.Count, _palas.Count];
                double tolvaAVG = 0;
                int[] numCamPorTipo = new int[_tipoCamiones.Count];
                foreach(Camion C in _camiones)
                {
                    numCamPorTipo[C.TipoCamion.Id-1] ++;
                    tolvaAVG += C.Capacidad_Ton;
                }
                tolvaAVG = tolvaAVG / _camiones.Count;

                foreach(Pala P in _palas)
                {
                    double tCarga = 0;
                    foreach(TipoCamion TC in _tipoCamiones.Values)
                    {
                        tCarga += tiempoCargaTipoCamionPala_hra[TC.Id - 1, P.Id - 1]*numCamPorTipo[TC.Id-1];
                    }
                    tiempoCarga_hra[P.Id-1] = tCarga / _camiones.Count;
                }
                foreach (Destino D in _destinos)
                {
                    double tDescarga = 0;
                    foreach (TipoCamion TC in _tipoCamiones.Values)
                    {
                        tDescarga += tiempoDescargaTipoCamionDestino_hra[TC.Id - 1, D.Id - 1] * numCamPorTipo[TC.Id - 1];
                    }
                    tiempoDescarga_hra[D.Id - 1] = tDescarga / _camiones.Count;
                }
                foreach (Pala P in _palas)
                {
                    int j=P.Id-1;
                    foreach (Destino D in _destinos)
                    {
                        int d=D.Id-1;
                        double tVCargado = 0;
                        double tVVacio = 0;
                        foreach (TipoCamion TC in _tipoCamiones.Values)
                        { 
                            int k = TC.Id-1;
                            tVCargado += tiempoViajePorTipo_PD_h[k, j, d] * numCamPorTipo[k];
                            tVVacio += tiempoViajePorTipo_DP_h[k, d, j] * numCamPorTipo[k];
                        }
                        tiempoViajeCargado_h[j, d] = tVCargado/_camiones.Count;
                        tiempoViajeVacio_h[d, j] = tVVacio / _camiones.Count;
                    }
                }



                output.Append("param intervaloTonelaje:=" + intervaloTonelaje.ToString().Replace(',', '.') + ";\n");
                output.Append("param numeroCamiones:=" + numeroCamiones.ToString().Replace(',', '.') + ";\n");
                output.Append("param tolvaAVG:=" + tolvaAVG.ToString().Replace(',', '.') + ";\n");
                output.Append("param intervaloPala:=" + intervaloPala.ToString().Replace(',', '.') + ";\n");
                output.Append(Variable.EscribeArregloEnString(tiempoCarga_hra, "tiempoCarga_hra"));
                output.Append(Variable.EscribeArregloEnString(tiempoDescarga_hra, "tiempoDescarga_hra"));
                output.Append(Variable.EscribeArregloEnString(tiempoViajeCargado_h, "tiempoViajeCargado_h"));
                output.Append(Variable.EscribeArregloEnString(tiempoViajeVacio_h, "tiempoViajeVacio_h"));
                output.Append(Variable.EscribeArregloEnString(capacidadPala_tph, "capacidadPala_tph"));
                output.Append(Variable.EscribeArregloEnString(capacidadDestino_tph, "capacidadDestino_tph"));
                output.Append(Variable.EscribeArregloEnString(toneladasPorHoraSegunPlanDiario, "toneladasPorHoraSegunPlanDiario"));
                output.Append(Variable.EscribeArregloEnString(isMaterialPalaDescargableEnDestino, "isMaterialPalaDescargableEnDestino"));
                output.Append(Variable.EscribeArregloEnString(isPalaDestinoEnReq, "isPalaDestinoEnReq"));
                
                #endregion
                
                string rutaCarpeta = "Datos_Dispatch";
                string nombreArchivo = "LP.dat";
                Utiles.CrearArchivoEnEstaCarpeta(rutaCarpeta, nombreArchivo, output.ToString());
                string ruta = "Datos_Dispatch/LP.dat";                
                return ruta;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Actualiza la lista de camiones considerados
        /// </summary>        
        private void ActualizarCamionesConsiderados_Y_CiclosPorCamion(MetodoSolucion MS)
        {
            idsCamionesConsiderados = new List<int>();
            #region Genero esta lista
            //Ordeno los camiones
            _camionPorTiempoDisponible.Sort(delegate(Camion c1, Camion c2)
            {
                return c1.InstanteSiguienteDisponible_Minutos.CompareTo(c2.InstanteSiguienteDisponible_Minutos);
            });

            instanteActual_min = _camionPorTiempoDisponible[0].InstanteSiguienteDisponible_Minutos;
            idCamionSolicitaDespacho = _camionPorTiempoDisponible[0].Id;
            #endregion

            if (MS == MetodoSolucion.MIP)
            {
                #region Metodología según el número a considerar
                if (false)
                {
                    /*
                    foreach (Camion C in _camionPorTiempoDisponible)
                    {
                        if (idsCamionesConsiderados.Count < numCamionesConsiderados)
                        {
                            idsCamionesConsiderados.Add(C.Id);
                        }
                        else
                        {
                            break;
                        }
                    }*/
                }
                #endregion
                #region Metodología según el horizonte
                if (true)
                {
                    double THorizonte = instanteActual_min + horizontePlanificacion_min;
                    foreach (Camion C in _camionPorTiempoDisponible)
                    {
                        if (C.InstanteSiguienteDisponible_Minutos <= THorizonte)
                        {
                            idsCamionesConsiderados.Add(C.Id);
                        }
                    }
                    if (!idsCamionesConsiderados.Contains(idCamionSolicitaDespacho)) throw new Exception("Error no tiene el camión");
                }
                #endregion                
                CalcularNumeroDeCiclosPorCamionE_InstanteDeInicio();
            }
            else //PARA ALGUNA HEURÍSTICA
            {
                idsCamionesConsiderados.Add(idCamionSolicitaDespacho);
                #region Número de ciclos por camión
                Camion CAM = _camiones[idCamionSolicitaDespacho - 1];
                numCiclosPorCamion = new int[_camiones.Count];
                numCiclosPorCamion[CAM.Id - 1] = 1;
                #endregion
                #region Instante de inicio aproximado del ciclo
                instanteInicioAproxCamionCiclo = new double[_camiones.Count, 1];
                instanteInicioAproxCamionCiclo[idCamionSolicitaDespacho - 1, 0] = CAM.InstanteSiguienteDisponible_Minutos;
                #endregion
            }
        }
        /// <summary>
        /// Método que restaura los datos dinámicos del problema para empezar a resolver con otro método
        /// </summary>
        /// <param name="MS">El nuevo método que se empleará</param>
        private void Reset(MetodoSolucion MS)
        {
            instanteActual_min = 0;
            numProblema = 1;
            solucionAnterior = null;
            planDia.Reset();

            foreach (Camion C in _camiones)
            {
                C.Reset();
            }
            ActualizarCamionesConsiderados_Y_CiclosPorCamion(MS);

            foreach (Pala P in _palas)
            {
                P.Reset();
            }
            foreach (Destino D in _destinos)
            {
                D.Reset();
            }
            foreach (Ruta R in _rutas)
            {
                R.Reset();
            }

            _grafo.Reset();
        }
        /// <summary>
        /// Devuelve el destino en función del nodo que se pide
        /// </summary>
        /// <param name="idNodo"></param>
        /// <returns></returns>
        private Destino GetDestinoSegunNodo(int idNodo)
        {
            Destino DD = null;
            foreach (Destino D in _destinos)
            {
                if (D.IdNodo == idNodo)
                {
                    DD = D;
                    break;
                }
            }
            if (DD == null)
            {
                throw new Exception("NO se encontró un destino en ese nodo");
            }
            return DD;
        }
        /// <summary>
        /// Busca la velocidad más rápidad entre todos los tipos de camiones para recorrer el arco
        /// </summary>
        /// <param name="A">El Arco</param>
        /// <returns></returns>
        private double VelocidadMasRapidaParaArco(Arco A)
        {
            double vmax = -1;            
            foreach (TipoCamion TC in _tipoCamiones.Values)
            {
                double v = TC.GetVelocidadSegunStatusYPendiente_Kph(StatusCamion.Vacio, A.Pendiente);
                if (v >= vmax)
                {
                    vmax = v;
                }
            }
            return vmax;
        }
        /// <summary>
        /// Te dice cuál es la pala más cercana dado un destino inicial
        /// </summary>
        /// <param name="C"></param>
        /// <param name="DInicio"></param>
        /// <returns></returns>
        private double GetTViajeEsperadoAPalaMasCercanoSegunCamion_min(Camion C, Destino DInicio)
        {
            double tiempoMasRapido = double.PositiveInfinity;
            foreach(int idP in C.IdsPalasAsignables)
            {
                //Busco esta ruta
                Ruta R = GetRuta(idP, DInicio.Id, false);
                double tViaje = R.GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(C.TipoCamion.Id);
                double demora = R.GetDemoraPromedio_Minutos(instanteActual_min);
                tViaje += demora;
                if (tViaje < tiempoMasRapido)
                {
                    tiempoMasRapido = tViaje;
                }
            }
            return tiempoMasRapido;
        }
        /// <summary>
        /// Calcula el tiempo de viaje mínimo para llegar a un nodo. [Método que se corre en cada instancia de problema]
        /// </summary>
        private void CalcularTiemposDeViajeMinYMaxANodos()
        {
            // @ .mod
            // param tiempoViajeMinimoEnLlegarANodo{i in CAMIONES, maxCiclos, s in STATUS_CAMIONES, u in NODOS};
            #region TIEMPOS MÍNIMOS
            
            #region Inicializo en 9999
            int maxCiclos = 1;
            foreach (int i in idsCamionesConsiderados)
            {
                if (numCiclosPorCamion[i - 1] > maxCiclos)
                {
                    maxCiclos = numCiclosPorCamion[i - 1];
                }
            }
            tiempoViajeMinimoFlujoLibreEnLlegarANodo = new double[_camiones.Count, maxCiclos, _statusCamiones.Count, _grafo.Nodos.Count];
            for (int i = 0; i < tiempoViajeMinimoFlujoLibreEnLlegarANodo.GetLength(0); i++)
            {
                for (int c = 0; c < tiempoViajeMinimoFlujoLibreEnLlegarANodo.GetLength(1); c++)
                {
                    for (int s = 0; s < tiempoViajeMinimoFlujoLibreEnLlegarANodo.GetLength(2); s++)
                    {
                        for (int u = 0; u < tiempoViajeMinimoFlujoLibreEnLlegarANodo.GetLength(3); u++)
                        {
                            tiempoViajeMinimoFlujoLibreEnLlegarANodo[i, c, s, u] = 9999;
                        }
                    }
                }
            }
            #endregion

            try
            {
                foreach (int i in idsCamionesConsiderados)
                {
                    Camion C = _camiones[i - 1];
                    int idD0 = GetDestinoSegunNodo(C.IdNodoDisponible).Id;
                    int ciclos = numCiclosPorCamion[i - 1];
                    int numCiclosCreados = SecuenciasDeRutasSegunNumCiclosDInicio.Keys.Max();
                    if (ciclos > numCiclosCreados)
                    {
                        int numCiclosParaCrear = ciclos - numCiclosCreados;
                        for (int k = 0; k < numCiclosParaCrear; k++)
                        {
                            AgregarUnCicloDeSecuencias();
                        }
                    }
                    List<List<int>> ListaSecuenciaRutas = SecuenciasDeRutasSegunNumCiclosDInicio[ciclos][idD0];
                    foreach (List<int> Secuencia in ListaSecuenciaRutas)
                    {
                        double TViajeAcum = 0;
                        if (Secuencia.Count / 2 != ciclos) throw new Exception("Error en el número de ciclos");
                        for (int c = 1; c <= ciclos; c++)
                        {
                            int idR1 = Secuencia[2 * c - 2];
                            Ruta R1 = _rutas[idR1 - 1];
                            int idR2 = Secuencia[2 * c - 1];
                            Ruta R2 = _rutas[idR2 - 1];
                            SimularRuteoParaVerTiempos(R1, C, c, ref TViajeAcum, ref tiempoViajeMinimoFlujoLibreEnLlegarANodo, true);
                            SimularRuteoParaVerTiempos(R2, C, c, ref TViajeAcum, ref tiempoViajeMinimoFlujoLibreEnLlegarANodo, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region TIEMPOS MÁXIMOS

            #region Inicializo en -1
            tiempoViajeMaximoFlujoLibreEnLlegarANodo = new double[_camiones.Count, maxCiclos, _statusCamiones.Count, _grafo.Nodos.Count];
            for (int i = 0; i < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(0); i++)
            {
                for (int c = 0; c < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(1); c++)
                {
                    for (int s = 0; s < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(2); s++)
                    {
                        for (int u = 0; u < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(3); u++)
                        {
                            tiempoViajeMaximoFlujoLibreEnLlegarANodo[i, c, s, u] = -1;
                        }
                    }
                }
            }
            #endregion

            #region Calculo los tiempos máximos
            try
            {
                foreach (int i in idsCamionesConsiderados)
                {
                    Camion C = _camiones[i - 1];
                    int idD0 = GetDestinoSegunNodo(C.IdNodoDisponible).Id;
                    int ciclos = numCiclosPorCamion[i - 1];
                    int numCiclosCreados = SecuenciasDeRutasSegunNumCiclosDInicio.Keys.Max();
                    if (ciclos > numCiclosCreados)
                    {
                        int numCiclosParaCrear = ciclos - numCiclosCreados;
                        for (int k = 0; k < numCiclosParaCrear; k++)
                        {
                            AgregarUnCicloDeSecuencias();
                        }
                    }
                    List<List<int>> ListaSecuenciaRutas = SecuenciasDeRutasSegunNumCiclosDInicio[ciclos][idD0];
                    foreach (List<int> Secuencia in ListaSecuenciaRutas)
                    {
                        double TViajeAcum = 0;
                        if (Secuencia.Count / 2 != ciclos) throw new Exception("Error en el número de ciclos");
                        for (int c = 1; c <= ciclos; c++)
                        {
                            int idR1 = Secuencia[2 * c - 2];
                            Ruta R1 = _rutas[idR1 - 1];
                            int idR2 = Secuencia[2 * c - 1];
                            Ruta R2 = _rutas[idR2 - 1];
                            SimularRuteoParaVerTiempos(R1, C, c, ref TViajeAcum, ref tiempoViajeMaximoFlujoLibreEnLlegarANodo, false);
                            SimularRuteoParaVerTiempos(R2, C, c, ref TViajeAcum, ref tiempoViajeMaximoFlujoLibreEnLlegarANodo, false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region Los -1 los llevo a infinito (nunca se pudo llegar a ese nodo
            for (int i = 0; i < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(0); i++)
            {
                for (int c = 0; c < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(1); c++)
                {
                    for (int s = 0; s < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(2); s++)
                    {
                        for (int u = 0; u < tiempoViajeMaximoFlujoLibreEnLlegarANodo.GetLength(3); u++)
                        {
                            if (tiempoViajeMaximoFlujoLibreEnLlegarANodo[i, c, s, u] == -1)
                            {
                                tiempoViajeMaximoFlujoLibreEnLlegarANodo[i, c, s, u] = 99999;
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion
        }
        /// <summary>
        /// Simula asignar el camión a esa ruta en este ciclo para ver los tiempos mínimos o máximos de viaje
        /// </summary>
        /// <param name="R">La ruta</param>
        /// <param name="C">El camión</param>
        /// <param name="numCiclo">El número del ciclo, >=1 </param>
        /// <param name="TViajeAcum">El tiempo de viaje en minutos acumulado que llevamos</param>
        /// <param name="tiempoViajeMinimoEnLlegarANodo">[i,c,s,u]</param>
        /// <param name="VerTiemposMinimos">True si se quieren ver los tiempos mínimos, false si los máximos</param>
        private void SimularRuteoParaVerTiempos(Ruta R, Camion C, int numCiclo, ref double TViajeAcum, ref double[, , ,] tiempoViajeEnLlegarANodo, bool VerTiemposMinimos)
        {
            try
            {
                int i = C.Id - 1;
                int s = R.EsRutaHaciaDestino ? 1 : 0;
                StatusCamion SC = s == 1 ? StatusCamion.Cargado : StatusCamion.Vacio;
                int c = numCiclo - 1;
                int idNodoInicial = R.Arcos[0].Id_Nodo_Inicial;
                int u = idNodoInicial - 1;

                if (VerTiemposMinimos)
                {
                    tiempoViajeEnLlegarANodo[i, c, s, u] = Math.Min(TViajeAcum, tiempoViajeEnLlegarANodo[i, c, s, u]);
                }
                else
                {
                    tiempoViajeEnLlegarANodo[i, c, s, u] = Math.Max(TViajeAcum, tiempoViajeEnLlegarANodo[i, c, s, u]);
                }

                int k = C.TipoCamion.Id - 1;
                foreach (Arco A in R.Arcos)
                {
                    int v = A.Id_Nodo_Final - 1;
                    double tviaje = tiempoViajeTipoCamionArco_min[k, s, u, v];
                    #region Obtengo tiempo de viaje
                    if (A.EsDeEspera)
                    {
                        tviaje = 0;
                    }
                    else if (A.EsDeAtencion)
                    {
                        if (s == 0) //Si voy a una pala
                        {
                            tviaje = tiempoCargaTipoCamionPala_min[k, R.IdPala - 1];
                        }
                        else //Voy a un destino
                        {
                            tviaje = tiempoDescargaTipoCamionDestino_min[k, R.IdDestino - 1];
                        }
                    }
                    #endregion

                    TViajeAcum += tviaje;
                    if (VerTiemposMinimos)
                    {
                        tiempoViajeEnLlegarANodo[i, c, s, v] = Math.Min(TViajeAcum, tiempoViajeEnLlegarANodo[i, c, s, v]);
                    }
                    else
                    {
                        tiempoViajeEnLlegarANodo[i, c, s, v] = Math.Max(TViajeAcum, tiempoViajeEnLlegarANodo[i, c, s, v]);
                    }
                    u = v;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Heurísticas
        /// <summary>
        /// Genera el vector asignacionDispatch[i,s,r]: 1 si el camión con status s utiliza la ruta r mediante la heurística de DISPATCH
        /// </summary>
        /// <returns></returns>
        private int[, ,] HeuristicaDispatch()
        {
            int[, ,] Heuristica = new int[_camiones.Count, _statusCamiones.Count, _rutas.Count];

            #region Versión 2.0
            // Se consideran todos los camiones posibles, pero se evalúa de a un requerimiento a la vez                        

            List<Camion> ListaCamiones = new List<Camion>();
            List<Ruta_Dispatch> RutasCargadasConsideradas = new List<Ruta_Dispatch>();
            #region Creo la lista de camiones y las rutas cargadas posibles
            foreach (Camion C in _camiones)
            {
                ListaCamiones.Add(C);
            }
            foreach (Ruta_Dispatch RD in _RutasDispatch_Cargadas)
            {
                RutasCargadasConsideradas.Add(RD);
            }
            #endregion
            #region Actualizo el need time de cada ruta Pala -> Destino
            foreach (Ruta_Dispatch RD in _RutasDispatch_Cargadas)
            {
                Ruta RGeneral = _rutas[RD.Id - 1];
                RD.NumeroDeVecesSeleccionada = 0;
                Pala P = _palas[RD.IdPala - 1];
                Destino D = _destinos[RD.IdDestino - 1];
                /* 
                 * NT = L + F(A-R)/P
                 * L -> Último instante en que un camión fue asignado al destino [min]
                 * F -> Porcentaje de flujo de esta ruta sobre el total que tiene la pala [%]         
                 * S -> Flujo de toneladas que sale de la pala según plan del LP [ton/min]
                 * A -> Toneladas asignadas a esta pala hasta el tiempo L [ton]                  
                 * R -> Toneladas que debieran haber sido asignadas a esta ruta en este instante [ton]
                 * P -> Flujo de la ruta según lo especificado por el LP de DISPATCH
                 */
                double NT, L, F, S, A, R, Pi;
                #region Inicializo los parámetros
                //VERSIÓN ANTES DEL 30/6. --> Produce resultados más estables en términos de la ley
                L = P.UltimoInstanteAsignacion;                 //  [min]
                S = P.TasaExcavacionSegun_DispatchLP_tph;       //  [tph]
                F = RD.FlujoTotal_tph / S;                      //  [%]
                A = RGeneral.ToneladasEnviadas;                 //  [ton]
                Pi = RD.FlujoTotal_tph / 60;                    //  [ton/min]
                R = Pi * instanteActual_min;                     //  [ton]
                
                
                // VERSIÓN EL 30/6
                
                /*
                L = P.UltimoInstanteAsignacion;                 //  [min]
                S = P.TasaExcavacionSegun_DispatchLP_tph;       //  [tph]
                F = RD.FlujoTotal_tph / S;                      //  [%]

                #region Cálculo A
                A = 0;
                foreach (Ruta_Dispatch RD2 in _RutasDispatch_Cargadas)
                {
                    Ruta RGeneral2 = _rutas[RD2.Id - 1];
                    if (RD2.IdPala == P.Id)
                    {
                        A += RGeneral2.ToneladasEnviadas;
                    }
                }
                #endregion
                R = (S / 60) * instanteActual_min;              //  [ton]
                Pi = RD.FlujoTotal_tph / 60;                    //  [ton/min]
                */

                #endregion
                #region Versiones anteriores
                // v1
                //NT = (F/S)*(A - R);

                // v2
                // VERSIÓN ACTUAL: EL NT CORRESPONDE AL PORCENTAJE CUMPLIDO DE LA RUTA
                /*
                double TonNecesita = instanteActual_min * F;
                if (TonNecesita == 0)
                {
                    NT = 0;
                }
                else
                {
                    NT = (A / TonNecesita);
                }
                */
                #endregion

                //v3: La ruta más needy es la que define inicialmente White y Olson (1992)
                NT = L + F*(A - R) / Pi;
                RD.NeedTime = NT;
            }
            #endregion
            #region Ordeno la lista de las rutas PD según Need Time ASCENDENTE
            RutasCargadasConsideradas.Sort(delegate(Ruta_Dispatch r1, Ruta_Dispatch r2)
            {
                return r1.NeedTime.CompareTo(r2.NeedTime);
            });

            #endregion

            Pala PalaAsignada = null;
            Destino DestinoAsignado = null;
            Dictionary<int, Ruta> IdsCamionesAsignadosRuta = new Dictionary<int, Ruta>();
            bool primeraVEz = true;
            Ruta_Dispatch RutaEscogida = null;
            while (true)
            {
                #region Acá está la ruta más needy.
                if (primeraVEz)
                {
                    RutaEscogida = GetNeediestRute(RutasCargadasConsideradas);
                    primeraVEz = false;
                }
                else
                {
                    RutaEscogida = RutasCargadasConsideradas[0];
                }
                RutaEscogida.NumeroDeVecesSeleccionada++;
                #endregion
                Destino D = _destinos[RutaEscogida.IdDestino - 1];
                Pala P = _palas[RutaEscogida.IdPala - 1];

                bool SeSobrepasoElDeltaMaximo = false;
                #region Analizo si se sobrepasó la diferencia máxima de cumplimiento porcentual de los Reqs
                double porcentajeCumplimientoMin = RutaEscogida.NeedTime;
                foreach (Ruta_Dispatch RD in _RutasDispatch_Cargadas)
                {
                    double porcentaje = RD.NeedTime;
                    double diferencia = porcentaje - porcentajeCumplimientoMin;
                    if (diferencia >= Configuracion.MaxDeltaPorcentual_DISPATCH / 100)
                    {
                        SeSobrepasoElDeltaMaximo = true;
                    }
                }
                #endregion

                //Ahora calculo las lost-tons de cada camión en esta ruta.
                //Luego escojo el camión que tiene las menos lost tons y lo pseudo asigno a esta ruta
                Camion MejorCamion = null;
                #region Cosas varias
                double LostTonsDelMejorCamion = Double.PositiveInfinity;
                bool EscribirArchivo = numProblema == -1;
                string T = "\t";
                string SL = Environment.NewLine; //Salto de Línea
                string contenidoArchivo = "";
                if (EscribirArchivo)
                {
                    contenidoArchivo += "Ruta" + RutaEscogida.Id + SL;
                    contenidoArchivo += "C\tTC\tTR\tRT\tTI\tET\tSR\tSI\tLT" + SL;
                }
                #endregion

                if (!SeSobrepasoElDeltaMaximo)
                {
                    #region Obtengo el mejor camión para esta ruta
                    foreach (Camion C in ListaCamiones)
                    {
                        if (!IdsCamionesAsignadosRuta.ContainsKey(C.Id))
                        {
                            #region Obtengo el mejor camión que se puede asignar a la ruta (Minimiza las lost tons)
                            #region LostTons, TC, RT, TR, TI, ET, SR, SI
                            double LostTons, TC, RT, TR, TI, ET, SR, SI;
                            /* LostTons = TC*(TR/RT)*(TI+ET)*(SR*SI) 
                             * TC = Proporción de la capacidad de este camión con repecto a la capacidad promedio
                             * TR = Total dig rate de todas las palas de la mina [ton/hra]
                             * RT = Total de camiones requeridos según el plan estacionario
                             * TI = Demora y espera del camión si se asigna a esta ruta (espera en cola atención y descarga en destino) [hra]
                             * ET = The additional travel time that the truck must make to reach the shovel instead of the nearest one [hra]
                             * SR = Suma de los flujos que salen o llegan a la pala [ton/hra]
                             * SI = Tiempo de ocio esperado por la pala si este camión se asigna al neediest path [hra]
                             */
                            #endregion
                            TC = C.Capacidad_Ton/capacidadPromedio;
                            TR = _tasaDeExcavacionTotal;
                            RT = _camiones.Count;

                            Destino DActual = GetDestinoSegunNodo(C.IdNodoDisponible);

                            // TI: Demora y espera del camión si se asigna a esta ruta (espera en cola pala y destino) [hra]
                            InfoSimulaRuteo ISR = SimularDespachoCamion(C, P.Id, D.Id, false, false);
                            TI = ISR.DemoraTotal_min / 60;

                            //Asigno el extra travel time [hra]
                            double TViajeAEstaPala_min = ISR.TiempoLLegadaAPala_min;
                            double TViajeAPalaMasCercana_min = double.PositiveInfinity;
                            foreach (Ruta_Dispatch RD in RutasCargadasConsideradas)
                            {
                                double tViajeAPala = SimularDespachoCamion(C, RD.IdPala, RD.IdDestino, false, true).TiempoLLegadaAPala_min;
                                TViajeAPalaMasCercana_min = Math.Min(TViajeAPalaMasCercana_min, tViajeAPala);
                            }
                            ET = (TViajeAEstaPala_min - TViajeAPalaMasCercana_min) / 60;

                            //Cálculo del tiempo de ocio esperado por la pala si se asigna este camión a ella [hra]
                            SI = ISR.TiempoOciosoPala_min / 60;
                            SR = P.TasaExcavacionSegun_DispatchLP_tph;

                            // FORMA ORIGINAL
                            LostTons = TC * (TR / RT) * (TI + ET) + SR * SI;

                            // Forma actualizada que pondera según la productividad del camión
                            //double TCicloFlujoLibre_h = tiempoDeCicloFlujoLibre_KD0PD[C.TipoCamion.Id - 1, DActual.Id - 1, P.Id - 1, D.Id - 1] / 60;
                            //double Productividad_tph = C.Capacidad_Ton / TCicloFlujoLibre_h;
                            //LostTons = Productividad_tph * (TI + ET) + SR * SI;


                            if (EscribirArchivo)
                            {
                                #region Escribo la info
                                contenidoArchivo +=
                                    TI.ToString().Replace(',', '.') + T +
                                    ET.ToString().Replace(',', '.') + T +
                                    SR.ToString().Replace(',', '.') + T +
                                    SI.ToString().Replace(',', '.') + T +
                                    LostTons.ToString().Replace(',', '.') + SL
                                    ;
                                #endregion
                            }

                            if (LostTons < LostTonsDelMejorCamion)
                            {
                                LostTonsDelMejorCamion = LostTons;
                                MejorCamion = C;
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                { 
                    //El camión se asigna directamente
                    MejorCamion = _camiones[idCamionSolicitaDespacho - 1];
                }


                if (EscribirArchivo)
                {
                    Utiles.CrearArchivoEnEstaCarpeta(GetNombreIdentificador(MetodoSolucion.DISPATCH), "LostTons_" + numProblema + ".txt", contenidoArchivo);
                }

                #region Asigno el camión recién obtenido al neediest path. Si este camión era el que solicitaba despacho termino el algoritmo
                if (MejorCamion.Id == idCamionSolicitaDespacho)
                {
                    PalaAsignada = P;
                    DestinoAsignado = D;

                    //Remuevo lo asignado por DISPATCH
                    foreach (int id in IdsCamionesAsignadosRuta.Keys)
                    {
                        Ruta R0 = IdsCamionesAsignadosRuta[id];
                        Camion C0 = _camiones[id - 1];
                        Pala P0 = _palas[R0.IdPala - 1];
                        Destino D0 = _destinos[R0.IdDestino-1];
                        DesSimularDespachoCamion(C0, P0.Id, D0.Id);
                    }

                    break;
                }
                else //Paso este path al último y saco al camión de la lista de posibilidades
                {
                    IdsCamionesAsignadosRuta.Add(MejorCamion.Id, GetRuta(P.Id, D.Id, true));
                    //Vuelvo a simular el despacho
                    SimularDespachoCamion(_camiones[MejorCamion.Id - 1], P.Id, D.Id, true, false);

                    RutasCargadasConsideradas.RemoveAt(0);
                    RutasCargadasConsideradas.Add(RutaEscogida);
                }
                #endregion

            }
            #region Genero el arreglo
            Heuristica[idCamionSolicitaDespacho - 1, 1, RutaEscogida.Id - 1] = 1; //[i,s,r]
            Destino DestinoActual = GetDestinoSegunNodo(_camiones[idCamionSolicitaDespacho - 1].IdNodoDisponible);

            //Busco la ruta vacía que usa el camión para llegar a la pala
            Ruta RVacia = GetRuta(PalaAsignada.Id, DestinoActual.Id, false);
            Heuristica[idCamionSolicitaDespacho - 1, 0, RVacia.Id - 1] = 1;
            #endregion




            #endregion

            return Heuristica;
        }
        /// <summary>
        /// Saca los datos del camión del grafo: Arcos, palas y destinos en que estuvo
        /// </summary>
        /// <param name="C"></param>
        private void DesSimularDespachoCamion(Camion C, int idPala, int idDestino)
        {
            Destino D0 = GetDestinoSegunNodo(C.IdNodoDisponible);
            Ruta HaciaPala = GetRuta(idPala, D0.Id, false);
            Ruta HaciaDestino = GetRuta(idPala, idDestino, true);

            Pala P = _palas[idPala - 1];
            P.VentanasDeAtencionUsadasPorCam_DISPATCH.Remove(C.Id);

            Destino D = _destinos[idDestino - 1];
            D.VentanasDeAtencionUsadasPorCam_DISPATCH.Remove(C.Id);

            foreach (Arco A in HaciaPala.Arcos)
            {
                if (A.EsDeTransito)
                {
                    if (A.TrayectoriaPorCamion.ContainsKey(C.Id))
                    {
                        A.TrayectoriaPorCamion.Remove(C.Id);
                    }
                    else
                    {
                        throw new Exception("Raro, no estaba el camión en la trayectoria");
                    }
                }
            }
            foreach (Arco A in HaciaDestino.Arcos)
            {
                if (A.EsDeTransito)
                {
                    if (A.TrayectoriaPorCamion.ContainsKey(C.Id))
                    {
                        A.TrayectoriaPorCamion.Remove(C.Id);
                    }
                    else
                    {
                        throw new Exception("Raro, no estaba el camión en la trayectoria");
                    }
                }
            }

        }
        /// <summary>
        /// Simula enviar este camión por la red para ver sus demoras y esperas [min]
        /// </summary>
        /// <param name="C">El camión</param>
        /// <param name="idPala">Id de la pala</param>
        /// <param name="idDestino">Id del destino</param>
        /// <param name="guardarTrayectoriasYAtenciones">True para almacenar las trayectorias y ventanas de atención</param>
        /// <param name="SoloHastaLlegadaAPala">True para terminar el algoritmo cuando el camión llegó a la pala</param>
        /// <returns></returns>
        private InfoSimulaRuteo SimularDespachoCamion(Camion C, int idPala, int idDestino, bool guardarTrayectoriasYAtenciones, bool SoloHastaLlegadaAPala)
        {
            Destino D0 = GetDestinoSegunNodo(C.IdNodoDisponible);
            Ruta HaciaPala = GetRuta(idPala, D0.Id, false);
            Ruta HaciaDestino = GetRuta(idPala, idDestino, true);

            double Wait = 0; 
            double TOcioPala = 0;
            double TiempoLlegadaPala = 0;
            double Tiempo = C.InstanteSiguienteDisponible_Minutos;
            StatusCamion S = StatusCamion.Vacio;
            
            foreach (Arco A in HaciaPala.Arcos)
            {
                #region Inicio vacío desde D0 hasta la pala
                double TiempoArco = GetTiempoViaje(A, C.TipoCamion.Id, S, HaciaPala);
                double T1 = Tiempo;
                double T2_min = Tiempo + TiempoArco;
                double T2 = T2_min; //El T2 final. <------ Se debe asignar

                if (A.EsDeTransito || A.EsDeEspera)
                {
                    #region Caso: Arco de tránsito o de espera
                    List<TimeWindow> TWS = A.TrayectoriasConSimuladas();
                    for (int t = 0; t < TWS.Count; t++ )
                    {
                        #region Intento estar antes que todas las trayectorias
                        TimeWindow TW = TWS[t];
                        if (T1 <= TW.Inicio_min && T2_min <= TW.Termino_min) //Estoy ubicado antes.
                        {
                            T2 = T2_min;

                            //Si estoy antes que esta trayectoria estaré antes que todas las que vienen, por lo tanto termino.
                            break;
                        }
                        else //Estoy ubicado después de la trayectoria
                        {
                            //Entonces lo más temprano que puedo salir es cuando termina la trayectoria (lo más tarde es infinito...)
                            T2 = Math.Max(TW.Termino_min, T2_min);                            
                        }
                        #endregion
                    }
                    //Asigno esta trayectoria al arco                    
                    if (A.EsDeTransito && guardarTrayectoriasYAtenciones)
                    {
                        TimeWindow NuevaTrayectoria = new TimeWindow(T1, T2, C.Id, numProblema);
                        A.TrayectoriaPorCamion.Add(C.Id, NuevaTrayectoria);
                    }
                    #endregion
                }
                else //Estoy en la pala. Tengo que escoger una ventana de tiempo para atenderme
                {
                    #region Me atiendo en la pala
                    // Lo más temprano que puedo empezar la atención es T1
                    // Lo más temprano que puedo terminar la atención es T2_min
                    Pala P = _palas[idPala-1];
                    TiempoLlegadaPala = T1;
                    double TiempoCarga = TiempoArco;
                    bool encontreAtencion = false;
                    TimeWindow Atencion = new TimeWindow(-1, -1, -1,-1);
                    foreach (TimeWindow TW in P.VentanasDeTiempoDisponibles_ConsideraSimulacionDispatch)
                    {
                        #region Veo si me puedo atender en esta ventana de tiempo
                        double TiempoMasTempranoQuePuedeEmpezar = Math.Max(TW.Inicio_min, T1);
                        double TiempoParaCargar = TW.Termino_min - TiempoMasTempranoQuePuedeEmpezar;
                        if (TiempoCarga <= TiempoParaCargar)
                        {
                            encontreAtencion = true;
                            T2 = TiempoMasTempranoQuePuedeEmpezar + TiempoCarga;
                            Atencion.Inicio_min = TiempoMasTempranoQuePuedeEmpezar;
                            Atencion.Termino_min = T2;
                            double MinutosOcio = Math.Max(0, T1 - TW.Inicio_min);
                            TOcioPala += MinutosOcio;
                            break;
                        }
                        else
                        {
                            TOcioPala += TW.LargoIntervalo_Minutos;
                        }
                        #endregion
                    }
                    if (!encontreAtencion)
                    {
                        throw new Exception("Problema: No me pude atender en la pala");
                    }
                    if (guardarTrayectoriasYAtenciones)
                    {
                        //Agrego la atención simulada a DISPATCH
                        P.VentanasDeAtencionUsadasPorCam_DISPATCH.Add(C.Id, Atencion);
                    }
                    #endregion
                }
                Tiempo = T2;
                double demora = T2 - T2_min;
                Wait += demora;
                #endregion
            }

            if (!SoloHastaLlegadaAPala)
            {
                S = StatusCamion.Cargado;
                foreach (Arco A in HaciaDestino.Arcos)
                {
                    #region Voy cargado desde la pala al destino
                    double TViaje = GetTiempoViaje(A, C.TipoCamion.Id, S, HaciaDestino);
                    double T1 = Tiempo;
                    double T2_min = Tiempo + TViaje;
                    double T2 = T2_min; //El T2 final

                    if (A.EsDeTransito || A.EsDeEspera)
                    {
                        #region Caso: Arco de tránsito o de espera
                        List<TimeWindow> TWS = A.TrayectoriasConSimuladas();
                        for (int t = 0; t < TWS.Count; t++)
                        {
                            #region Intento estar antes que todas las trayectorias
                            TimeWindow TW = TWS[t];
                            if (T1 <= TW.Inicio_min && T2_min <= TW.Termino_min) //Estoy ubicado antes y alcanzo a salir antes de que este tipo
                            {
                                //Asigno el T2
                                T2 = T2_min;
                                break;
                            }
                            else //Estoy ubicado después de la trayectoria
                            {
                                //Entonces lo más temprano que puedo salir es cuando termina la trayectoria (lo más tarde es infinito...)
                                T2 = Math.Max(TW.Termino_min, T2_min);
                            }
                            #endregion
                        }
                        //Asigno esta trayectoria al arco                    
                        if (A.EsDeTransito && guardarTrayectoriasYAtenciones)
                        {
                            TimeWindow NuevaTrayectoria = new TimeWindow(T1, T2, C.Id, numProblema);
                            A.TrayectoriaPorCamion.Add(C.Id, NuevaTrayectoria);
                        }
                        #endregion
                    }
                    else //Estoy en el destino. Tengo que escoger una ventana de tiempo para atenderme
                    {
                        #region Me atiendo en el destino
                        // Lo más temprano que puedo empezar la atención es T1
                        // Lo más temprano que puedo terminar la atención es T2_min
                        Destino D = _destinos[idDestino - 1];
                        double TiempoDescarga = TViaje;
                        bool encontreAtencion = false;
                        TimeWindow Atencion = new TimeWindow(-1, -1, -1,-1);
                        foreach (TimeWindow TW in D.VentanasDeTiempoDisponibles_ConsideraSimulacionDispatch)
                        {
                            #region Veo si me puedo atender en esta ventana de tiempo
                            double TiempoMasTempranoQuePuedeEmpezar = Math.Max(TW.Inicio_min, T1);
                            double TiempoParaDescargar = TW.Termino_min - TiempoMasTempranoQuePuedeEmpezar;
                            if (TiempoDescarga <= TiempoParaDescargar)
                            {
                                encontreAtencion = true;
                                T2 = TiempoMasTempranoQuePuedeEmpezar + TiempoDescarga;
                                Atencion.Inicio_min = TiempoMasTempranoQuePuedeEmpezar;
                                Atencion.Termino_min = T2;
                                break;
                            }
                            #endregion
                        }
                        if (!encontreAtencion)
                        {
                            throw new Exception("Problema: No me pude atender en este destino");
                        }
                        if (guardarTrayectoriasYAtenciones)
                        {
                            //Agrego la atención simulada a DISPATCH
                            D.VentanasDeAtencionUsadasPorCam_DISPATCH.Add(C.Id, Atencion);
                        }
                        #endregion
                    }
                    Tiempo = T2;
                    double demora = T2 - T2_min;
                    Wait += demora;
                    #endregion
                }
            }

            InfoSimulaRuteo ISR = new InfoSimulaRuteo(Wait, TOcioPala, TiempoLlegadaPala);
            return ISR;
        }
        /// <summary>
        /// Retorna el tiempo de viaje del arco
        /// </summary>
        /// <param name="A"></param>
        /// <param name="idTipoCamion"></param>
        /// <param name="S"></param>
        /// <returns></returns>
        private double GetTiempoViaje(Arco A, int idTipoCamion, StatusCamion S, Ruta R)
        {
            double TViaje = 0;
            if (A.EsDeTransito)
            {
                TViaje = tiempoViajeTipoCamionArco_min[idTipoCamion - 1, (int)S - 1, A.Id_Nodo_Inicial - 1, A.Id_Nodo_Final - 1];
            }
            else if (A.EsDeEspera)
            {
                TViaje = 0;
            }
            else if (A.EsDeAtencion)
            {
                if (S == StatusCamion.Vacio)
                {
                    //Voy llegando a una pala
                    int idP = R.IdPala;
                    TViaje = tiempoCargaTipoCamionPala_min[idTipoCamion - 1, idP - 1];
                }
                else
                { 
                    //Voy llegando a un destino
                    int idD = R.IdDestino;
                    TViaje = tiempoDescargaTipoCamionDestino_min[idTipoCamion - 1, idD - 1];
                }
            }
            return TViaje;
        }
        /// <summary>
        /// Identifica la ruta a partir de la pala el destino y el orden de visita
        /// </summary>
        /// <param name="P"></param>
        /// <param name="D"></param>
        /// <param name="EsRutaHaciaDestino"></param>
        /// <returns></returns>
        private Ruta GetRuta(int idP, int idD, bool EsRutaHaciaDestino)
        {
            int idRuta = IdRutaSegun_HaciaDestino_Pala_Dest[EsRutaHaciaDestino][idP - 1, idD - 1];
            Ruta R = _rutas[idRuta - 1];
            return R;
        }
        /// <summary>
        /// Genera la heurística que minimiza los req de los destinos
        /// </summary>
        /// <returns></returns>
        private int[, ,] Heuristica_DescargaAlRequerimientoMasAtrasado()
        {
            /*  Se identifica el requerimiento más atrasado según el PD. Luego se envía el camión a este REQ
             */

            Camion C = _camiones[idCamionSolicitaDespacho - 1];
            int k = C.TipoCamion.Id;
            Destino D0 = GetDestinoSegunNodo(C.IdNodoDisponible);
            Requerimiento R_Atrasado = null;
            #region Identifico el requerimiento más atrasado
            List<Requerimiento> Reqs = new List<Requerimiento>(planDia.Requerimientos);
            Reqs.Sort(delegate(Requerimiento r1, Requerimiento r2)
            {
                return r1.PorcentajeActual.CompareTo(r2.PorcentajeActual);
            });

            //Desempato por el menor tiempo de ciclo
            double MenorPorcentajeCumplido = Reqs[0].PorcentajeActual;
            List<Requerimiento> MasAtrasados = new List<Requerimiento>();
            foreach (Requerimiento R in Reqs)
            {
                if (R.PorcentajeActual == MenorPorcentajeCumplido)
                {
                    MasAtrasados.Add(R);
                }
            }

            double menorTC = double.MaxValue;
            foreach (Requerimiento R in MasAtrasados)
            {
                double tiempoCiclo = tiempoDeCicloFlujoLibre_KD0PD[k - 1, D0.Id - 1, R.IdPala - 1, R.IdDestino - 1];
                if (tiempoCiclo < menorTC)
                {
                    menorTC = tiempoCiclo;
                    R_Atrasado = R;
                }
            }
            #endregion


            // Para cada uno de los destinos asignables de este REQ, busco el que provea el menor tiempo de ciclo
            int idP = R_Atrasado.IdPala;
            int idD = R_Atrasado.IdDestino;
            Ruta Vacio = GetRuta(idP, D0.Id, false);
            Ruta Cargado = GetRuta(idP, idD, true);

            #region Genero el arreglo
            int[, ,] Heuristica = new int[_camiones.Count, _statusCamiones.Count, _rutas.Count];
            Heuristica[idCamionSolicitaDespacho - 1, 0, Vacio.Id - 1] = 1;
            Heuristica[idCamionSolicitaDespacho - 1, 1, Cargado.Id - 1] = 1;
            #endregion
            return Heuristica;

        }
        /// <summary>
        /// Obtiene la ruta más needy, si hay empate selecciona la más corta
        /// </summary>
        /// <param name="Consideradas"></param>
        /// <returns></returns>
        private Ruta_Dispatch GetNeediestRute(List<Ruta_Dispatch> Consideradas_ordenadas)
        {
            Ruta_Dispatch RFinal = null;
            if (Consideradas_ordenadas.Count > 1)
            {
                List<Ruta_Dispatch> IgualNeedTime = new List<Ruta_Dispatch>();
                double minNeed = Consideradas_ordenadas[0].NeedTime;
                foreach (Ruta_Dispatch RD in Consideradas_ordenadas)
                {
                    if (RD.NeedTime == minNeed)
                    {
                        IgualNeedTime.Add(RD);
                    }
                }
                if (IgualNeedTime.Count > 1)
                {
                    //Las ordeno según la que represente el menor tiempo de ciclo para el camión que solicita despacho
                    double minTCiclo = double.PositiveInfinity;
                    Camion C = _camiones[idCamionSolicitaDespacho - 1];
                    int k = C.TipoCamion.Id;
                    Destino DActual = GetDestinoSegunNodo(C.IdNodoDisponible);                    
                    foreach (Ruta_Dispatch RD in IgualNeedTime)
                    {
                        //Calculo el tiempo de ciclo para esta RD
                        Pala P = _palas[RD.IdPala-1];
                        Destino D = _destinos[RD.IdDestino-1];
                        double tCiclo = 0;
                        double tviaje1, dem1, esp1, carga, tviaje2, dem2, esp2, descarga;

                        Ruta HaciaPala = GetRuta(RD.IdPala, DActual.Id, false);
                        tviaje1 = HaciaPala.GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                        dem1 = HaciaPala.GetDemoraPromedio_Minutos(instanteActual_min);
                        esp1 = P.GetEsperaEnColaPromedio_Minutos(instanteActual_min);
                        carga = tiempoCargaTipoCamionPala_min[k - 1, P.Id - 1];

                        Ruta HaciaDest = _rutas[RD.Id - 1];
                        tviaje2 = HaciaDest.GetTiempoDeViajeFlujoLibrePorTipoCamion_Minutos(k);
                        dem2 = HaciaDest.GetDemoraPromedio_Minutos(instanteActual_min);
                        esp2 = D.GetEsperaEnColaPromedio_Minutos(instanteActual_min);
                        descarga = tiempoDescargaTipoCamionDestino_min[k - 1, D.Id - 1];

                        tCiclo = tviaje1 + tviaje2 + dem1 + dem2 + esp1 + esp2 + carga + descarga;

                        if (tCiclo < minTCiclo)
                        {
                            minTCiclo = tCiclo;
                            RFinal = RD;
                        }
                    }
                }
                else
                {
                    RFinal = IgualNeedTime[0];
                }
            }
            else
            {
                RFinal = Consideradas_ordenadas[0];
            }           

            return RFinal;
        }
        #endregion

        #endregion
    }
}