import { ThemeService } from './../../core/services/theme.service';
import { DiagnosticService } from './../../core/services/diagnostic.service';
import { Logger } from './../../core/services/logger.service';
import { Http } from '@angular/http';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { ComponentService } from '../../core/services/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';


import { SettingsPageService } from '../settingsPage.service';
import { InfoService } from './../../core/services/info.service';


@Component({
    selector: 'tb-theme-changer',
    templateUrl: './theme-changer.component.html',
    styleUrls: ['./theme-changer.component.scss']
})
export class ThemeChangerComponent {
   

    constructor(
        private httpMenuService: HttpMenuService,
        private logger: Logger,
        private diagnosticService: DiagnosticService, 
        private themeService: ThemeService
    ) {
      
    }
   
    //---------------------------------------------------------------------------------------------
    changeTheme(theme) {
        this.themeService.changeTheme(theme);
    }
}