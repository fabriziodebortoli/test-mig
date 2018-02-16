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
    @Input() icon: string = 'erp-routing-c';

    @Output() clicked: EventEmitter<any> = new EventEmitter();

    emitClickEvent($event: Event): void {
        if (this.disabled)
            return;

        this.clicked.emit($event);
    }
}