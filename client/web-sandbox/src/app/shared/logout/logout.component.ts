import { Component } from '@angular/core';

import { LoginService } from './../../core/login.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.scss']
})
export class LogoutComponent {

  constructor(private loginService: LoginService) {
    this.loginService.logout().subscribe();
  }

}
