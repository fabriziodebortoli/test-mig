import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Credentials } from './../../authentication/credentials';
import { LoginService } from './../../services/login.service';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import { AccountInfo } from '../../authentication/account-info';
import { OperationResult } from '../../services/operationResult';
import { Subscription } from 'rxjs';

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
  subscription: Subscription;
  errorMessage: string;
  // ChangePasswordDialog variables
  openChangePasswordDialog: boolean = false;
  changePasswordResult: boolean = false;
  changePasswordFields: Array<{ label: string, value: string, hide: boolean }>;

  //--------------------------------------------------------------------------------
  constructor(private route: ActivatedRoute, private loginService: LoginService) { 
        
    this.credentials = new Credentials();
    this.instancesList = new Array<string>();
    this.selectedInstanceKey = '';
    this.welcomeMessage = 'Sign in';
    this.loginStep = 1;
    this.appBusy = false;
    this.errorMessage = '';
    
    // aggiungo i fields da visualizzare nella ChangePasswordDialog
    this.changePasswordFields = [
      { label: 'New password', value: '', hide: true },
      { label: 'Confirm new password', value: '', hide: true },
    ];

    this.subscription = this.loginService.getMessage().subscribe(msg => { 

      // use content result

      let opRes:OperationResult = msg;

      if (!opRes.Result) {
        this.errorMessage = opRes.Message;
      }

      this.appBusy = false;
      this.loginStep = 3;
    })
  }

  //--------------------------------------------------------------------------------
  inputExists() {
    if (
      (this.loginStep == 1 && this.credentials.accountName == '') ||
      (this.loginStep == 2 && this.selectedInstanceKey == '') ||
      (this.loginStep == 3 && this.credentials.password == ''))
       {
      return true;
    }

    return false;
  }

  //--------------------------------------------------------------------------------
  onKeyDown(event) {
    if (event.keyCode == 13) {
      this.errorMessage = '';
      this.doNext();
      return;
    }
  }

  //--------------------------------------------------------------------------------
  doNext() {
    this.errorMessage = '';
    switch (this.loginStep)
    {
      case 1:
        this.preLogin();
      break;

      case 2:
        if (this.selectedInstanceKey === '') {
          this.errorMessage = 'Please select an instance.';
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
      this.errorMessage = 'Account name cannot be empty.';
      this.appBusy = false;
      return;
    }

    let accountInfoStored = localStorage.getItem(this.credentials.accountName);
    if (accountInfoStored !== null) {
      let accountInfo: AccountInfo = JSON.parse(accountInfoStored);
      if (accountInfo !== undefined && accountInfo !== null && accountInfo.instanceKey !== '') {
        this.selectedInstanceKey = accountInfo.instanceKey;
        this.loginStep+=2;
        //this.inputPsw.nativeElement.focus();
        return;
      }
    }

    // load the instances for specified account

    this.loginService.getInstances(this.credentials.accountName)
      .subscribe(
        instances => {

          let opRes:OperationResult = instances;

          if (!opRes.Result) {
            this.errorMessage = opRes.Message;
            this.appBusy = false;
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

            this.loginStep++;
            //this.inputPsw.nativeElement.focus();
          }
        },
        err => {
          this.errorMessage = err;
          this.appBusy = false;
          this.loginStep = 1;
        }
    )
  }

  //--------------------------------------------------------------------------------
  submitLogin() {

    if (this.credentials.accountName == '' || this.credentials.password == '') {
      this.errorMessage = 'Password cannot be empty.';
      return;
    }

    if (this.selectedInstanceKey == '') {
      this.errorMessage = 'Please select an instance.';
      return;
    }

    this.appBusy = true;
    this.loginService.login(this.credentials, this.returnUrl, this.selectedInstanceKey);
  }

  //--------------------------------------------------------------------------------
  doForgottenPassword() {
    alert('Sorry, not implemented yet.');
    return;
  }

  //--------------------------------------------------------------------------------
  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    this.appBusy = false;
  }

  //--------------------------------------------------------------------------------------------------------
  doChangePassword() {
    this.openChangePasswordDialog = true;
  }

  // evento sulla chiusura della dialog di cambio password
  //--------------------------------------------------------------------------------------------------------
  onCloseChangePasswordDialog() {
    // if 'No' button has been clicked I return
    if (!this.changePasswordResult)
      return;

    //test admin connection first!

    let newPw: string = this.changePasswordFields[0].value;
    let confirmNewPw: string = this.changePasswordFields[1].value;

    if (newPw === '' || confirmNewPw === '') {
      alert('Field empty!');
      return;
    }

    if (newPw !== confirmNewPw) {
      alert('Passwords are different!');
      return;
    }

    let changePassword = this.loginService.changePassword(newPw).
      subscribe(
      changeResult => {

        if (changeResult.Result) {
        }
        else
          alert('Error changing password! ' + changeResult.Message);

        // clear local array with dialog values
        //this.credentialsFields.forEach(element => { element.value = '' });
        changePassword.unsubscribe();
      },
      changeError => {
        console.log(changeError);
        alert(changeError);
        changePassword.unsubscribe();
      }
      );
  }
}