import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_JE_SAVETEMPLATEService } from './IDD_JE_SAVETEMPLATE.service';

@Component({
    selector: 'tb-IDD_JE_SAVETEMPLATE',
    templateUrl: './IDD_JE_SAVETEMPLATE.component.html',
    providers: [IDD_JE_SAVETEMPLATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_JE_SAVETEMPLATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_JE_SAVETEMPLATEService,
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
        
        		this.bo.appendToModelStructure({'global':['NameTemplateFromToSave','TemplateDescriFromToSave','bSaveGroup','bSaveCurrency','bSaveGLDetail','bSaveAmountsGLJournal','bSaveCustSuppGLJournal','bSaveTaxJournal','bSaveCustSuppHeader','bSaveTaxDetail','bSaveAmountsTaxJournals']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_JE_SAVETEMPLATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_JE_SAVETEMPLATEComponent, resolver);
    }
} 