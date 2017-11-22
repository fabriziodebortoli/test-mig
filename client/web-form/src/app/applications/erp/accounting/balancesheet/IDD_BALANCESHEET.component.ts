import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCESHEETService } from './IDD_BALANCESHEET.service';

@Component({
    selector: 'tb-IDD_BALANCESHEET',
    templateUrl: './IDD_BALANCESHEET.component.html',
    providers: [IDD_BALANCESHEETService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BALANCESHEETComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCESHEETService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FromDate','ToDate','WithClosing','ByPostDate','WithOpening','OtherPeriod','PrevFromDate','PrevToDate','AL','AllALRadio','ALRadioSel','FromALLedger','ToALLedger','PL','AllPLRadio','PLRadioSel','FromPLLedger','ToPLLedger','RadioSegment0','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','Segment0Tot','RadioSegment4','SegmentName4','Segment4Tot','RadioSegment1','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName1','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','Segment1Tot','RadioSegment5','SegmentName5','Segment5Tot','RadioSegment2','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName2','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','Segment2Tot','RadioSegment6','SegmentName6','Segment6Tot','RadioSegment3','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','SegmentName3','Segment3Tot','RadioSegment7','SegmentName7','Segment3Tot','RadioSegment7','SegmentName7','NoCustSupp','RadioSegment4','SegmentName4','Segment4Tot','SegmentName4','Segment4Tot','RadioSegment5','SegmentName5','Segment5Tot','SegmentName5','Segment5Tot','RadioSegment6','SegmentName6','Segment6Tot','SegmentName6','Segment6Tot','RadioSegment7','SegmentName7','AllKind','Forecast','bAccSimulSel','HeadingAdditionalData','Simplified','strViewCorrSection','IgnoreSignSection','OnlyDebitBalMemo','Language','ReferenceCurrency','NaturalCurrencyBalance']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCESHEETFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCESHEETComponent, resolver);
    }
} 