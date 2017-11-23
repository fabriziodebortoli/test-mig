import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CALENDARSService } from './IDD_CALENDARS.service';

@Component({
    selector: 'tb-IDD_CALENDARS',
    templateUrl: './IDD_CALENDARS.component.html',
    providers: [IDD_CALENDARSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CALENDARSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CALENDARSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        		this.bo.appendToModelStructure({'global':['WorkingDays','WorkingMonths','CalendarHolidays','CalendarShifts','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'WorkingDays':['VCalendarWorkingPeriod_p1','VCalendarWorkingPeriod_p2'],'WorkingMonths':['VCalendarWorkingPeriod_p1','VCalendarWorkingPeriod_p2'],'CalendarHolidays':['StartingDay','EndingDay','ReasonOfExclusion'],'Calendars':['ShiftDays','MoveShiftsOnExclDays'],'CalendarShifts':['DayNo','StartingHour','StartingMinute','EndingHour','EndingMinute','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CALENDARSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CALENDARSComponent, resolver);
    }
} 