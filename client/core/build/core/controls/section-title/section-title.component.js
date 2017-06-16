import { Component } from '@angular/core';
import { ControlComponent } from './../control.component';
export class SectionTitleComponent extends ControlComponent {
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
SectionTitleComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-section-title',
                template: "<div class=\"separator\"> <hr class=\"line-separator\"> <label id=\"{{cmpId}}\" class=\"class1\">{{caption}}</label> </div>",
                styles: [".class1 { color: #3399ff; } .line-separator { height: 1px; background: #3399ff; border-bottom: 1px solid #3399ff; } .separator { padding-top: 10px; padding-bottom: 10px; } "]
            },] },
];
/**
 * @nocollapse
 */
SectionTitleComponent.ctorParameters = () => [];
function SectionTitleComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    SectionTitleComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    SectionTitleComponent.ctorParameters;
}
