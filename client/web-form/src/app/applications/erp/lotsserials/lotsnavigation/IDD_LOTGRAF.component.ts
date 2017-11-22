import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOTGRAFService } from './IDD_LOTGRAF.service';

@Component({
    selector: 'tb-IDD_LOTGRAF',
    templateUrl: './IDD_LOTGRAF.component.html',
    providers: [IDD_LOTGRAFService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOTGRAFComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOTGRAFService,
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
		boService.appendToModelStructure({'global':['Item','Lot','CriticalItems','DBTNodeDetail'],'DBTNodeDetail':['l_FieldName','l_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOTGRAFFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOTGRAFComponent, resolver);
    }
} 