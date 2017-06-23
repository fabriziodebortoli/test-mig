import { OnInit, OnDestroy } from '@angular/core';
import { MdDialogRef } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';
export declare class ConnectionInfoDialogComponent implements OnInit, OnDestroy {
    dialogRef: MdDialogRef<ConnectionInfoDialogComponent>;
    private httpMenuService;
    private localizationService;
    private connectionInfos;
    private showdbsize;
    private connectionInfoSub;
    constructor(dialogRef: MdDialogRef<ConnectionInfoDialogComponent>, httpMenuService: HttpMenuService, localizationService: LocalizationService);
    ngOnInit(): void;
    ngOnDestroy(): void;
}
