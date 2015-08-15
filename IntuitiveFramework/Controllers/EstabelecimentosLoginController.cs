using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using IntuitiveFramework.Models;

namespace IntuitiveFramework.Controllers
{
    public class EstabelecimentosLoginController : CadastrosEntityRelationshipController<ControleDeLoginEntities>, ICadastros
    {
        private TPermissionCheck _permissionCheck;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this._permissionCheck = new TPermissionCheck(getLoginUser(), "Estabelecimentos");
            base.OnActionExecuting(filterContext);
        }

        private Func<Estabelecimentos, bool> CommomIdentifier(MultKeys multkeys)
        {            
            return x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString()));            
        }

        public ActionResult List()
        {
            return base.List<Estabelecimentos>(this._permissionCheck);
        }

        [HttpPost]
        public ActionResult List(FormCollection collection)
        {
            return base.List<Estabelecimentos, string>(this._permissionCheck, collection,
                                                       x => x.RazaoSocial.Contains(collection["NomePesquisa"]) && 
                                                            x.IdSistema == base._idSistema, y => y.RazaoSocial);
        }

        public ActionResult Create()
        {
            return base.Create<Estabelecimentos>(this._permissionCheck, null);
        } 

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            return base.Create<Estabelecimentos>(this._permissionCheck, null, BusinessValidatorForCreate, collection);
        }

        public ActionResult Edit(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Estabelecimentos>(this._permissionCheck, null, CommomIdentifier(multkeys));
        }

        [HttpPost]
        public ActionResult Edit(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Estabelecimentos>(this._permissionCheck, null, BusinessValidator, CommomIdentifier(multkeys), collection);            
        }

        public ActionResult Delete(string keys, bool certeza)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Delete<Estabelecimentos>(this._permissionCheck, null, BusinessValidatorForDelete, CommomIdentifier(multkeys), certeza);
        }

        public ActionResult Details(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Details<Estabelecimentos>(this._permissionCheck, CommomIdentifier(multkeys));
        }

        [HttpPost]
        public ActionResult AdicionarEmailConfig(FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(collection["keys"].ToString());
            return base.Add<Estabelecimentos, EmailConfigs>(null, collection, CommomIdentifier(multkeys));
        }

        public ActionResult ExcluirEmailConfig(string keysFilho, string keysPai)
        {
            MultKeys multkeys = new MultKeys(keysFilho);
            return base.Remove<Estabelecimentos, EmailConfigs>(null, x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString())), keysPai);
        }

        public Boolean BusinessValidatorForDelete<T>(FormCollection formcollection, ref T table, out string msnErro)
        {
            msnErro = "Exclusão de estabelecimentos ainda não implementada!";
            return false;
        }

        public bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class
        {
            List<ArquivoCollection> arquivos = new List<ArquivoCollection>();
            foreach (string file in Request.Files)
            {
                int tamanho = (int)Request.Files[file].InputStream.Length;
                
                if (tamanho == 0)
                    continue;

                byte[] dados = new byte[tamanho];

                Request.Files[file].InputStream.Read(dados, 0, tamanho);

                if ((Request.Files[file].ContentType != "image/jpeg") &&
                    (Request.Files[file].ContentType != "image/png"))
                    throw new Exception("Apenas imagens jpeg e png são permitidas no upload!");

                arquivos.Add(new ArquivoCollection()
                                {
                                    Nome = file,
                                    Dados = dados
                                }
                            );
            }

            collection["LogoTop"] = null;
            collection["Logo"] = null;

            return base.ValidateEntity<T>(collection, ref objTable, null, arquivos);
        }

        public bool BusinessValidatorForCreate<T>(FormCollection collection, ref T objTable) where T : class
        {
            Estabelecimentos objEstab = objTable as Estabelecimentos;

            if (!LoginBusinessApplications.adicionarSistemaAoEstab(ref objEstab, base._idSistema))
                throw new Exception("Acontenceu um erro ao tentar adicionar o sistema ao Estabelecimento!");

            List<ArquivoCollection> arquivos = new List<ArquivoCollection>();
            foreach (string file in Request.Files)
            {
                int tamanho = (int)Request.Files[file].InputStream.Length;
                byte[] dados = new byte[tamanho];

                Request.Files[file].InputStream.Read(dados, 0, tamanho);

                if ((Request.Files[file].ContentType != "image/jpeg") &&
                    (Request.Files[file].ContentType != "image/png"))
                    throw new Exception("Apenas imagens jpeg e png são permitidas no upload!");

                arquivos.Add(new ArquivoCollection() { 
                                                        Nome = file,
                                                        Dados = dados
                                                     }
                            );
            }

            collection["LogoTop"] = null;
            collection["Logo"] = null;

            return base.ValidateEntity<T>(collection, ref objTable, null, arquivos);
        }
    }
}