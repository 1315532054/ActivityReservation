import { ReservationService } from '../../services/ReservationService';
import { ReservationPlaceService } from '../../services/ReservationPlaceService';
import { Reservation } from '../../models/Reservation';
import { ReservationPlace } from '../../models/ReservationPlace';

const reservationSvc = new ReservationService();
const reservationPlaceSvc = new ReservationPlaceService();

Page({
  data: {
    steps: [
      {
        text: '选择活动室',
        desc: '选择要预约的活动室'
      },
      {
        text: '选择日期',
        desc: '选择要预约的日期'
      },
      {
        text: '选择时间段',
        desc: '选择要预约的时间段'
      },
      {
        text: '预约信息',
        desc: '填写预约信息'
      },
      {
        text: '确认预约',
        desc: '确认要预约的信息'
      }
    ],
    stepIndex: 0,
    places: [] as Array<ReservationPlace>,
    placeNames: [] as Array<string>,
    currentDate: new Date().getTime(),
  },
  onLoad(params: any) {
    console.log(params);
    reservationPlaceSvc.GetAll((result)=>{
      (<any>this).setData({
        places: result,
        placeNames: result.map(p=>p.PlaceName)
      });
    })
  },
  prevStep(event: any) {
    (<any>this).setData({
      stepIndex: --this.data.stepIndex
    });
  },
  nextStep(event:any){
    (<any>this).setData({
      stepIndex: ++this.data.stepIndex
    });
  },

  onPlaceChange(event:any) {
    const { picker, value, index } = event.detail;
    // `当前值：${value}, 当前索引：${index}`
    wx.showToast({
      title: `${value}`,
      duration: 2000,
    });

  },

  submit(event: any){

  }
})