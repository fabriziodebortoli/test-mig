import { WebSaloonPage } from './app.po';

describe('web-saloon App', () => {
  let page: WebSaloonPage;

  beforeEach(() => {
    page = new WebSaloonPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
