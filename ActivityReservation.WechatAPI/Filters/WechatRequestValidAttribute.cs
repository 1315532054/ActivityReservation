﻿using ActivityReservation.WechatAPI.Helper;
using ActivityReservation.WechatAPI.Model;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Web.Mvc;
using WeihanLi.Common.Helpers;

namespace ActivityReservation.WechatAPI.Filters
{
    public class WechatRequestValidAttribute : FilterAttribute, IAuthorizationFilter
    {
        private static LogHelper logger = new LogHelper(typeof(WechatRequestValidAttribute));
        private WechatMsgRequestModel model = new WechatMsgRequestModel();

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            model.Nonce = filterContext.HttpContext.Request.QueryString["nonce"];
            model.Signature = filterContext.HttpContext.Request.QueryString["signature"];
            model.Timestamp = filterContext.HttpContext.Request.QueryString["timestamp"];
            model.Msg_Signature = filterContext.HttpContext.Request.QueryString["msg_signature"];
            if (filterContext.HttpContext.Request.HttpMethod.ToUpper() == "POST")
            {
                var request = filterContext.HttpContext.Request;

                // Fix request.InputStream can not read twice problem,see <https://stackoverflow.com/questions/1678846/how-to-log-request-inputstream-with-httpmodule-then-reset-inputstream-position>

                var bytes = new byte[request.InputStream.Length];
                request.InputStream.Position = 0;
                request.InputStream.Read(bytes, 0, bytes.Length);
                model.RequestContent = Encoding.UTF8.GetString(bytes);
                // reset stream position

                request.ContentType = model.RequestContent;
            }
            logger.Debug("微信请求信息:" + JsonConvert.SerializeObject(model));
            //验证
            if (!CheckSignature(model))
            {
                logger.Error("微信请求签名验证不通过");
                filterContext.Result = new ContentResult()
                {
                    Content = "微信请求验证失败",
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "text/html"
                };
            }
        }

        private bool CheckSignature(WechatMsgRequestModel model)
        {
            string signature, timestamp, nonce, tempStr;
            //获取请求来的参数
            signature = model.Signature;
            timestamp = model.Timestamp;
            nonce = model.Nonce;
            //创建数组，将 Token, timestamp, nonce 三个参数加入数组
            string[] array = { WeChatConsts.Token, timestamp, nonce };
            //进行排序
            Array.Sort(array);
            //拼接为一个字符串
            tempStr = String.Join("", array);
            //对字符串进行 SHA1加密
            tempStr = SecurityHelper.SHA1_Encrypt(tempStr);
            //判断signature 是否正确
            if (tempStr.Equals(signature.ToUpperInvariant()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}