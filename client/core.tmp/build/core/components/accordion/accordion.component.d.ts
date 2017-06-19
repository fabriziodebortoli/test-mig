import { OnDestroy } from '@angular/core';
export declare class AccordionComponent {
    groups: Array<AccordionGroupComponent>;
    addGroup(group: AccordionGroupComponent): void;
    closeOthers(openGroup: AccordionGroupComponent): void;
    removeGroup(group: AccordionGroupComponent): void;
}
export declare class AccordionGroupComponent implements OnDestroy {
    private accordion;
    private _isOpen;
    heading: string;
    isOpen: boolean;
    constructor(accordion: AccordionComponent);
    ngOnDestroy(): void;
    toggleOpen(event: MouseEvent): void;
}
