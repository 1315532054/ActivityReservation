﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public partial class DALReservation
    {        
        public List<Models.Reservation> GetReservationList<Tkey,TKey1>(int pageIndex, int pageSize, out int rowsCount, Expression<Func<Models.Reservation, bool>> whereLambda, Expression<Func<Models.Reservation,Tkey>> orderBy, Expression<Func<Models.Reservation, TKey1>> orderby1, bool isAsc, bool isAsc1)
        {
            try
            {
                //查询总的记录数
                rowsCount = db.Set<Models.Reservation>().Where(whereLambda).Count();
                // 分页 一定注意： Skip 之前一定要 OrderBy
                if (isAsc)
                {
                    if (isAsc1)
                    {

                        return db.Set<Models.Reservation>().Include("Place").Where(whereLambda).OrderBy(orderBy).ThenBy(orderby1).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                    else
                    {

                        return db.Set<Models.Reservation>().Include("Place").Where(whereLambda).OrderBy(orderBy).ThenByDescending(orderby1).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                }
                else
                {
                    if (isAsc1)
                    {

                        return db.Set<Models.Reservation>().Include("Place").Where(whereLambda).OrderByDescending(orderBy).ThenBy(orderby1).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                    else
                    {

                        return db.Set<Models.Reservation>().Include("Place").Where(whereLambda).OrderByDescending(orderBy).ThenByDescending(orderby1).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                rowsCount = -1;
                return null;
            }
        }
    }
}
