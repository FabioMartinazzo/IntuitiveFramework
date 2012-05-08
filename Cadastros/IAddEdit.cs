using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TelasControllers
{
    public interface IAddEdit
    {
        ActionResult Add();
        ActionResult Add(FormCollection collection);
        ActionResult Edit(string keys);
        ActionResult Edit(string keys, FormCollection collection);
        ActionResult Delete(string keys, Boolean certeza);
        bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class;
    }
}
