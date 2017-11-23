import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCOUNTINGBALANCESHEETService } from './IDD_COSTACCOUNTINGBALANCESHEET.service';

@Component({
    selector: 'tb-IDD_COSTACCOUNTINGBALANCESHEET',
    templateUrl: './IDD_COSTACCOUNTINGBALANCESHEET.component.html',
    providers: [IDD_COSTACCOUNTINGBALANCESHEETService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COSTACCOUNTINGBALANCESHEETComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTACCOUNTINGBALANCESHEETService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','CompareWith','strComparedWith','PrevFromDate','PrevToDate','AllKind','Forecast','strBalanceBy','AllRadio','RadioSel','FromCode','ToCode','RadioSegment0','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','RadioSegment1','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','RadioSegment2','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','RadioSegment3','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','Segment3Tot','RadioSegment7','SegmentName7','RadioSegment4','SegmentName4','Segment4Tot','SegmentName4','Segment4Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName5','Segment5Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName6','Segment6Tot','RadioSegment7','SegmentName7','strViewCorrSection','IgnoreSignSection','Language']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCOUNTINGBALANCESHEETFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCOUNTINGBALANCESHEETComponent, resolver);
    }
} 