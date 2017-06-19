
import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'tb-tilepanel',
    template: "<div class=\"tile-panel\"> <md-card-title (click)=\"toggleCollapse($event)\" *ngIf=\"showAsTile\" [ngClass]=\"{'c-pointer': isCollapsible }\"> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </md-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </div>",
    styles: [""]
})

export class TilePanelComponent implements OnInit {

    private _showAsTile: boolean = true;
    private _isCollapsed: boolean = false;
    private _isCollapsible: boolean = true;


    tilePanel: TilePanelComponent;
    constructor() {
    }

    ngOnInit() {
    }


    @Input()
    set showAsTile(value: boolean) {
        this._showAsTile = value;
    }

    get showAsTile(): boolean {
        return this._showAsTile;
    }

    toggleCollapse(event: MouseEvent): void {
        if (!this._isCollapsible)
            return;

        // event.preventDefault();
        this._isCollapsed = !this._isCollapsed;
    }

    getArrowIcon() {
        return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
    }


}
