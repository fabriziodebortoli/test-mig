import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCE_SHEET_SUMMARIZEDService } from './IDD_BALANCE_SHEET_SUMMARIZED.service';

@Component({
    selector: 'tb-IDD_BALANCE_SHEET_SUMMARIZED',
    templateUrl: './IDD_BALANCE_SHEET_SUMMARIZED.component.html',
    providers: [IDD_BALANCE_SHEET_SUMMARIZEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BALANCE_SHEET_SUMMARIZEDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCE_SHEET_SUMMARIZEDService,
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
        
        		this.bo.appendToModelStructure({'global':['FromDate','ToDate','WithClosing','ByPostDate','WithOpening','FromDate','ToDate','WithClosing','WithOpening','Previous','PreviousOnlyBalances','FromDatePrev','ToDatePrev','AllRadio','RadioSel','FromLedger','ToLedger','Posted','NotZero','All','RadioSegment0','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','RadioSegment1','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','RadioSegment2','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','RadioSegment3','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','Segment3Tot','RadioSegment7','SegmentName7','NoCustSupp','RadioSegment4','SegmentName4','Segment4Tot','SegmentName4','Segment4Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName5','Segment5Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName6','Segment6Tot','RadioSegment7','SegmentName7','AllKind','Forecast','bAccSimulSel','Initial','Complete','Period','Total','HeadingAdditionalData','Simplified','Language','ReferenceCurrency','NaturalCurrencyBalance']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCE_SHEET_SUMMARIZEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCE_SHEET_SUMMARIZEDComponent, resolver);
    }
} 