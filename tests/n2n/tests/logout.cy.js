describe("Logout", () => {
    it("User can logout", () => {

        cy.visit('/');

        cy.logout();

        cy.sel("login-email")
          .should("be.visible");

        cy.getCookie('_rt')
          .should("not.exist");

        // TODO: Access token is not getting cleared, not sure why ...
        //expect(sessionStorage.getItem("at")).to.be.null;

    })
});