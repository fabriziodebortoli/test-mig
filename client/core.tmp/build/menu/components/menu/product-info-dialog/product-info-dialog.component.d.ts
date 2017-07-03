import { OnInit } from '@angular/core';
import { MdDialogRef } from '@angular/material';
import { UtilsService } from './../../../../core/services/utils.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';
export declare class ProductInfoDialogComponent implements OnInit {
    dialogRef: MdDialogRef<ProductInfoDialogComponent>;
    private httpMenuService;
    private utilsService;
    private localizationService;
    private productInfos;
    constructor(dialogRef: MdDialogRef<ProductInfoDialogComponent>, httpMenuService: HttpMenuService, utilsService: UtilsService, localizationService: LocalizationService);
    ngOnInit(): void;
}
