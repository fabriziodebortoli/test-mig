import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INVENTORYJOURNAL_FULLService } from './IDD_INVENTORYJOURNAL_FULL.service';

@Component({
    selector: 'tb-IDD_INVENTORYJOURNAL_FULL',
    templateUrl: './IDD_INVENTORYJOURNAL_FULL.component.html',
    providers: [IDD_INVENTORYJOURNAL_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INVENTORYJOURNAL_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INVENTORYJOURNAL_FULLService,
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
		boService.appendToModelStructure({'InventoryJournal':['FiscalYear','BalanceYear','MonthBalance','LastPrintingDate','NoOfPrintedLines','NoOfPrintedPages','DefinitivelyPrinted'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INVENTORYJOURNAL_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INVENTORYJOURNAL_FULLComponent, resolver);
    }
} 