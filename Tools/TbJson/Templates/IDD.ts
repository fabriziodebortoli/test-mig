import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService, DocumentService, TbComponentService, CommandCategory, ContextMenuItem } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { @@NAME@@Service } from '@@SERVICEFILE@@';

@Component({
    selector: 'tb-@@NAME@@',
    templateUrl: './@@NAME@@.component.html',
    providers: [
      @@NAME@@Service,
      { provide: BOService, useExisting: @@NAME@@Service },
      { provide: DocumentService, useExisting: @@NAME@@Service },
      { provide: TbComponentService, useExisting: @@NAME@@Service },
      ComponentInfoService
    ],
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

        this.eventData.change.subscribe(_ => changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        /*definizione variabili*/
        @@INITCODE@@
        this.hideToolbarsWhenRadarVisible();
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }

    openRadar = () => { this.eventData.showRadar.next(true); return true; }

    hideToolbarsWhenRadarVisible = () =>
        this.eventData.showRadar.subscribe(hide =>
            ['Search', 'Navigation']
                .map(x => `hide${x}Toolbar`)
                .forEach(x => this[x] = hide));
}

@Component({
    template: ''
})
export class @@NAME@@FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(@@NAME@@Component, resolver);
    }
} 