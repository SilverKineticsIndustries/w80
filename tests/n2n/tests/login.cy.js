describe('Login', () => {

    it('User can login with correct credentials', () => {

      cy.sel("greeter-useridentifier")
        .should("contain", Cypress.env("USER_EMAIL"))

      cy.window()
        .its("sessionStorage")
        .invoke("getItem", "at")
        .should("exist");

    });

    it('User cannot login with incorrect credentials', { skipBeforeEach: true }, () => {

      cy.visit("/")

      cy.sel("login-already-member")
          .check();

      cy.sel("login-email")
        .type("invalidemail");

      cy.sel("login-password")
        .type("invalidpassword");

      cy.sel("login-login")
        .click();

      cy.sel("error-details")
        .should("contain", "Invalid credentials.");
    });
});