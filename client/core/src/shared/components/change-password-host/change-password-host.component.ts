import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener, EventEmitter, ViewChild, ChangeDetectorRef } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { AuthService } from './../../../core/services/auth.service';

import { TbComponent } from '../tb.component';

import { ChangePasswordComponent } from './../change-password/change-password.component';
import { PasswordComponent } from './../../controls/password/password.component';

@Component({
    selector: 'tb-change-password-host',
    templateUrl: './change-password-host.component.html',
    styleUrls: ['./change-password-host.component.scss']
})

export class ChangePasswordHostComponent extends TbComponent implements OnDestroy {
    @ViewChild('changePassword') changePassword: ChangePasswordComponent
    passwordChanged: boolean = false;
    constructor(
        public authService: AuthService,
        public tbComponentService: TbComponentService,
        protected changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
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
