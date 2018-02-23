import { WebSocketService } from './../../../core/services/websocket.service';
import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, ContentChild,
         OnChanges, AfterContentInit, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef, 
         ContentChildren, QueryList } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Store } from './../../../core/services/store.service';

import { ControlComponent } from '../control.component';

import { ContextMenuItem } from './../../models/context-menu-item.model';
import { createSelector } from './../../../shared/commons/selector';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BehaviorSubject, Observable } from './../../../rxjs.imports';
import * as _ from 'lodash';

@Component({
    selector: 'tb-control-container',
    templateUrl: './control-container.component.html',
    styleUrls: ['./control-container.component.scss']
})

export class ControlContainerComponent extends ControlComponent {
    @Input() type = '';
    @Input() errorMessage = '';
    private stateButtonEnabled$: Observable<boolean>;
    private currentComponentId: '';
    constructor(public layoutService: LayoutService,
                tbComponentService: TbComponentService,
                changeDetectorRef: ChangeDetectorRef,
                private eventDataService: EventDataService,
                private store: Store) {
        super(layoutService, tbComponentService, changeDetectorRef);
        this.stateButtonEnabled$ = this.store.select(this.stateButtonState && this.stateButtonState.model || '');
    }

    stateButtonClick(e: any) {
        _.set(this.eventDataService.model,
              this.stateButtonState.model,
              !_.get(this.eventDataService.model, this.stateButtonState.model));
        this.eventDataService.change.emit(this.stateButtonState.cmpId);
    }
}


