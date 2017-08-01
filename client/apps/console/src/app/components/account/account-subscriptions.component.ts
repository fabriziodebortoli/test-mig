import {AccountRole} from 'app/model/accountRole';
import { Component, OnInit, Input } from '@angular/core';
import { Subscription } from "app/model/subscription";

@Component({
  selector: 'app-account-subscriptions',
  templateUrl: './account-subscriptions.component.html',
  styleUrls: ['./account-subscriptions.component.css']
})
export class AccountSubscriptionsComponent implements OnInit {

  @Input() accountName: string;
  @Input() accountRoles: Array<AccountRole>;

  constructor() { 
    this.accountRoles = new Array<AccountRole>();
  }

  ngOnInit() {
    // load subscriptions for accountName
  }

}
