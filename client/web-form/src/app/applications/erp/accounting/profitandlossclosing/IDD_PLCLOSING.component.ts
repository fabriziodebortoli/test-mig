import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PLCLOSINGService } from './IDD_PLCLOSING.service';

@Component({
    selector: 'tb-IDD_PLCLOSING',
    templateUrl: './IDD_PLCLOSING.component.html',
    providers: [IDD_PLCLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PLCLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PLCLOSINGService,
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
		boService.appendToModelStructure({'global':['ProfitLoss','PLDescri','FiscalYearProfit','YearProfitDescri','FiscalYearLoss','YearLossDescri','PostDate','AccrualDate','DocNumberCost','DocNumberRevenue','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PLCLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PLCLOSINGComponent, resolver);
    }
} 