import { DatabaseService } from './../../services/database.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { DatabaseCredentials } from '../../authentication/credentials';
import { ModelService } from 'app/services/model.service';

@Component({
  selector: 'app-database-testconnection',
  templateUrl: './database-testconnection.component.html',
  styleUrls: ['./database-testconnection.component.css']
})

export class DatabaseTestconnectionComponent implements OnInit {
  
  // model
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
  testConnection() {

    if (this.dbCredentials.Server == '' || this.dbCredentials.Login == '') {
      alert('Check credentials first!');
      return;
    }

    let subs = this.modelService.testConnection(this.subscriptionKey, this.dbCredentials).
    subscribe(
      result => {
        if (result.Result) {
          this.databaseService.dbCredentials = this.dbCredentials;
          this.databaseService.testConnectionOK = true;
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
