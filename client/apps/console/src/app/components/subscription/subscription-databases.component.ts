import { Router, ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-databases',
  templateUrl: './subscription-databases.component.html',
  styleUrls: ['./subscription-databases.component.css']
})

export class SubscriptionDatabaseComponent implements OnInit {

   model: SubscriptionDatabase;
   editing: boolean = false;
   useDMS: boolean = false;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) { 
    this.model = new SubscriptionDatabase();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
    let dbName: string = this.route.snapshot.queryParams['databaseToEdit'];

    if (subscriptionKey === undefined || dbName === undefined)
      return;
    
    this.editing = true;

    // now I load the database information

    this.modelService.getDatabase(subscriptionKey, dbName)
    .subscribe(
      res => 
      {
        let databases:SubscriptionDatabase[] = res['Content'];

        if (databases.length == 0)
          return;

        this.model = databases[0];
      },
      err => { alert(err);}
    )
  }

  //--------------------------------------------------------------------------------------------------------
  onUseDMSChange(event) {
    this.useDMS = this.model.UseDMS;
  }

  //--------------------------------------------------------------------------------------------------------
  submitDatabase() {
    if (this.model.SubscriptionKey == undefined || this.model.Name == undefined) {
      alert('Mandatory fields are empty! Check subscription key / database name!');
      return;
    }

    let subscriptionKey: string = this.model.SubscriptionKey;
    
    let databaseOperation: Observable<OperationResult> = this.modelService.saveDatabase(this.model);

    let subs = databaseOperation.subscribe(
      databaseResult => {
        this.model = new SubscriptionDatabase();
        if (this.editing)
          this.editing = !this.editing;
        subs.unsubscribe();
        
        // after save I return to parent page
        // this.router.navigateByUrl('/subscription'); // I have to set the queryparams!
        this.router.navigateByUrl('/subscription', { queryParams: { subscriptionToEdit: subscriptionKey } });
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }
}
