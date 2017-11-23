import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WEEECTG_FULLService } from './IDD_WEEECTG_FULL.service';

@Component({
    selector: 'tb-IDD_WEEECTG_FULL',
    templateUrl: './IDD_WEEECTG_FULL.component.html',
    providers: [IDD_WEEECTG_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WEEECTG_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WEEECTG_FULLService,
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
		boService.appendToModelStructure({'WEEECategories':['Category','Description','TaxCode','Offset'],'HKLTaxCode':['Description'],'HKLCoAOffset':['Description'],'global':['WEEEAmount','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'WEEEAmount':['StartingValidityDate','EndingValidityDate','Amount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WEEECTG_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WEEECTG_FULLComponent, resolver);
    }
} 