import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_QUESTIONService } from './IDD_LOAD_QUESTION.service';

@Component({
    selector: 'tb-IDD_LOAD_QUESTION',
    templateUrl: './IDD_LOAD_QUESTION.component.html',
    providers: [IDD_LOAD_QUESTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOAD_QUESTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_QUESTIONService,
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
        
        		this.bo.appendToModelStructure({'global':['AnswersToCopy','IncompatibilityToCopy','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'AnswersToCopy':['l_bSelected','AnswerNo','Answer','Notes'],'IncompatibilityToCopy':['l_bSelected','AnswerNo','IncompatQuestionNo','IncompatAnswerNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_QUESTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_QUESTIONComponent, resolver);
    }
} 