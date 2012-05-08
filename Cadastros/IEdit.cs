using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TelasControllers
{
    public interface IEdit
    {
        ActionResult List();
        ActionResult Edit(string keys);
        ActionResult Edit(string keys, FormCollection collection);
        bool BusinessValidator<T>(FormCollection collection, ref T objTable) where T : class;
    }
}
