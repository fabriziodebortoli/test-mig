import { HttpMenuService, Logger, DiagnosticService, ThemeService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'tb-theme-changer',
    templateUrl: './theme-changer.component.html',
    styleUrls: ['./theme-changer.component.scss']
})
export class ThemeChangerComponent {

    constructor(
        public httpMenuService: HttpMenuService,
        public logger: Logger,
        public diagnosticService: DiagnosticService,
        public themeService: ThemeService
    ) { }

    //---------------------------------------------------------------------------------------------
    changeTheme(theme) {
        this.themeService.changeTheme(theme);
    }
}