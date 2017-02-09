import { WebProvisioningPage } from './app.po';

describe('web-provisioning App', function() {
  let page: WebProvisioningPage;

  beforeEach(() => {
    page = new WebProvisioningPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
