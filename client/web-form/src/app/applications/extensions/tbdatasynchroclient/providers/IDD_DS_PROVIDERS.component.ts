import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DS_PROVIDERSService } from './IDD_DS_PROVIDERS.service';

@Component({
    selector: 'tb-IDD_DS_PROVIDERS',
    templateUrl: './IDD_DS_PROVIDERS.component.html',
    providers: [IDD_DS_PROVIDERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DS_PROVIDERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DS_PROVIDERSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'Providers':['Name','Description','Disabled','SkipCrtValidation','ProviderUrl','ProviderUser','ProviderPassword'],'global':['ProvidersParameters','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ProvidersParameters':['l_VName','l_VDescription','l_VValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DS_PROVIDERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DS_PROVIDERSComponent, resolver);
    }
} 