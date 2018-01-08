﻿using ActivityReservation.HelperModels;
using ActivityReservation.Helpers;
using ActivityReservation.ViewModels;
using ActivityReservation.WorkContexts;
using Business;
using Common;
using Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using WeihanLi.AspNetMvc.MvcSimplePager;
using WeihanLi.Common.Helpers;
using RequestHelper = Common.RequestHelper;

namespace ActivityReservation.Controllers
{
    public class HomeController : FrontBaseController
    {
        public HomeController() : this(LogHelper.GetLogHelper<HomeController>())
        {
        }

        public HomeController(LogHelper logger) : base(logger)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 预约记录数据页
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public ActionResult ReservationList(SearchHelperModel search)
        {
            Expression<Func<Reservation, bool>> whereLambda = (m =>
                DbFunctions.DiffDays(DateTime.Today, m.ReservationForDate) <= 7 &&
                DbFunctions.DiffDays(DateTime.Today, m.ReservationForDate) >= 0);
            var rowsCount = 0;
            //补充查询条件
            //根据预约日期查询
            if (!String.IsNullOrEmpty(search.SearchItem0))
            {
                whereLambda = (m =>
                    DbFunctions.DiffDays(DateTime.Parse(search.SearchItem0), m.ReservationForDate) == 0);
            }
            //根据预约人联系方式查询
            if (!String.IsNullOrEmpty(search.SearchItem1))
            {
                whereLambda = (m => m.ReservationPersonPhone.Contains(search.SearchItem1));
            }
            //load data
            var list = new BLLReservation().GetReservationList(search.PageIndex, search.PageSize, out rowsCount,
                whereLambda, m => m.ReservationForDate, m => m.ReservationTime, false, false);
            var dataList = list.ToPagedList(search.PageIndex, search.PageSize, rowsCount);
            return View(dataList);
        }

        /// <summary>
        /// 预约页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Reservate()
        {
            var places = new BLLReservationPlace().GetAll(s => s.IsDel == false && s.IsActive, s => s.PlaceId, true);
            return View(places);
        }

        /// <summary>
        /// 预约日期是否可以预约
        /// </summary>
        /// <returns></returns>
        public ActionResult IsReservationForDateValid(DateTime reservationForDate)
        {
            var jsonResult = new JsonResultModel<bool>() { Status = JsonResultStatus.Success };
            string msg;
            var isValid = ReservationHelper.IsReservationForDateAvailabel(reservationForDate, false, out msg);
            if (isValid)
            {
                jsonResult.Data = true;
            }
            else
            {
                jsonResult.Data = false;
                jsonResult.Msg = msg;
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据预约日期和预约地点获取可用的预约时间段
        /// </summary>
        /// <param name="dt">预约日期</param>
        /// <param name="placeId">预约地点id</param>
        /// <returns></returns>
        public ActionResult GetAvailablePeriods(DateTime dt, Guid placeId)
        {
            var periodsStatus = ReservationHelper.GetAvailabelPeriodsByDateAndPlace(dt, placeId);
            return Json(periodsStatus);
        }

        /// <summary>
        /// 预约接口
        /// </summary>
        /// <param name="model">预约信息实体</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MakeReservation(ReservationViewModel model)
        {
            var result = new JsonResultModel { Data = false, Status = JsonResultStatus.RequestError };
            try
            {
                if (ModelState.IsValid)
                {
                    string msg;
                    if (!ReservationHelper.IsReservationAvailabel(model, out msg))
                    {
                        result.Msg = msg;
                        return Json(result);
                    }

                    var reservation = new Reservation()
                    {
                        ReservationForDate = model.ReservationForDate,
                        ReservationForTime = model.ReservationForTime,
                        ReservationPlaceId = model.ReservationPlaceId,

                        ReservationUnit = model.ReservationUnit,
                        ReservationActivityContent = model.ReservationActivityContent,
                        ReservationPersonName = model.ReservationPersonName,
                        ReservationPersonPhone = model.ReservationPersonPhone,

                        ReservationFromIp = HttpContext.Request.UserHostAddress, //记录预约人IP地址

                        UpdateBy = model.ReservationPersonName,
                        UpdateTime = DateTime.Now,
                        ReservationId = Guid.NewGuid()
                    };
                    //TODO:验证最大可预约时间段，同一个手机号，同一个IP地址

                    foreach (var item in model.ReservationForTimeIds.Split(',').Select(_ => Convert.ToInt32(_)))
                    {
                        reservation.ReservationPeriod += (1 << item);
                    }
                    var bValue = new BLLReservation().Add(reservation);
                    if (bValue > 0)
                    {
                        result.Data = true;
                        result.Msg = "预约成功";
                        result.Status = JsonResultStatus.Success;
                    }
                    else
                    {
                        result.Msg = "预约失败";
                        result.Status = JsonResultStatus.ProcessFail;
                    }
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Status = JsonResultStatus.ProcessFail;
                result.Msg = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// Print
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Check(Guid id)
        {
            var r = new BLLReservation().Fetch(re => re.ReservationId == id);
            return View(r);
        }

        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public ActionResult Notice()
        {
            return View();
        }

        /// <summary>
        /// 公告列表
        /// </summary>
        /// <returns></returns>
        public ActionResult NoticeList(SearchHelperModel search)
        {
            Expression<Func<Notice, bool>> whereLamdba = (n => !n.IsDeleted && n.CheckStatus);
            if (!String.IsNullOrEmpty(search.SearchItem1))
            {
                whereLamdba = n => n.NoticeTitle.Contains(search.SearchItem1) && n.CheckStatus;
            }
            try
            {
                int count;
                var noticeList = new BLLNotice().GetPagedList(search.PageIndex, search.PageSize, out count, whereLamdba,
                    n => n.NoticePublishTime, false);
                var data = noticeList.ToPagedList(search.PageIndex, search.PageSize, count);
                return View(data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// 公告详情
        /// </summary>
        /// <param name="path">访问路径</param>
        /// <returns></returns>
        public ActionResult NoticeDetails(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return RedirectToAction("Notice");
            }
            try
            {
                var notice = new BLLNotice().Fetch(n => n.NoticePath == path);
                if (notice != null)
                {
                    return View(notice);
                }
                else
                {
                    return RedirectToAction("Notice");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 获取Geetest验证码
        /// </summary>
        /// <returns></returns>
        public JsonResult GetGeetestValidCode()
        {
            var helper = new GeetestHelper();
            var userIp = RequestHelper.GetRequestIP();
            var gtServerStatus = helper.PreProcess(userIp);
            Session[GeetestConsts.GeetestUserId] = userIp;
            Session[GeetestConsts.GtServerStatusSessionKey] = gtServerStatus;
            return Json(helper.Response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 验证Geetest验证码
        /// </summary>
        /// <returns></returns>
        public JsonResult ValidateGeetestCode()
        {
            var geetestRequest = new GeetestRequestModel
            {
                challenge = Request[GeetestConsts.FnGeetestChallenge],
                validate = Request[GeetestConsts.FnGeetestValidate],
                seccode = Request[GeetestConsts.FnGeetestSeccode]
            };

            return Json(new GeetestHelper()
                .ValidateRequest(geetestRequest,
                    Session[GeetestConsts.GeetestUserId]?.ToString() ?? "",
                    Convert.ToByte(Session[GeetestConsts.GtServerStatusSessionKey]),
                () => { Session.Remove(GeetestConsts.GeetestUserId); }));
        }

        /// <summary>
        /// 验证谷歌验证码
        /// </summary>
        /// <param name="response">response</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ValidateGoogleRecaptchaResponse(string response)
        {
            return Json(GoogleRecaptchaHelper.IsValidRequest(response));
        }
    }
}