﻿using System;
using System.IO;
using System.Linq;
using ActivityReservation.WechatAPI.Helper;
using ActivityReservation.WechatAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Log;
using WeihanLi.Extensions;

namespace ActivityReservation.WechatAPI.Controllers
{
    public class HomeController : WechatBaseController
    {
        [HttpGet]
        [ActionName("Index")]
        public void Get(WechatMsgRequestModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var echoStr = HttpContext.Request.Query["echostr"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(echoStr))
                    {
                        //将随机生成的 echostr 参数 原样输出
                        Response.Body.Write(echoStr.GetBytes());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Wechat GET 发生异常,异常信息：{ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="model">微信消息</param>
        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post([FromBody]WechatMsgRequestModel model)
        {
            if (model.RequestContent == null)
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    Logger.Debug($"Request Length:{Request.Body.Length}");
                    model.RequestContent = reader.ReadToEnd();
                    Logger.Debug($"RequestContent from Request.Body:{model.RequestContent}");
                }
            }
            if (string.IsNullOrEmpty(model.RequestContent))
            {
                return Content("RequestContent 为空");
            }
            var context = new WechatContext(model);
            return Wechat(context);
        }
    }
}
