import { Component, ViewContainerRef, Input, ViewChild } from '@angular/core';
import { ComponentService } from '../services/component.service';
import { MessageDialogComponent } from '../containers/message-dialog/message-dialog.component';
export class DynamicCmpComponent {
    /**
     * @param {?} componentService
     */
    constructor(componentService) {
        this.componentService = componentService;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.createComponent();
    }
    /**
     * @return {?}
     */
    createComponent() {
        if (this.componentInfo) {
            this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
            this.cmpRef.instance.cmpId = this.componentInfo.id; //assegno l'id al componente
            this.cmpRef.instance.document.init(this.componentInfo.id); //assegno l'id al servizio (uguale a quello del componente)
            this.cmpRef.instance.args = this.componentInfo.args;
            this.messageDialogOpenSubscription = this.cmpRef.instance.document.eventData.openMessageDialog.subscribe(args => this.openMessageDialog(this.cmpRef.instance.cmpId, args));
            //se la eseguo subito, lancia un'eccezione quando esegue l'aggiornamento dei binding, come se fosse in un momento sbagliato
            setTimeout(() => {
                this.componentInfo.document = this.cmpRef.instance.document;
                this.componentService.onComponentCreated(this.componentInfo);
            }, 1);
        }
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        if (this.cmpRef) {
            this.cmpRef.destroy();
        }
        if (this.messageDialogOpenSubscription) {
            this.messageDialogOpenSubscription.unsubscribe();
        }
    }
    /**
     * @param {?} mainCmpId
     * @param {?} args
     * @return {?}
     */
    openMessageDialog(mainCmpId, args) {
        this.messageDialog.open(args, this.cmpRef.instance.document.eventData);
    }
}
DynamicCmpComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-dynamic-cmp',
                template: '<div #cmpContainer></div><tb-message-dialog></tb-message-dialog>'
            },] },
];
/**
 * @nocollapse
 */
DynamicCmpComponent.ctorParameters = () => [
    { type: ComponentService, },
];
DynamicCmpComponent.propDecorators = {
    'componentInfo': [{ type: Input },],
    'cmpContainer': [{ type: ViewChild, args: ['cmpContainer', { read: ViewContainerRef },] },],
    'messageDialog': [{ type: ViewChild, args: [MessageDialogComponent,] },],
};
function DynamicCmpComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    DynamicCmpComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DynamicCmpComponent.ctorParameters;
    /** @type {?} */
    DynamicCmpComponent.propDecorators;
    /** @type {?} */
    DynamicCmpComponent.prototype.cmpRef;
    /** @type {?} */
    DynamicCmpComponent.prototype.componentInfo;
    /** @type {?} */
    DynamicCmpComponent.prototype.cmpContainer;
    /** @type {?} */
    DynamicCmpComponent.prototype.messageDialog;
    /** @type {?} */
    DynamicCmpComponent.prototype.messageDialogOpenSubscription;
    /** @type {?} */
    DynamicCmpComponent.prototype.componentService;
}
