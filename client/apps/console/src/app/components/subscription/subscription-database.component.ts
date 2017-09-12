import { AccountInfo } from './../../authentication/account-info';
import { Router, ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-database',
  templateUrl: './subscription-database.component.html',
  styleUrls: ['./subscription-database.component.css']
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

    // I read the queryparams
    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
    let dbName: string = this.route.snapshot.queryParams['databaseToEdit'];

    if (subscriptionKey === undefined)
      return;

    this.model.SubscriptionKey = subscriptionKey;
      
    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.modelService.currentAccountName);

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      this.model.InstanceKey = accountInfo.instanceKey;
    }

    if (dbName === undefined)
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
        this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: subscriptionKey } });
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }
}
