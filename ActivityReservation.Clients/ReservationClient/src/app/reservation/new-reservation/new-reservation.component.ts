import { Component, OnInit } from '@angular/core';
import { Reservation } from '../../models/Reservation';
import { ReservationService } from '../../services/ReservationService';
import { ReservationPlaceService } from 'src/app/services/ReservationPlaceService';
import { ReservationPlace } from 'src/app/models/ReservationPlace';
import {FormBuilder, FormGroup, Validators, FormArray, FormControl} from '@angular/forms';
import { ReservationPeriod } from 'src/app/models/ReservationPeriod';

@Component({
  selector: 'app-new-reservation',
  templateUrl: './new-reservation.component.html'
})
export class NewReservationComponent implements OnInit {

  public reservationPlaces: Array<ReservationPlace>;
  public reservationPeriods: Array<ReservationPeriod>;

  public minDate: Date;
  public maxDate: Date;
  
  placeFormGroup: FormGroup;
  dateFormGroup: FormGroup;
  periodFormGroup: FormGroup;
  personFormGroup: FormGroup;

  checkedPeriodsFormArray: FormArray;

  reservation: Reservation;

  constructor(private reservationSvc: ReservationService, 
    private reservationPlaceSvc: ReservationPlaceService,
    private _formBuilder: FormBuilder) {
    var now = new Date();
    this.minDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    this.maxDate = new Date(this.minDate.getTime()+ 24 *60*60*1000*7);
  }

  ngOnInit() {    
    this.placeFormGroup = this._formBuilder.group({
      placeCtrl: ['', Validators.required]
    });
    this.dateFormGroup = this._formBuilder.group({
      dateCtrl: ['', Validators.required]
    });
    this.periodFormGroup = this._formBuilder.group({
      periods: this._formBuilder.array([])
    });
    this.personFormGroup = this._formBuilder.group({
      unitCtrl: ['', Validators.required],
      contentCtrl: ['', Validators.required],
      personNameCtrl: ['', Validators.required],
      phoneCtrl: ['', Validators.required]
    });
    this.checkedPeriodsFormArray = this.periodFormGroup.get('periods') as FormArray;

    this.loadData();

    this.reservation = new Reservation();
  }

  private loadData(): void{
    this.reservationPlaceSvc.GetAll()
    .subscribe(data => {
      this.reservationPlaces = data;
    });
  }

  onSelectionChange(event): void{
    let stepIndex = event.selectedIndex;
    console.log(`stepIndex: ${stepIndex}`);
    //
    switch(stepIndex)
    {
      case 0:
        //
        break;

      case 1:
        console.log(this.placeFormGroup.value.placeCtrl);
        this.reservation.ReservationPlaceId = this.placeFormGroup.value.placeCtrl;
        this.reservation.ReservationPlaceName = this.reservationPlaces.filter(p=>p.PlaceId== this.reservation.ReservationPlaceId)[0].PlaceName;
        break;

      case 2:
        let date = new Date(this.dateFormGroup.value.dateCtrl);
        let dt = `${date.getFullYear()}-${date.getMonth()<9? `0${date.getMonth()+1}` : date.getMonth()+1}-${date.getDate()}`;
        console.log(dt);
        this.reservation.ReservationForDate = dt;
        // load periods
        this.reservationPlaceSvc.getAvailablePeriods(this.reservation.ReservationPlaceId, this.reservation.ReservationForDate)
        .subscribe(x=> 
        {
          this.reservationPeriods = x;
          this.checkedPeriodsFormArray.clear();
          this.reservationPeriods.forEach(x => this.checkedPeriodsFormArray.push(new FormControl(false)));
        });
        
        break;
      
      case 3:
        console.log(this.periodFormGroup);
        // period
        console.log(this.reservationPeriods);
        //
        let checkedPeriods = this.reservationPeriods.filter(p=>p.Checked);
        this.reservation.ReservationForTime = checkedPeriods.map(p=>p.PeriodTitle).join(","); 
        this.reservation.ReservationForTimeIds = checkedPeriods.map(p=>p.PeriodIndex).join(",");
        break;

      case 4:
        console.log(this.personFormGroup);
        console.log(this.reservation);
        break;

      default:
        console.log(this.reservation);
        break;
    }
  }

  onSubmitReservation(): void{
    this.reservationSvc.NewReservation(this.reservation, 'None', '')
    .subscribe(x=> console.log(x));
  }
}
