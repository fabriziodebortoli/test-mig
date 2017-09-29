import { DatabaseService } from './../../services/database.service';
import { AccountInfo } from './../../authentication/account-info';
import { Router, ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-database',
  templateUrl: './subscription-database.component.html',
  styleUrls: ['./subscription-database.component.css']
})

export class SubscriptionDatabaseComponent implements OnInit {
  
  @Input() model: SubscriptionDatabase;
  editing: boolean = false;
  
  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService, 
    private databaseService: DatabaseService, 
    private router: Router,
    private route: ActivatedRoute) { 
    }
    
    //--------------------------------------------------------------------------------------------------------
    ngOnInit() {
      
      // I prefill input data if is the first configuration
      if (this.model.Name === undefined || this.model.Name === '')
      {
        let suffix = this.model.Test ? '_Test' : '_Master'
        let prefix = this.model.InstanceKey + '_' + this.model.SubscriptionKey;
        this.model.Name = prefix + suffix;
        this.model.Provider = this.databaseService.dbCredentials.Provider;
        this.model.DBName =  this.model.Name + '_DB';
        this.model.DBOwner = prefix + '_Admin';
        this.model.DMSDBName =  this.model.Name + '_DBDMS';
        this.model.DMSDBOwner = prefix + '_Admin';
        return;
      }
      
      this.editing = true;
      this.databaseService.needsAskCredentials = false;
      
      // I load the database information only if Name is filled
      
      this.modelService.getDatabase(this.model.SubscriptionKey, this.model.Name)
      .subscribe(
        res => 
        {
          let databases:SubscriptionDatabase[] = res['Content'];
          
          if (databases.length == 0)
          return;
          
          // for each field we have to assign the value!
          this.model.assign(databases[0]);
        },
        err => { alert(err);}
      )
    }
    
    
  }
  