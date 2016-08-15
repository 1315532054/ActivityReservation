﻿using System.Web;
using System.Web.Mvc;

namespace ActivityReservation.Filters
{
    /// <summary>
    /// 需要登录才能进行操作
    /// </summary>
    public class PermissionRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["User"]==null)
            {
                filterContext.Result = new RedirectResult("~/Admin/Account/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// 需要有管理员权限
    /// </summary>
    public class AdminPermissionRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.HttpContext.Session["User"] == null) || !((filterContext.HttpContext.Session["User"] as Models.User).IsSuper))
            {
                filterContext.Result = new RedirectResult("~/Admin/Account/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
