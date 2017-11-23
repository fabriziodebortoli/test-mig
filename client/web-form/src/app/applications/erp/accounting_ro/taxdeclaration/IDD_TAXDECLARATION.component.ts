import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDECLARATIONService } from './IDD_TAXDECLARATION.service';

@Component({
    selector: 'tb-IDD_TAXDECLARATION',
    templateUrl: './IDD_TAXDECLARATION.component.html',
    providers: [IDD_TAXDECLARATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXDECLARATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDECLARATIONService,
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
        
        		this.bo.appendToModelStructure({'global':['DeclType','Year','Period','PrintFile','PrintPaper','Name','Surname','Function','ProcessStatus']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDECLARATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDECLARATIONComponent, resolver);
    }
} 