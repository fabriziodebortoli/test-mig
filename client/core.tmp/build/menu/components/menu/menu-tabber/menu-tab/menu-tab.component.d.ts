import { OnDestroy } from '@angular/core';
import { MenuTabberComponent } from '../menu-tabber.component';
export declare class MenuTabComponent implements OnDestroy {
    private tabs;
    active: boolean;
    title: string;
    constructor(tabs: MenuTabberComponent);
    ngOnDestroy(): void;
}
