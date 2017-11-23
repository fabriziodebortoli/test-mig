import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TEMPLATESCONSOLIDService } from './IDD_TEMPLATESCONSOLID.service';

@Component({
    selector: 'tb-IDD_TEMPLATESCONSOLID',
    templateUrl: './IDD_TEMPLATESCONSOLID.component.html',
    providers: [IDD_TEMPLATESCONSOLIDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TEMPLATESCONSOLIDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TEMPLATESCONSOLIDService,
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
		boService.appendToModelStructure({'ConsolidationTemplates':['Template','Description','Suffix','CompanyIdentifier','Mask'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['Description','l_DebitBalanceAccount','l_CreditBalanceAccount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TEMPLATESCONSOLIDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TEMPLATESCONSOLIDComponent, resolver);
    }
} 