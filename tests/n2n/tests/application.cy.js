describe("Application", () => {

    it("User can create application", () => {

        cy.visit("/");

        cy.sel("application-add-new")
           .click();

        cy.sel("application-edit-company-name")
          .type("Company Name");

        cy.sel("application-edit-company-role")
          .type("Role");

        cy.sel("application-edit-role-description")
          .type("Role Description");

        cy.sel("application-edit-save")
          .click();

        cy.get("[data-test^='application-view'][data-test$='company-name']")
          .contains("Company Name")

    });
});