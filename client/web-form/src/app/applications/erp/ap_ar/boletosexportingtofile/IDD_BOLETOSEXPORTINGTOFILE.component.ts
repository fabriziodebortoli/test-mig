import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLETOSEXPORTINGTOFILEService } from './IDD_BOLETOSEXPORTINGTOFILE.service';

@Component({
    selector: 'tb-IDD_BOLETOSEXPORTINGTOFILE',
    templateUrl: './IDD_BOLETOSEXPORTINGTOFILE.component.html',
    providers: [IDD_BOLETOSEXPORTINGTOFILEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOLETOSEXPORTINGTOFILEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOLETOSEXPORTINGTOFILEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BankCode','BankCondition','FromDueDate','ToDueDate','bReprint','FromIssuingDate','ToIssuingDate','ExportFilePath','bDefPrint'],'HKLBanks':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLETOSEXPORTINGTOFILEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLETOSEXPORTINGTOFILEComponent, resolver);
    }
} 