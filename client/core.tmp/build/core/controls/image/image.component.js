import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { HttpService } from './../../services/http.service';
export class ImageComponent extends ControlComponent {
    /**
     * @param {?} httpService
     */
    constructor(httpService) {
        super();
        this.httpService = httpService;
        this.title = '';
    }
    /**
     * @return {?}
     */
    getStyles() {
        let /** @type {?} */ imgStyles = {};
        if (+(this.width) > +(this.height)) {
            imgStyles['width'] = this.width + 'px';
        }
        else {
            imgStyles['height'] = this.height + 'px';
        }
        return imgStyles;
    }
    /**
     * @param {?} namespace
     * @return {?}
     */
    getImageUrl(namespace) {
        return this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
    }
}
ImageComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-image',
                template: "<div [title]=\"title\"> <img id=\"{{cmpId}}\" [src]=\"getImageUrl(model?.value)\" *ngIf=\"model\" class=\"tb-static-component\" [ngStyle]=\"getStyles()\" /> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
ImageComponent.ctorParameters = () => [
    { type: HttpService, },
];
ImageComponent.propDecorators = {
    'width': [{ type: Input },],
    'height': [{ type: Input },],
    'title': [{ type: Input },],
};
function ImageComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ImageComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ImageComponent.ctorParameters;
    /** @type {?} */
    ImageComponent.propDecorators;
    /** @type {?} */
    ImageComponent.prototype.width;
    /** @type {?} */
    ImageComponent.prototype.height;
    /** @type {?} */
    ImageComponent.prototype.title;
    /** @type {?} */
    ImageComponent.prototype.httpService;
}
