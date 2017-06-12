import { ModelService } from './services/model.service';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  title = 'Admin Server';
  username:string;
  resText:string;
  errText:string;

  // services
  accountService: ModelService;

  constructor(accountService:ModelService) {
    this.accountService = accountService;
  }

  // getAccountInfo() {
  //   alert(this.username);
  //   this.accountService.GetAccount(this.username)
  //         .subscribe(
  //           str => this.resText = str,
  //           error => this.errText);
  // }
}
