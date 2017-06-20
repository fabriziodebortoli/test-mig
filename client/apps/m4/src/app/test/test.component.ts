import { Component, OnInit } from '@angular/core';
import { Logger } from '@taskbuilder/core';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.scss']
})
export class TestComponent implements OnInit {

  constructor(private logger: Logger) {
    logger.warn('test-component');
  }

  ngOnInit() {

  }

}
