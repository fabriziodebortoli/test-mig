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
    public themes: any;

    constructor(
        private httpMenuService: HttpMenuService,
        private logger: Logger,
        private diagnosticService: DiagnosticService
    ) {
        this.getThemes();
    }

    //---------------------------------------------------------------------------------------------
    getThemeName(theme) {
        theme.name =  theme.path.split("\\").pop();
        // var tempName = theme.path.split("\\").pop();
        // theme.name = tempName.replace(/.theme/gi, "");
    }

    //---------------------------------------------------------------------------------------------
    getThemes() {
        let subs = this.httpMenuService.getThemes().subscribe((res) => {
            this.themes = res.Themes.Theme;

            for (var i = 0; i < this.themes.length; i++) {
                this.getThemeName(this.themes[i]);
            }
            subs.unsubscribe();
        });
    }

    //---------------------------------------------------------------------------------------------
    changeTheme(theme) {
        let subs = this.httpMenuService.changeThemes(theme.path).subscribe((res) => {
            if (!res.error) {
                this.getThemes();
            }
            else if (res.messages) {
                this.diagnosticService.showDiagnostic(res.messages);
            }
            subs.unsubscribe();
        });
    }
}