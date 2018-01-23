// import { StateButtonComponent } from './../controls/state-button/state-button.component';
import { Directive, AfterContentInit, ViewContainerRef, ComponentFactoryResolver, ComponentRef } from '@angular/core';

import { Logger } from './../../core/services/logger.service';

@Directive({
  selector: '[tbStateButtons]'
})
export class StateButtonDirective implements AfterContentInit {
  public stateButtonsRef: ComponentRef<any>;
  constructor(public vcr: ViewContainerRef, public componentResolver: ComponentFactoryResolver, public logger: Logger) {
    logger.debug(vcr);
  }
  renderComponent() {
    // if (this.stateButtonsRef) this.stateButtonsRef.instance.value = this.value;
  }
  ngAfterContentInit() {

    // this.stateButtonsTarget = (<any>this.vcr)._data.componentView.component.stateButtons;
    // this.logger.debug('_data.componentView', this.stateButtonsTarget);

    // let componentFactory = this.componentResolver.resolveComponentFactory(StateButtonComponent);
    // this.stateButtonsRef = this.stateButtonsTarget.createComponent(componentFactory);
    // this.renderComponent();
  }

  ngOnChanges(changes: Object) {
    this.renderComponent();
  }
}
