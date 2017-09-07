import { Component, OnInit } from '@angular/core';
import { Credentials } from './../../authentication/credentials';
import { LoginService } from './../../services/login.service';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  credentials: Credentials;
  returnUrl: string;
  instancesList: Array<string>;
  selectedInstanceKey: string;
  welcomeMessage: string;

  //--------------------------------------------------------------------------------
  constructor(private route: ActivatedRoute, private loginService: LoginService) { 
        
    this.credentials = new Credentials();
    this.instancesList = new Array<string>();
    this.selectedInstanceKey = '';
    this.welcomeMessage = 'Sign in';
  }

  //--------------------------------------------------------------------------------
  preLogin() {
    if (this.credentials.accountName == '') {
      alert('Account name is empty!');
      return;
    }

    // load the instances for specified account

    this.loginService.getInstances(this.credentials.accountName)
      .subscribe(
        instances => {
          this.instancesList = instances['Content'];

          if (this.instancesList.length > 0)
            this.welcomeMessage = 'Welcome';
        },
        err => {
          alert(err);
        }
    )
  }

  //--------------------------------------------------------------------------------
  submitLogin() {

    if (this.credentials.accountName == '' || this.credentials.password == '') {
      alert('Account name / password empty!');
      return;
    }

    if (this.selectedInstanceKey == '') {
      alert('Select an instance!');
      return;
    }

    this.loginService.login(this.credentials, this.returnUrl, this.selectedInstanceKey);
  }

  //--------------------------------------------------------------------------------
  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
}
