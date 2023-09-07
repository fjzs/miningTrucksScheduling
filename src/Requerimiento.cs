using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Un requerimiento es la componente básica del plan día. Se compone de una pala y un destino
    /// </summary>
    public class Requerimiento
    {
        #region Atributos

        public int Id;
        /// <summary>
        /// Lista de destinos involucrados en este requerimiento
        /// </summary>
        public int IdDestino;
        /// <summary>
        /// Las toneladas que debieran recibir al fin del día la suma de los destinos involucrados
        /// </summary>
        public double ToneladasNecesita;
        /// <summary>
        /// Lista de palas que pueden satisfacer este requerimiento
        /// </summary>
        public int IdPala;
        /// <summary>
        /// El porcentaje que representa este plan del plan día completo
        /// </summary>
        public double PorcentajeDelPlan;

        //Atributos dinámicos
        public double ToneladasEntregadas;
        /// <summary>
        /// El último instante de descarga
        /// </summary>
        public double UltimoInstanteDescarga;
        /// <summary>
        /// True si se completó el requerimiento
        /// </summary>
        public bool Completado;
        /// <summary>
        /// El porcentaje que lleva cumplido
        /// </summary>
        public double PorcentajeActual;

        #endregion

        
        /// <summary>
        /// El constructor de un requerimiento
        /// </summary>
        /// <param name="_idDestinos">Lista de destinos involucrados en este requerimiento</param>        
        /// <param name="_toneladas">Las toneladas que debieran recibir al fin del día la suma de los destinos involucrados</param>        
        public Requerimiento(int idD, double _toneladas, int id, int idP)
        {
            this.IdDestino = idD;            
            this.ToneladasNecesita = _toneladas;
            Id = id;
            this.IdPala = idP;  
        }

        public void AgregarDescarga(double instanteDescarga, double tons)
        {
            UltimoInstanteDescarga = Math.Max(UltimoInstanteDescarga, instanteDescarga);
            ToneladasEntregadas += tons;
            Completado = ToneladasEntregadas >= ToneladasNecesita;
            PorcentajeActual = ToneladasEntregadas / ToneladasNecesita;
        }

    }
}
