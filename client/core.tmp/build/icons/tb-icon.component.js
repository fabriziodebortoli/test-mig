import { Component, Input } from '@angular/core';
export class TbIconComponent {
    constructor() {
        this.iconType = 'IMG'; // TB, CLASS, IMG  
        this.icon = '';
    }
}
// TODO import core http services
// constructor(private httpService: HttpService) {
//     this.imgUrl = this.httpService.getDocumentBaseUrl() + 'getImage/?src=';
// }
TbIconComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-icon',
                template: "<div [ngSwitch]=\"iconType\" class=\"div-icon\"> <img *ngSwitchCase=\"'IMG'\" src=\"{{imgUrl}}{{icon}}\" /> <m4-icon *ngSwitchCase=\"'M4'\" icon=\"{{icon}}\"></m4-icon> <i *ngSwitchCase=\"'CLASS'\" class=\"{{icon}}\">asdgf</i> <h5 *ngSwitchDefault>no-icon</h5> </div>"
            },] },
];
/**
 * @nocollapse
 */
TbIconComponent.ctorParameters = () => [];
TbIconComponent.propDecorators = {
    'iconType': [{ type: Input },],
    'icon': [{ type: Input },],
};
function TbIconComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TbIconComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TbIconComponent.ctorParameters;
    /** @type {?} */
    TbIconComponent.propDecorators;
    /** @type {?} */
    TbIconComponent.prototype.iconType;
    /** @type {?} */
    TbIconComponent.prototype.icon;
    /** @type {?} */
    TbIconComponent.prototype.imgUrl;
}
