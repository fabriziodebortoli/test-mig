import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_BR_UNUSED_NUMBERSService } from './IDD_TD_BR_UNUSED_NUMBERS.service';

@Component({
    selector: 'tb-IDD_TD_BR_UNUSED_NUMBERS',
    templateUrl: './IDD_TD_BR_UNUSED_NUMBERS.component.html',
    providers: [IDD_TD_BR_UNUSED_NUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TD_BR_UNUSED_NUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TD_BR_UNUSED_NUMBERSService,
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
		boService.appendToModelStructure({'global':['Model','Series','AllDate','DateSel','StartingDate','EndingDate','AllNum','NumSel','StartingNum','EndingNum','Reason','Detail'],'Detail':['TEnhUnused_bSelected','TEnhUnused_FromDocNo','TEnhUnused_ToDocNo','TEnhUnused_cStat','TEnhUnused_Motivo','TEnhUnused_ElabDate','TEnhUnused_Prot','TEnhUnused_OperationDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_BR_UNUSED_NUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_BR_UNUSED_NUMBERSComponent, resolver);
    }
} 