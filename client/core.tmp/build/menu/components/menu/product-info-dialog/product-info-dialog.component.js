import { Component } from '@angular/core';
import { MdDialogRef } from '@angular/material';
import { UtilsService } from './../../../../core/services/utils.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';
export class ProductInfoDialogComponent {
    /**
     * @param {?} dialogRef
     * @param {?} httpMenuService
     * @param {?} utilsService
     * @param {?} localizationService
     */
    constructor(dialogRef, httpMenuService, utilsService, localizationService) {
        this.dialogRef = dialogRef;
        this.httpMenuService = httpMenuService;
        this.utilsService = utilsService;
        this.localizationService = localizationService;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        let /** @type {?} */ sub = this.httpMenuService.getProductInfo().subscribe(result => {
            this.productInfos = result.ProductInfos;
            sub.unsubscribe();
        });
    }
}
ProductInfoDialogComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-product-info-dialog',
                template: " <table> <tr> <td class=\"loginStyleOff pointer backButtonPadding\" ></td> <td> </td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.InstallationVersion}}: </td> <td><b>{{productInfos?.installationVersion}} {{productInfos?.debugState}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.Provider}}: </td> <td><b>{{productInfos?.providerDescription}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.Edition}}: </td> <td><b>{{productInfos?.edition}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.InstallationName}}: </td> <td><b>{{productInfos?.installationName}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.ActivationState}}: </td> <td><b>{{productInfos?.activationState}}</b></td> </tr> <tr *ngFor=\"let current of utilsService.toArray(productInfos?.Applications)\"> <td style=\"text-align:right\"> {{current?.application}}: </td> <td> <b>{{current?.licensed}}</b> </td> </tr> </table> <button type=\"button\" (click)=\"dialogRef.close('yes')\">{{localizationService.localizedElements?.CloseLabel}}</button> ",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
ProductInfoDialogComponent.ctorParameters = () => [
    { type: MdDialogRef, },
    { type: HttpMenuService, },
    { type: UtilsService, },
    { type: LocalizationService, },
];
function ProductInfoDialogComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ProductInfoDialogComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ProductInfoDialogComponent.ctorParameters;
    /** @type {?} */
    ProductInfoDialogComponent.prototype.productInfos;
    /** @type {?} */
    ProductInfoDialogComponent.prototype.dialogRef;
    /** @type {?} */
    ProductInfoDialogComponent.prototype.httpMenuService;
    /** @type {?} */
    ProductInfoDialogComponent.prototype.utilsService;
    /** @type {?} */
    ProductInfoDialogComponent.prototype.localizationService;
}
