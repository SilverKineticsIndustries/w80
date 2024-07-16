describe('Logout', () => {
    it('User can logout', () => {

        cy.visit('/');

        cy.logout();

        cy.get('[data-test="login-email"]')
          .should('be.visible');
    })
});