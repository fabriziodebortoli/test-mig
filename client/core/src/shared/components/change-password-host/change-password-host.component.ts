import { PasswordComponent } from './../../controls/password/password.component';

import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener, EventEmitter, ViewChild } from '@angular/core';
import { AuthService } from './../../../core/services/auth.service';
import { ChangePasswordComponent } from './../change-password/change-password.component';



@Component({
    selector: 'tb-change-password-host',
    templateUrl: './change-password-host.component.html',
    styleUrls: ['./change-password-host.component.scss']
})

export class ChangePasswordHostComponent implements OnDestroy {
    @ViewChild('changePassword') changePassword: ChangePasswordComponent
    passwordChanged: boolean = false;
    constructor(
        public authService: AuthService
    ) {
    }

    ngOnDestroy() {
    }

    openChangePassword() {
        this.changePassword.changePasswordOpened = true;
        const thiz = this;
        const sub = this.changePassword.passwordChanged.subscribe((newPassword) => {
            sub.unsubscribe();
            if (newPassword != "") {
                thiz.passwordChanged = true;
                setTimeout(() => {
                    thiz.passwordChanged = false;
                }, 5000);
                //password changed
            }
        });
    }
}
