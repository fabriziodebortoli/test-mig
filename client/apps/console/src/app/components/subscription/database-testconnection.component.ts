import { SubscriptionDatabase } from './../../model/subscriptionDatabase';
import { DatabaseService } from './../../services/database.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit, Input } from '@angular/core';
import { DatabaseCredentials } from '../../authentication/credentials';
import { ModelService } from 'app/services/model.service';

@Component({
  selector: 'app-database-testconnection',
  templateUrl: './database-testconnection.component.html',
  styleUrls: ['./database-testconnection.component.css']
})

export class DatabaseTestconnectionComponent implements OnInit {
  
  @Input() subDBModel: SubscriptionDatabase;
  
  dbCredentials: DatabaseCredentials;
  subscriptionKey: string;
  
  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private databaseService: DatabaseService, private route: ActivatedRoute) { 
    
    this.dbCredentials = new DatabaseCredentials();
  }
  
  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.subscriptionKey = this.route.snapshot.queryParams['subscriptionToEdit'];
  }
  
  //--------------------------------------------------------------------------------------------------------
  onKeyUp(event) {
    // if I press Enter I call testConnection method
    if (event.keyCode == 13) {
      this.testConnection();  
    }
  }

  //--------------------------------------------------------------------------------------------------------
  testConnection() {
    
    if (this.dbCredentials.Provider == '' || this.dbCredentials.Server == '' || this.dbCredentials.Login == '') {
      alert('Check credentials first!');
      return;
    }
    
    let subs = this.modelService.testConnection(this.subscriptionKey, this.dbCredentials).
    subscribe(
      result => {
        if (result.Result) {
          this.databaseService.dbCredentials = this.dbCredentials;
          this.databaseService.testConnectionOK = true;

          // init provider
          this.subDBModel.Provider = this.dbCredentials.Provider;
        }
        else
          alert('Unable to connect! ' + result.Message);
        
        subs.unsubscribe();
      },
      error => {
        subs.unsubscribe();
      }
    );
  }
}
