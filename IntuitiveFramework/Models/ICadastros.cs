using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntuitiveFramework.Models
{
    public interface ICadastros
    {
        ActionResult List();
        ActionResult List(FormCollection collection);
        ActionResult Create();
        ActionResult Create(FormCollection collection);
        ActionResult Edit(string keys);
        ActionResult Edit(string keys, FormCollection collection);
        ActionResult Delete(string keys, Boolean certeza);
        ActionResult Details(string keys);
        bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class;
    }
}
