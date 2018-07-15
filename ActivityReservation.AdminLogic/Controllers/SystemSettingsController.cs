﻿using System;
using System.Linq.Expressions;
using ActivityReservation.Helpers;
using ActivityReservation.Models;
using ActivityReservation.WorkContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeihanLi.AspNetMvc.MvcSimplePager;

namespace ActivityReservation.AdminLogic.Controllers
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public class SystemSettingsController : AdminBaseController
    {
        /// <summary>
        /// 系统设置首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 加载系统设置数据
        /// </summary>
        /// <param name="search">分页信息，查询条件</param>
        /// <returns></returns>
        public ActionResult List(SearchHelperModel search)
        {
            //默认查询所有
            Expression<Func<SystemSettings, bool>> whereLambda = (s => 1 == 1);
            var rowsCount = 0;
            if (!string.IsNullOrEmpty(search.SearchItem1))
            {
                whereLambda = (s => s.SettingName.Contains(search.SearchItem1));
            }
            var settingsList = BusinessHelper.SystemSettingsHelper.GetPagedList(search.PageIndex, search.PageSize,
                out rowsCount, whereLambda, s => s.SettingName);
            var data = settingsList.ToPagedList(search.PageIndex, search.PageSize, rowsCount);
            return View(data);
        }

        /// <summary>
        /// 新增设置
        /// </summary>
        /// <param name="setting">设置</param>
        /// <returns></returns>
        public ActionResult AddSetting(SystemSettings setting)
        {
            try
            {
                setting.SettingId = Guid.NewGuid();
                var count = BusinessHelper.SystemSettingsHelper.Add(setting);
                if (count == 1)
                {
                    OperLogHelper.AddOperLog($"新增系统设置 {setting.SettingName}：{setting.SettingValue}",
                        OperLogModule.Settings, Username);
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return Json(false);
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="setting">设置</param>
        /// <returns></returns>
        public ActionResult UpdateSettings(SystemSettings setting)
        {
            try
            {
                var count = BusinessHelper.SystemSettingsHelper.Update(setting, "SettingValue");
                if (count == 1)
                {
                    OperLogHelper.AddOperLog(
                        $"更新系统设置{setting.SettingId}---{setting.SettingName}：{setting.SettingValue}", OperLogModule.Settings, Username);
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return Json(false);
        }

        public SystemSettingsController(ILogger<SystemSettingsController> logger, OperLogHelper operLogHelper) : base(logger, operLogHelper)
        {
        }
    }
}
