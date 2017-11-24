import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INTENTJOURNALService } from './IDD_INTENTJOURNAL.service';

@Component({
    selector: 'tb-IDD_INTENTJOURNAL',
    templateUrl: './IDD_INTENTJOURNAL.component.html',
    providers: [IDD_INTENTJOURNALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INTENTJOURNALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INTENTJOURNALService,
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
        
        		this.bo.appendToModelStructure({'global':['Year','FromDate','ToDate','Issued','Received','PrintDate','DefinitivelyPrinted','ContextualHeading','NoPrefix','StartingPage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INTENTJOURNALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INTENTJOURNALComponent, resolver);
    }
} 