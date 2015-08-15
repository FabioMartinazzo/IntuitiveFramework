using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Text;

using IntuitiveEstruturas;

namespace IntuitiveFuncoes
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

        public static string getDescricaoEnumPorId<T>(int id)
        {
            return EnumHelper.GetDescription(typeof(T),
                                             Enum.GetValues(typeof(T)).
                                                  Cast<T>().
                                                  Where(x => x.GetHashCode() == id).
                                                  FirstOrDefault().ToString());
        }

        public static string convertFromASCIItoUTF8(string input)
        {
            UTF8Encoding utf8Text = new UTF8Encoding();
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            byte[] utf8Array = Encoding.Convert(Encoding.ASCII, Encoding.UTF8, byteArray);
            return utf8Text.GetString(utf8Array);
        }

        public static string getErrorMessageFromException(Exception ex)
        {
            return (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
        }
    }
}