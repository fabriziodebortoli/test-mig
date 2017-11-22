import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TEMPLATEIMPORTService } from './IDD_TEMPLATEIMPORT.service';

@Component({
    selector: 'tb-IDD_TEMPLATEIMPORT',
    templateUrl: './IDD_TEMPLATEIMPORT.component.html',
    providers: [IDD_TEMPLATEIMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TEMPLATEIMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TEMPLATEIMPORTService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FileName','Template','nCurrentElement','GaugeDescription'],'HKLTemplate':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TEMPLATEIMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TEMPLATEIMPORTComponent, resolver);
    }
} 