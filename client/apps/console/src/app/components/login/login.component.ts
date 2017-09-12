import {OperationResult} from '../../services/operationResult';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Credentials } from './../../authentication/credentials';
import { LoginService } from './../../services/login.service';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import { AccountInfo } from '../../authentication/account-info';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {

  // model
  credentials: Credentials;
  instancesList: Array<string>;
  selectedInstanceKey: string;

  // behaviour
  returnUrl: string;
  welcomeMessage: string;
  loginStep: number;
  appBusy: boolean;

  //--------------------------------------------------------------------------------
  constructor(private route: ActivatedRoute, private loginService: LoginService) { 
        
    this.credentials = new Credentials();
    this.instancesList = new Array<string>();
    this.selectedInstanceKey = '';
    this.welcomeMessage = 'Sign in';
    this.loginStep = 1;
    this.appBusy = false;
  }

  //--------------------------------------------------------------------------------
  doNext() {
    switch (this.loginStep)
    {
      case 1:
        this.preLogin();
        this.loginStep++;
      break;

      case 2:
        if (this.selectedInstanceKey === '') {
          alert('Please select an instance');
          return;
        }
        this.loginStep++;
      break;

      case 3:
        this.submitLogin();
      break;
    }
  }

  //--------------------------------------------------------------------------------
  clearSelectedInstance()
  {
    this.selectedInstanceKey = "";

    let accountInfoStored = localStorage.getItem(this.credentials.accountName);
    if (accountInfoStored !== null) {
      let accountInfo: AccountInfo = JSON.parse(accountInfoStored);
      if (accountInfo !== undefined && accountInfo !== null && accountInfo.instanceKey !== '') {
        accountInfo.instanceKey = '';
        localStorage.setItem(this.credentials.accountName, JSON.stringify(accountInfo));
      }
    }
    
    this.loginStep = 1;
    this.doNext();
    return;
  }

  //--------------------------------------------------------------------------------
  preLogin() {

    if (this.credentials.accountName == '') {
      alert('Account name is empty!');
      return;
    }

    let accountInfoStored = localStorage.getItem(this.credentials.accountName);
    if (accountInfoStored !== null) {
      let accountInfo: AccountInfo = JSON.parse(accountInfoStored);
      if (accountInfo !== undefined && accountInfo !== null && accountInfo.instanceKey !== '') {
        this.selectedInstanceKey = accountInfo.instanceKey;
        this.loginStep++;
        return;
      }
    }

    // load the instances for specified account

    this.loginService.getInstances(this.credentials.accountName)
      .subscribe(
        instances => {

          let opRes:OperationResult = instances;

          if (!opRes.Result) {
            alert(opRes.Message);
            this.loginStep = 1;
            return;
          }
          
          this.instancesList = instances['Content'];

          // if at least one instance has been loaded I change the welcome message string
          // and I read from localstorage the AccountInfo (if exist)

          if (this.instancesList.length > 0) {

            this.welcomeMessage = 'Welcome, ' + this.credentials.accountName;
            let localAccountInfo = localStorage.getItem(this.credentials.accountName);
            
            if (localAccountInfo != null && localAccountInfo != '') {
              let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
              this.selectedInstanceKey = accountInfo.instanceKey;
            }
          }
        },
        err => {
          alert(err);
          this.loginStep = 1;
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
      alert('Please select an instance');
      return;
    }

    this.appBusy = true;
    this.loginService.login(this.credentials, this.returnUrl, this.selectedInstanceKey);
  }

  //--------------------------------------------------------------------------------
  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    this.appBusy = false;
  }

}
