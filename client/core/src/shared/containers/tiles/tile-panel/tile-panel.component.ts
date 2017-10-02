
import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'tb-tilepanel',
    templateUrl: './tile-panel.component.html',
    styleUrls: ['./tile-panel.component.scss']
})

export class TilePanelComponent implements OnInit {

    public _showAsTile: boolean = true;
    isCollapsed: boolean = false;
    isCollapsible: boolean = true;


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
        if (!this.isCollapsed)
            return;

        // event.preventDefault();
        this.isCollapsed = !this.isCollapsed;
    }

    getArrowIcon() {
        return this.isCollapsed ? 'tb-expandarrowfilled' : 'tb-collapsearrowfilled';
    }


}
