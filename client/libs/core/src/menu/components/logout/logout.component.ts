import { Component } from '@angular/core';
import { AuthService } from './../../../core/services/auth.service';

@Component({
  selector: 'tb-logout',
  template: ''
})
export class LogoutComponent {

  constructor(public authService: AuthService) {
    authService.logout();
  }

}
