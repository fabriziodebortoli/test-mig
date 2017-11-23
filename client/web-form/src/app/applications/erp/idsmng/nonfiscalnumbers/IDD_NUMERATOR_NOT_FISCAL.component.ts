import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_NOT_FISCALService } from './IDD_NUMERATOR_NOT_FISCAL.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_NOT_FISCAL',
    templateUrl: './IDD_NUMERATOR_NOT_FISCAL.component.html',
    providers: [IDD_NUMERATOR_NOT_FISCALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_NUMERATOR_NOT_FISCALComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_NONOEND_PREFIXFORMAT_itemSource: any;

    constructor(document: IDD_NUMERATOR_NOT_FISCALService,
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
        this.IDC_NONOEND_PREFIXFORMAT_itemSource = {
  "name": "TypeNoPrefixEnumCombo",
  "namespace": "ERP.IdsMng.Services.TypeNoPrefixEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'NonFiscalNumbers':['CodeType','DisableManualMod','LastDocDate','LastDocNo','PrefixFormat','Separators','Suffix','SuffixChars','PrefixWithSiteCode'],'global':['ExtendedNo','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_NOT_FISCALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_NOT_FISCALComponent, resolver);
    }
} 