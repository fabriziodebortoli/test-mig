import { Component, OnInit, Input } from '@angular/core';
import { Subscription } from "app/model/subscription";

@Component({
  selector: 'app-account-subscriptions',
  templateUrl: './account-subscriptions.component.html',
  styleUrls: ['./account-subscriptions.component.css']
})
export class AccountSubscriptionsComponent implements OnInit {

  @Input() accountName: string;
  subscriptionList: Array<Subscription>;

  constructor() { 
    this.subscriptionList = new Array<Subscription>();
  }

  ngOnInit() {
    // load subscriptions for accountName
  }

}
