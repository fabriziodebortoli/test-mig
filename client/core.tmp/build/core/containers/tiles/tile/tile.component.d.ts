import { OnInit } from '@angular/core';
export declare class TileComponent implements OnInit {
    title: string;
    private _isCollapsed;
    private _isCollapsible;
    private _hasTitle;
    constructor();
    ngOnInit(): void;
    getArrowIcon(): "keyboard_arrow_down" | "keyboard_arrow_up";
    toggleCollapse(event: MouseEvent): void;
    isCollapsed: boolean;
    isCollapsible: boolean;
    hasTitle: boolean;
}
