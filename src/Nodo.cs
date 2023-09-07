using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    public class Nodo
    {
        #region Atributos fijos
        /// <summary>
        /// El id del nodo
        /// </summary>
        private int _id;
        /// <summary>
        /// La lista de arcos entrantes al nodo
        /// </summary>
        private List<Arco> _arcos_Entrantes;
        /// <summary>
        /// La lista de arcos salientes del nodo
        /// </summary>
        private List<Arco> _arcos_Salientes;

        #endregion

        #region Atributos dinámicos

        #endregion

        #region Constructor

        public Nodo(int id)
        {
            _id = id;
            _arcos_Entrantes = new List<Arco>();
            _arcos_Salientes = new List<Arco>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// La lista de arcos salientes del
        /// </summary>
        public List<Arco> Arcos_Salientes
        {
            get { return _arcos_Salientes; }
        }
        /// <summary>
        /// El id del nodo
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        #endregion

        #region Métodos privados

        #endregion

        #region Métodos públicos
        /// <summary>
        /// Resetea los datos dinámicos del nodo
        /// </summary>
        public void Reset()
        {

        }
        /// <summary>
        /// Agrega un arco entrante a este nodo
        /// </summary>
        /// <param name="A_Actual">El arco</param>
        public void AgregaArcoEntrante(Arco A)
        {
            if (_arcos_Entrantes.Contains(A))
            {
                throw new Exception("Error el arco ya estaba");
            }
            else
            {
                _arcos_Entrantes.Add(A);
            }
        }
        /// <summary>
        /// Agrega un arco saliente al nodo
        /// </summary>
        /// <param name="A_Actual">El arco</param>
        public void AgregaArcoSaliente(Arco A)
        {
            if (_arcos_Salientes.Contains(A))
            {
                throw new Exception("Error el arco ya estaba");
            }
            else
            {
                _arcos_Salientes.Add(A);
            }
        }
        #endregion
    }
}
