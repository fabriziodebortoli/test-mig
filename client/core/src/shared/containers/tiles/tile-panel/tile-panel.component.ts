
import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'tb-tilepanel',
    templateUrl: './tile-panel.component.html',
    styleUrls: ['./tile-panel.component.scss']
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
        return this._isCollapsed ? 'tb-expandarrowfilled' : 'tb-collapsearrowfilled';
    }


}
