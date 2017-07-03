import { OnInit, ElementRef, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
export declare class SearchComponent implements OnInit, OnDestroy {
    private menuService;
    private localizationService;
    selected: string;
    inputControl: FormControl;
    filteredElements: any;
    maxElements: number;
    myInput: ElementRef;
    valueChangesSubscription: any;
    constructor(menuService: MenuService, localizationService: LocalizationService);
    ngOnInit(): void;
    ngOnDestroy(): void;
    onSelect(val: any): void;
    filter(val: string): string[];
    displayElement(element: any): string;
    isObject(val: any): boolean;
}
