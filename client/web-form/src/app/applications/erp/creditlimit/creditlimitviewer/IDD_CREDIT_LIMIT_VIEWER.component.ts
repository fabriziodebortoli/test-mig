import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CREDIT_LIMIT_VIEWERService } from './IDD_CREDIT_LIMIT_VIEWER.service';

@Component({
    selector: 'tb-IDD_CREDIT_LIMIT_VIEWER',
    templateUrl: './IDD_CREDIT_LIMIT_VIEWER.component.html',
    providers: [IDD_CREDIT_LIMIT_VIEWERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CREDIT_LIMIT_VIEWERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CREDIT_LIMIT_VIEWERService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CLViewerSingleOrderLimit','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerOrderedLimit','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerTurnoverLimit','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverBills','CLViewerTurnoverOtherPaymentTerms','CLViewerTurnoverInvWithoutRec','CLViewerDeliveredDocNotInvoiced','CLViewerTotalExposureLimit','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','Documents'],'Documents':['CreditType','DocumentType','DocumentNo','DocumentDate','Amount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CREDIT_LIMIT_VIEWERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CREDIT_LIMIT_VIEWERComponent, resolver);
    }
} 