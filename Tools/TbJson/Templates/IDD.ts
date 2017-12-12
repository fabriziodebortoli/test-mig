import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { @@NAME@@Service } from '@@SERVICEFILE@@';

@Component({
    selector: 'tb-@@NAME@@',
    templateUrl: './@@NAME@@.component.html',
    providers: [@@NAME@@Service, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class @@NAME@@Component extends BOComponent implements OnInit, OnDestroy {
     /*dichiarazione variabili*/
    constructor(document: @@NAME@@Service,
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
        /*definizione variabili*/
        @@INITCODE@@
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class @@NAME@@FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(@@NAME@@Component, resolver);
    }
} 