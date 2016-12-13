import { browser, element, by } from 'protractor';

export class TBCorePage {
  navigateTo() {
    return browser.get('/');
  }

  getParagraphText() {
    return element(by.css('tb-root h1')).getText();
  }
}
