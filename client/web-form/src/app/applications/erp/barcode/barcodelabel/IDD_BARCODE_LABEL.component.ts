import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BARCODE_LABELService } from './IDD_BARCODE_LABEL.service';

@Component({
    selector: 'tb-IDD_BARCODE_LABEL',
    templateUrl: './IDD_BARCODE_LABEL.component.html',
    providers: [IDD_BARCODE_LABELService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BARCODE_LABELComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BARCODE_LABELService,
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
		boService.appendToModelStructure({'DBTBarcodeLabel':['NamespaceReport','BarcodeStructureCode','BarcodeStructureCodeAlt1','BarcodeStructureCodeAlt2'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BARCODE_LABELFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BARCODE_LABELComponent, resolver);
    }
} 