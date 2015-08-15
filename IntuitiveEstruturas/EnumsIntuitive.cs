using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace IntuitiveEstruturas
{
    public class EnumsIntuitive
    {
        public enum TipoPermissao
        {
            [Description("Create")]
            Create = 1,
            [Description("Edit")]
            Edit = 2,
            [Description("Delete")]
            Delete = 3,
            [Description("List")]
            List = 4,
            [Description("Details")]
            Details = 5,
        }

        public enum Operators
        {
            Menor = -2,
            MenorIgual = -1,
            Igual = 0,
            MaiorIgual = 1,
            Maior = 2,
        }

        //public enum TipoEmail
        //{
        //    [Description("Faturas Clientes")]
        //    FaturasClientes = 1,
        //}
    }
}