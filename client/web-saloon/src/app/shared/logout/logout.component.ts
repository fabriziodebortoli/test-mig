import { Component } from '@angular/core';

import { LoginService } from './../../core/login.service';

@Component({
  selector: 'app-logout',
  template: ''
})
export class LogoutComponent {

  constructor(private loginService: LoginService) {
    this.loginService.logout().subscribe();
  }

}
