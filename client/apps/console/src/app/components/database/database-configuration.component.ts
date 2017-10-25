import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { ModelService } from 'app/services/model.service';

@Component({
  selector: 'app-database-configuration',
  templateUrl: './database-configuration.component.html',
  styleUrls: ['./database-configuration.component.css']
})

export class DatabaseConfigurationComponent implements OnInit {
  
  isWorking: boolean;
  subscriptionKey: string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
   }
  
  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.isWorking = false;
    this.subscriptionKey = this.route.snapshot.queryParams['subscriptionToEdit'];
  }
  
  //--------------------------------------------------------------------------------------------------------
  configureDatabase(automatic: boolean) {
    
    if (!automatic) {
      // route to add database
      this.router.navigate(['/database'], { queryParamsHandling: "preserve" } );
      return;
    }

    if (this.subscriptionKey === undefined)
      return;

    this.isWorking = true;
      
    let subs = this.modelService.quickConfigureDatabase(this.subscriptionKey).
      subscribe(
        result => {
          console.log('*** configureDatabase result: ' + result.Message);
   
          subs.unsubscribe();
          this.isWorking = false;
        },
        error => {
          console.log('*** configureDatabase error: ' + error);
          subs.unsubscribe();
          this.isWorking = false;
        }
    )
  }
}
