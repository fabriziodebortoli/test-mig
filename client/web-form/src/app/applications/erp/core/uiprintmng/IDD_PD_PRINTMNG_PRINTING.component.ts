import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_PRINTMNG_PRINTINGService } from './IDD_PD_PRINTMNG_PRINTING.service';

@Component({
    selector: 'tb-IDD_PD_PRINTMNG_PRINTING',
    templateUrl: './IDD_PD_PRINTMNG_PRINTING.component.html',
    providers: [IDD_PD_PRINTMNG_PRINTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PD_PRINTMNG_PRINTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_PRINTMNG_PRINTINGService,
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
		boService.appendToModelStructure({'global':['DeviceName','PageFmtName','ReportName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_PRINTMNG_PRINTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_PRINTMNG_PRINTINGComponent, resolver);
    }
} 