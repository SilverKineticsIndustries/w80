describe('Login', () => {
    it('User can login with correct credentials', () => {

      cy.get("[data-test='greeter-useridentifier']")
        .should("contain", Cypress.env("USER_EMAIL"))

      cy.window()
        .its("sessionStorage")
        .invoke("getItem", "at")
        .should("exist");

    });

    it('User cannot login with incorrect credentials', { skipBeforeEach: true }, () => {

      cy.visit("/")

      cy.get('[data-test="login-already-member"]')
          .check();

      cy.get('[data-test="login-email"]')
        .type("invalidemail");

      cy.get('[data-test="login-password"]')
        .type("invalidpassword");

      cy.get('[data-test="login-login"]')
        .click();

      cy.get("[data-test='error-details']")
        .should("contain", "Invalid credentials.");
    });
});