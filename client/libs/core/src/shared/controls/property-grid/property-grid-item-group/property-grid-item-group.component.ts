import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { ControlComponent } from './../../control.component';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../core/services/layout.service';

@Component({
    selector: 'tb-property-grid-item-group',
    templateUrl: './property-grid-item-group.component.html',
    styleUrls: ['./property-grid-item-group.component.scss']
})
export class PropertyGridItemGroupComponent extends ControlComponent {
    @Input() text: string;
    @Input() hint: string;
    private _isCollapsed: boolean = false;
    private _isCollapsible: boolean = true;

    get isCollapsed() {
        return this._isCollapsed;
    }

    toggleCollapse($event) {
        this._isCollapsed = !this._isCollapsed;
    }

    getIcon() {
        return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
    }

    @Input()
    set isCollapsible(value: boolean) {
        this._isCollapsible = value;
    }

    get isCollapsible(): boolean {
        return this._isCollapsible;
    }

    constructor(
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef)
    }
}
