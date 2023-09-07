using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class IntervaloLey
    {
        public double T1_min;
        public double T2_min;
        /// <summary>
        /// [IdDestino; [IdPala ; Toneladas de esta pala]]
        /// </summary>
        private Dictionary<int, Dictionary<int, double>> ToneladasPorPalaPorDest;
        public Dictionary<int, double> LeyEfectivaPorDestino;

        public IntervaloLey(double t1, double t2, int numPalas, int numDest)
        {
            T1_min = t1;
            T2_min = t2;
            ToneladasPorPalaPorDest = new Dictionary<int, Dictionary<int, double>>();
            for (int d = 1; d <= numDest; d++)
            {
                Dictionary<int, double> TonsPorPala = new Dictionary<int, double>();
                for (int j = 1; j <= numPalas; j++)
                {
                    TonsPorPala.Add(j, 0);
                }
                ToneladasPorPalaPorDest.Add(d, TonsPorPala);
            }

            LeyEfectivaPorDestino = new Dictionary<int, double>();
            for (int d = 1; d <= numDest; d++)
            {
                LeyEfectivaPorDestino.Add(d, 0);
            }
        }
        public void CalcularLeyEfectiva(List<Pala> Palas)
        {
            foreach (int d in ToneladasPorPalaPorDest.Keys)
            {
                double totalTons = 0;
                double totalTonsLey = 0;
                foreach (int idP in ToneladasPorPalaPorDest[d].Keys)
                {
                    Pala P = Palas[idP - 1];
                    double tons = ToneladasPorPalaPorDest[d][idP];
                    double tonsLey = tons * P.Ley;
                    totalTonsLey += tonsLey;
                    totalTons += tons;
                }
                if (totalTons > 0)
                {
                    double leyEf = totalTonsLey / totalTons;
                    LeyEfectivaPorDestino[d] = leyEf;
                }
            }
        }
        public bool Contiene(double tiempo_min)
        {
            if (tiempo_min > T1_min && tiempo_min <= T2_min)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void AgregarTonelaje(int idDestino, int idPala, double tons)
        {
            ToneladasPorPalaPorDest[idDestino][idPala] += tons;
        }
        /// <summary>
        /// Obtiene la suma de los porcentajes de cumplimiento
        /// </summary>
        /// <returns></returns>
        public double SumPorcentajesCumplimiento(List<Destino> Destinos)
        {
            double suma = 0;
            foreach (int idD in LeyEfectivaPorDestino.Keys)
            {
                Destino D = Destinos[idD - 1];
                if (D.TipoDestino != TipoDestino.Botadero)
                {
                    double leyEf = LeyEfectivaPorDestino[idD];
                    double porcentaje = 1 - Math.Abs(leyEf - Configuracion.LeyDePlanta) / Configuracion.LeyDePlanta;
                    suma += porcentaje;
                }
            }

            return suma;
        }

    }
}
