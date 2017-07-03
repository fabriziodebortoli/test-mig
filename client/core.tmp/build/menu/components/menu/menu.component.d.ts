import { OnInit, OnDestroy } from '@angular/core';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { HttpMenuService } from './../../services/http-menu.service';
export declare class MenuComponent implements OnInit, OnDestroy {
    private httpMenuService;
    private menuService;
    private localizationService;
    private settingsService;
    private eventManagerService;
    private eventData;
    private enumsService;
    private subscriptions;
    constructor(httpMenuService: HttpMenuService, menuService: MenuService, localizationService: LocalizationService, settingsService: SettingsService, eventManagerService: EventManagerService, eventData: EventDataService, enumsService: EnumsService);
    ngOnInit(): void;
    ngOnDestroy(): void;
}
