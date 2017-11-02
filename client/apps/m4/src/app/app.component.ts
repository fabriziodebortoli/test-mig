import { Component } from '@angular/core';
import { InfoService } from '@taskbuilder/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  constructor(public infoService: InfoService) { }
}
