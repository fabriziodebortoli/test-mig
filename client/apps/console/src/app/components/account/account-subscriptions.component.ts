import {SubscriptionAccount} from '../../model/subscriptionAccount';
import {AccountRole} from 'app/model/accountRole';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-account-subscriptions',
  templateUrl: './account-subscriptions.component.html',
  styleUrls: ['./account-subscriptions.component.css']
})
export class AccountSubscriptionsComponent implements OnInit {

  @Input() subscriptions: Array<SubscriptionAccount>;
  columnNames:string[] = ['SubscriptionKey'];

  constructor() { 
    this.subscriptions = new Array<SubscriptionAccount>();
  }

  ngOnInit() {
    // load subscriptions for accountName
  }

}
