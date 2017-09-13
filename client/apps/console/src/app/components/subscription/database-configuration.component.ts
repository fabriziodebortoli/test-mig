import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { ModelService } from 'app/services/model.service';

@Component({
  selector: 'app-database-configuration',
  templateUrl: './database-configuration.component.html',
  styleUrls: ['./database-configuration.component.css']
})

export class DatabaseConfigurationComponent implements OnInit {
  
  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
   }
  
  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
  }
  
  //--------------------------------------------------------------------------------------------------------
  configureDatabase(automatic: boolean) {
    
    if (!automatic) {
      // route to add database
      this.router.navigate(['/database'], { queryParamsHandling: "preserve" } );
      return;
    }
    
    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
    if (subscriptionKey === undefined)
      return;
    
    let subs = this.modelService.quickConfigureDatabase(subscriptionKey).
      subscribe(
        result => {
          console.log('*** configureDatabase result: ' + result.Message);
          
          // route to subscription page with same query param
          this.router.navigate(['/subscription'], { queryParamsHandling: "preserve" } );
    
          subs.unsubscribe();
        },
        error => {
          console.log('*** configureDatabase error: ' + error);
          subs.unsubscribe();
        }
    )

  }
}
