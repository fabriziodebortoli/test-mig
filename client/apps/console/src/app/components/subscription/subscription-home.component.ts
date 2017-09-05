import { AppSubscription } from 'app/model/subscription';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizationProperties } from 'app/authentication/auth-info';

@Component({
  selector: 'app-subscription-home',
  templateUrl: './subscription-home.component.html',
  styleUrls: ['./subscription-home.component.css']
})

export class SubscriptionHomeComponent implements OnInit {

  subscriptions: AppSubscription[];
  readingData:boolean;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router) {
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored == undefined) {
      alert('User must be logged in.');
      return;
    }

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

    // ask to GWAM the list of subscriptions

    this.readingData = true;

    this.modelService.getSubscriptions()
      .subscribe(
      subscriptions => {
        this.subscriptions = subscriptions['Content'];
        this.readingData = false;
      },
      err => {
        alert(err);
        this.readingData = false;
      }
      )
  }

  //--------------------------------------------------------------------------------------------------------
  openSubscription(item: object) {
    // route to edit subscription
    this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: item['SubscriptionKey'] } });
  }
}
