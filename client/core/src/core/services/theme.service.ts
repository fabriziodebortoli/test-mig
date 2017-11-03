import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { HttpService } from './http.service';

@Injectable()
export class ThemeService {

    public theme: string = '';

    constructor(
        public httpService: HttpService,
        public router: Router
    ) {
        // this.applyTheme('darcula');// DEBUG
    }

    changeTheme(theme: string) {
        this.applyTheme(theme);
    }

    applyTheme(theme: string) {
        console.log("applyTheme() -> ", theme);
        this.router.navigate([{ outlets: { theme: theme }, skipLocationChange: true, replaceUrl: false }]).then(() => {
            this.router.navigate([{ outlets: { theme: null }, skipLocationChange: true, replaceUrl: false }]);
            console.log("applyTheme() -> applied");
        });
    }

    resetTheme() {
        console.log("resetTheme()");
        this.router.navigate([{ outlets: { theme: 'reset' }, skipLocationChange: true, replaceUrl: false }]).then(() => {
            this.router.navigate([{ outlets: { theme: null }, skipLocationChange: true, replaceUrl: false }]);
            console.log("resetTheme() -> reset");
        });
    }
}