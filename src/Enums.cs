namespace SolverDemo
{
    /// <summary>
    /// Indica los tipos de destino donde se puede dirigir un camión a descargar el material
    /// </summary>
    public enum TipoDestino
    {
        Botadero,
        Chancador,
        Stock
    };
    
    /// <summary>
    /// Indica los posibles métodos con que se puede solucionar el PDCMCA
    /// </summary>
    public enum MetodoSolucion
    {
        GeneraRuta,
        MIP,
        Asignacion_Fija,
        DRMA,
        DISPATCH
    };
   
    /// <summary>
    /// Indica los posibles Status del camión
    /// </summary>
    public enum StatusCamion
    {
        Cargado = 2,
        Vacio = 1
    };

    /// <summary>
    /// Las categorías de los mensajes que se imprimen en la consola
    /// </summary>
    public enum CategoriaMensaje
    { 
        Categoria_1,
        Categoria_2,
        Categoria_3,
        Categoria_4
    }
    
    /// <summary>
    /// Enum con el nombre de todas las variables indexadas del modelo
    /// </summary>
    public enum NombreVariablesIndexadas
    {
        //Variables típicas del modelo de asignación online
        AsignaCamionPala,
        AsignaCamionPalaDestino,
        AsignaCamionDestino,
        AsignaCamionStatusRuta,
        AsignaCamionStatusArco,
        AsignaCamionTWPala,
        AsignaCamionTWDestino,
        InstanteCamionStatusNodo,
        AsignaCamionAntesArco,
        AsignaCamionAntesTwArco,
        AsignaCamionDespuesTwArco,
        DemoraEnArco_Minutos,
        InstanteSalidaDestino,
        TiempoDeCiclo,
        CamionCargaAntesEnPala,
        EsperaEnColaPala_Minutos,
        CamionDescargaAntesEnDestino,
        EsperaEnColaDestino_Minutos,
        

        //Variables del modelo LP de DISPATCH
        TasaExcavacion_tph,
        FlujoToneladas_PD_tph,
        PenalidadFlujoFaltante        
        
    };
    /// <summary>
    /// Enum con las variables escalares del modelo
    /// </summary>
    public enum NombreVariablesEscalares
    {
        FO_CostosDeTransporte,
        FO_CostosPorTiempoDeCiclo,
        FO_CostosPorDemora,
        FO_CostosPorToneladasFaltantes,
        

        //Variables del modelo LP de DISPATCH
        FO_Productividad,
        FO_PenalidadFlujoTonFaltante,
        FO_PenalidadNoUtilizacionPalas,
        NumCamionesUsados
    }


    public enum TIPO_VARIABLE { ENTERA, DECIMAL };
    

    #region Parámetros del CPLEX

    public enum ParamCPLEX_ClockType
    { 
        /// <summary>
        /// CPLEX decide
        /// </summary>
        _0_Default = 0,
        /// <summary>
        /// Tiempo de CPU (para cosas paralelas)
        /// </summary>
        _1_CPU_Time = 1,
        /// <summary>
        /// Tiempo físico
        /// </summary>
        _2_Wall_Clock_Time
    }

    /// <summary>
    /// Tipo de búsqueda en el branch and bound
    /// </summary>
    public enum ParamCPLEX_MIPEmphasis
    {
        /// <summary>
        /// With the default setting of BALANCED, CPLEX works toward a rapid proof of an optimal
        /// solution, but balances that with effort toward finding high quality feasible solutions early
        /// in the optimization.
        /// </summary>
        _0_Balancea_Optimalidad_y_Factibilidad = 0,
        /// <summary>
        /// When this parameter is set to FEASIBILITY, CPLEX frequently will generate more feasible
        /// solutions as it optimizes the problem, at some sacrifice in the speed to the proof of optimality.
        /// </summary>
        _1_Enfatiza_Factibilidad = 1,
        /// <summary>
        /// When set to OPTIMALITY, less effort may be applied to finding feasible solutions early.
        /// </summary>
        _2_Enfatiza_Optimalidad = 2,
        /// <summary>
        /// With the setting BESTBOUND, even greater emphasis is placed on proving optimality through moving the best bound value, so that the detection of feasible solutions along the way becomes almost incidental.
        /// </summary>
        _3_Enfatiza_MejorarLaCota = 3,
        /// <summary>
        /// When the parameter is set to HIDDENFEAS, the MIP optimizer works hard to find high quality feasible solutions that are otherwise very difficult to find, so consider this setting when the FEASIBILITY setting has difficulty finding solutions of acceptable quality.
        /// </summary>
        _4_Enfatiza_EncontrarSolucionesEscondidas = 4
    }
    /// <summary>
    /// Used to select the type of generic priority order to generate when no priority order is present en el BranchBound.
    /// </summary>
    public enum ParamCPLEX_MIPOrderType
    {
        _0_NoGenerarOrden = 0,
        _1_DecreasingCost = 1,
        _2_IncreasingBoundRange = 2,
        _3_IncreasingCostPerCoefficient = 3
    }
    /// <summary>
    /// Description: MIP node selection strategy. 
    /// Used to set the rule for selecting the next node to process when backtracking.  
    /// </summary>
    public enum ParamCPLEX_NodeSelection
    {
        /// <summary>
        /// The depth-first search strategy chooses the most recently created node. 
        /// </summary>
        _0_DepthFirst = 0,
        /// <summary>
        /// The best-bound strategy chooses the node with the best objective function for the associated LP relaxation. 
        /// </summary>
        _1_BestBound = 1,
        /// <summary>
        /// The best-estimate strategy selects the node with the best estimate of the integer objective value that would be obtained from a node once all integer infeasibilities are removed. 
        /// </summary>
        _2_BestEstimate = 2,
        /// <summary>
        /// An alternative best-estimate search is also available.
        /// </summary>
        _3_AlternativeBestEstime = 3
    }
    /// <summary>
    /// Description: MIP node log display information. 
    /// Determines what CPLEX reports to the screen during mixed integer optimization. The amount of information displayed increases with increasing values of this parameter. A setting of 0 causes no node log to be displayed until the optimal solution is found. A setting of 1 displays an entry for each integer feasible solution found. Each entry contains the objective function value, the node count, the number of unexplored nodes in the tree, and the current optimality gap. A setting of 2 also generates an entry for every n-th node (where n is the setting of the MIP INTERVAL parameter). A setting of 3 additionally generates an entry for every nth node giving the number of cuts added to the problem for the previous INTERVAL nodes. A_Actual setting of 4 additionally generates entries for the LP root relaxation according to the set simplex display setting. A setting of 5 additionally generates entries for the LP subproblems, also according to the set simplex display setting. 
    /// </summary>
    public enum ParamCPLEX_MIPDisplay
    { 
        _0_NoDisplay = 0,
        _1_DisplayIntegerFeasibleSolutions = 1,
        _2_DisplayNodesUnder_MIPInterval = 2,
        _3_SameAs2_WithInfoAboutNodeCuts = 3,
        _4_SameAs3_WithLPSubproblemInfoAtRoot = 4,
        _5_SameAs4_WithLPSubproblemInfoAtNodes = 5,
    }
    /// <summary>
    /// Sets the parallel optimization mode. Possible modes are automatic, deterministic, and
    /// opportunistic.
    /// In this context, deterministic means that multiple runs with the same model at the same
    /// parameter settings on the same platform will reproduce the same solution path and results.
    /// In contrast, opportunisitc implies that even slight differences in timing among threads or
    /// in the order in which tasks are executed in different threads may produce a different solution
    /// path and consequently different timings or different solution vectors during optimization
    /// executed in parallel threads. In multithreaded applications, the opportunistic setting entails
    /// less synchronization between threads and consequently may provide better performance.
    /// By default, CPLEX® applies as much parallelism as possible while still achieving
    /// deterministic results. That is, when you run the same model twice on the same platform
    /// with the same parameter settings, you will see the same solution and optimization run. This
    /// condition is referred to as the deterministic mode.
    /// </summary>
    public enum ParamCplex_ParallelMode
    {
        __1_Opportunistic = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_Deterministic = 1
    }
    /// <summary>
    /// If set to 1 or 2, this parameter indicates that CPLEX should use advanced starting information when optimization is initiated.
    /// </summary>
    public enum ParamCplex_AdvancedStart
    {
        /// <summary>
        /// No utiliza información a priori
        /// </summary>
        _0_DoNotUseAdvancedStartInformation,
        /// <summary>
        /// For MIP models, setting 1 will cause CPLEX to continue with a partially explored MIP tree if one is available. If tree exploration has not yet begun, setting 1 specifies that CPLEX® should use a loaded MIP start, if available 
        /// </summary>
        _1_Default_UsarAdvancedBasis,
        /// <summary>
        /// For MIP models, Setting 2 retains the current incumbent (if there is one), re-applies presolve, and starts a new search from a new root.
        /// </summary>
        _2_UsarVectorInicial
    }

    #region Parámetros de Cortes
    /// <summary>
    /// Description: MIP MIR (mixed integer rounding) cut indicator. -- MIR cuts are generated by applying integer rounding on the coefficients of integer variables and the right-hand side of a constraint -- .
    /// Determines whether or not to generate MIR cuts for the problem. Setting the value to 0, the default, indicates that the attempt to generate MIR cuts should continue only if it seems to be helping. 
    /// </summary>
    public enum ParamCplex_Cuts_MIR
    { 
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: A MIP problem can be divided into two subproblems with disjunctive feasible regions of their LP relaxations by branching on an integer variable. Disjunctive cuts are inequalities valid for the feasible regions of LP relaxations of the subproblems, but not valid for the feasible region of LP relaxation of the MIP problem.
    /// Decides whether or not disjunctive cuts should be generated for the problem. Setting the
    /// value to 0 (zero), the default, indicates that the attempt to generate disjunctive cuts should
    /// continue only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_Disjuntive
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2,
        _3_GenerarMuyAgresivamente = 3
    }
    /// <summary>
    /// Description: A clique is a relationship among a group of binary variables such that at most one variable in the group can be positive in any integer feasible solution. Before optimization starts, ILOG CPLEX constructs a graph representing these relationships and finds maximal cliques in the graph.
    /// Decides whether or not clique cuts should be generated for the problem. Setting the value to 0 (zero), the default, indicates that the attempt to generate cliques should continue only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_Cliques
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2,
        _3_GenerarMuyAgresivamente = 3
    }
    /// <summary>
    /// Description: If a constraint takes the form of a knapsack constraint (that is, a sum of binary variables with nonnegative coefficients less than or equal to a nonnegative right-hand side), then there is a minimal cover associated with the constraint. A_Actual minimal cover is a subset of the variables of the inequality such that if all the subset variables were set to one, the knapsack constraint would be violated, but if any one subset variable were excluded, the constraint would be satisfied. ILOG CPLEX can generate a constraint corresponding to this condition, and this cut is called a cover cut.
    /// Decides whether or not cover cuts should be generated for the problem. Setting the value to 0 (zero), the default, indicates that the attempt to generate cliques should continue only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_Covers
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2,
        _3_GenerarMuyAgresivamente = 3
    }
    /// <summary>
    /// Description: Flow covers are generated from constraints that contain continuous variables, where the continuous variables have variable upper bounds that are zero or positive depending on the setting of associated binary variables. The idea of a flow cover comes from considering the constraint containing the continuous variables as describing a single node in a network where the continuous variables are in-flows and out-flows. The flows will be on or off depending on the settings of the associated binary variables for the variable upper bounds. The flows and the demand at the single node imply a knapsack constraint. That knapsack constraint is then used to generate a cover cut on the flows (that is, on the continuous variables and their variable upper bounds).
    /// Decides whether or not to generate flow cover cuts for the problem. Setting the value to
    /// 0 (zero), the default, indicates that the attempt to generate flow cover cuts should continue
    /// only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_FlowCovers
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: Flow path cuts are generated by considering a set of constraints containing the continuous variables that describe a path structure in a network, where the constraints are nodes and the continuous variables are in-flows and out-flows. The flows will be on or off depending on the settings of the associated binary variables.
    /// Decides whether or not to generate flow path cuts for the problem. Setting the value to
    /// 0 (zero), the default, indicates that the attempt to generate flow cover cuts should continue
    /// only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_FlowPaths
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: Gomory fractional cuts are generated by applying integer rounding on a pivot row in the optimal LP tableau for a (basic) integer variable with a fractional solution value.
    /// Decides whether or not to generate Gomory fractional cuts for the problem. Setting the value to
    /// 0 (zero), the default, indicates that the attempt to generate flow cover cuts should continue
    /// only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_Gomory
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: Generalized Upper Bound (GUB) Cover Cuts -> A_Actual GUB constraint for a set of binary variables is a sum of variables less than or equal to one. If the variables in a GUB constraint are also members of a knapsack constraint, then the minimal cover can be selected with the additional consideration that at most one of the members of the GUB constraint can be one in a solution. This additional restriction makes the GUB cover cuts stronger (that is, more restrictive) than ordinary cover cuts.
    /// Decides whether or not to generate GUB cuts for the problem. Setting the value to 0 (zero),
    /// the default, indicates that the attempt to generate GUB cuts should continue only if it seems
    /// to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_GUB
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: In some models, binary variables imply bounds on continuous variables. ILOG CPLEX generates potential cuts to reflect these relationships.
    /// Decides whether or not to generate implied bound cuts for the problem. Setting the value
    /// to 0 (zero), the default, indicates that the attempt to generate implied bound cuts should
    /// continue only if it seems to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_ImpliedBound
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: The cuts that CPLEX generates state that the capacities installed on arcs pointing into a component of the network must be at least as large as the total flow demand of the component that can not be satisfied by flow sources within the component.
    /// Specifies whether CPLEX should generate multi-commodity flow cuts in a problem
    /// where CPLEX detects the characteristics of a multi-commodity flow network with arc
    /// capacities. By default, CPLEX decides whether or not to generate such cuts.
    /// </summary>
    public enum ParamCplex_Cuts_MultiCommodityFlow
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    /// <summary>
    /// Description: Zero-half cuts are based on the observation that when the lefthand side of an inequality consists of integral variables and integral coefficients, then the righthand side can be rounded down to produce a zero-half cut. Zero-half cuts are also known as 0-1/2 cuts
    /// Decides whether or not to generate zero-half cuts for the problem. The value 0 (zero), the
    /// default, specifies that the attempt to generate zero-half cuts should continue only if it seems
    /// to be helping.
    /// </summary>
    public enum ParamCplex_Cuts_ZeroHalf
    {
        __1_NoGenerar = -1,
        _0_Default_QueCPLEXDecida = 0,
        _1_GenerarModeradamente = 1,
        _2_GenerarAgresivamente = 2
    }
    






    #endregion


    #endregion




}