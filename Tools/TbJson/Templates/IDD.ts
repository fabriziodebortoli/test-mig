import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService, DocumentService, TbComponentService, CommandCategory } from '@taskbuilder/core';
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
    toolbarButtons = [];

    constructor(document: @@NAME@@Service,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);

        this.eventData.change.subscribe(() => changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        /*definizione variabili*/
        @@INITCODE@@

        this.eventData.showRadar.subscribe(hide =>
            this.toolbarButtons
                .filter(x => x.category === CommandCategory.Search || x.category === CommandCategory.Navigation)
                .forEach(x => this['hide' + x.id] = hide)
        );
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }

    openRadar = () => this.eventData.showRadar.next(true);
}

@Component({
    template: ''
})
export class @@NAME@@FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(@@NAME@@Component, resolver);
    }
} 