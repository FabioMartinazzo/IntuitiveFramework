using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using System.Reflection;
using System.Configuration;
using IntuitiveFramework.Models;

namespace IntuitiveFramework.Controllers
{
    public abstract class ListController<Z> : LoginController where Z : ObjectContext, new()
    {
        protected Z bdInstance;

        protected ListController()
        {
            try
            {
                this.bdInstance = bdContext<Z>.Instance.BD;
            }
            catch(Exception ex)
            {
                ViewData["Erro"] = "Erro: " + ex.Message;
            }
        }

        protected void CollectionToViewData(FormCollection collection)
        {
            foreach (string item in collection)
            {
                ViewData[item] = collection[item];
            }
        }

        protected ActionResult List(TPermissionCheck permissionCheck, TCommonMethod readdropdownListPesquisa = null)
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.List);
            if (falhaAtenticacao == null)
                return CommomListMethod(readdropdownListPesquisa);
            else
                return falhaAtenticacao;
            
        }

        private ActionResult CommomListMethod(TCommonMethod readdropdownListPesquisa = null)
        {
            if (readdropdownListPesquisa != null)
                readdropdownListPesquisa();

            return View();
        }
    }
}
