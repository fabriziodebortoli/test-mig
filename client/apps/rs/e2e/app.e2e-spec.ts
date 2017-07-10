import { RsPage } from './app.po';

describe('rs App', () => {
  let page: RsPage;

  beforeEach(() => {
    page = new RsPage();
  });

  it('should display welcome message', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('Welcome to app!!');
  });
});
