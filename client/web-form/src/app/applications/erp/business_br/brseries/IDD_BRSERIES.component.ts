import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BRSERIESService } from './IDD_BRSERIES.service';

@Component({
    selector: 'tb-IDD_BRSERIES',
    templateUrl: './IDD_BRSERIES.component.html',
    providers: [IDD_BRSERIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BRSERIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BRSERIESService,
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
        
        		this.bo.appendToModelStructure({'DBTBRSeries':['Series','Disabled','Description','Model'],'global':['DBTBRSeriesUnusedNumbersDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTBRSeriesUnusedNumbersDetail':['FromNumber','ToNumber','OperationDate','ElabDate','AuthProtocol','AnswerStatus','AnswerStatusDescri','InutReason','MagoUserID']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BRSERIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BRSERIESComponent, resolver);
    }
} 