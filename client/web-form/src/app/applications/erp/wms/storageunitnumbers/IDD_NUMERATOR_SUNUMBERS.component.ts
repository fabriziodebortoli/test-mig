import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_SUNUMBERSService } from './IDD_NUMERATOR_SUNUMBERS.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_SUNUMBERS',
    templateUrl: './IDD_NUMERATOR_SUNUMBERS.component.html',
    providers: [IDD_NUMERATOR_SUNUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_NUMERATOR_SUNUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SUNO_PREFIXFORMAT_itemSource: any;

    constructor(document: IDD_NUMERATOR_SUNUMBERSService,
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
        this.IDC_SUNO_PREFIXFORMAT_itemSource = {
  "name": "PrefixFormatCombo",
  "namespace": "ERP.WMS.Components.SorageUnitPrefixEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'StorageUnitNumbers':['LastSUNo','LastSUDocDate','PrefixFormat','BalanceYear','SeparatorCode','SuffixChars'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_SUNUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_SUNUMBERSComponent, resolver);
    }
} 