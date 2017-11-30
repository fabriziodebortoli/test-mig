import {OperationResult} from './services/operationResult';
import { LoginService } from './services/login.service';
import { ModelService } from './services/model.service';
import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { Subject } from "rxjs/Subject";
import { Subscription } from "rxjs/Subscription";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit, OnDestroy {

  loginServiceSubscription: Subscription;
  userAccountName: string;

  constructor(private router: Router, private loginService: LoginService) {
    this.userAccountName = '';

    this.loginServiceSubscription = this.loginService.getMessage().subscribe(
      message => {
        let opRes:OperationResult = message;
        if (opRes.Result) {
          this.userAccountName = opRes.Message;
        }
      },
      err => {
        console.log('An error occurred while listening to loginService.getMessage()');
      }
    );
  }

  @HostListener('window:unload', [ '$event' ])
  clearLocalStorage() {
    this.loginService.logout();
  }  

  ngOnInit() {
    this.router.navigateByUrl('/appHome', { skipLocationChange:true });
  }

  ngOnDestroy() {
    this.loginServiceSubscription.unsubscribe();
  }
  
  logout() {
    this.loginService.logout();
  }

  openAccount(event) {
    if (event === undefined || event === '')
      return;
    this.router.navigate(['/account'], { queryParams: { accountNameToEdit: event, redirectOnSave: false } });
  }

  openLogin() {
    this.router.navigateByUrl('/loginComponent');
  }
}