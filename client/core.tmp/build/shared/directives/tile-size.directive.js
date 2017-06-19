import { Directive, ElementRef, Renderer } from '@angular/core';
export class TileMicroDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-micro', true);
    }
}
TileMicroDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbTileMicro]' },] },
];
/**
 * @nocollapse
 */
TileMicroDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function TileMicroDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    TileMicroDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileMicroDirective.ctorParameters;
}
export class TileMiniDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-mini', true);
    }
}
TileMiniDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbTileMini]' },] },
];
/**
 * @nocollapse
 */
TileMiniDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function TileMiniDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    TileMiniDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileMiniDirective.ctorParameters;
}
export class TileStandardDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-standard', true);
    }
}
TileStandardDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbTileStandard]' },] },
];
/**
 * @nocollapse
 */
TileStandardDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function TileStandardDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    TileStandardDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileStandardDirective.ctorParameters;
}
export class TileWideDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-wide', true);
    }
}
TileWideDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbTileWide]' },] },
];
/**
 * @nocollapse
 */
TileWideDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function TileWideDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    TileWideDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileWideDirective.ctorParameters;
}
export class TileAutofillDirective {
    /**
     * @param {?} el
     * @param {?} renderer
     */
    constructor(el, renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-autofill', true);
    }
}
TileAutofillDirective.decorators = [
    { type: Directive, args: [{ selector: '[tbTileAutofill]' },] },
];
/**
 * @nocollapse
 */
TileAutofillDirective.ctorParameters = () => [
    { type: ElementRef, },
    { type: Renderer, },
];
function TileAutofillDirective_tsickle_Closure_declarations() {
    /** @type {?} */
    TileAutofillDirective.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileAutofillDirective.ctorParameters;
}
