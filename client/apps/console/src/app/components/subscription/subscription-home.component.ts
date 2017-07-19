import { Observable } from 'rxjs/Observable';
import { Subscription } from 'app/model/subscription';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-home',
  templateUrl: './subscription-home.component.html',
  styleUrls: ['./subscription-home.component.css']
})

export class SubscriptionHomeComponent implements OnInit {

  model: Subscription;
  editing: boolean = false;

  constructor(private modelService: ModelService) {
    this.model = new Subscription();
  }

  ngOnInit() {
  }

  submitSubscription() {
    if (this.model.subscriptionKey == undefined) {
      alert('Mandatory fields are empty! Check email/password!');
      return;
    }

    let subscriptionOperation: Observable<OperationResult>;

    if (!this.editing) {
      subscriptionOperation = this.modelService.addSubscription(this.model)
    } else {
      //subscriptionOperation = this.modelService.updateSubscription(this.model)
    }

    let subs = subscriptionOperation.subscribe(
      subscriptionResult => {
        this.model = new Subscription();
        if (this.editing) this.editing = !this.editing;
        alert(subscriptionResult.Message);
        subs.unsubscribe();
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }
}
