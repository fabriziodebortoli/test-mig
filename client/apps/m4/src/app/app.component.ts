import { Component } from '@angular/core';

import { Logger } from '@taskbuilder/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';

  constructor(private logger: Logger) {
    logger.error('test');
  }
}
