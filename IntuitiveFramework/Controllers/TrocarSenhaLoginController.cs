using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using IntuitiveFramework.Models;

namespace IntuitiveFramework.Controllers
{
    public class TrocarSenhaLoginController : ListController<ControleDeLoginEntities>
    {
        public ActionResult List()
        {
            this.ViewData["MetodoEControlador"] = new string[2];
            (this.ViewData["MetodoEControlador"] as string[])[0] = "MudarSenha";
            (this.ViewData["MetodoEControlador"] as string[])[1] = "TrocarSenha";
            return base.List(null);
        }

        [HttpPost]
        public ActionResult MudarSenha(FormCollection collection)
        {
            Usuarios objUsuario = base.getLoginUser();

            if (!LoginBusinessApplications.IsSenhaAntigaIgualBanco(ref objUsuario, collection["SenhaAntiga"]))
            {
                ModelState.AddModelError("SenhaAntiga", "A senha está errada!");
                return View("List");
            }

            if (!LoginBusinessApplications.IsSenhaNovaIgualSenhaRepetida(collection["SenhaNova"], collection["SenhaNovaRepetida"]))
            {
                ModelState.AddModelError("SenhaNova", "As senhas estão diferentes!");
                ModelState.AddModelError("SenhaNovaRepetida", "As senhas estão diferentes!");
                return View("List");
            }

            string senhaAntigaReal = objUsuario.Senha;

            objUsuario.Senha = collection["SenhaNova"].ToString();
            if (!LoginBusinessApplications.IsTamanhoSenhaValido(objUsuario.Senha))
            {
                ModelState.AddModelError("SenhaNova", "Campo Senha deve conter no mínimo 8 caracteres.");
                objUsuario.Senha = senhaAntigaReal;
                return View("List");
            }

            if (LoginBusinessApplications.IsSenhaIgualLogin(ref objUsuario))
            {
                ModelState.AddModelError("SenhaNova", "Campo Senha deve ser diferente do campo Login.");
                objUsuario.Senha = senhaAntigaReal;
                return View("List");
            }

            if (!LoginBusinessApplications.criptografarSenha(ref objUsuario))
            {
                ModelState.AddModelError("SenhaNova", "Acontenceu um erro ao tentar criptografar a senha.");
                objUsuario.Senha = senhaAntigaReal;
                return View("List");
            }

            bdInstance.SaveChanges();

            Session["Login"] = null;
            Session["Senha"] = null;

            return RedirectToAction("Index", "Home");
        }      
    }
}