import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { AppSubscription } from '../../model/subscription';
import { ModelService } from '../../services/model.service';
import { Observable } from 'rxjs/Observable';
import { OperationResult } from '../../services/operationResult';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';

@Component({
  selector: 'app-subscription',
  templateUrl: './subscription.component.html',
  styleUrls: ['./subscription.component.css']
})

export class SubscriptionComponent implements OnInit {

  model: AppSubscription;
  editing: boolean = false;
  databases: SubscriptionDatabase[];
  readingData: boolean;
  existDatabases: boolean = false;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new AppSubscription();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];

    if (subscriptionKey === undefined) 
      return;
    
    this.editing = true;
    this.readingData = true;

    // first I load the subscription 

    this.modelService.getSubscriptions(subscriptionKey)
      .subscribe(
      res => {
        let subscriptions: AppSubscription[] = res['Content'];

        if (subscriptions.length == 0) {
          this.readingData = true;
          return;
        }

        this.model = subscriptions[0];

        // then I load the databases of selected subscription

        this.modelService.getDatabases(subscriptionKey)
          .subscribe(
          res => {
            this.databases = res['Content'];
            this.readingData = false;
            this.existDatabases = this.databases.length > 0;
          },
          err => {
            alert(err);
            this.readingData = false;
          }
          )
      },
      err => {
        alert(err);
        this.readingData = false;
      }
      )
    }

  //--------------------------------------------------------------------------------------------------------
  submitSubscription() {
    if (this.model.SubscriptionKey == undefined) {
      alert('Mandatory fields are empty! Check subscription key!');
      return;
    }

    let subscriptionOperation: Observable<OperationResult> = this.modelService.saveSubscription(this.model);

    let subs = subscriptionOperation.subscribe(
      subscriptionResult => {
        this.model = new AppSubscription();
        if (this.editing)
          this.editing = !this.editing;
        subs.unsubscribe();
        // after save I return to parent page
        this.router.navigateByUrl('/subscriptionsHome');
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }

  //--------------------------------------------------------------------------------------------------------
  openDatabase(item: object) {
    // route to edit database, I add in the existing query string the database name
    this.router.navigate(['/database'], { queryParams: { databaseToEdit: item['Name'] }, queryParamsHandling: "merge" });
  }

  //--------------------------------------------------------------------------------------------------------
  addDatabase() {
     // route to add database
     this.router.navigate(['/database'], { queryParamsHandling: "preserve" } );
    }
}
