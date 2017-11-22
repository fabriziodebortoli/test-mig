import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INTRA_SERVICESService } from './IDD_INTRA_SERVICES.service';

@Component({
    selector: 'tb-IDD_INTRA_SERVICES',
    templateUrl: './IDD_INTRA_SERVICES.component.html',
    providers: [IDD_INTRA_SERVICESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INTRA_SERVICESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INTRA_SERVICESService,
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
		boService.appendToModelStructure({'global':['PeriodMonth','Period','Year','PeriodQuarter','Period','Year','NrRef','DispatchesSummary','ArrivalsSummary','HeaderData','BranchCode','DailyNo','SuppTaxIdNumber','CheckNormal','CheckFirst','CheckChange','CheckBoth','DelegTaxIdNo','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INTRA_SERVICESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INTRA_SERVICESComponent, resolver);
    }
} 