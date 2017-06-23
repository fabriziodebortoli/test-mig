import { EventEmitter } from '@angular/core';
import { TbComponent } from '../../../../core';
import { MenuTabComponent } from './menu-tab/menu-tab.component';
export declare class MenuTabberComponent extends TbComponent {
    tabs: MenuTabComponent[];
    selectedTab: EventEmitter<any>;
    selectTab(tab: MenuTabComponent): void;
    addTab(tab: MenuTabComponent): void;
    removeTab(tab: MenuTabComponent): void;
}
