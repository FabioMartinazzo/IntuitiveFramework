using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using IntuitiveFramework.Models;

namespace IntuitiveFramework.Controllers
{
    public class GrupoLoginController : CadastrosController<ControleDeLoginEntities>, ICadastros
    {
        private void PopulaViewDatasUsuariosVinculados(int idGrupo)
        {
            int idEstabelecimento = (int)Session["IdEstabelecimento"];
            List<Usuarios> allUsers = bdInstance.Usuarios.Where(x => x.GrupoUsuarios.Any(g => g.IdEstabelecimento == idEstabelecimento)).ToList();
            List<GrupoUsuarios> objGrupoUsuarios;
            foreach (var item in allUsers)
            {
                objGrupoUsuarios = item.GrupoUsuarios.ToList();
                this.ViewData["usr_" + item.Id.ToString()] = objGrupoUsuarios.Where(x => x.IdGrupo.Equals(idGrupo) && x.IdEstabelecimento == idEstabelecimento).Count() > 0;
            }
        }

        private void PopulaViewDatasPermissoesVinculadas(int idGrupo)
        {
            List<Visao> allModules = bdInstance.Visao.ToList();
            GrupoVisoes objGrupoVisoes;
            List<Permissao> objPermissoes;
            Boolean PermissaoCreate;
            Boolean PermissaoEdit;
            Boolean PermissaoDelete;
            Boolean PermissaoList;
            Boolean PermissaoDetails;

            foreach (var item in allModules)
            {
                objGrupoVisoes = item.GrupoVisoes.Where(x => x.IdGrupo == idGrupo).FirstOrDefault();

                if (objGrupoVisoes != null)
                {
                    objPermissoes = objGrupoVisoes.Permissao.ToList();

                    PermissaoCreate = objPermissoes.Where(x => x.Tipo == (item.TipoPermissoesPorVisao.Create ? (int)EnumsIntuitive.TipoPermissao.Create : -1)).Count() > 0;
                    PermissaoEdit = objPermissoes.Where(x => x.Tipo == (item.TipoPermissoesPorVisao.Edit ? (int)EnumsIntuitive.TipoPermissao.Edit : -1)).Count() > 0;
                    PermissaoDelete = objPermissoes.Where(x => x.Tipo == (item.TipoPermissoesPorVisao.Delete ? (int)EnumsIntuitive.TipoPermissao.Delete : -1)).Count() > 0;
                    PermissaoList = objPermissoes.Where(x => x.Tipo == (item.TipoPermissoesPorVisao.List ? (int)EnumsIntuitive.TipoPermissao.List : -1)).Count() > 0;
                    PermissaoDetails = objPermissoes.Where(x => x.Tipo == (item.TipoPermissoesPorVisao.Details ? (int)EnumsIntuitive.TipoPermissao.Details : -1)).Count() > 0;
                }
                else
                {
                    PermissaoCreate = false;
                    PermissaoEdit = false;
                    PermissaoDelete = false;
                    PermissaoList = false;
                    PermissaoDetails = false;
                }
                
                if (item.TipoPermissoesPorVisao.Create)
                    this.ViewData["perm_" + item.Id.ToString() + ";Create"] = PermissaoCreate;
                if (item.TipoPermissoesPorVisao.Edit)
                    this.ViewData["perm_" + item.Id.ToString() + ";Edit"] = PermissaoEdit;
                if (item.TipoPermissoesPorVisao.Delete)
                    this.ViewData["perm_" + item.Id.ToString() + ";Delete"] = PermissaoDelete;
                if (item.TipoPermissoesPorVisao.List)
                    this.ViewData["perm_" + item.Id.ToString() + ";List"] = PermissaoList;
                if (item.TipoPermissoesPorVisao.Details)
                    this.ViewData["perm_" + item.Id.ToString() + ";Details"] = PermissaoDetails;

            }
        }

        private TPermissionCheck _permissionCheck;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this._permissionCheck = new TPermissionCheck(getLoginUser(), "Grupo");
            base.OnActionExecuting(filterContext);
        }

        private Func<Grupos, bool> CommomIdentifier(MultKeys multkeys)
        {            
            return x => x.Id.Equals(int.Parse(multkeys.GetKeyinIndex(0).ToString()));
        }

        public ActionResult List()
        {
            return base.List<Grupos>(this._permissionCheck);
        }

        [HttpPost]
        public ActionResult List(FormCollection collection)
        {
            return base.List<Grupos, string>(this._permissionCheck, collection,
                                                                    x => x.Nome.Contains(collection["NomePesquisa"]) && 
                                                                         x.IdSistema == base._idSistema, y => y.Nome);
        }

        public ActionResult Create()
        {
            return base.Create<Grupos>(this._permissionCheck, null);
        } 

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            return base.Create<Grupos>(this._permissionCheck, null, BusinessValidatorForCreate, collection);
        }
        
        public ActionResult Edit(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            PopulaViewDatasUsuariosVinculados(int.Parse(multkeys.GetKeyinIndex(0).ToString()));
            PopulaViewDatasPermissoesVinculadas(int.Parse(multkeys.GetKeyinIndex(0).ToString()));
            return base.Edit<Grupos>(this._permissionCheck, null, CommomIdentifier(multkeys));
        }


        [HttpPost]
        public ActionResult Edit(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Grupos>(this._permissionCheck, null, BusinessValidator, CommomIdentifier(multkeys), collection);
        }

        public ActionResult Delete(string keys, bool certeza)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Delete<Grupos>(this._permissionCheck, null, BusinessValidatorForDelete, CommomIdentifier(multkeys), certeza);
        }

        public ActionResult Details(string keys)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Details<Grupos>(this._permissionCheck, CommomIdentifier(multkeys));
        }        

        [HttpPost]
        public ActionResult AdicionarGruposAoUsuario(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Grupos>(this._permissionCheck, null, BusinessValidatorForAdicionarGruposAoUsuario, CommomIdentifier(multkeys), collection);
        }

        [HttpPost]
        public ActionResult AdicionarPermissoesAoGrupo(string keys, FormCollection collection)
        {
            MultKeys multkeys = new MultKeys(keys);
            return base.Edit<Grupos>(this._permissionCheck, null, BusinessValidatorForAdicionarPermissoesAoGrupo, CommomIdentifier(multkeys), collection);
        }

        private void getIDVisaoPlusTipoPerm(string itemFromView, out int idVisao, out int tipoPerm)
        {
            idVisao = int.Parse(itemFromView.Substring(0, itemFromView.IndexOf(@";")));
            string tmpPerm = itemFromView.Substring((itemFromView.IndexOf(@";") + 1), (itemFromView.Length - (itemFromView.IndexOf(@";") + 1)));
            tipoPerm = tmpPerm == "Create" ? (int)EnumsIntuitive.TipoPermissao.Create :
                       (tmpPerm == "Edit" ? (int)EnumsIntuitive.TipoPermissao.Edit :
                       (tmpPerm == "Delete" ? (int)EnumsIntuitive.TipoPermissao.Delete :
                       (tmpPerm == "List" ? (int)EnumsIntuitive.TipoPermissao.List :
                       (tmpPerm == "Details" ? (int)EnumsIntuitive.TipoPermissao.Details : -1))));
        }

        public Boolean BusinessValidatorForDelete<T>(FormCollection formcollection, ref T table, out string msnErro)
        {
            Grupos objGrupo = table as Grupos;

            msnErro = "";

            if (LoginBusinessApplications.isAnyUsuarioVinculadoGrupo(ref objGrupo))
            {
                msnErro = "Só poderá excluir o grupo depois de desvincular todos os usuários.";
                return false;
            }

            if (!LoginBusinessApplications.removerTodosPrivilegiosGrupo(ref objGrupo))
            {
                msnErro = "Erro ao tentar desvincular os privilégios.";
                return false;
            }            

            return true;
        }

        public bool BusinessValidatorForAdicionarPermissoesAoGrupo<T>(FormCollection collection, ref T objTable) where T : class
        {
            Grupos objGrupo = objTable as Grupos;

            Boolean tmpBool;
            foreach (var item in collection)
            {
                if (item.ToString() == "keys")
                    continue;

                int idVisao = -1;
                int idPerm = -1;

                getIDVisaoPlusTipoPerm(item.ToString().Substring(5), out idVisao, out idPerm);

                if (Boolean.TryParse(collection[item.ToString()].ToString(), out tmpBool))
                {
                    if (!LoginBusinessApplications.removerPermissaoDoGrupo(ref objGrupo, idVisao, idPerm))
                        return false;
                }
                else
                {
                    if (!LoginBusinessApplications.adicionarPermissaoAoGrupo(ref objGrupo, idVisao, idPerm))
                        return false;
                }
            }

            return true;
        }

        public bool BusinessValidatorForAdicionarGruposAoUsuario<T>(FormCollection collection, ref T objTable) where T : class
        {
            Grupos objGrupo = objTable as Grupos;

            Boolean tmpBool;
            foreach (var item in collection)
            {
                if (item.ToString() == "keys")
                    continue;

                int idEstab = (int)Session["IdEstabelecimento"];

                if (Boolean.TryParse(collection[item.ToString()].ToString(), out tmpBool))
                {                    
                    if (!LoginBusinessApplications.removerUsuarioDoGrupo(ref objGrupo, int.Parse(item.ToString().Substring(4)), idEstab))
                        return false;
                }
                else
                {                    
                    if (!LoginBusinessApplications.adicionarUsuarioAoGrupo(ref objGrupo, int.Parse(item.ToString().Substring(4)), idEstab))
                        return false;
                }
            }

            return true;
        }

        private bool BusinessValidatorForCreate<T>(FormCollection collection, ref T objTable) where T : class
        {
            Grupos objGrupo = (objTable as Grupos);

            if (!LoginBusinessApplications.adicionarSistemaAoGrupo(ref objGrupo, base._idSistema))
                throw new Exception("Acontenceu um erro ao tentar adicionar o sistema ao Grupo!");

            return base.ValidateEntity<T>(collection, ref objTable);
        }

        public bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class
        {
            return base.ValidateEntity<T>(collection, ref objTable);
        }
    }
}