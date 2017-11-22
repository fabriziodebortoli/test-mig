import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TEMPLATEEXPORTService } from './IDD_TEMPLATEEXPORT.service';

@Component({
    selector: 'tb-IDD_TEMPLATEEXPORT',
    templateUrl: './IDD_TEMPLATEEXPORT.component.html',
    providers: [IDD_TEMPLATEEXPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TEMPLATEEXPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TEMPLATEEXPORTService,
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
		boService.appendToModelStructure({'global':['Template','FileName','nCurrentElement','GaugeDescription'],'HKLTemplate':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TEMPLATEEXPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TEMPLATEEXPORTComponent, resolver);
    }
} 