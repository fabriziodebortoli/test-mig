import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'admin-sidenav-container',
  templateUrl: './admin-sidenav-container.component.html',
  styleUrls: ['./admin-sidenav-container.component.css']
})
export class AdminSidenavContainerComponent implements OnInit {

    @Input() appTitle:string;
    @Input() userAccountName:string;

    @Output() openAccountProfile: EventEmitter<string>;
    @Output() openSignIn: EventEmitter<boolean>;

    loginIcon: string;

    constructor() { 
      this.userAccountName = '';
      this.openAccountProfile = new EventEmitter<string>();
      this.openSignIn = new EventEmitter<boolean>();
      this.loginIcon = 'person';
    }

    ngOnInit() {
    }

    openProfile() {
      this.openAccountProfile.emit(this.userAccountName);
      return;
    }

    signIn() {
      this.openSignIn.emit();
    }

    loginOrAccount() {

      if (this.userAccountName === '' || this.userAccountName === undefined) {
        this.signIn();
        return;
      }

      this.openProfile();
    }
}
