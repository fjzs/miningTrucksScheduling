using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class ResultadoCorrida
    {
        //Parámetros del problema
        public MetodoSolucion Metodo;
        public string IdentificadorMetodo;
        public double Horizonte;
        public double IntervaloTonelaje;
        public double PesosPorTonelada;
        public double ToneladasPorMinuto;
        public double CostoTotalTransporte;
        public double ToneladasTransportadas;
        public double CostoPorTonelada;
        public double TiempoCicloAVG;
        public double TiempoSolucionAVG;
        public double CamionesCicloAVG;
        public double EsperaPalas;
        public double EsperaDestinos;
        public double Demoras;
        public double RetrasoTotal;
        public double CoeficienteLey;
        public double TiempoRequerido;
        public double MinutosTotales;
        public double Retraso_TonMin;
        public double MaximaProductividadAlcanzada;
        public double MultiplicadorPendiente;
        public List<IntervaloLey> IntervalosLeyes;
        public double PorcentajeCumplimientoLey;
        /// <summary>
        /// Porcentaje de las instancias en que se alcanza a resolver a optimalidad antes del siguiente despacho
        /// </summary>
        public double PorcentajeAlcanzaAResolver;

        /// <summary>
        /// [Requerimientos]
        /// </summary>
        public List<double> EntregadoCRPD;

        public ResultadoCorrida(int numReqs)
        {
            EntregadoCRPD = new List<double>();
            for (int i = 0; i < numReqs; i++)
            {
                EntregadoCRPD.Add(0);
            }
        }

    }
}
