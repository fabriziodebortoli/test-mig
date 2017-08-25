import { Component, OnInit } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';

@Component({
  selector: 'app-subscription-databases',
  templateUrl: './subscription-databases.component.html',
  styleUrls: ['./subscription-databases.component.css']
})

export class SubscriptionDatabasesComponent implements OnInit {

   model: SubscriptionDatabase;
  
  constructor() { 
    this.model = new SubscriptionDatabase();
  }

  ngOnInit() {
  }

}
