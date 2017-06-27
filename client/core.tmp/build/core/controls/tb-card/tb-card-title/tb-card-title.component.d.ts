import { OnInit } from '@angular/core';
export declare class TbCardTitleComponent implements OnInit {
    private _isCollapsible;
    private _isCollapsed;
    title: string;
    isCollapsible: boolean;
    isCollapsed: boolean;
    constructor();
    ngOnInit(): void;
    getArrowIcon(): "keyboard_arrow_down" | "keyboard_arrow_up";
}
