describe("Application", () => {
    describe("Creation", () => {
        it("User can create application", () => {

            const appName = "Company Name";

            cy.visit("/");

            cy.sel("application-add-new")
              .click();

            cy.sel("application-edit-company-name")
              .type(appName);

            cy.sel("application-edit-company-role")
              .type("Role");

            cy.sel("application-edit-role-description")
              .type("Role Description");

            cy.sel("application-edit-save")
              .click();

            cy.get("[data-test^='application-view'][data-test$='company-name']")
              .contains(appName)

        });
    });

    describe("State", () => {
        it("User can set application state forward", () => {

            cy.visit("/");

            cy.sel("application-add-new")
              .click();

            cy.get("[data-test^=application-edit-id-]")
              .invoke('attr', 'data-test')
              .then(value =>
              {
                const id = value.replace("application-edit-id-", "");

                cy.sel("application-edit-company-name")
                  .type("Application State Next Test");

                cy.sel("application-edit-company-role")
                  .type("Role");

                cy.sel("application-edit-role-description")
                  .type("Role Description");

                cy.sel("application-edit-save")
                  .click();

                cy.sel(`next-state-${id}-button`)
                  .click();

                cy.sel(`state-label-${id}`)
                  .should('have.attr', 'alt', 'Response Received')
              })
        });
        it("User can set application state backwards", () => {

          cy.visit("/");

          cy.sel("application-add-new")
            .click();

          cy.get("[data-test^=application-edit-id]")
            .invoke('attr', 'data-test')
            .then((value) =>
            {
              const id = value.replace("application-edit-id-", "");

              cy.sel("application-edit-company-name")
                .type("Application State Prev Test");

              cy.sel("application-edit-company-role")
                .type("Role");

              cy.sel("application-edit-role-description")
                .type("Role Description");

              cy.sel("application-edit-save")
                .click();

              cy.sel(`next-state-${id}-button`)
                .click();

              cy.sel(`prev-state-${id}-button`)
                .click();

              cy.sel(`state-label-${id}`)
                .should('have.attr', 'alt', 'Applied');
            });
        });
    });

    describe("Sort", () => {
        it("User application sort persists between logins.", () => {

            cy.visit("/");

            const sortType = 'CompanyName';
            cy.sel('sort-type-filter').select(sortType);
            cy.sel('sort-type-invert-filter').check();

            cy.logout();

            const email = Cypress.env("USER_EMAIL");
            const password = Cypress.env("USER_PASSWORD");
            cy.loginAsUser(email, password);

            cy.sel('sort-type-filter').should('have.value', sortType);
            cy.sel('sort-type-invert-filter').should('be.checked');

        });
    });
});