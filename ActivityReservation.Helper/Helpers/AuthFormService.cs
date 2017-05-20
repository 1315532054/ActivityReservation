﻿using System;
using System.Web;
using System.Web.Security;

namespace ActivityReservation.Helpers
{
    public class AuthFormService
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        private const string EncryptString = "ReservationSystem";
        /// <summary>
        /// loginCookieName 登录cookie名称
        /// </summary>
        private const string LoginCookieName = "LoginCookieName";

        /// <summary>
        /// 授权缓存key
        /// </summary>
        private const string AuthCacheKey = "Admin";

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <returns></returns>
        public static Models.User GetCurrentUser()
        {
            return Common.RedisHelper.Get<Models.User>(AuthCacheKey);
        }

        /// <summary>
        /// 设置当前登录用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns></returns>
        public static bool SetCurrentUser(Models.User user)
        {
            return Common.RedisHelper.Set(AuthCacheKey, user, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// 登录成功，保存用户登录信息
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="rememberMe">是否保存cookie</param>
        public static void Login(string loginName,bool rememberMe)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(loginName+EncryptString, rememberMe, 30);
            string cookieVal = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(LoginCookieName, cookieVal) { Expires = DateTime.Now.AddDays(1),HttpOnly = true };
            FormsAuthentication.SetAuthCookie(loginName, rememberMe);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 尝试自动登录
        /// </summary>
        /// <returns>是否登录成功</returns>
        public static bool TryAutoLogin()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[LoginCookieName];
            if (cookie != null)
            {
                string cookieValue = cookie.Value;
                var ticket = FormsAuthentication.Decrypt(cookieValue);
                string loginName = ticket.Name.Substring(0,ticket.Name.IndexOf(EncryptString));
                Models.User user= new Business.BLLUser().GetOne(u => u.UserName == loginName);
                if (user != null)
                {
                    Common.RedisHelper.Set<Models.User>(AuthCacheKey, user);
                    cookie.Expires = DateTime.Now.AddDays(1);
                    cookie.HttpOnly = true;
                    HttpContext.Current.Response.Cookies.Add(cookie);
                    FormsAuthentication.SetAuthCookie(loginName, true);
                    return true;
                }                
            }            
            return false;
        }

        /// <summary>
        /// 退出登录 logout
        /// </summary>
        public static void Logout()
        {
            //sign out
            FormsAuthentication.SignOut();
            //remove and set cookie expires  
            //remove first,and then set expires,or you will still have the cookie,can not log out
            HttpContext.Current.Response.Cookies.Remove(LoginCookieName);
            HttpContext.Current.Response.Cookies[LoginCookieName].Expires = DateTime.Now.AddDays(-1);
            //remove cache
            Common.RedisHelper.Remove(AuthCacheKey);
            //remove session
            HttpContext.Current.Session.Abandon();
        }
    }
}