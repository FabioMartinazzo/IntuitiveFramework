using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using System.Reflection;

using Persistencia;
using ControleDeLogin.Models;
using IntuitiveEstruturas;

namespace ControleDeLogin.Controllers
{
    public abstract class LoginController : Controller
    {
        protected int _idSistema;

        protected Usuarios getLoginUser()
        {
            try
            {
                return LoginBusinessApplications.
                       getUsuarioFromLoginSessions(Session["Login"].ToString(),
                                                   Session["Senha"].ToString(),
                                                   _idSistema);
            }
            catch
            {
                return null;
            }
        }

        private bool? IsPermitido(TPermissionCheck permissionCheck, int tipoPerm)
        {
            if (permissionCheck == null)
                return true;

            Usuarios objUsuario = permissionCheck.getUsuarioPermissao();

            if (objUsuario != null)
            {
                if (objUsuario.GrupoUsuarios.Count() <= 0)
                    return false;
            }
            else
                return null;

            Grupos objGrupo = null;
            GrupoVisoes objGrupoVisao = null;

            List<GrupoUsuarios> objGrupos = objUsuario.GrupoUsuarios.ToList();
            foreach (var item_objGrupo in objGrupos)
            {
                objGrupo = item_objGrupo.Grupos;
                objGrupoVisao = objGrupo.GrupoVisoes.Where(x => x.Visao.Descricao == permissionCheck.getNomeControlador()).FirstOrDefault();
                if (objGrupoVisao != null)
                {
                    if (objGrupoVisao.Permissao.Where(x => x.Tipo == tipoPerm).Count() > 0)
                        return true;
                }
            }

            return false;
        }

        private ActionResult ErroPrivilegio()
        {
            return RedirectToAction("Erro", "Home", new { msnErro = "Erro: Usuário sem privilégios de acesso." });
        }

        protected ActionResult ChecaPermissao(TPermissionCheck permissionCheck, EnumsIntuitive.TipoPermissao tipoPermissao)
        {
            bool? IsPerm = IsPermitido(permissionCheck, (int)tipoPermissao);
            if (IsPerm == false)
                return ErroPrivilegio();
            else if (IsPerm == null)
                return RedirectToAction("Index", "Home");

            return null;
        }
    }
}
