import { LoginSessionService } from './../../../core/';
import { LoginSession } from './../../../shared/';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  connectionData: LoginSession = new LoginSession();
  constructor(private loginSessionService: LoginSessionService) { }

  ngOnInit() {
  }

  login() {
    this.loginSessionService.login(this.connectionData);
  }
}
