import { WebSocketService } from './../../../core/services/websocket.service';
import {
    Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, ContentChild,
    OnChanges, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef,
    ContentChildren, Optional, Injector
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
    @Input() disableStateButton = false;
    @Input() contextMenuLocked = false;
    @Output() stateInfo: EventEmitter<{ invertState: boolean, model: string }> = new EventEmitter();

    private stateButtonEnabled$: Observable<boolean>;
    private get editIcon(): string {
        return this.stateData.iconEdit ? this.stateData.iconEdit : 'tb-edit';
    };
    private get executeIcon(): string {
        return this.stateData.iconExecute ? this.stateData.iconExecute : 'tb-execute';
    };
    constructor(public layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private eventData: EventDataService,
        private store: Store,
        private injector: Injector,
        private logger: Logger) {
        super(layoutService, tbComponentService, changeDetectorRef);
        this.stateButtonEnabled$ = this.store
            .select(s => _.get(s, this.stateData && this.stateData.model + '.value' || ''))
            .map(s => this.stateData.invertState ? !s : s);
    }

    valuePath = () => this.stateData && this.stateData.model + '.value';

    stateButtonClick(e: any) {
        _.set(this.eventData.model, this.valuePath(),
            !_.get(this.eventData.model, this.valuePath()));
        this.eventData.change.emit('');
        this.eventData.raiseControlCommand(this.stateData.cmpId);
    }

    ngOnInit() {
        if (this.stateData) {
            if (!this.stateData.cmpId)
                this.retrieveParentComponentId();
            this.stateInfo.emit({ invertState: this.stateData.invertState, model: this.stateData.model });
        }
    }

    private retrieveParentComponentId() {
        try {
            let _injector = this.injector as any;
            this.stateData.cmpId = _injector.view.component.cmpId;
        } catch (e) {
            this.logger.error('Errore durante la ricerca dell\' Id della componente:' + e);
        }
    }

    ngOnDestroy() {
        this.stateInfo.complete();
    }
}




