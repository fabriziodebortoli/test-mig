import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_SERIALNOS_FULLService } from './IDD_NUMERATOR_SERIALNOS_FULL.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_SERIALNOS_FULL',
    templateUrl: './IDD_NUMERATOR_SERIALNOS_FULL.component.html',
    providers: [IDD_NUMERATOR_SERIALNOS_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_NUMERATOR_SERIALNOS_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MATRIX_PREFIXFORMAT_itemSource: any;

    constructor(document: IDD_NUMERATOR_SERIALNOS_FULLService,
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
        this.IDC_MATRIX_PREFIXFORMAT_itemSource = {
  "name": "PrefixFormatCombo",
  "namespace": "ERP.LotsSerials.Components.LotsSerialsPrefixEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'SerialNumbers':['LastDocNo','LastDocDate','NoPrefix','PrefixFormat','BalanceYear','SeparatorCode','Suffix','SuffixChars'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_SERIALNOS_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_SERIALNOS_FULLComponent, resolver);
    }
} 