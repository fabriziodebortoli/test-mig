import { Component, OnInit, Input } from '@angular/core';
import { OperationResult } from '../../services/operationResult';
import { ModelService } from 'app/services/model.service';
import { ExtendedSubscriptionDatabase } from '../../authentication/credentials';
import { Router } from '@angular/router';

@Component({
  selector: 'app-database-operations',
  templateUrl: './database-operations.component.html',
  styleUrls: ['./database-operations.component.css']
})

export class DatabaseOperationsComponent implements OnInit {

  @Input() operations: OperationResult[];
  @Input() readingData: boolean;
  @Input() operationsToDo: boolean;
  @Input() extSubDatabase: ExtendedSubscriptionDatabase;

  isWorking: boolean;

  //-----------------------------------------------------------------------------	
  constructor(private modelService: ModelService, private router: Router, ) {
    this.operations = [];
  }

  //-----------------------------------------------------------------------------	
  ngOnInit() {
    this.isWorking = false;
  }

  //-----------------------------------------------------------------------------	
  confirmDbOperations() {

    if (this.extSubDatabase === undefined) {
      alert('Invalid data');
      return;
    }

    this.isWorking = true;

    let test = this.modelService.testConnection(this.extSubDatabase.Database.SubscriptionKey, this.extSubDatabase.AdminCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {
          let update = this.modelService.updateDatabase(this.extSubDatabase.Database.SubscriptionKey, this.extSubDatabase).
            subscribe(
            updateResult => {

              if (!updateResult.Result) {
                alert(updateResult.Message);
              }
              else
                this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.extSubDatabase.Database.SubscriptionKey } });

              update.unsubscribe();
            },
            updateError => {
              console.log(updateError);
              alert(updateError);
              update.unsubscribe();
            }
            )
        }
        else
          alert('Unable to connect! ' + testResult.Message);

        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );
  }
}
