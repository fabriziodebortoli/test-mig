import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-property-grid-item',
    templateUrl: './property-grid-item.component.html',
    styleUrls: ['./property-grid-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PropertyGridItemComponent extends ControlComponent {
    @Input() itemSource: any;
    @Input() public hotLink: { namespace: string, name: string };
}
