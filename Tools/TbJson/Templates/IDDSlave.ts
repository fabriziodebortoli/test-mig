import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { ComponentService, EventDataService, BOSlaveComponent, ControlComponent, ComponentInfoService, BOService  } from '@taskbuilder/core';

@Component({
    selector: 'tb-@@NAME@@',
    templateUrl: './@@NAME@@.component.html',
    providers: [ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class @@NAME@@Component extends BOSlaveComponent implements OnInit, OnDestroy {
     /*dichiarazione variabili*/
	constructor(eventData: EventDataService,
		ciService: ComponentInfoService,
		changeDetectorRef: ChangeDetectorRef) {
        super(eventData, ciService, changeDetectorRef);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        @@INITCODE@@
    }
    ngOnDestroy() {
        super.ngOnDestroy();
        /*definizione variabili*/
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