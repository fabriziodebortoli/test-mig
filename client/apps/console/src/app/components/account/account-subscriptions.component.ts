import { Component, OnInit } from '@angular/core';
import { Subscription } from "app/model/subscription";

@Component({
  selector: 'app-account-subscriptions',
  templateUrl: './account-subscriptions.component.html',
  styleUrls: ['./account-subscriptions.component.css']
})
export class AccountSubscriptionsComponent implements OnInit {

  subscriptionList: Array<Subscription>;

  constructor() { 
    this.subscriptionList = new Array<Subscription>();
    let s:Subscription = new Subscription();
    s.SubscriptionKey = "USS-001";
    s.Description = "New subscription";
    this.subscriptionList.push(s);
  }

  ngOnInit() {
  }

}
