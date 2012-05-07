using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

using IntuitiveEstruturas;

namespace TelasControllers
{
    public abstract class CadastrosEntityRelationshipController<Z> : CadastrosController<Z> where Z : ObjectContext, new()
    {
        protected EntityCollection<U> MergeTables<T, U>(T tablePai) where T : class where U : class
        {
            Type typeT = typeof(T);
            PropertyInfo tmpProperty = typeT.GetProperty(typeof(U).Name);

            return (tmpProperty.GetValue(tablePai, null) as EntityCollection<U>);
        }

        protected ActionResult Add<T, U>(TCommonMethod commommethod, FormCollection collection, Func<T, bool> where,
                                         Dictionary<string, string> valuesOverload = null,
                                         TBusinessValidatorPlusError<U> businessValidatorPlusError = null, 
                                         List<ArquivoCollection> arquivos = null,  
                                         string Mensagem = null) where T : class where U : class, new()
        {
            string keys = collection["keys"];
            T objTablePai = (bdInstance as Z).CreateObjectSet<T>().Where(where).FirstOrDefault();
            ViewData.Model = objTablePai;

            try
            {
                U objTableFilho = new U();
                if (base.ValidateEntity<U>(collection, ref objTableFilho, valuesOverload, arquivos))
                {
                    MergeTables<T, U>(objTablePai).Add(objTableFilho);
                    if (businessValidatorPlusError != null)
                    {
                        string msnErro = "";
                        if (!businessValidatorPlusError(collection, ref objTableFilho, out msnErro))
                        {
                            (bdInstance as Z).DeleteObject(objTableFilho);
                            throw new Exception(msnErro);
                        }
                    }

                    (bdInstance as Z).SaveChanges();

                    this.ViewData["Mensagem"] = Mensagem;

                    return RedirectToAction("Edit", new { keys = keys });
                }
                else
                {
                    if (commommethod != null)
                        commommethod();
                    return View("Edit");
                }
            }
            catch (Exception ex)
            {
                if (commommethod != null)
                    commommethod();
                this.ViewData["Erro"] = "Erro: " + ex.Message;
                return View("Edit");
            }
        }

        protected ActionResult Remove<T, U>(TCommonMethod commommethod, Func<U, bool> where, string keysPai, 
                                            TBusinessValidatorPlusError<U> businessValidatorForDelete = null, 
                                            Func<T, bool> wherePai = null, string Mensagem = null) where T : class where U : class
        {

            T objPai = null;
            if (wherePai != null)
            {
                objPai = (bdInstance as Z).CreateObjectSet<T>().Where(wherePai).FirstOrDefault();
                ViewData.Model = objPai;
            }

            try
            {
                U objFilho = (bdInstance as Z).CreateObjectSet<U>().Where(where).FirstOrDefault();

                if (businessValidatorForDelete != null)
                {
                    string msnErro = "";
                    if (businessValidatorForDelete(null, ref objFilho, out msnErro))
                        SubRemove<U>(ref objFilho, Mensagem);
                    else
                        throw new Exception(msnErro);
                }
                else
                    SubRemove<U>(ref objFilho, Mensagem);

                return RedirectToAction("Edit", new { keys = keysPai });
            }
            catch (Exception ex)
            {
                if (commommethod != null)
                    commommethod();
                this.ViewData["Erro"] = "Erro: " + ex.Message;
                return View("Edit");
            }
        }

        protected void SubRemove<T>(ref T objTable, string Mensagem)
        {
            (bdInstance as Z).DeleteObject(objTable);
            (bdInstance as Z).SaveChanges();

            this.ViewData["Mensagem"] = Mensagem;
        }
    }
}
