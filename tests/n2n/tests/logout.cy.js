describe("Logout", () => {
    it("User can logout", () => {

        cy.visit('/');

        cy.logout();

        cy.getCookie('_rt')
          .wrap()
          .should("not.exist");

        cy.wait(5000)
          .then(() => {
            cy.wrap(sessionStorage.getItem("at"))
              .should("not.exist");
          })
    })
});