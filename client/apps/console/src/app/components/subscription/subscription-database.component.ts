import { DatabaseService } from './../../services/database.service';
import { AccountInfo } from './../../authentication/account-info';
import { Router, ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-database',
  templateUrl: './subscription-database.component.html',
  styleUrls: ['./subscription-database.component.css']
})

export class SubscriptionDatabaseComponent implements OnInit, OnDestroy {

  model: SubscriptionDatabase;
  editing: boolean = false;
  
  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService, 
    private databaseService: DatabaseService, 
    private router: Router,
    private route: ActivatedRoute) { 
      
      this.model = new SubscriptionDatabase();
    }
    
    //--------------------------------------------------------------------------------------------------------
    ngOnInit() {
      
      // I read queryparams
      let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
      let dbName = this.route.snapshot.queryParams['databaseToEdit'];
      
      if (subscriptionKey === undefined)
      return;
      
      this.model.SubscriptionKey = subscriptionKey;
      
      // I need the instanceKey where the currentAccount is logged
      let localAccountInfo = localStorage.getItem(this.modelService.currentAccountName);
      
      if (localAccountInfo != null && localAccountInfo != '') {
        let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
        this.model.InstanceKey = accountInfo.instanceKey;
      }

      if (dbName === undefined) {
        this.model.Name = 'MASTER';
        return;
      }
      
      this.editing = true;
      this.databaseService.needsAskCredentials = false;
      
      // I load the database information only if dbName is filled
      
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
    ngOnDestroy(): void {
      this.databaseService.testConnectionOK = false;
      this.databaseService.needsAskCredentials = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    submitDatabase() {
      if (this.model.SubscriptionKey == undefined || this.model.Name == undefined) {
        alert('Mandatory fields are empty! Check subscription key / database name!');
        return;
      }
      
      let subscriptionKey: string = this.model.SubscriptionKey;
      
      let subs = this.modelService.saveDatabase(this.model).
      subscribe(
        databaseResult => {
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
  