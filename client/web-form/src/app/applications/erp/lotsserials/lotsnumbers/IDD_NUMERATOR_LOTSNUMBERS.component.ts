import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_LOTSNUMBERSService } from './IDD_NUMERATOR_LOTSNUMBERS.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_LOTSNUMBERS',
    templateUrl: './IDD_NUMERATOR_LOTSNUMBERS.component.html',
    providers: [IDD_NUMERATOR_LOTSNUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_NUMERATOR_LOTSNUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LOTSNO_PREFIXFORMAT_itemSource: any;

    constructor(document: IDD_NUMERATOR_LOTSNUMBERSService,
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
        this.IDC_LOTSNO_PREFIXFORMAT_itemSource = {
  "name": "PrefixFormatCombo",
  "namespace": "ERP.LotsSerials.Components.LotsSerialsPrefixEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'LotsNumbers':['LastLotNo','LastLotDocDate','PrefixFormat','BalanceYear','SeparatorCode','SuffixChars'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_LOTSNUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_LOTSNUMBERSComponent, resolver);
    }
} 