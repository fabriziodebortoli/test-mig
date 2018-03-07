import { WebSocketService } from './../../../core/services/websocket.service';
import {
    Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, ContentChild,
    OnChanges, AfterContentInit, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef,
    ContentChildren, QueryList, Optional, Injector
} from '@angular/core';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Store } from './../../../core/services/store.service';
import { ControlComponent } from '../control.component';
import { ContextMenuItem } from './../../models/context-menu-item.model';
import { createSelector } from './../../../shared/commons/selector';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BehaviorSubject, Observable } from './../../../rxjs.imports';
import { Logger } from './../../../core/services/logger.service';
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
    constructor(public layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private eventDataService: EventDataService,
        private store: Store,
        private injector: Injector,
        private logger: Logger) {
        super(layoutService, tbComponentService, changeDetectorRef);
        this.stateButtonEnabled$ = this.store.select(this.stateData && this.stateData.model || '');
    }

    stateButtonClick(e: any) {
        _.set(this.eventDataService.model,
            this.stateData.model,
            !_.get(this.eventDataService.model, this.stateData.model));
        this.eventDataService.raiseControlCommand(this.stateData.cmpId);
    }

    ngOnInit() {
        if (this.stateData && !this.stateData.cmpId) {
            this.retrieveParentComponentId();
        }
    }

    private retrieveParentComponentId() {
        try {
            let _injector = this.injector as any;
            this.stateData.cmpId = _injector.view.component.cmpId;
            this.logger.debug('L\' Id della componente padre è ' + _injector.view.component.cmpId);
        } catch (e) {
            this.logger.error('Errore durante la ricerca dell\' Id della componente:' + e);
        }
    }


}


