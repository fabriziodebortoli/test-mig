import { AfterContentInit, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
export declare class StateButtonDirective implements AfterContentInit {
    private vcr;
    private componentResolver;
    private stateButtonsRef;
    constructor(vcr: ViewContainerRef, componentResolver: ComponentFactoryResolver);
    renderComponent(): void;
    ngAfterContentInit(): void;
    ngOnChanges(changes: Object): void;
}
