import { LoginService } from './services/login.service';
import { ModelService } from './services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
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
    this.subscription = this.loginService.getMessage().subscribe(message => {
      this.userAccountName = message;
    });
  }

  ngOnInit() {
    this.router.navigateByUrl('/appHome');
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  logout() {
    this.loginService.logout();
  }

  openAccount(event) {
    alert(event);
    return;
  }

  openLogin() {
    this.router.navigateByUrl('/loginComponent');
  }
}
