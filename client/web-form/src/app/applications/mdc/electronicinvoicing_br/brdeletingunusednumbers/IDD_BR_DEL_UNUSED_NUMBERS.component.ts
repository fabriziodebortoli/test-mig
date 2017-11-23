import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_DEL_UNUSED_NUMBERSService } from './IDD_BR_DEL_UNUSED_NUMBERS.service';

@Component({
    selector: 'tb-IDD_BR_DEL_UNUSED_NUMBERS',
    templateUrl: './IDD_BR_DEL_UNUSED_NUMBERS.component.html',
    providers: [IDD_BR_DEL_UNUSED_NUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_DEL_UNUSED_NUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_DEL_UNUSED_NUMBERSService,
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
		boService.appendToModelStructure({'global':['Model','Series','bAllAuthAndNot','bAuthorized','bNotAuthorized','cStat','bAllDate','bDateSel','StartingDate','EndingDate','Detail'],'Detail':['TEnhDelUnus_bSelected','Series','Model','FromNumber','ToNumber','OperationDate','ElabDate','InutReason','AuthProtocol','AnswerStatus','AnswerStatusDescri','AnswerStatusDescri']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_DEL_UNUSED_NUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_DEL_UNUSED_NUMBERSComponent, resolver);
    }
} 