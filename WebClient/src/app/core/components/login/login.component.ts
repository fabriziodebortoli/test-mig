import { LoginSessionService } from './../../services/login-session.service';
import { LoginSession } from './../../models/login-session';
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
