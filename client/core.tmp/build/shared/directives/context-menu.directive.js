import { Directive, ViewChild, ViewContainerRef, ComponentFactoryResolver, Input } from '@angular/core';
import { ContextMenuComponent } from '../../core/components/context-menu/context-menu.component';
export class ContextMenuDirective {
    /**
     * @param {?} vcr
     * @param {?} componentResolver
     */
    constructor(vcr, componentResolver) {
        this.vcr = vcr;
        this.componentResolver = componentResolver;
    }
    /**
     * @return {?}
     */
    renderComponent() {
        // if (this.contextMenuRef) this.contextMenuRef.instance.value = this.value;
    }
    /**
     * @return {?}
     */
    ngAfterContentInit() {
        this.cm = ((this.vcr))._data.componentView.component.contextMenu;
        let /** @type {?} */ componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
        this.contextMenuRef = this.cm.createComponent(componentFactory);
        this.contextMenuRef.instance.contextMenuBinding = this.tbContextMenu;
        this.renderComponent();
    }
    /**
     * @param {?} changes
     * @return {?}
     */
    ngOnChanges(changes) {
        this.renderComponent();
    }
}
ContextMenuDirective.decorators = [
    { type: Directive, args: [{
                selector: '[tbContextMenu]',
            },] },
];
/**
 * @nocollapse
 */
ContextMenuDirective.ctorParameters = () => [
    { type: ViewContainerRef, },
    { type: ComponentFactoryResolver, },
];
ContextMenuDirective.propDecorators = {
    'tbContextMenu': [{ type: Input },],
    'contextMenu': [{ type: ViewChild, args: ['contextMenu', { read: ViewContainerRef },] },],
};
function ContextMenuDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    ContextMenuDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ContextMenuDirective.ctorParameters;
    /** @type {?} */
    ContextMenuDirective.propDecorators;
    /** @type {?} */
    ContextMenuDirective.prototype.tbContextMenu;
    /** @type {?} */
    ContextMenuDirective.prototype.contextMenu;
    /** @type {?} */
    ContextMenuDirective.prototype.contextMenuRef;
    /** @type {?} */
    ContextMenuDirective.prototype.cm;
    /** @type {?} */
    ContextMenuDirective.prototype.vcr;
    /** @type {?} */
    ContextMenuDirective.prototype.componentResolver;
}
