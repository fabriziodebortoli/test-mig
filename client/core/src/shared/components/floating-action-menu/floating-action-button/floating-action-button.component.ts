import { ComponentInfoService } from './../../../../core/services/component-info.service';
import { EventDataService } from './../../../../core/services/eventdata.service';
import { Component, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'tb-floating-action-button',
    styleUrls: ['./floating-action-button.component.scss'],
    templateUrl: './floating-action-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FloatingActionButtonComponent {

    @ViewChild('elementref') elementref;
    @ViewChild('contentref') contentref;

    @Input() caption: string = '--unknown--';
    @Input() cmpId: string = '';
    @Input() disabled: boolean = false;
    @Input() iconType: string = 'M4';
    @Input() _icon: string = 'erp-routing-c';

    @Input()
    set icon(icon: any) {
        this._icon = icon instanceof Object ? icon.value : icon;
    }

    get icon() {
        return this._icon;
    }


    @Output() clicked: EventEmitter<any> = new EventEmitter();

    constructor(
        public eventData: EventDataService,
        public ciService: ComponentInfoService

    ) { }

    emitClickEvent($event: Event): void {
        if (this.disabled)
            return;

        this.clicked.emit($event);

        //if (!this.click())
        //     return;
        this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
    }
}