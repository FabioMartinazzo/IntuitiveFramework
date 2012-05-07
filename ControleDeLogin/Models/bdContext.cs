using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Web.UI;
using System.Reflection;
using System.Configuration;

namespace ControleDeLogin.Models
{
    public class bdContext<Z> where Z : ObjectContext, new()
    {
        private static bdContext<Z> instance;
        private string _aliasName;

        //Usado para poder trabalhar com sessões dentro dos Models.
        Page pagina = new Page();

        private bdContext()
        {            
            Type tipoZ = typeof(Z);
            
            if (tipoZ.Name != "ControleDeLoginEntities")
            {
                string AliasConnection = "";
                if (!LoginBusinessApplications.getConnectionAliasByIdEstab((int)pagina.Session["IdEstabelecimento"], out AliasConnection))
                    throw new Exception("Acontenceu um erro ao tentar descobrir a conexão para este estabelecimento!");

                this._aliasName = AliasConnection;

                Type typeZ = typeof(Z);
                ConstructorInfo ZConstructor = typeZ.GetConstructor(new Type[] { typeof(string) });
                try
                {
                    bd = ZConstructor.Invoke(new object[] { ConfigurationManager.ConnectionStrings[this._aliasName].ConnectionString }) as Z;
                }
                catch
                {
                    throw new Exception("Aconteceu um erro ao tentar usar a conexão deste estabelecimento!");
                }
            }
            else
                bd = new Z();
        }

        public static bdContext<Z> Instance
        {
            get
            {
                if (instance == null)
                    instance = new bdContext<Z>();
                
                return instance;
            }
        }

        public static string GetCurrentAlias()
        {
            return instance._aliasName;
        }

        public static void zerarInstance()
        {
            instance = null;
        }

        private Z bd;

        public Z BD
        {
            get { return bd; }
        }
    }
}