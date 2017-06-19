import { Directive, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
export class StateButtonDirective {
    /**
     * @param {?} vcr
     * @param {?} componentResolver
     */
    constructor(vcr, componentResolver) {
        this.vcr = vcr;
        this.componentResolver = componentResolver;
        console.log(vcr);
    }
    /**
     * @return {?}
     */
    renderComponent() {
        // if (this.stateButtonsRef) this.stateButtonsRef.instance.value = this.value;
    }
    /**
     * @return {?}
     */
    ngAfterContentInit() {
        // this.stateButtonsTarget = (<any>this.vcr)._data.componentView.component.stateButtons;
        // console.log('_data.componentView', this.stateButtonsTarget);
        // let componentFactory = this.componentResolver.resolveComponentFactory(StateButtonComponent);
        // this.stateButtonsRef = this.stateButtonsTarget.createComponent(componentFactory);
        // this.renderComponent();
    }
    /**
     * @param {?} changes
     * @return {?}
     */
    ngOnChanges(changes) {
        this.renderComponent();
    }
}
StateButtonDirective.decorators = [
    { type: Directive, args: [{
                selector: '[tbStateButtons]'
            },] },
];
/**
 * @nocollapse
 */
StateButtonDirective.ctorParameters = () => [
    { type: ViewContainerRef, },
    { type: ComponentFactoryResolver, },
];
function StateButtonDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    StateButtonDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    StateButtonDirective.ctorParameters;
    /** @type {?} */
    StateButtonDirective.prototype.stateButtonsRef;
    /** @type {?} */
    StateButtonDirective.prototype.vcr;
    /** @type {?} */
    StateButtonDirective.prototype.componentResolver;
}
