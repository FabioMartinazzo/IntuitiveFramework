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
    public abstract class CadastrosController<Z> : ListController<Z> where Z : ObjectContext, new()
    {
        /// <summary>
        /// Lista padrão, sem pesquisa
        /// </summary>
        /// <typeparam name="T">Tipo da tabela</typeparam>
        /// <returns>Exibe a View</returns>
        protected ActionResult List<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdownListPesquisa = null, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.List);
            if (falhaAtenticacao == null)
                return CommomListMethod<T>(readdropdownListPesquisa, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult CommomListMethod<T>(TCommonMethod readdropdownListPesquisa = null, string Mensagem = null) where T : class
        {
            if (readdropdownListPesquisa != null)
                readdropdownListPesquisa();

            ViewData.Model = (bdInstance as Z).CreateObjectSet<T>().Take(0);
            ViewData["Mensagem"] = Mensagem;

            return View();
        }

        /// <summary>
        /// Lista uma pesquisa com order by
        /// </summary>
        /// <typeparam name="T">Tipo da tabela</typeparam>
        /// <typeparam name="U">Tipo do order by</typeparam>
        /// <param name="where">Expressão lambda para a cláusula where.</param>
        /// <param name="orderby">Expressão lambda para a cláusula orderby.</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult List<T, U>(TPermissionCheck permissionCheck, FormCollection collection, Func<T, bool> where, Func<T, U> orderby,
                                          TCommonMethod readdropdownListPesquisa = null, bool ascending = true, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.List);
            if (falhaAtenticacao == null)
                return ComplexListMethod<T, U>(collection, where, orderby, readdropdownListPesquisa, ascending, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult ComplexListMethod<T, U>(FormCollection collection, Func<T, bool> where, Func<T, U> orderby, 
                                                     TCommonMethod readdropdownListPesquisa = null, bool ascending = true, 
                                                     string Mensagem = null) where T : class
        {
            try
            {
                CollectionToViewData(collection);

                if (readdropdownListPesquisa != null)
                    readdropdownListPesquisa();

                if (ascending)
                    ViewData.Model = (bdInstance as Z).CreateObjectSet<T>().Where(where).ToList().OrderBy(orderby);
                else
                    ViewData.Model = (bdInstance as Z).CreateObjectSet<T>().Where(where).ToList().OrderByDescending(orderby);

                ViewData["Mensagem"] = Mensagem;

                return View();
            }
            catch (Exception ex)
            {
                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Abre tela para nova inclusão
        /// </summary>
        /// <param name="commommethod">Método a ser executado antes, normalmente um DropDownList</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Create<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdown = null, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Create);
            if (falhaAtenticacao == null)
                return CommomCreateMethod<T>(readdropdown, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult CommomCreateMethod<T>(TCommonMethod readdropdown, string Mensagem) where T : class
        {
            GetMaxLengthValues<T>();

            if (readdropdown != null)
                readdropdown();

            ViewData["Mensagem"] = Mensagem;

            return View();
        }

        /// <summary>
        /// Salva uma nova inclusão
        /// </summary>
        /// <typeparam name="T">Tipo da tabela</typeparam>
        /// <param name="commommethod">Método a ser executado antes, normalmente um DropDownList</param>
        /// <param name="businessValidator">Método de validação de campos</param>
        /// <param name="collection">Collection do formulário</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Create<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdown, TBusinessValidator<T> businessValidator,
                                         FormCollection collection, List<string> redirectToEdit = null, string Mensagem = null) where T : class, new()
        {
            CollectionToViewData(collection);

            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Create);
            if (falhaAtenticacao == null)
                return ComplexCreateMethod<T>(readdropdown, businessValidator, collection, redirectToEdit, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult ComplexCreateMethod<T>(TCommonMethod readdropdown, TBusinessValidator<T> businessValidator,
                                                    FormCollection collection, List<string> redirectToEdit, string Mensagem) where T : class, new()
        {
            try
            {
                T objTable = new T();
                if (businessValidator(collection, ref objTable))
                {                    
                    (bdInstance as Z).CreateObjectSet<T>().AddObject(objTable);
                    (bdInstance as Z).SaveChanges();

                    ViewData["Mensagem"] = Mensagem;

                    if (redirectToEdit != null)
                    {
                        string _keys = "";
                        foreach (var item in redirectToEdit)
                        {
                            Type typeT = typeof(T);
                            PropertyInfo tmpProperty = typeT.GetProperty(item);
                            _keys = _keys + tmpProperty.GetValue(objTable, null) + ";";
                        }

                        _keys = _keys.Substring(0, (_keys.Length - 1));
                        return RedirectToAction("Edit", new { keys = _keys });
                    }
                    else
                        return RedirectToAction("List");

                }

                if (readdropdown != null)
                    readdropdown();

                return View("Create");
            }
            catch (Exception ex)
            {
                if (readdropdown != null)
                    readdropdown();
                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View("Create");
            }
        }

        /// <summary>
        /// Entra na tela de edição
        /// </summary>
        /// <typeparam name="T">Tipo da tabela</typeparam>
        /// <param name="commommethod">Método a ser executado antes, normalmente um DropDownList</param>
        /// <param name="where">Expressão lambda para a cláusula where.</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Edit<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdown, Func<T, bool> where, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Edit);
            if (falhaAtenticacao == null)
                return CommomEditMethod<T>(readdropdown, where, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult CommomEditMethod<T>(TCommonMethod readdropdown, Func<T, bool> where, string Mensagem) where T : class
        {
            GetMaxLengthValues<T>();
            try
            {
                ViewData["Mensagem"] = Mensagem;
                return EditDeleteDetailStandard<T>(readdropdown, where);
            }
            catch (Exception ex)
            {
                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Salva uma edição
        /// </summary>
        /// <typeparam name="T">Tipo da tabela</typeparam>
        /// <param name="commommethod">Método a ser executado antes, normalmente um DropDownList</param>
        /// <param name="businessValidator">Método de validação de campos</param>
        /// <param name="where">Expressão lambda para a cláusula where</param>
        /// <param name="collection">Collection do formulário</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Edit<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdown, TBusinessValidator<T> businessValidator, 
                                       Func<T, bool> where, FormCollection collection, string Mensagem = null) where T : class
        {
            CollectionToViewData(collection);
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Edit);
            if (falhaAtenticacao == null)
                return ComplexEditMethod<T>(readdropdown, businessValidator, where, collection, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult ComplexEditMethod<T>(TCommonMethod readdropdown, TBusinessValidator<T> businessValidator, 
                                                  Func<T, bool> where, FormCollection collection, string Mensagem) where T : class
        {
            T objTable = (bdInstance as Z).CreateObjectSet<T>().Where(where).ToList().First();
            ViewData.Model = objTable;
            try
            {
                if (readdropdown != null)
                    readdropdown();

                if (businessValidator(collection, ref objTable))
                {                    
                    (bdInstance as Z).SaveChanges();
                    this.ViewData["Mensagem"] = Mensagem;
                    return RedirectToAction("List");
                }

                return View("Edit");
            }
            catch (Exception ex)
            {
                if (readdropdown != null)
                    readdropdown();

                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View("Edit");
            }
        }

        /// <summary>
        /// Deleta um registro, ou entra na tela de confirmação
        /// </summary>
        /// <typeparam name="T">Tipo de tabela</typeparam>
        /// <param name="commommethod">DropDownList a ser executado antes.</param>
        /// <param name="where">Expressão lambda para a cláusula where</param>
        /// <param name="certeza">Caso true, irá deletar. Caso false irá entrar na tela de confirmação.</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Delete<T>(TPermissionCheck permissionCheck, TCommonMethod readdropdown, 
                                         TBusinessValidatorPlusError<T> businessValidatorForDelete, 
                                         Func<T, bool> where, Boolean certeza, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Delete);
            if (falhaAtenticacao == null)
                return DeleteMethod<T>(readdropdown, businessValidatorForDelete, where, certeza, Mensagem);
            else
                return falhaAtenticacao;
        }

        private ActionResult DeleteMethod<T>(TCommonMethod readdropdown, TBusinessValidatorPlusError<T> businessValidatorForDelete, 
                                             Func<T, bool> where, Boolean certeza, string Mensagem) where T : class
        {
            T objTable = (bdInstance as Z).CreateObjectSet<T>().Where(where).FirstOrDefault();
            string tmpError = null;

            try
            {
                if (!certeza)
                    return EditDeleteDetailStandard<T>(readdropdown, where);
                else
                {
                    if (businessValidatorForDelete != null)
                    {
                        if (businessValidatorForDelete(null, ref objTable, out tmpError))
                            return SubDeleteMethod(ref objTable, Mensagem);
                        else
                            throw new Exception(tmpError);
                    }
                    else
                        return SubDeleteMethod(ref objTable, Mensagem);                    
                }
            }
            catch (Exception ex)
            {
                if (readdropdown != null)
                    readdropdown();

                ViewData.Model = objTable;

                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                if ((certeza) && (string.IsNullOrEmpty(tmpError)))
                    return RedirectToAction("List");
                else
                    return View();
            }
        }

        protected ActionResult SubDeleteMethod<T>(ref T objTable, string Mensagem)
        {
            (bdInstance as Z).DeleteObject(objTable);
            (bdInstance as Z).SaveChanges();

            this.ViewData["Mensagem"] = Mensagem;

            return RedirectToAction("List");
        }

        /// <summary>
        /// Método privado que contém o método de seleção de registro único padrão, usado tanto no método de deletar (que exija confirmação) como no de edição como no de detalhar.
        /// </summary>
        /// <typeparam name="T">Tipo de tabela.</typeparam>
        /// <param name="commommethod">Método a ser executado antes, normalmente um DropDownList</param>
        /// <param name="where">Expressão lambda para a cláusula where</param>
        /// <returns>Exibe a View</returns>
        private ActionResult EditDeleteDetailStandard<T>(TCommonMethod readdropdown, Func<T, bool> where) where T : class
        {
            T objTable = (bdInstance as Z).CreateObjectSet<T>().Where(where).FirstOrDefault();
            ViewData.Model = objTable;
            try
            {
                if (readdropdown != null)
                    readdropdown();
                return View();
            }
            catch (Exception ex)
            {
                if (readdropdown != null)
                    readdropdown();
                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Chamado quando se quer detalhar um registro.
        /// </summary>
        /// <typeparam name="T">Tipo de tabela.</typeparam>
        /// <param name="where">Expressão lambda para a cláusula where</param>
        /// <returns>Exibe a View</returns>
        protected ActionResult Details<T>(TPermissionCheck permissionCheck, Func<T, bool> where, string Mensagem = null) where T : class
        {
            ActionResult falhaAtenticacao = base.ChecaPermissao(permissionCheck, EnumsIntuitive.TipoPermissao.Details);
            if (falhaAtenticacao == null)
                return DetailsMethod<T>(where, Mensagem);
            else
                return falhaAtenticacao;            
        }

        private ActionResult DetailsMethod<T>(Func<T, bool> where, string Mensagem) where T : class
        {
            try
            {
                this.ViewData["Mensagem"] = Mensagem;
                return EditDeleteDetailStandard<T>(null, where);
            }
            catch (Exception ex)
            {
                this.ViewData["Erro"] = "<br/>Erro: " + ex.Message;
                return View();
            }
        }        

        private void CommommModelError(string nomeKey, string reasonMessage = " é Obrigatório!")
        {
            ModelState.AddModelError(nomeKey, "O Campo " + nomeKey + reasonMessage);
        }

        protected void GetMaxLengthValues<T>(string Prefix = null) where T : class
        {
            ObjectSet<T> tmpTable = (bdInstance as Z).CreateObjectSet<T>();

            Type typeT = typeof(T);
            PropertyInfo[] columnsData = typeT.GetProperties();

            string keyName;
            string tmpPrefix;

            if (Prefix == null)
                tmpPrefix = "";
            else
                tmpPrefix = Prefix + "_";

            foreach (PropertyInfo item in columnsData)
            {
                keyName = "MaxLength_" + tmpPrefix + item.Name;
                try
                {
                    ViewData[keyName] = tmpTable.EntitySet.ElementType.Properties[item.Name].TypeUsage.Facets["MaxLength"].Value;
                }
                catch { }
            }
        }

        /// <summary>
        /// Valida a entidade.
        /// </summary>
        /// <typeparam name="T">Tipo da tabela.</typeparam>
        /// <param name="collection">Conjunto de dados vindos da View.</param>
        /// <param name="objTable">instância da tabela.</param>
        /// <param name="valuesOverload">Dicionário com os dados que serão atualizados </param>
        /// <returns>Retorna verdadeiro se validou.</returns>
        protected bool ValidateEntity<T>(FormCollection collection, ref T objTable, Dictionary<string, string> valuesOverload = null, 
                                         List<ArquivoCollection> arquivos = null) where T : class
        {
            List<string> originalCollection = new List<string>();

            int z = 0;
            while (z < collection.Count)
            {
                originalCollection.Add(collection.GetKey(z));
                z++;
            }

            string tmpValue;
            if (valuesOverload != null)
            {
                int x = 0;
                while (x < valuesOverload.Count)
                {
                    tmpValue = collection[valuesOverload.Keys.ElementAt(x).ToString()];
                    collection.Remove(valuesOverload.Keys.ElementAt(x).ToString());
                    originalCollection.Remove(valuesOverload.Keys.ElementAt(x).ToString()); //Removido e depois adicionado para manter os índices iguais.
                    //Faria isso no collection, mas o .net fez o favor de deixar seus index read-only.                    

                    collection[valuesOverload.Values.ElementAt(x).ToString()] = tmpValue;
                    originalCollection.Add(valuesOverload.Keys.ElementAt(x).ToString());
                    x++;
                }
            }            

            int n = 0;
            while (n < collection.Count)
            {
                if (ColummIsNullable<T>(collection.GetKey(n)) != null)
                {
                    if (GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(string)))
                    {
                        int lengthColumn;
                        if (!LengthIsValid<T>(collection.GetKey(n), collection[n].ToString().Length, out lengthColumn))
                            CommommModelError(originalCollection[n], " deve ter no máximo " + lengthColumn.ToString() + " caracteres, mas tem " + collection[n].ToString().Length.ToString() + " caracteres.");
                        else if ((ColummIsNullable<T>(collection.GetKey(n)).Equals(false)) && (string.IsNullOrEmpty(collection[n])))
                            CommommModelError(originalCollection[n]);
                        else 
                            addingProperty<T, string>(collection.GetKey(n), ref objTable, collection[n]);
                    }
                    else if ((GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(int))) || (GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(int?))))
                    {
                        int intParser;
                        if (int.TryParse(collection[n], out intParser))
                            addingProperty<T, int>(collection.GetKey(n), ref objTable, intParser);
                        else if (ColummIsNullable<T>(collection.GetKey(n)).Equals(false))
                            CommommModelError(originalCollection[n]);
                    }
                    else if ((GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(double))) || (GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(double?))))
                    {
                        double dblParser;
                        if (double.TryParse(collection[n], out dblParser))
                            addingProperty<T, double>(collection.GetKey(n), ref objTable, dblParser);
                        else if (ColummIsNullable<T>(collection.GetKey(n)).Equals(false))
                            CommommModelError(originalCollection[n]);
                    }
                    else if ((GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(DateTime))) || (GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(DateTime?))))
                    {
                        DateTime dateParser;
                        if (DateTime.TryParse(collection[n], out dateParser))
                            addingProperty<T, DateTime>(collection.GetKey(n), ref objTable, dateParser);
                        else if (ColummIsNullable<T>(collection.GetKey(n)).Equals(false))
                            CommommModelError(originalCollection[n]);
                    }
                    else if ((GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(TimeSpan))) || (GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(TimeSpan?))))
                    {
                        TimeSpan timeParser;
                        if (TimeSpan.TryParse(collection[n], out timeParser))
                            addingProperty<T, TimeSpan>(collection.GetKey(n), ref objTable, timeParser);
                        else if (ColummIsNullable<T>(collection.GetKey(n)).Equals(false))
                            CommommModelError(originalCollection[n]);
                    }
                    else if ((GetPropertyType<T>(collection.GetKey(n)).Equals(typeof(Boolean))))
                    {
                        Boolean boolParser;
                        if (Boolean.TryParse(collection[n], out boolParser))
                            addingProperty<T, Boolean>(collection.GetKey(n), ref objTable, boolParser);
                        else
                            addingProperty<T, Boolean>(collection.GetKey(n), ref objTable, true); //Controladores CheckBox do MVC2 não ficam em true, mas ficam em true | false.
                    }
                }
                n++;
            }

            if (arquivos != null)
            {
                foreach (var item in arquivos)
                {
                    try
                    {
                        addingProperty<T, Byte[]>(item.Nome, ref objTable, item.Dados);
                    }
                    catch
                    {
                        ModelState.AddModelError(item.Nome, "O Campo " + item.Nome + "não pôde ser gravado.");
                    }
                }
            }

            return ModelState.IsValid;
        }

        private void addingProperty<T, U>(string name, ref T objTable, U value) where T : class
        {
            Type tmpType = typeof(T);
            PropertyInfo tmpProperty = tmpType.GetProperty(name);
            try
            {
                tmpProperty.SetValue(objTable, value, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Verifica se a coluna da tabela é nula.
        /// </summary>
        /// <typeparam name="T">Tipo de tabela.</typeparam>
        /// <param name="propertyName">Nome da coluna da tabela.</param>
        /// <returns>Retorna true se a propriedade for nullable, false se não for ou null caso não exista a coluna.</returns>
        private bool? ColummIsNullable<T>(string colummName) where T : class
        {
            ObjectSet<T> tmpTable = bdContext<Z>.Instance.BD.CreateObjectSet<T>();
            try
            {
                return tmpTable.EntitySet.ElementType.Properties[colummName].Nullable;
            }
            catch
            {
                return null;
            }
        }

        private Type GetPropertyType<T>(string propertyName) where T : class
        {
            Type tmpType = typeof(T);
            PropertyInfo tmpProperty = tmpType.GetProperty(propertyName);
            return tmpProperty.PropertyType;
        }

        private bool LengthIsValid<T>(string columnName, int valueLength, out int columnLength) where T : class
        {
            ObjectSet<T> tmpTable = bdContext<Z>.Instance.BD.CreateObjectSet<T>();
            Type typeT = typeof(T);
            PropertyInfo columnsData = typeT.GetProperty(columnName);

            try
            {
                columnLength = int.Parse(tmpTable.EntitySet.ElementType.Properties[columnName].TypeUsage.Facets["MaxLength"].Value.ToString());
                return valueLength <= columnLength;
            }
            catch
            {
                columnLength = -1;
                return true; //Campos nvarchar retornam -1;
            }
        }
    }
}
