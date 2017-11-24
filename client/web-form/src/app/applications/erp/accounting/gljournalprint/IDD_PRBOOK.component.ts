import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRBOOKService } from './IDD_PRBOOK.service';

@Component({
    selector: 'tb-IDD_PRBOOK',
    templateUrl: './IDD_PRBOOK.component.html',
    providers: [IDD_PRBOOKService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRBOOKComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRBOOKService,
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
        
        		this.bo.appendToModelStructure({'global':['FromMonth','FromYear','ToMonth','ToYear','FromPostDate','ToPostDate','DefinitivelyPrinted','bPrepareForSOS','PrintAccRsn','DiffRef','DotMatrixPrinter','PageTotals','DotMatrixPrinter80Col','GeneralTotals','PrintCustSupp','PreviousPosting','bByPostDate','bByAccrDate','ContextualHeading','NoPrefix','Page']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRBOOKFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRBOOKComponent, resolver);
    }
} 