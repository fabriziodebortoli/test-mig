import { Component, ViewChild, Input, ViewEncapsulation } from '@angular/core';
import { FormControl } from '@angular/forms';
import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
export class SearchComponent {
    /**
     * @param {?} menuService
     * @param {?} localizationService
     */
    constructor(menuService, localizationService) {
        this.menuService = menuService;
        this.localizationService = localizationService;
        this.selected = '';
        this.maxElements = 20;
        this.inputControl = new FormControl();
        this.filteredElements = this.inputControl.valueChanges
            .startWith(null)
            .map(name => this.filteredElements(name));
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.filteredElements = this.inputControl.valueChanges
            .startWith(null)
            .map(val => val ? this.filter(val) : this.menuService.searchSources.slice(0, (val && val.length > 0) ? this.maxElements : 0));
        this.valueChangesSubscription = this.inputControl.valueChanges.subscribe(data => {
            if (this.isObject(data))
                this.onSelect(data);
        });
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.valueChangesSubscription.unsubscribe();
    }
    /**
     * @param {?} val
     * @return {?}
     */
    onSelect(val) {
        //commentato perchè autocomplete kendo non ritorna l'object selezionato, ma solo la stringa, e con solo il text (ad esempio customers)
        //non ho gli elementi per fare una runfunction sensata
        this.menuService.runFunction(val);
        this.selected = undefined;
        this.myInput.nativeElement.value = "";
    }
    /**
     * @param {?} val
     * @return {?}
     */
    filter(val) {
        return this.menuService.searchSources.filter(option => new RegExp(val, 'gi').test(option.title)).slice(0, (val && val.length > 0) ? this.maxElements : 0);
    }
    /**
     * @param {?} element
     * @return {?}
     */
    displayElement(element) {
        return element ? element.title : '';
    }
    /**
     * @param {?} val
     * @return {?}
     */
    isObject(val) {
        return val instanceof Object;
    }
}
SearchComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-search',
                template: "<div class=\"tb-search\"> <md-input-container> <span md-prefix><md-icon>search</md-icon></span> <input #myInput type=\"text\" mdInput [mdAutocomplete]=\"auto\" [formControl]=\"inputControl\" placeholder=\"Search...\"> </md-input-container> <md-autocomplete #auto=\"mdAutocomplete\" [displayWith]=\"displayElement\"> <md-option *ngFor=\"let option of filteredElements | async\" [value]=\"option\"> <tb-menu-element class=\"search-element\" [object]=\"option\"></tb-menu-element> </md-option> </md-autocomplete> </div>",
                styles: [".tb-search .mat-input-container { background: #ffffff; padding: 1px 5px; border-radius: 3px; border: 1px solid #044e7b; border: none; width: 330px; margin-bottom: 3px; box-shadow: 0 3px 1px -2px rgba(0, 0, 0, 0.1), 0 1px 1px 0 rgba(0, 0, 0, 0.14), 0 1px 5px 0 rgba(0, 0, 0, 0.1); } .tb-search .mat-focused .mat-input-placeholder.mat-float, .tb-search .mat-input-placeholder.mat-float:not(.mat-empty) { display: none; } .tb-search .mat-input-placeholder.mat-empty { color: #00578c; font-size: 16px; position: relative; padding: 0; line-height: 34px; } .tb-search .mat-input-element { font-size: 14px; line-height: 26px; color: #00578c; margin-bottom: 3px; } .tb-search .mat-input-wrapper { margin: 0; padding: 0; } .tb-search .mat-input-underline { display: none; } .tb-search .dropdown-menu { width: 300px; } .tb-search .mat-icon { margin: 0px 5px 0 0; color: #00578c; } /************/ .search-element .menu-element { margin: 0; padding: 0; background: #fff; } .search-element .menu-element-content .mat-icon { margin-right: 0; } .search-element .menu-element span { font-size: 12px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; flex: 1; } .search-element .menu-element .object-type { margin: 2px 0px 2px 5px; } .cdk-overlay-pane { width: 351px !important; margin-left: -35px; } .mat-option { line-height: 28px; height: 28px; padding: 0; } .mat-option.mat-active .search-element .menu-element { background: #fffbe0; transition: background 0.2s; } "],
                encapsulation: ViewEncapsulation.None
            },] },
];
/**
 * @nocollapse
 */
SearchComponent.ctorParameters = () => [
    { type: MenuService, },
    { type: LocalizationService, },
];
SearchComponent.propDecorators = {
    'maxElements': [{ type: Input },],
    'myInput': [{ type: ViewChild, args: ['myInput',] },],
};
function SearchComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    SearchComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    SearchComponent.ctorParameters;
    /** @type {?} */
    SearchComponent.propDecorators;
    /** @type {?} */
    SearchComponent.prototype.selected;
    /** @type {?} */
    SearchComponent.prototype.inputControl;
    /** @type {?} */
    SearchComponent.prototype.filteredElements;
    /** @type {?} */
    SearchComponent.prototype.maxElements;
    /** @type {?} */
    SearchComponent.prototype.myInput;
    /** @type {?} */
    SearchComponent.prototype.valueChangesSubscription;
    /** @type {?} */
    SearchComponent.prototype.menuService;
    /** @type {?} */
    SearchComponent.prototype.localizationService;
}
