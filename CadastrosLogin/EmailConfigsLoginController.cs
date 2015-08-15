using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;

using IntuitiveEstruturas;
using Persistencia;
using TelasControllers;
using ControleDeLogin.Models;

namespace TelasControllersLogin
{
    public class EmailConfigsLoginController : CadastrosEntityRelationshipController<ControleDeLoginEntities>
    {
        private TPermissionCheck _permissionCheck;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this._permissionCheck = new TPermissionCheck(getLoginUser(), "Configurações Email");
            base.OnActionExecuting(filterContext);
        }

        private Func<EmailConfigs, bool> CommomIdentifier(MultKeys multkeys)
        {            
            return x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString()));            
        }
        
        public ActionResult List()
        {
            return RedirectToAction("List", "Estabelecimentos");
        }

        public ActionResult Edit(string keys, TCommonMethod ReadDropDownLists = null)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<EmailConfigs>(this._permissionCheck, ReadDropDownLists, CommomIdentifier(multkeys));
        }

        [HttpPost]
        public ActionResult Edit(string keys, FormCollection collection, TCommonMethod ReadDropDownLists = null)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<EmailConfigs>(this._permissionCheck, ReadDropDownLists, BusinessValidator, CommomIdentifier(multkeys), collection);
        }

        [HttpPost]
        public ActionResult AdicionarDadosEmails(FormCollection collection, TCommonMethod ReadDropDownLists = null)
        {
            MultKeys multkeys = new MultKeys(collection["keys"].ToString());
            return base.Add<EmailConfigs, DadosEmails>(ReadDropDownLists, collection, CommomIdentifier(multkeys), null, BusinessValidatorForAddDadosEmail);
        }

        public ActionResult ExcluirDadosEmails(string keysFilho, string keysPai, TCommonMethod ReadDropDownLists = null)
        {
            MultKeys multkeys = new MultKeys(keysFilho);
            return base.Remove<EmailConfigs, DadosEmails>(ReadDropDownLists, x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString())), keysPai);
        }

        //private void ReadDropDownLists()
        //{
        //    var tiposEmails = from EnumsIntuitive.TipoEmail s in Enum.GetValues(typeof(EnumsIntuitive.TipoEmail))
        //                      select new
        //                      {
        //                          Id = s.GetHashCode(),
        //                          Descricao = EnumHelper.GetDescription(typeof(EnumsIntuitive.TipoEmail), s.ToString())
        //                      };
        //    this.ViewData["TiposEmails"] = new SelectList(tiposEmails, "Id", "Descricao");
        //}

        public bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class
        {
            return base.ValidateEntity<T>(collection, ref objTable);
        }

        public bool BusinessValidatorForAddDadosEmail<T>(FormCollection formcollection, ref T table, out string msnErro)
        {
            msnErro = "";

            DadosEmails objDadosEmail = (table as DadosEmails);

            string pass = null;

            if (!LoginBusinessApplications.criptografarAES(objDadosEmail.Password, out pass))
            {
                msnErro = "Aconteceu um erro ao tentar encriptografar a senha.";
                return false;
            }

            objDadosEmail.Password = pass;            

            return true;
        }
    }
}