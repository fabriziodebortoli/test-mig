import { WebSandboxPage } from './app.po';

describe('web-sandbox App', () => {
  let page: WebSandboxPage;

  beforeEach(() => {
    page = new WebSandboxPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
