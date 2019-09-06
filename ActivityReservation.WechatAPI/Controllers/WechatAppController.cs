﻿using System;
using System.Text;
using System.Threading.Tasks;
using ActivityReservation.Common;
using ActivityReservation.WechatAPI.Helper;
using ActivityReservation.WechatAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Extensions;
using WeihanLi.Redis;

namespace ActivityReservation.WechatAPI.Controllers
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    public class WeChatAppController : WeChatBaseController
    {
        public WeChatAppController(ILogger<WeChatAppController> logger) : base(logger)
        {
        }

        [HttpGet]
        [ActionName("Index")]
        public async Task Get(string echoStr)
        {
            if (!string.IsNullOrEmpty(echoStr))
            {
                await Response.WriteAsync(echoStr, HttpContext.RequestAborted);
            }
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> Post()
        {
            var body = await Request.Body.ReadToEndAsync();
            Logger.LogInformation($"received msg: {body}");
            var model = body.JsonToType<WeChatTextMsgModel>();
            if (!string.IsNullOrWhiteSpace(model?.FromUserName))
            {
                try
                {
                    if (await RedisManager.GetFirewallClient($"wxAppMsg:{model.MsgId}", TimeSpan.FromMinutes(2)).HitAsync())
                    {
                        Logger.LogInformation($"msgModel: {model.ToJson()}");

                        switch (model.MsgType)
                        {
                            case "text":
                                var chatbotHelper = HttpContext.RequestServices.GetRequiredService<ChatBotHelper>();
                                var reply = await chatbotHelper.GetBotReplyAsync(model.Content, HttpContext.RequestAborted);
                                if (reply.IsNotNullOrEmpty())
                                {
                                    Logger.LogInformation($"bot reply:{reply}");
                                    //
                                    var wechatHelper = HttpContext.RequestServices.GetRequiredService<WeChatHelper>();
                                    await wechatHelper.SendWechatMsg(new
                                    {
                                        touser = model.FromUserName,
                                        msgtype = "text",
                                        text = new
                                        {
                                            content = reply
                                        }
                                    }, WxAppConsts.AppId, WxAppConsts.AppSecret).ConfigureAwait(false);
                                }
                                break;

                            case "image":
                                break;

                            case "event":
                                //
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "微信小程序客服消息处理发生");
                }
            }

            return Content("success", "text/plain", Encoding.UTF8);
        }
    }
}
