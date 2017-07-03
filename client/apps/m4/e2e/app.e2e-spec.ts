import { M4Page } from './app.po';

describe('m4 App', () => {
  let page: M4Page;

  beforeEach(() => {
    page = new M4Page();
  });

  it('should display welcome message', done => {
    page.navigateTo();
    page.getParagraphText()
      .then(msg => expect(msg).toEqual('Welcome to app!!'))
      .then(done, done.fail);
  });
});
