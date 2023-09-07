#----------------------------#
#----- MODELO PDCMCA v1 -----#
#----------------------------#

# Este modelo genera la ruta para completa

### CONJUNTOS ESTÁTICOS
set CAMIONES;
set PALAS;
set DESTINOS;
set VARIABLES_GEOMETALURGICAS;
set NODOS;
#set ARCOS within {NODOS,NODOS};
set STATUS_CAMIONES;

### PARÁMETROS ESTÁTICOS
param existeArco{u in NODOS, v in NODOS};
param tiempoTotalHorizonte;
param nodoDestino{d in DESTINOS};
param nodoPala {j in PALAS};
param capacidadCamion_ton {i in CAMIONES};
param tiempoViajeCamionArco{i in CAMIONES, s in STATUS_CAMIONES, u in NODOS, v in NODOS};
param costoViaje{i in CAMIONES, s in STATUS_CAMIONES, u in NODOS, v in NODOS};
param costoDemora{i in CAMIONES};
param costoEsperaEnCola{i in CAMIONES};
param isCamionAsignablePala{i in CAMIONES, j in PALAS};
param isMaterialPalaDescargableEnDestino{j in PALAS, d in DESTINOS};
param tiempoDescargaCamionDestino_hra{i in CAMIONES, d in DESTINOS};
param tiempoCargaCamionPala{i in CAMIONES, j in PALAS};
param porcentajeVariableEnFrente{k in VARIABLES_GEOMETALURGICAS, j in PALAS};

### PARÁMETROS DADOS POR EL PLAN DIARIO
param porcentajeRecomendado{k in VARIABLES_GEOMETALURGICAS, d in DESTINOS};
param toneladasTotalesSegunPlanDiario{d in DESTINOS};

### PENALIDADES DEL PLAN DIARIO
param numLineasPenalidadToneladas{d in DESTINOS};
param interceptoPenalidadToneladas{d in DESTINOS, 1..numLineasPenalidadToneladas[d]};
param pendientePenalidadToneladas{d in DESTINOS, 1..numLineasPenalidadToneladas[d]};
param numLineasPenalidadMezcla{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS};
param interceptoPenalidadMezcla{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS,n in 1..numLineasPenalidadMezcla[d,k]};
param pendientePenalidadMezcla{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS, n in 1..numLineasPenalidadMezcla[d,k]};

### PARÁMETROS DEPENDIENTES
param isCamionAsignableDestino{i in CAMIONES, d in DESTINOS} := max{j in PALAS}isCamionAsignablePala[i,j]*isMaterialPalaDescargableEnDestino[j,d];

### CONJUNTOS DEPENDIENTES
set ARCOS := setof{u in NODOS, v in NODOS: existeArco[u,v]=1}(u,v);
set NODOS_DESTINOS := setof{u in NODOS, d in DESTINOS: u=nodoDestino[d]}u;
set NODOS_PALAS := setof{u in NODOS, j in PALAS: u=nodoPala[j]}u;
set NODOS_ESTACIONES :=  NODOS_DESTINOS union NODOS_PALAS;
set ARCOS_NO_EXCLUSIVOS := setof{u in NODOS, v in NODOS: v in NODOS_ESTACIONES and (u,v) in ARCOS}(u,v);
set ARCOS_EXCLUSIVOS := setof{u in NODOS, v in NODOS: (u,v) in ARCOS and (u,v) not in ARCOS_NO_EXCLUSIVOS}(u,v);
set ARCOS_CAMION_VACIO := setof{u in NODOS, v in NODOS: (u,v) in ARCOS and v not in NODOS_DESTINOS and u not in NODOS_PALAS}(u,v);
set ARCOS_CAMION_LLENO := setof{u in NODOS, v in NODOS: (u,v) in ARCOS and v not in NODOS_PALAS and u not in NODOS_DESTINOS}(u,v);
set ARCOS_POR_STATUS{s in STATUS_CAMIONES} :=   if(s=1) then ARCOS_CAMION_VACIO else ARCOS_CAMION_LLENO;
set ARCOS_POR_STATUS_EXCLUSIVOS{s in STATUS_CAMIONES} := setof{u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS[s] and (u,v) in ARCOS_EXCLUSIVOS}(u,v);

### VARIABLES DE ESTADO
set CAMIONES_CONSIDERADOS;
param instanteActual;
param nodoInicioCamion{i in CAMIONES_CONSIDERADOS};
param instanteCamionDisponible{i in CAMIONES_CONSIDERADOS};
param instanteHorizonte := max{i in CAMIONES_CONSIDERADOS}instanteCamionDisponible[i]+1/6;
param numTimeWindows{u in NODOS, v in NODOS};
param maxNumTW := max{u in NODOS, v in NODOS}numTimeWindows[u,v];
param instanteInicioTW{u in NODOS, v in NODOS, 1..maxNumTW};
param instanteTerminoTW{u in NODOS, v in NODOS, 1..maxNumTW};
param porcentajeMezclaRecibido{k in VARIABLES_GEOMETALURGICAS, d in DESTINOS};
param toneladasRecibidas{d in DESTINOS};
param toneladasDebieraRecibirDestino{d in DESTINOS} := if((instanteHorizonte/tiempoTotalHorizonte)*toneladasTotalesSegunPlanDiario[d]>toneladasRecibidas[d]) then (instanteHorizonte/tiempoTotalHorizonte)*toneladasTotalesSegunPlanDiario[d]-toneladasRecibidas[d] else 0;

param BigM:=1;
param BigM_EsperaEnCola := 1;
param BigM_LlegadaNodo := 1;
param BigM_DemoraArco := 10;
param BigM_PerteneceTW := 1;
param BigM_SalidaDestino := 10;

#-------------------------
### VARIABLES DE DECISIÓN
#-------------------------

### Variables de asignación a palas y destinos
var AsignaCamionPala{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1}, binary;
var AsignaCamionPalaDestino{i in CAMIONES_CONSIDERADOS, j in PALAS, d in DESTINOS: isCamionAsignablePala[i,j]=1 and isMaterialPalaDescargableEnDestino[j,d]=1}, binary;
var AsignaCamionDestino{i in CAMIONES_CONSIDERADOS, d in DESTINOS: isCamionAsignableDestino[i,d]=1}, binary;

### Variables de generación de ruta
var AsignaCamionArco{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS[s]}, binary;
var AsignaCamionArcoTW{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS, 1..numTimeWindows[u,v]: (u,v) in ARCOS_POR_STATUS[s]}, binary;
var InstanteLlegadaCamionNodo{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS: if(s=2) then u not in NODOS_DESTINOS}, >= instanteCamionDisponible[i] <= instanteHorizonte + BigM;
var AsignaCamionAntesArco{i1 in CAMIONES_CONSIDERADOS, s1 in STATUS_CAMIONES, i2 in CAMIONES_CONSIDERADOS, s2 in STATUS_CAMIONES, u in NODOS, v in NODOS: i1<>i2 and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s1] and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s2]}, binary;
var DemoraEnArco{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS}, >=0;
var InstanteSalidaDestino{i in CAMIONES_CONSIDERADOS, d in DESTINOS: isCamionAsignableDestino[i,d]=1}, >=instanteCamionDisponible[i] <= instanteHorizonte + BigM  ;

### Variables de atención en palas y destinos
var CamionCargaAntesEnPala{i1 in CAMIONES_CONSIDERADOS, i2 in CAMIONES_CONSIDERADOS, j in PALAS: i1<>i2 and isCamionAsignablePala[i1,j]=1 and isCamionAsignablePala[i2,j]=1}, binary;
var EsperaEnColaPala{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1}, >= 0 <=1;
var CamionDescargaAntesEnDestino {i1 in CAMIONES_CONSIDERADOS, i2 in CAMIONES_CONSIDERADOS, d in DESTINOS: i1<>i2 and isCamionAsignableDestino[i1,d]=1 and isCamionAsignableDestino[i2,d]=1}, binary;
var EsperaEnColaDestino{i in CAMIONES_CONSIDERADOS, d in DESTINOS: isCamionAsignableDestino[i,d] = 1}, >=0 <=1;

### Variables del plan diario
param minTonEntregadasCRPD{d in DESTINOS} := -toneladasDebieraRecibirDestino[d];
param maxTonEntregadasCRPD{d in DESTINOS} := sum{i in CAMIONES_CONSIDERADOS}capacidadCamion_ton[i] -toneladasDebieraRecibirDestino[d];
var ToneladasEntregadasCRPD{d in DESTINOS}, >= minTonEntregadasCRPD[d] <= maxTonEntregadasCRPD[d];
var PorcentajeDeMezclaCRPD{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS}, >=-10 <=10;
param minPenalidadCumpTon{d in DESTINOS} := min{l in 1..numLineasPenalidadToneladas[d]}(interceptoPenalidadToneladas[d,l]+pendientePenalidadToneladas[d,l]*maxTonEntregadasCRPD[d]);
param maxPenalidadCumpTon{d in DESTINOS} := max{l in 1..numLineasPenalidadToneladas[d]}(interceptoPenalidadToneladas[d,l]+pendientePenalidadToneladas[d,l]*minTonEntregadasCRPD[d]);
var PenalidadCumplimientoToneladas{d in DESTINOS}, >= minPenalidadCumpTon[d] <= maxPenalidadCumpTon[d];
param penalidadMezclaConPorcentajeMinimo {d in DESTINOS, k in VARIABLES_GEOMETALURGICAS} := max{l in 1..numLineasPenalidadMezcla[d,k]}(interceptoPenalidadMezcla[d,k,l] + pendientePenalidadMezcla[d,k,l]*-10);
param penalidadMezclaConPorcentajeMaximo {d in DESTINOS, k in VARIABLES_GEOMETALURGICAS} := max{l in 1..numLineasPenalidadMezcla[d,k]}(interceptoPenalidadMezcla[d,k,l] + pendientePenalidadMezcla[d,k,l]*10);
param maxPenalidadMezcla{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS} := if(penalidadMezclaConPorcentajeMinimo[d,k] > penalidadMezclaConPorcentajeMaximo[d,k]) then penalidadMezclaConPorcentajeMinimo[d,k] else penalidadMezclaConPorcentajeMaximo[d,k];
var PenalidadCumplimientoMezcla{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS}, >= 0 <= maxPenalidadMezcla[d,k];

### FUNCIÓN OBJETIVO
var FO_CostosDeTransporte, >=0;
var FO_CostosDeEspera, >=0;
var FO_PenalidadCumplimientoToneladas <=99999999 >=-99999999;
var FO_PenalidadCumplimientoMezcla, >=0 <= 99999999;
var FO_CostosPorDemora, >=0;

subject to FO_1: FO_CostosDeTransporte = sum{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS[s]}AsignaCamionArco[i,s,u,v]*costoViaje[i,s,u,v];
subject to FO_2: FO_PenalidadCumplimientoToneladas = sum{d in DESTINOS}PenalidadCumplimientoToneladas[d];
subject to FO_3: FO_PenalidadCumplimientoMezcla = sum{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS}PenalidadCumplimientoMezcla[d,k];
subject to FO_4: FO_CostosDeEspera = sum{i in CAMIONES_CONSIDERADOS}costoEsperaEnCola[i]*(sum{j in PALAS: isCamionAsignablePala[i,j]=1}EsperaEnColaPala[i,j] +sum{d in DESTINOS: isCamionAsignableDestino[i,d] = 1}EsperaEnColaDestino[i,d]);
subject to FO_5: FO_CostosPorDemora = sum{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS}DemoraEnArco[i,s,u,v]*costoDemora[i];

minimize Costo:
+ FO_CostosDeTransporte
+ FO_PenalidadCumplimientoToneladas
+ FO_PenalidadCumplimientoMezcla
+ FO_CostosDeEspera
+ FO_CostosPorDemora
;

### RESTRICCIONES
#subject to TODOS_ASIGNAN_A_MISMA_PALA:
#sum{i in CAMIONES_CONSIDERADOS}AsignaCamionPala[i,2] = card(CAMIONES_CONSIDERADOS);
#subject to TODOS_DESCARGAN_EN_MISMO_DESTINO:
#sum{i in CAMIONES_CONSIDERADOS}AsignaCamionDestino[i,2] = card(CAMIONES_CONSIDERADOS);


#------------------------------------------
#
# (a) Restricciones de asignación
#
#------------------------------------------

#El camión debe ser asignado a alguna pala
subject to REST_ASIGNACION_CAMION_PALA{i in CAMIONES_CONSIDERADOS}: 
sum{j in PALAS: isCamionAsignablePala[i,j]=1}AsignaCamionPala[i,j]=1;

#Desde la pala asignada se debe asignar el destino al que se descargará el material
subject to REST_ASIGNACION_PALA_DESTINO{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1}:
AsignaCamionPala[i,j] = sum{d in DESTINOS: isMaterialPalaDescargableEnDestino[j,d]=1}AsignaCamionPalaDestino[i,j,d];

#Definición de la variable AsignaCamionDestino
subject to REST_ASIGNACION_CAMION_DESTINO{i in CAMIONES_CONSIDERADOS, d in DESTINOS: isCamionAsignableDestino[i,d]}:
AsignaCamionDestino[i,d] = sum{j in PALAS: isMaterialPalaDescargableEnDestino[j,d]=1}AsignaCamionPalaDestino[i,j,d];


#------------------------------------------
#
# (b) Restricciones de generación de ruta
#
#------------------------------------------

#Instante de inicio del nodo de inicio es el momento en que pide asignación
subject to REST_ASIGNA_INSTANTE_DE_INICIO_DEL_NODO_INICIAL{i in CAMIONES_CONSIDERADOS}:
InstanteLlegadaCamionNodo[i,1,nodoInicioCamion[i]] = instanteCamionDisponible[i];

#Llegada a la pala escogida
subject to REST_LLEGADA_A_PALA_ESCOGIDA{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1}:
sum{u in NODOS: (u,nodoPala[j]) in ARCOS}AsignaCamionArco[i,1,u,nodoPala[j]]=AsignaCamionPala[i,j];

#Salida de pala escogida
subject to REST_SALIDA_DE_PALA_ESCOGIDA{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1}:
sum{u in NODOS: (nodoPala[j],u) in ARCOS}AsignaCamionArco[i,2,nodoPala[j],u]=AsignaCamionPala[i,j];

#Llegada al destino escogido
subject to REST_LLEGADA_A_DESTINO_ESCOGIDO{i in CAMIONES_CONSIDERADOS, d in DESTINOS}:
sum{u in NODOS: (u,nodoDestino[d]) in ARCOS}AsignaCamionArco[i,2,u,nodoDestino[d]] = AsignaCamionDestino[i,d];

#Salida desde el nodo de origen de un camión
subject to REST_SALIDA_DEL_NODO_ORIGEN{i in CAMIONES_CONSIDERADOS}:
sum{v in NODOS: (nodoInicioCamion[i],v) in ARCOS}AsignaCamionArco[i,1,nodoInicioCamion[i],v]=1;

# YA NO SON NECESARIAS ESTAS RESTRICCIONES. ESTÁN DENTRO DEL CONJUNTO ARCOS_POR_STATUS
#Ningún camión puede llegar a un destino vacío (NUEVA)
#subject to REST_NO_SE_PUEDE_LLEGAR_A_DESTINO_VACIO{i in CAMIONES_CONSIDERADOS, d in DESTINOS, u in NODOS: (u,nodoDestino[d]) in ARCOS}:
#AsignaCamionArco[i,1,u,nodoDestino[d]]=0;

#Ningún camión puede salir de un destino cargado (NUEVA)
#subject to REST_NO_SE_PUEDE_SALIR_DE_UN_DESTINO_CARGADO{i in CAMIONES_CONSIDERADOS, d in DESTINOS, u in NODOS: (nodoDestino[d],u) in ARCOS}:
#AsignaCamionArco[i,2,nodoDestino[d],u]=0;

#Ningún camión puede llegar a una pala cargado (NUEVA)
#subject to REST_NO_SE_PUEDE_LLEGAR_A_PALA_CARGADO{i in CAMIONES_CONSIDERADOS, j in PALAS, u in NODOS: (u,nodoPala[j]) in ARCOS}:
#AsignaCamionArco[i,2,u,nodoPala[j]]=0;

#Ningún camión puede salir de una pala vacío (NUEVA)
#subject to REST_NO_SE_PUEDE_SALIR_DE_UNA_PALA_VACIO{i in CAMIONES_CONSIDERADOS, j in PALAS, u in NODOS: (nodoPala[j],u) in ARCOS}:
#AsignaCamionArco[i,1,nodoPala[j],u]=0;

#Continuidad de flujo en la red (MODIFICADA)
subject to REST_CONTINUIDAD_DE_FLUJO{i in CAMIONES_CONSIDERADOS,s in STATUS_CAMIONES, u in NODOS: u <> if(s=1) then nodoInicioCamion[i] else -1 and u not in NODOS_DESTINOS and u not in NODOS_PALAS}:
sum{v in NODOS: (v,u) in ARCOS_POR_STATUS[s]}AsignaCamionArco[i,s,v,u]- sum{w in NODOS: (u,w) in ARCOS_POR_STATUS[s]}AsignaCamionArco[i,s,u,w] = 0;

#Si se usa un arco entonces se debe usar alguna de las ventanas de tiempo disponibles
subject to REST_USO_DE_ARCO_IMPLICA_USO_DE_ALGUNA_TW{s in STATUS_CAMIONES, i in CAMIONES_CONSIDERADOS, u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS[s]}:
sum{tw in 1..numTimeWindows[u,v]}AsignaCamionArcoTW[i,s,u,v,tw] = AsignaCamionArco[i,s,u,v];

#Si se usa un arco el tiempo de llegada al nodo final corresponde al menos al tiempo de llegada al nodo inicial más el tiempo de viaje por el arco más la demora experimentada. Esto aplica para arcos exclusivos solamente
subject to REST_TIEMPOS_DE_LLEGADA_A_NODOS_DE_ARCO_USADO{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s]}:
InstanteLlegadaCamionNodo[i,s,u] + tiempoViajeCamionArco[i,s,u,v] <= InstanteLlegadaCamionNodo[i,s,v] + BigM_LlegadaNodo*(1-AsignaCamionArco[i,s,u,v]);

#(NUEVA) Cálculo de la demora en un arco cualquiera
subject to REST_ASIGNA_DEMORA{i in CAMIONES_CONSIDERADOS, s in STATUS_CAMIONES, u in NODOS, v in NODOS: (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s]}:
DemoraEnArco[i,s,u,v] >= InstanteLlegadaCamionNodo[i,s,v] - (InstanteLlegadaCamionNodo[i,s,u] + tiempoViajeCamionArco[i,s,u,v]) - BigM_DemoraArco*(1-AsignaCamionArco[i,s,u,v]);

#Un camión puede usar un arco siempre y cuando esté dentro de alguna de las ventanas de tiempo disponibles del arco
subject to REST_USO_DE_TW_IMPLICA_PERTENECER_AL_INTERVALO_1{i in CAMIONES_CONSIDERADOS, u in NODOS, v in NODOS, s in STATUS_CAMIONES, tw in 1..numTimeWindows[u,v]: (u,v) in ARCOS_POR_STATUS[s]}:
InstanteLlegadaCamionNodo[i,s,u] + BigM_PerteneceTW*(1-AsignaCamionArcoTW[i,s,u,v,tw]) >= instanteInicioTW[u,v,tw];
subject to REST_USO_DE_TW_IMPLICA_PERTENECER_AL_INTERVALO_2{i in CAMIONES_CONSIDERADOS, u in NODOS, v in NODOS, s in STATUS_CAMIONES, tw in 1..numTimeWindows[u,v]: (u,v) in ARCOS_POR_STATUS[s]}:
InstanteLlegadaCamionNodo[i,s,v] <= instanteTerminoTW[u,v,tw] + BigM_PerteneceTW*(1-AsignaCamionArcoTW[i,s,u,v,tw]); 

#Si dos camiones usan un arco implica que solo uno de ellos lo usa antes
subject to REST_USO_DE_ARCO_POR_DOS_CAMIONES_IMPLICA_SOLO_UNO_LO_USA_ANTES{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, s1 in STATUS_CAMIONES, s2 in STATUS_CAMIONES, u in NODOS, v in NODOS: i<>h and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s1] and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s2]}:
AsignaCamionAntesArco[i,s1,h,s2,u,v] + AsignaCamionAntesArco[h,s2,i,s1,u,v] <= 1;

#El camión i con status s1 usa antes el arco que el camión h con status s2:
subject to REST_USO_ANTES_DE_ARCO_IMPLICA_LLEGADA_ANTES{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, s1 in STATUS_CAMIONES, s2 in STATUS_CAMIONES, u in NODOS, v in NODOS: i<>h and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s1] and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s2]}:
InstanteLlegadaCamionNodo[i,s1,v] <= InstanteLlegadaCamionNodo[h,s2,u] + BigM*(1-AsignaCamionAntesArco[i,s1,h,s2,u,v]);

#El camión i con status s1 usa despúes el arco que el camión h con status s2:
subject to REST_USO_DESPUES_DE_ARCO_IMPLICA_LLEGADA_DESPUES{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, s1 in STATUS_CAMIONES, s2 in STATUS_CAMIONES, u in NODOS, v in NODOS: i<>h and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s1] and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s2]}:
InstanteLlegadaCamionNodo[h,s2,v] <= InstanteLlegadaCamionNodo[i,s1,u] + BigM*(1-AsignaCamionAntesArco[h,s2,i,s1,u,v]);

#(MODIFICADA) Un camión i con status s puede usar antes un arco que otro camión h con status s' solo si ambos camiones fueron asignados al arco 
subject to REST_USO_ANTES_DE_ARCO_IMPLICA_AMBOS_CAM_USARON_EL_ARCO{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, s1 in STATUS_CAMIONES, s2 in STATUS_CAMIONES, u in NODOS, v in NODOS: i<>h and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s1] and (u,v) in ARCOS_POR_STATUS_EXCLUSIVOS[s2]}:
AsignaCamionArco[i,s1,u,v] + AsignaCamionArco[h,s2,u,v] <= AsignaCamionAntesArco[i,s1,h,s2,u,v] + AsignaCamionAntesArco[h,s2,i,s1,u,v] + 1;



#------------------------------------------
#
# (c) Restricciones de atención en palas
#
#------------------------------------------

# (MODIFICADA) El tiempo de salida de una pala corresponde a la suma del tiempo de llegada, el tiempo en cola y el tiempo de atención siempre y cuando el camión haya sido asignado a la pala
subject to REST_TIEMPO_DE_SALIDA_DE_PALA{i in CAMIONES_CONSIDERADOS, j in PALAS, u in NODOS: (u,nodoPala[j]) in ARCOS and isCamionAsignablePala[i,j]=1}:
#InstanteLlegadaCamionNodo[i,1,u] + tiempoCargaCamionPala[i,j] + EsperaEnColaPala[i,j] = InstanteLlegadaCamionNodo[i,2,nodoPala[j]] + BigM*(1-AsignaCamionPala[i,j]);
InstanteLlegadaCamionNodo[i,1,u] + tiempoCargaCamionPala[i,j] <= InstanteLlegadaCamionNodo[i,2,nodoPala[j]] + BigM*(1-AsignaCamionPala[i,j]);

# (NUEVA) Cálculo de la espera en cola, al igual que la demora
subject to REST_ASIGNA_ESPERA_EN_COLA_PALA{i in CAMIONES_CONSIDERADOS, j in PALAS, u in NODOS: (u,nodoPala[j]) in ARCOS and isCamionAsignablePala[i,j]=1}:
EsperaEnColaPala[i,j] >= InstanteLlegadaCamionNodo[i,2,nodoPala[j]] - (InstanteLlegadaCamionNodo[i,1,u] + tiempoCargaCamionPala[i,j]) - BigM_EsperaEnCola*(1-AsignaCamionPala[i,j]);

#El camión i se atiende antes que el camión h en la pala j si carga antes que él
#VERSIÓN ANTIGUA
#subject to REST_SECUENCIAMIENTO_EN_ATENCION_DE_PALA{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, j in PALAS, u in NODOS: i<>h and (u,nodoPala[j]) in ARCOS and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
#InstanteLlegadaCamionNodo[i,1,u] <= InstanteLlegadaCamionNodo[h,1,u] + BigM*(1-CamionCargaAntesEnPala[i,h,j]);
#VERSIÓN NUEVA
#El momento en que empieza a atenderse el camión h en la pala debe ser mayor o igual al instante en que el camión i terminó de atenderese
subject to REST_SECUENCIAMIENTO_DE_ATENCION_DE_PALA{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
InstanteLlegadaCamionNodo[h,2,nodoPala[j]] - tiempoCargaCamionPala[h,j] >= InstanteLlegadaCamionNodo[i,2,nodoPala[j]] - BigM*(1-CamionCargaAntesEnPala[i,h,j]);

#El camión i se puede atender antes que el camión h en la pala j solo si ambos camiones fueron asignados a esa pala
subject to REST_ATENCION_ANTES_EN_PALA_IMPLICA_USO_DE_PALA_1{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS,j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
CamionCargaAntesEnPala[i,h,j] <= AsignaCamionPala[i,j];
subject to REST_ATENCION_ANTES_EN_PALA_IMPLICA_USO_DE_PALA_2{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS,j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
CamionCargaAntesEnPala[i,h,j] <= AsignaCamionPala[h,j];

#(NUEVA) Si dos camiones son asignados a una pala uno se debe atender antes que el otro
subject to REST_ATENCION_DE_DOS_CAMIONES_EN_PALA_IMPLICA_SECUENCIAMIENTO{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
AsignaCamionPala[i,j] + AsignaCamionPala[h,j] <= CamionCargaAntesEnPala[i,h,j] + CamionCargaAntesEnPala[h,i,j] + 1;

#Si el camión i se atendió antes que el camión h en la pala j entonces el camión i salió antes que el camión h.
#subject to REST_TIEMPO_DE_SALIDA_DE_PALA_SEGUN_ORDEN_DE_ATENCION{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS,j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
#InstanteLlegadaCamionNodo[i,2,nodoPala[j]] <= InstanteLlegadaCamionNodo[h,2,nodoPala[j]] + BigM*(1-CamionCargaAntesEnPala[i,h,j]);

#Si el camión i se atendió antes que el camión h en la pala j entonces el camión h no se puede atender antes que el i y viceversa
subject to REST_ATENCION_ANTES_EN_PALA_IMPLICA_USO_DE_AMBOS{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS,j in PALAS: i<>h and isCamionAsignablePala[i,j] = 1 and isCamionAsignablePala[h,j] = 1}:
CamionCargaAntesEnPala[i,h,j] + CamionCargaAntesEnPala[h,i,j] <= 1;


#----------------------------------------------
#
# (d) Restricciones de atención en los destinos
#
#----------------------------------------------

#(MODIFICADA) El tiempo de salida de un destino corresponde a la suma del tiempo de llegada, el tiempo en cola y el tiempo de atención siempre y cuando el camión haya sido asignado al destino
subject to REST_TIEMPO_DE_SALIDA_DE_DESTINO{i in CAMIONES_CONSIDERADOS, d in DESTINOS, u in NODOS: (u,nodoDestino[d]) in ARCOS and isCamionAsignableDestino[i,d]=1}:
InstanteLlegadaCamionNodo[i,2,u] + tiempoDescargaCamionDestino_hra[i,d] <= InstanteSalidaDestino[i,d] + BigM_SalidaDestino*(1-AsignaCamionDestino[i,d]);

#(NUEVA) Cálculo de la espera en el destino
subject to REST_ASIGNA_ESPERA_EN_COLA_DESTINO{i in CAMIONES_CONSIDERADOS, d in DESTINOS, u in NODOS: (u,nodoDestino[d]) in ARCOS and isCamionAsignableDestino[i,d]=1}:
EsperaEnColaDestino[i,d] >= InstanteSalidaDestino[i,d] - (InstanteLlegadaCamionNodo[i,2,u] + tiempoDescargaCamionDestino_hra[i,d]) - BigM_EsperaEnCola*(1-AsignaCamionDestino[i,d]);

#El camión i se atiende antes que el camión h en el destino d
subject to REST_ORDEN_DE_ATENCION_EN_DESTINO{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS, u in NODOS: i<>h and (u,nodoDestino[d]) in ARCOS and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
InstanteLlegadaCamionNodo[i,2,u] <= InstanteLlegadaCamionNodo[h,2,u] + BigM*(1-CamionDescargaAntesEnDestino[i,h,d]);

#El camión i se puede atender antes que el camión h en el destino d solo si ambos camiones fueron asignados a ese destino
subject to REST_ATENCION_ANTES_EN_DESTINO_SI_FUERON_ASIGNADOS_A_ESTE_1{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS: i<>h and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
CamionDescargaAntesEnDestino[i,h,d] <= AsignaCamionDestino[i,d];
subject to REST_ATENCION_ANTES_EN_DESTINO_SI_FUERON_ASIGNADOS_A_ESTE_2{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS: i<>h and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
CamionDescargaAntesEnDestino[i,h,d] <= AsignaCamionDestino[i,d];

#(NUEVA) Si dos camiones se atienden en un destino uno de ellos se debe atender antes que el otro
subject to REST_ATENCION_DE_DOS_CAMIONES_EN_DESTINO_IMPLICA_SECUENCIAMIENTO{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS: i<>h and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
AsignaCamionDestino[i,d] + AsignaCamionDestino[h,d] <= CamionDescargaAntesEnDestino[i,h,d] + CamionDescargaAntesEnDestino[h,i,d] + 1;

#Si el camión i se atendió antes que el camión h en el destino d entonces el camión i se desocupó antes que el camión h
subject to REST_ATENCION_ANTES_EN_DESTINO_IMPLICA_SALIDA_ANTES{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS: i<>h and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
InstanteLlegadaCamionNodo[i,1,nodoDestino[d]] <= InstanteLlegadaCamionNodo[h,1,nodoDestino[d]] + BigM*(1-CamionDescargaAntesEnDestino[i,h,d]);

#Si el camión i se atendió antes que el camión h en el destino d entonces el camión h no se puede atender antes que el i y viceversa:
subject to REST_ATENCION_ANTES_EN_DESTINO_IMPLICA_EL_OTRO_SE_ATIENDE_DESPUES{i in CAMIONES_CONSIDERADOS, h in CAMIONES_CONSIDERADOS, d in DESTINOS: i<>h and isCamionAsignableDestino[i,d] = 1 and isCamionAsignableDestino[h,d] = 1}:
CamionDescargaAntesEnDestino[i,h,d] + CamionDescargaAntesEnDestino[h,i,d] <= 1;


#--------------------------------------------------
#
# (e) Restricciones de cumplimiento del plan diario
#
#--------------------------------------------------

#Asignación de las toneladas transportadas al destino
subject to REST_ASIGNACION_DE_TONELADAS_TRANSPORTADAS{d in DESTINOS}:
ToneladasEntregadasCRPD[d] = sum{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1 and isMaterialPalaDescargableEnDestino[j,d]=1}AsignaCamionPalaDestino[i,j,d]*capacidadCamion_ton[i]-toneladasDebieraRecibirDestino[d];

#Penalidad asociada al cumplimiento de tonelaje
subject to REST_PENALIDAD_DE_TONELAJE{d in DESTINOS, l in 1..numLineasPenalidadToneladas[d]}:
PenalidadCumplimientoToneladas[d] >= interceptoPenalidadToneladas[d,l] + pendientePenalidadToneladas[d,l]*ToneladasEntregadasCRPD[d];

#Asignación del porcentaje de mezcla obtenido en cada caso
subject to REST_ASIGNACION_DE_MEZCLA{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS}:
PorcentajeDeMezclaCRPD[d,k] = (porcentajeMezclaRecibido[k,d]*toneladasRecibidas[d] + sum{i in CAMIONES_CONSIDERADOS, j in PALAS: isCamionAsignablePala[i,j]=1 and isMaterialPalaDescargableEnDestino[j,d]=1}AsignaCamionPalaDestino[i,j,d]*capacidadCamion_ton[i]*porcentajeVariableEnFrente[k,j] )/ (toneladasRecibidas[d] + toneladasDebieraRecibirDestino[d])-porcentajeRecomendado[k,d];

#Penalidad asociada al cumplimiento de mezcla
subject to REST_PENALIDAD_DE_MEZCLA{d in DESTINOS, k in VARIABLES_GEOMETALURGICAS, l in 1..numLineasPenalidadMezcla[d,k]}:
PenalidadCumplimientoMezcla[d,k] >= interceptoPenalidadMezcla[d,k,l] + pendientePenalidadMezcla[d,k,l]*PorcentajeDeMezclaCRPD[d,k];
