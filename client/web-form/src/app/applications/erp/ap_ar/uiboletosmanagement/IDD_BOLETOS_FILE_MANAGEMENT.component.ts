import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLETOS_FILE_MANAGEMENTService } from './IDD_BOLETOS_FILE_MANAGEMENT.service';

@Component({
    selector: 'tb-IDD_BOLETOS_FILE_MANAGEMENT',
    templateUrl: './IDD_BOLETOS_FILE_MANAGEMENT.component.html',
    providers: [IDD_BOLETOS_FILE_MANAGEMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOLETOS_FILE_MANAGEMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOLETOS_FILE_MANAGEMENTService,
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
        
        		this.bo.appendToModelStructure({'global':['BoletosFileImportFileName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLETOS_FILE_MANAGEMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLETOS_FILE_MANAGEMENTComponent, resolver);
    }
} 