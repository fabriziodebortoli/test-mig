import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_QUESTIONSService } from './IDD_QUESTIONS.service';

@Component({
    selector: 'tb-IDD_QUESTIONS',
    templateUrl: './IDD_QUESTIONS.component.html',
    providers: [IDD_QUESTIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_QUESTIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_QUESTIONSService,
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
        
        		this.bo.appendToModelStructure({'Questions':['QuestionNo','Question','CreationDate','Notes','Deletable','DeletingText'],'global':['Answers','Incompatibility','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Answers':['AnswerNo','Answer','Notes'],'Incompatibility':['AnswerNo','IncompatQuestionNo','IncompatAnswerNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_QUESTIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_QUESTIONSComponent, resolver);
    }
} 