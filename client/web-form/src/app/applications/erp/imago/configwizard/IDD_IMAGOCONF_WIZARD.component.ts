import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMAGOCONF_WIZARDService } from './IDD_IMAGOCONF_WIZARD.service';

@Component({
    selector: 'tb-IDD_IMAGOCONF_WIZARD',
    templateUrl: './IDD_IMAGOCONF_WIZARD.component.html',
    providers: [IDD_IMAGOCONF_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMAGOCONF_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMAGOCONF_WIZARDService,
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
		boService.appendToModelStructure({'global':['InfinityURL','InfinityUserName','InfinityPassword','SkipCertificateValidation','InfinityCheckBtn','AppUsername','AppPassword','InfinityCompanies','IAFModules']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMAGOCONF_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMAGOCONF_WIZARDComponent, resolver);
    }
} 