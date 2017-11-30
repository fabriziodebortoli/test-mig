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

  subscription: Subscription;
  userAccountName: string;

  constructor(private router: Router, private loginService: LoginService) {
    this.userAccountName = '';

    this.subscription = this.loginService.getMessage().subscribe(message => {
      try
      {
        let opRes:OperationResult = message;
        if (opRes.Result) {
          this.userAccountName = opRes.Message;
        }
      }
      catch(Error){
      }
    });
  }

  @HostListener('window:unload', [ '$event' ])
  clearLocalStorage() {
    this.loginService.logout();
  }  

  ngOnInit() {
    this.router.navigateByUrl('/appHome', { skipLocationChange:true });
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
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
