using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using IntuitiveFramework.Models;

namespace IntuitiveFramework.Controllers
{
    public class UsuarioLoginController : CadastrosController<ControleDeLoginEntities>, ICadastros
    {
        private void PopulaViewDatasGruposVinculados(int idUsuario)
        {
            List<Grupos> allGrps = bdInstance.Grupos.ToList();
            List<GrupoUsuarios> objGrupoUsuarios;
            foreach (var item in allGrps)
            {
                objGrupoUsuarios = item.GrupoUsuarios.ToList();
                this.ViewData["grp_" + item.Id.ToString()] = objGrupoUsuarios.Where(x => x.IdUsuario.Equals(idUsuario) && x.IdEstabelecimento == (int)Session["IdEstabelecimento"]).Count() > 0;
            }
        }

        private TPermissionCheck _permissionCheck;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this._permissionCheck = new TPermissionCheck(getLoginUser(), "Usuario");
            base.OnActionExecuting(filterContext);
        }

        private Func<Usuarios, bool> CommomIdentifier(MultKeys multkeys)
        {            
            return x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString()));            
        }

        public ActionResult List()
        {
            return base.List<Usuarios>(this._permissionCheck);
        }

        [HttpPost]
        public ActionResult List(FormCollection collection)
        {
            return base.List<Usuarios, string>(this._permissionCheck, collection,
                                                                      x => x.Nome.Contains(collection["NomePesquisa"]) && 
                                                                           x.IdSistema == base._idSistema, y => y.Nome);
        }

        public ActionResult Create()
        {
            return base.Create<Usuarios>(this._permissionCheck, null);
        } 

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            return base.Create<Usuarios>(this._permissionCheck, null, BusinessValidatorForCreate, collection);
        }

        public ActionResult Edit(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            PopulaViewDatasGruposVinculados(int.Parse(multkeys.GetKeyinIndex(0).ToString()));
            return base.Edit<Usuarios>(this._permissionCheck, null, CommomIdentifier(multkeys));
        }

        [HttpPost]
        public ActionResult Edit(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            int idUsuario = int.Parse(multkeys.GetKeyinIndex(0).ToString());

            if (bdContext<ControleDeLoginEntities>.Instance.BD.Usuarios.Where(x => x.Id.Equals(idUsuario)).FirstOrDefault() == getLoginUser())
            {                
                Session["Login"] = null;
                Session["Senha"] = null;
            }            

            return base.Edit<Usuarios>(this._permissionCheck, null, BusinessValidator, CommomIdentifier(multkeys), collection);            
        }

        public ActionResult Delete(string keys, bool certeza)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Delete<Usuarios>(this._permissionCheck, null, BusinessValidatorForDelete, CommomIdentifier(multkeys), certeza);
        }

        public ActionResult Details(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Details<Usuarios>(this._permissionCheck, CommomIdentifier(multkeys));
        }

        [HttpPost]
        public ActionResult AdicionarGruposAoUsuario(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Usuarios>(this._permissionCheck, null, BusinessValidatorForAdicionarGruposAoUsuario, CommomIdentifier(multkeys), collection);
        }

        public Boolean BusinessValidatorForDelete<T>(FormCollection formcollection, ref T table, out string msnErro)
        {
            Usuarios objUsuario = table as Usuarios;

            msnErro = "";

            if (LoginBusinessApplications.isAnyGrupoVinculadoUsuario(ref objUsuario))
            {
                msnErro = "Só poderá excluir o usuário depois de desvincular todos os grupos.";
                return false;
            }            

            return true;
        }

        public bool BusinessValidatorForAdicionarGruposAoUsuario<T>(FormCollection collection, ref T objTable) where T : class
        {
            Usuarios objUsuario = objTable as Usuarios;
            
            Boolean tmpBool;
            foreach (var item in collection)
            {
                if (item.ToString() == "keys")
                    continue;

                int idEstab = (int)Session["IdEstabelecimento"];

                //Caso seja true o plugin do mvc2 não traz o valor correto.
                if (Boolean.TryParse(collection[item.ToString()].ToString(), out tmpBool))
                {
                    if (!LoginBusinessApplications.removerGrupoDoUsuario(ref objUsuario, int.Parse(item.ToString().Substring(4)), idEstab))
                        return false;
                }
                else
                {
                    if (!LoginBusinessApplications.adicionarGrupoAoUsuario(ref objUsuario, int.Parse(item.ToString().Substring(4)), idEstab))
                        return false;
                }
            }

            return true;
        }

        public bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class
        {
            Usuarios objUsuario = objTable as Usuarios;

            if ((collection["NovaSenha"].ToString() != "") || (collection["NovaSenhaRepetida"].ToString() != ""))
            {
                if (!LoginBusinessApplications.IsSenhaNovaIgualSenhaRepetida(collection["NovaSenha"].ToString(), collection["NovaSenhaRepetida"].ToString()))
                {
                    ModelState.AddModelError("NovaSenha", "Senhas não conferem.");
                    ModelState.AddModelError("NovaSenhaRepetida", "Senhas não conferem.");
                    return false;
                }

                objUsuario.Senha = collection["NovaSenha"].ToString();
                if (!LoginBusinessApplications.IsTamanhoSenhaValido(objUsuario.Senha))
                {
                    ModelState.AddModelError("NovaSenha", "Campo Senha deve conter no mínimo 8 caracteres.");
                    return false;
                }

                if (LoginBusinessApplications.IsSenhaIgualLogin(ref objUsuario))
                {
                    ModelState.AddModelError("NovaSenha", "Campo Senha deve ser diferente do campo Login.");
                    return false;
                }

                if (!LoginBusinessApplications.criptografarSenha(ref objUsuario))
                {
                    ModelState.AddModelError("NovaSenha", "Acontenceu um erro ao tentar criptografar a senha.");
                    return false;
                }
            }

            if (LoginBusinessApplications.isOutroLoginMesmoNome(collection["Login"].ToString(), ref objUsuario, base._idSistema))
            {
                ModelState.AddModelError("Login", "Este login já existe no banco de dados.");
                return false;
            }

            return base.ValidateEntity<T>(collection, ref objTable);
        }

        public bool BusinessValidatorForCreate<T>(FormCollection collection, ref T objTable) where T : class
        {
            Usuarios objUsuario = objTable as Usuarios;

            if (!LoginBusinessApplications.AdicionarDataPadraoCadastroUsuario(ref objUsuario))
                return false;

            if (LoginBusinessApplications.isOutroLoginMesmoNome(collection["Login"].ToString(), ref objUsuario, base._idSistema))
            {
                ModelState.AddModelError("Login", "Este login já existe no banco de dados.");
                return false;
            }

            if (!base.ValidateEntity<T>(collection, ref objTable))
                return false;

            if (!LoginBusinessApplications.IsTamanhoSenhaValido(objUsuario.Senha))
            {
                ModelState.AddModelError("Senha", "Campo Senha deve conter no mínimo 8 caracteres.");
                return false;
            }

            if (LoginBusinessApplications.IsSenhaIgualLogin(ref objUsuario))
            {
                ModelState.AddModelError("Senha", "Campo Senha deve ser diferente do campo Login.");
                return false;
            }

            if (!LoginBusinessApplications.criptografarSenha(ref objUsuario))
            {
                ModelState.AddModelError("Senha", "Acontenceu um erro ao tentar criptografar a senha.");
                return false;
            }

            if (!LoginBusinessApplications.adicionarSistemaAoUsuario(ref objUsuario, base._idSistema))
                throw new Exception("Acontenceu um erro ao tentar adicionar o sistema ao Usuário!");

            return true;
        }
    }
}