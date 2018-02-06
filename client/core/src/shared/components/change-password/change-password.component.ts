import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener, EventEmitter, ChangeDetectorRef } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { AuthService } from './../../../core/services/auth.service';
import { Logger } from './../../../core/services/logger.service';

import { TbComponent } from '../tb.component';

@Component({
    selector: 'tb-change-password',
    templateUrl: './change-password.component.html',
    styleUrls: ['./change-password.component.scss']
})

export class ChangePasswordComponent extends TbComponent implements OnDestroy {
    passwordChanged: EventEmitter<string> = new EventEmitter();

    public errorMessage: string = "";
    confirmPassword: string = "";
    newPassword: string = "";
    oldPassword: string = "";
    public changePasswordOpened: boolean = false;
    constructor(
        public authService: AuthService,
        public tbComponentService: TbComponentService,
        protected changeDetectorRef: ChangeDetectorRef,
        public logger: Logger
    ) {
        super(tbComponentService, changeDetectorRef);
    }

    ngOnDestroy() {
    }

    changePasswordOk() {
        this.errorMessage = "";
        if (this.confirmPassword != this.newPassword){
            return;
        }


        this.authService.changePassword(localStorage.getItem('_user'), this.oldPassword, this.newPassword).
            subscribe((res) => {

                this.logger.debug(res);
                if (res.success) {
                    this.changePasswordOpened = false;
                    //this.connectionData.password = this.newPassword;

                    this.passwordChanged.emit(this.newPassword);
                    //resetto tutte le variabili per eventuali cambi successivi
                    this.confirmPassword = "";
                    this.newPassword = "";
                    this.oldPassword = "";
                    this.changePasswordOpened = false;
                }
                else {
                    this.errorMessage = res.message;
                }
            });
    }

    close() {
        this.changePasswordOpened = false;
        this.passwordChanged.emit("");
        //resetto tutte le variabili per eventuali cambi successivi
        this.confirmPassword = "";
        this.newPassword = "";
        this.oldPassword = "";
    }

    keyUpFunction(event) {
        if (event.keyCode === 13) {
            this.changePasswordOk()
        }
    }

}
