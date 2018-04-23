import { get } from 'lodash';
import { InfoService } from './info.service';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { DiagnosticService } from './diagnostic.service';
import { HttpService } from './http.service';
import { StorageService } from './storage.service';

@Injectable()
export class ThemeService {

    public currentTheme: string = '';
    public themes: any;
    private storageKey: string;
    constructor(
        public httpService: HttpService,
        public router: Router,
        public infoService: InfoService,
        private diagnosticService: DiagnosticService
    ) {
        this.storageKey = localStorage.getItem('_user') + '_theme';
    }

    applyTheme(theme: string) {
        this.router.navigate([{ outlets: { theme: theme }, skipLocationChange: true, replaceUrl: false }]).then(() => {
            this.router.navigate([{ outlets: { theme: null }, skipLocationChange: true, replaceUrl: false }]);
        });
    }

    resetTheme() {
        this.router.navigate([{ outlets: { theme: 'reset' }, skipLocationChange: true, replaceUrl: false }]).then(() => {
            this.router.navigate([{ outlets: { theme: null }, skipLocationChange: true, replaceUrl: false }]);
        });
    }

    //---------------------------------------------------------------------------------------------
    getThemeName(theme) {
        theme.name = theme.path.split("\\").pop();
        var tempName = theme.path.split("\\").pop();
        theme.name = tempName.replace(/.theme/gi, "");
    }

    //---------------------------------------------------------------------------------------------
    loadThemes() {
        this.themes = [];
        this.currentTheme = localStorage.getItem(this.storageKey)//todoluca, storage

        let subs = this.httpService.getThemes().subscribe((res) => {
            this.themes = res.Themes.Theme;
            for (var i = 0; i < this.themes.length; i++) {
                this.getThemeName(this.themes[i]);
                if (this.themes[i].isFavoriteTheme)
                    this.currentTheme = this.themes[i].name.toLowerCase();

                if (this.currentTheme == this.themes[i].name.toLowerCase()) {
                    this.themes[i].isFavoriteTheme = true;
                }
            }
            subs.unsubscribe();
            if (this.currentTheme)
                this.applyTheme(this.currentTheme);
        });
    }

    //---------------------------------------------------------------------------------------------
    changeTheme(theme) {
        if (this.infoService.isDesktop) {
            let subs = this.httpService.changeThemes(theme.path).subscribe((res) => {
                if (!res.error) {
                    this.loadThemes();
                }
                else if (res.messages && res.messages.length) {
                    this.diagnosticService.showDiagnostic(res.messages);
                }
                subs.unsubscribe();
            });
        }
        else {

            this.currentTheme = theme.name.toLowerCase();
            localStorage.setItem(this.storageKey, this.currentTheme);
            if (this.currentTheme) {
                this.applyTheme(this.currentTheme);
                for (var i = 0; i < this.themes.length; i++) {
                    this.themes[i].isFavoriteTheme = false;
                }
                theme.isFavoriteTheme = true;
            }

        }
    }
}