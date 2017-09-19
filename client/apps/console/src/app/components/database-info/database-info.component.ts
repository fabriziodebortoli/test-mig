import { SubscriptionDatabase } from './../../model/subscriptionDatabase';
import { Component, OnInit, Input } from '@angular/core';
import { DatabaseService } from '../../services/database.service';

@Component({
  selector: 'app-database-info',
  templateUrl: './database-info.component.html',
  styleUrls: ['./database-info.component.css']
})

export class DatabaseInfoComponent implements OnInit {

  @Input() subDBModel: SubscriptionDatabase;
  @Input() isDMS: boolean;

  databaseType: string;

   //--------------------------------------------------------------------------------------------------------
   constructor(private databaseService: DatabaseService) { 
   }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    
    this.databaseType = this.isDMS ? 'DMS' : 'ERP';

    // I initialize server names with the one specified in the testconnection 

    if (this.databaseService.needsAskCredentials)
      this.subDBModel.DBServer = this.subDBModel.DMSDBServer = this.databaseService.dbCredentials.Server;
  }
}
