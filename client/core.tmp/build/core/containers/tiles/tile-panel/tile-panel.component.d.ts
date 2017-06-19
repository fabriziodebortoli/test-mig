import { OnInit } from '@angular/core';
export declare class TilePanelComponent implements OnInit {
    private _showAsTile;
    private _isCollapsed;
    private _isCollapsible;
    tilePanel: TilePanelComponent;
    constructor();
    ngOnInit(): void;
    showAsTile: boolean;
    toggleCollapse(event: MouseEvent): void;
    getArrowIcon(): "keyboard_arrow_down" | "keyboard_arrow_up";
}
