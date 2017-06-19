import { Directive, ElementRef, Renderer } from '@angular/core';
export class LayoutTypeColumnDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-column', true);
    }
}
LayoutTypeColumnDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbLayoutTypeColumn]' },] },
];
/**
 * @nocollapse
 */
LayoutTypeColumnDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function LayoutTypeColumnDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    LayoutTypeColumnDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LayoutTypeColumnDirective.ctorParameters;
}
export class LayoutTypeHboxDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-hbox', true);
    }
}
LayoutTypeHboxDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbLayoutTypeHbox]' },] },
];
/**
 * @nocollapse
 */
LayoutTypeHboxDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function LayoutTypeHboxDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    LayoutTypeHboxDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LayoutTypeHboxDirective.ctorParameters;
}
export class LayoutTypeVboxDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-vbox', true);
    }
}
LayoutTypeVboxDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbLayoutTypeVbox]' },] },
];
/**
 * @nocollapse
 */
LayoutTypeVboxDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function LayoutTypeVboxDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    LayoutTypeVboxDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LayoutTypeVboxDirective.ctorParameters;
}
