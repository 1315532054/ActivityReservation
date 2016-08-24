﻿using ActivityReservation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ActivityReservation.AdminLogic.Controllers
{    
    public class BaseAdminController:Controller
    {
        #region BusinessHelper 提供对Business层的访问对象
        private IBusinessHelper businessHelper;
        protected IBusinessHelper BusinessHelper
        {
            get
            {
                if (businessHelper == null)
                {
                    businessHelper = new BusinessHelper();
                }
                return businessHelper;
            }
        }
        #endregion

        protected static Common.LogHelper logger = new Common.LogHelper(typeof(BaseAdminController));

        /// <summary>
        /// 管理员姓名
        /// </summary>
        public string Username { get; set; }

        public Models.User CurrentUser
        {
            get
            {
                return (Session["User"] as Models.User);
            }
        }
        
    }
}
