import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { DiagnosticService } from './diagnostic.service';
import { HttpService } from './http.service';

@Injectable()
export class ThemeService {

    public currentTheme: string = '';
    public themes: any;
    constructor(
        public httpService: HttpService,
        public router: Router,
        private diagnosticService: DiagnosticService
    ) {
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
        let subs = this.httpService.getThemes().subscribe((res) => {
            this.themes = res.Themes.Theme;

            for (var i = 0; i < this.themes.length; i++) {
                this.getThemeName(this.themes[i]);
                if (this.themes[i].isFavoriteTheme)
                    this.currentTheme = this.themes[i].name.toLowerCase();
            }
            subs.unsubscribe();
            if (this.currentTheme)
                this.applyTheme(this.currentTheme);
        });
    }

    //---------------------------------------------------------------------------------------------
    changeTheme(theme) {
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
}