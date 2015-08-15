using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Persistencia;

namespace IntuitiveEstruturas
{
    public delegate Boolean TBusinessValidator<T>(FormCollection formcollection, ref T table);
    public delegate Boolean TBusinessValidatorPlusError<T>(FormCollection formcollection, ref T table, out string msnErro);
    public delegate void TCommonMethod();

    public class ValDescr
    {
        private int _id;
        private string _desc;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Descricao
        {
            get { return _desc; }
            set { _desc = value; }
        }
    }

    public struct ArquivoCollection
    {
        public Byte[] Dados;
        public string Nome;
    }

    public static class UseOperator<T> where T : IComparable
    {
        public static bool Compare(T compare1, T compare2, EnumsIntuitive.Operators operador)
        {
            switch (operador)
            {
                case EnumsIntuitive.Operators.Menor:
                    return compare1.CompareTo(compare2) < 0;
                case EnumsIntuitive.Operators.MenorIgual:
                    return compare1.CompareTo(compare2) <= 0;
                case EnumsIntuitive.Operators.Igual:
                    return compare1.CompareTo(compare2) == 0;
                case EnumsIntuitive.Operators.MaiorIgual:
                    return compare1.CompareTo(compare2) >= 0;
                case EnumsIntuitive.Operators.Maior:
                    return compare1.CompareTo(compare2) > 0;
            }
            return false;
        }
    }

    public class TPermissionCheck
    {
        private Usuarios _usuarioPermissao;
        private string _nomeControlador;

        public TPermissionCheck(Usuarios objUsuario, string nomeControlador)
        {
            this._usuarioPermissao = objUsuario;
            this._nomeControlador = nomeControlador;
        }

        public void setUsuarioPermissao(Usuarios objUsuario)
        {
            this._usuarioPermissao = objUsuario;
        }

        public void setNomeControlador(string nomeControlador)
        {
            this._nomeControlador = nomeControlador;
        }

        public Usuarios getUsuarioPermissao()
        {
            return this._usuarioPermissao;
        }

        public string getNomeControlador()
        {
            return this._nomeControlador;
        }
    }

    /// <summary>
    /// Métodos de chamada das Views para o Controlador que antes eram "mesclados" e usavam hora Id, hora uma chave composta, agora estão sendo generalizados para esse tipo de classe.
    /// A passagem do parâmetro será como string visto que as páginas asp.net só aceitam tipos primários ou do modelo e não podem herdar mais do que um tipo.
    /// Mas uma vez passado esse parâmetro, ele será convertido nessa classe para todas as chaves que formam a chave composta.
    /// </summary>
    public class MultKeys
    {
        private object[] _keys;        
        private int[][] separators;
        private int positionSeparators;

        public MultKeys(string multKeys)
        {
            AssignMultKeys(multKeys);
        }

        /// <summary>
        /// Método a ser chamado no momento que construir a classe.
        /// </summary>
        /// <param name="multKeys">string passada como parâmetro pela View.</param>
        public void AssignMultKeys(string multKeys)
        {
            StringToMultKeys(multKeys);
            _keys = new object[100];            

            int n = 0;
            while (n < positionSeparators)
            {
                _keys.SetValue(multKeys.Substring(separators[n][0], (separators[n][1] - separators[n][0])), n);                
                n++;
            }
        }

        /// <summary>
        /// Utilizado para popular uma matriz para facilitar a localização das chaves, formando pares de números inteiros das posições dos chars
        /// que se localizam os caracteres válidos para concatenados formarem uma chave válida.
        /// </summary>
        /// <param name="multKeys">string passada como parâmetro pela View.</param>
        private void StringToMultKeys(string multKeys)
        {
            separators = new int[100][];
            int starter = 0;
            positionSeparators = 0;

            int n = 0;
            while (n < multKeys.Length)
            {
                if (multKeys[n].Equals(';'))
                {
                    separators.SetValue(new int[2] { starter, n }, positionSeparators);
                    starter = n + 1;
                    positionSeparators++;
                }
                n++;
            }
            separators.SetValue(new int[2] { starter, n }, positionSeparators);
            positionSeparators++;
        }

        /// <summary>
        /// Propriedade que retorna o vetor das chaves.
        /// </summary>
        public object[] Keys
        {
            get
            {
                return _keys;
            }
        }

        /// <summary>
        /// Retorna uma determinada chave pelo índice dado.
        /// </summary>
        /// <param name="index">Índice da chave.</param>
        /// <returns>Retorna a chave.</returns>
        public object GetKeyinIndex(int index)
        {
            return _keys[index];
        }
    }
}