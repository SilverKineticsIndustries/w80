before(() => {
    cy.cloneDb();
});

after(() => {
    cy.restoreDb();
});

beforeEach(function () {

    if (this.currentTest._testConfig.unverifiedTestConfig.skipBeforeEach)
        cy.log("Skipping beforeEach ...")
    else
    {
        const email = Cypress.env("USER_EMAIL");
        const password = Cypress.env("USER_PASSWORD");
        cy.loginAsUser(email, password);
    }
});

Cypress.Commands.add('showUserMenu', () => {

    cy.sel("header-expandermenu")
      .invoke('show')
});

Cypress.Commands.add('loginAsUser', (email, password) => {

    //TODO: We are getting errors that session already exists, need to read up more on how to use sessions
    //cy.session([email, password], () => {
        cy.visit('/')

        cy.get('[data-test="login-already-member"]')
          .check();

        cy.get('[data-test="login-email"]')
          .type(email);

        cy.get('[data-test="login-password"]')
          .type(password, { log: false });

        cy.get('[data-test="login-login"]')
         .click();

        cy.location().should((loc) => {
           expect(loc.hash).to.eq('#/open')
        });
    //});
});

Cypress.Commands.add('logout', () => {

    cy.showUserMenu();

    cy.get('[data-test="header-logout"]')
      .click();
});

Cypress.Commands.add('sel', (selector, ...args) => {
    return cy.get(`[data-test="${selector}"]`, ...args)
});

Cypress.Commands.add('cloneDb', () => {
    cy.task("cloneDb");
});

Cypress.Commands.add('restoreDb', () => {
    cy.task("restoreDb");
});