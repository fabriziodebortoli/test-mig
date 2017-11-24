import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_IPI_LEGAL_CODEService } from './IDD_BR_IPI_LEGAL_CODE.service';

@Component({
    selector: 'tb-IDD_BR_IPI_LEGAL_CODE',
    templateUrl: './IDD_BR_IPI_LEGAL_CODE.component.html',
    providers: [IDD_BR_IPI_LEGAL_CODEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_IPI_LEGAL_CODEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_IPI_LEGAL_CODEService,
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
        
        		this.bo.appendToModelStructure({'DBTBRIPILegalCode':['IPILegalCode','Description','Disabled'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_IPI_LEGAL_CODEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_IPI_LEGAL_CODEComponent, resolver);
    }
} 