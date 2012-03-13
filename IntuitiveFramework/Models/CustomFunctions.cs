using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace IntuitiveFramework.Models
{
    public static class CustomFunctions
    {
        public static Func<T, bool> And<T>(Func<T, bool> expr1, Func<T, bool> expr2)
        {            
            return x => expr1(x) && expr2(x);
        }

        public static Func<T, bool> Or<T>(Func<T, bool> expr1, Func<T, bool> expr2)
        {
            return x => expr1(x) || expr2(x);
        }

        private static int AdicionarTentativa(ref int tentativas)
        {
            return tentativas++;
        }

        private static void BloqueiaUsuario(string login, int idSistema)
        {
            bdContext<ControleDeLoginEntities>.
                Instance.BD.Usuarios.
                Where(x => x.Login == login && x.IdSistema == idSistema).
                FirstOrDefault().Bloqueado = true;

            bdContext<ControleDeLoginEntities>.Instance.BD.SaveChanges();
        }

        public static int getIdSistema(string sistema)
        {
            return bdContext<ControleDeLoginEntities>.
                             Instance.BD.Sistemas.Where(x => x.Descricao == sistema).
                             FirstOrDefault().Id;
        }

        public static void VerificarSeBloqueiaUsuario(ref int tentativas, string login, 
                                                      string sistema)
        {
            if (AdicionarTentativa(ref tentativas) >= 5)
                BloqueiaUsuario(login, getIdSistema(sistema));
        }

        public static Usuarios getUsuarioById(int idUsuario)
        {
            return bdContext<ControleDeLoginEntities>.Instance.BD.Usuarios.Where(x => x.Id.Equals(idUsuario)).FirstOrDefault();
        }
    }
}