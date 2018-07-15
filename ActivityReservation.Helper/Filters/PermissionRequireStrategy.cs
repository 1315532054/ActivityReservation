﻿using ActivityReservation.Helpers;
using ActivityReservation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.AspNetMvc.AccessControlHelper;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Models;

namespace ActivityReservation.Filters
{
    public class AdminPermissionRequireStrategy : IActionAccessStrategy
    {
        public bool IsActionCanAccess(HttpContext httpContext, string accessKey)
        {
            if (httpContext.Session.TryGetValue(AuthFormService.AuthCacheKey, out var bytes))
            {
                var user = new JsonDataSerializer().Deserializer<User>(bytes);
                if (user != null)
                {
                    return user.IsSuper;
                }
            }
            return false;
        }

        public IActionResult DisallowedCommonResult => new ContentResult
        {
            Content = "No Permission",
            ContentType = "text/plain",
            StatusCode = 403
        };

        public JsonResult DisallowedAjaxResult => new JsonResult(new JsonResultModel
        {
            ErrorMsg = "No Permission",
            Status = JsonResultStatus.NoPermission
        });
    }
}
