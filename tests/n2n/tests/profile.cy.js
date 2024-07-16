describe('Profile', () => {

    it('User can update nickname', () => {

        const newNickname = new Date().getTime();

        cy.visit('/');
        cy.showUserMenu();

        cy.sel("header-showprofile")
          .click();

        cy.sel("profile-email")
          .should('have.value', Cypress.env("USER_EMAIL"));

        cy.sel("profile-nickname")
          .clear()
          .type(newNickname);

        cy.sel("profile-save")
          .click()

        cy.showUserMenu()
          .sel("header-showprofile")
          .click();

        cy.sel("profile-nickname")
          .should('have.value', newNickname);
    })

    it('User cannot update password with same password', () => {

        cy.visit('/');

        cy.showUserMenu();

        cy.sel("header-showprofile")
          .click();

        cy.sel("profile-password")
          .type(Cypress.env("USER_PASSWORD"));

        cy.sel("profile-save")
          .click()

        cy.sel("validation-item-text")
          .first()
          .should("have.text", "New password is same as current password.");
    })
});
