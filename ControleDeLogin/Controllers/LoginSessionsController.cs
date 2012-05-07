using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using System.Reflection;
using System.Configuration;

using ControleDeLogin.Models;
using Persistencia;
using IntuitiveEstruturas;

namespace ControleDeLogin.Controllers
{
    public abstract class LoginSessionsController : Controller
    {
        protected ActionResult SelecionarEstab(string Login, string Password, int IdSistema)
        {
            List<ValDescr> estabs = null;
            Usuarios objUsuario = LoginBusinessApplications.getUsuarioFromLoginSessions(Login, Password, IdSistema);
            if (!LoginBusinessApplications.diferentesEstabsGruposUsuarios(ref objUsuario, out estabs))
                throw new Exception("Aconteceu um erro ao tentar selecionar os diferentes estabelecimentos pertencentes a este usuário!");
            ViewData["Estabelecimentos"] = new SelectList(estabs, "Id", "Descricao");
            return View();
        }

        protected ActionResult SelecionarEstab(FormCollection collection)
        {
            Session["IdEstabelecimento"] = int.Parse(collection["IdEstabelecimento"].ToString());
            return View("Index");
        }

        protected ActionResult Login<T>(FormCollection collection, string sistema) where T : ObjectContext, new()
        {
            Usuarios objUsuario = null;

            bool zerarContador = true;
            int idSistema = -1;

            //string AliasConnection = null;

            //if (!LoginBusinessApplications.getConnectionAliasByIdEstab((int)Session["IdEstabelecimento"], out AliasConnection))
            //    throw new Exception("Acontenceu um erro ao tentar descobrir a conexão para este estabelecimento!");

            //if (bdContext<ControleDeLoginEntities>.GetCurrentAlias() != AliasConnection)
            if (Session["IdEstabelecimento"] == null)
                bdContext<T>.zerarInstance();

            if (Session["Login"] == null)
            {
                if (Session["LastLoginTry"] == null)
                    Session["LastLoginTry"] = collection["Login"].ToString();

                string login = collection["Login"].ToString();
                string pass = LoginBusinessApplications.getSenhaCriptografada(collection["Senha"].ToString());
                idSistema = LoginBusinessApplications.getIdSistema(sistema);

                if ((login != Session["LastLoginTry"].ToString()) || (Session["tentativasLogin"] == null))
                    Session["tentativasLogin"] = 0;

                if (LoginBusinessApplications.getUsuarioFromLoginSessions(login, pass, idSistema) != null)
                {
                    if (!LoginBusinessApplications.getUsuarioFromLoginSessions(login, pass, idSistema).Bloqueado)
                    {
                        Session["Login"] = login;
                        Session["Senha"] = pass;

                        objUsuario = LoginBusinessApplications.getUsuarioFromLoginSessions(Session["Login"].ToString(), 
                                                                                           Session["Senha"].ToString(), 
                                                                                           idSistema);
                    }
                    else
                        this.ViewData["FalhaLogin"] = "Usuário bloqueado.";
                }
                else
                {
                    Session["Login"] = null;
                    Session["Senha"] = null;
                    this.ViewData["FalhaLogin"] = "Usuário ou senha inválida.";

                    zerarContador = false;

                    int tmpSession = int.Parse(Session["tentativasLogin"].ToString());
                    LoginBusinessApplications.VerificarSeBloqueiaUsuario(ref tmpSession, login, sistema);
                    Session["tentativasLogin"] = tmpSession;
                }
            }

            if (zerarContador)
                Session["tentativasLogin"] = 0;

            if (objUsuario != null)
            {
                if (LoginBusinessApplications.haMaisDeUmEstabNosGruposUsuarios(ref objUsuario))
                {
                    return RedirectToAction("SelecionarEstab", new
                    {
                        Login = Session["Login"].ToString(),
                        Password = Session["Senha"].ToString(),
                        IdSistema = idSistema
                    });
                }
                else
                {
                    List<ValDescr> estabs = null;
                    if (!LoginBusinessApplications.diferentesEstabsGruposUsuarios(ref objUsuario, out estabs))
                        throw new Exception("Aconteceu um erro ao tentar selecionar o estabelecimento deste usuário!");
                    Session["IdEstabelecimento"] = estabs.FirstOrDefault().Id;
                }
            }
            else
            {
                Session["Login"] = null;
                Session["IdEstabelecimento"] = null;
            }

            return View("Index");
        }
    }
}
