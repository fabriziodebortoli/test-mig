import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CTG_PRODUCT_HEADERService } from './IDD_CTG_PRODUCT_HEADER.service';

@Component({
    selector: 'tb-IDD_CTG_PRODUCT_HEADER',
    templateUrl: './IDD_CTG_PRODUCT_HEADER.component.html',
    providers: [IDD_CTG_PRODUCT_HEADERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CTG_PRODUCT_HEADERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CTG_PRODUCT_HEADERService,
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
        
        		this.bo.appendToModelStructure({'ProductCtg':['Category','CodeType','Description','Notes'],'global':['SubCtg','__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SubCtg':['SubCategory','Description'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CTG_PRODUCT_HEADERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CTG_PRODUCT_HEADERComponent, resolver);
    }
} 