using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Especifica las necesidades del día
    /// </summary>
    public class PlanDia
    {
        #region Atributos
        /// <summary>
        /// La lista de requerimientos del plan día
        /// </summary>
        public List<Requerimiento> Requerimientos;
        /// <summary>
        /// True si se completó el plan día!
        /// </summary>
        public bool Completo;
        /// <summary>
        /// El porcentaje actual de la corrida
        /// </summary>
        public double PorcentajeCumplido;

        #endregion

        #region Constructor

        public PlanDia(List<Requerimiento> _requerimientos)
        {
            Requerimientos = _requerimientos;
        }
        #endregion
        /// <summary>
        /// Agrega las toneladas entregadas al plan día y actualiza el cumplimiento del requerimiento
        /// </summary>
        /// <param name="idPala"></param>
        /// <param name="idDestino"></param>
        /// <param name="toneladas"></param>
        /// <param name="instanteDescarga"></param>
        public void AgregarToneladas(int idPala, int idDestino, double toneladas, double instanteDescarga)
        {
            Requerimiento R0 = GetRequerimiento(idPala, idDestino);
            R0.AgregarDescarga(instanteDescarga, toneladas);

            double sumaPorcentajes = 0;
            foreach (Requerimiento R in Requerimientos)
            {
                double Porcentaje = Math.Min(1, R.ToneladasEntregadas / R.ToneladasNecesita);
                sumaPorcentajes += Porcentaje;
            }
            PorcentajeCumplido = sumaPorcentajes / Requerimientos.Count;

            //Veo si completé el plan día
            Completo = PorcentajeCumplido == 1;
        }

        public void Reset()
        {
            Completo = false;
            PorcentajeCumplido = 0;
            foreach (Requerimiento R in Requerimientos)
            {
                R.PorcentajeActual = 0;
                R.ToneladasEntregadas = 0;
                R.UltimoInstanteDescarga = 0;
                R.Completado = false;
            }
        }
        /// <summary>
        /// Obtiene el requerimiento asociado a esta pala y este destino
        /// </summary>
        /// <param name="idPala"></param>
        /// <param name="idDest"></param>
        /// <returns></returns>
        public Requerimiento GetRequerimiento(int idPala, int idDest)
        {
            Requerimiento RR = null;
            foreach (Requerimiento R in this.Requerimientos)
            {
                if (R.IdPala == idPala && R.IdDestino == idDest)
                {
                    RR = R;
                    break;
                }
            }
            if (RR == null)
            {
                throw new Exception("Requerimiento no encontrado");
            }
            return RR;
        }
    }
}
