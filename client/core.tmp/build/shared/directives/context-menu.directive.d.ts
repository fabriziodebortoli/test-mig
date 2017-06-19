import { AfterContentInit, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
export declare class ContextMenuDirective implements AfterContentInit {
    private vcr;
    private componentResolver;
    tbContextMenu: any;
    contextMenu: ViewContainerRef;
    private contextMenuRef;
    private cm;
    constructor(vcr: ViewContainerRef, componentResolver: ComponentFactoryResolver);
    renderComponent(): void;
    ngAfterContentInit(): void;
    ngOnChanges(changes: Object): void;
}
