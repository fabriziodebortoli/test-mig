import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALES_PEOPLE_BALANCE_REBUILDINGService } from './IDD_SALES_PEOPLE_BALANCE_REBUILDING.service';

@Component({
    selector: 'tb-IDD_SALES_PEOPLE_BALANCE_REBUILDING',
    templateUrl: './IDD_SALES_PEOPLE_BALANCE_REBUILDING.component.html',
    providers: [IDD_SALES_PEOPLE_BALANCE_REBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALES_PEOPLE_BALANCE_REBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALES_PEOPLE_BALANCE_REBUILDINGService,
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
		boService.appendToModelStructure({'global':['StartMonth','EndMonth','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALES_PEOPLE_BALANCE_REBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALES_PEOPLE_BALANCE_REBUILDINGComponent, resolver);
    }
} 