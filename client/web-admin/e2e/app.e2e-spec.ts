import { WebAdminPage } from './app.po';

describe('web-admin App', function() {
  let page: WebAdminPage;

  beforeEach(() => {
    page = new WebAdminPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
