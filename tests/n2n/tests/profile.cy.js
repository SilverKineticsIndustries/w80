describe("Profile", () => {

    it("User can update nickname", () => {

        const newNickname = new Date().getTime();

        cy.visit('/');
        cy.showUserMenu();

        cy.sel("header-showprofile")
          .click();

        cy.sel("profile-email")
          .should("have.value", Cypress.env("USER_EMAIL"));

        cy.sel("profile-nickname")
          .clear()
          .type(newNickname);

        cy.sel("profile-save")
          .click()

        cy.showUserMenu()
          .sel("header-showprofile")
          .click();

        cy.sel("profile-nickname")
          .should("have.value", newNickname);
    })

    it("User can update email", () => {

        const emailParts = Cypress.env("USER_EMAIL").split("@");
        const newEmail = emailParts[0] + "-updated" + "@" + emailParts[1];

        cy.visit("/");
        cy.showUserMenu();

        cy.sel("header-showprofile")
          .click();

        cy.sel("profile-email")
          .clear()
          .type(newEmail);

        cy.sel("profile-save")
          .click()

        cy.showUserMenu()
          .sel("header-showprofile")
          .click();

        cy.sel("profile-email")
          .should("have.value", newEmail);

        cy.sel("profile-email")
          .clear()
          .type(Cypress.env("USER_EMAIL"));

        cy.sel("profile-save")
          .click()
    })

    it("User can update culture", () => {

      cy.visit('/');
      cy.showUserMenu();

      cy.sel("header-showprofile")
        .click();

      cy.sel("profile-culture")
        .select("de-DE")

      cy.sel("profile-save")
        .click()

      cy.showUserMenu()
        .sel("header-showprofile")
        .click();

      cy.sel("profile-culture")
        .should("have.value", "de-DE");

      // Reset back to English so our tests run find (where we need to check content)
      cy.sel("profile-culture")
        .select("en-US")

      cy.sel("profile-save")
        .click()
    })

    it("User can update timezone", () => {

      cy.visit("/");
      cy.showUserMenu();

      cy.sel("header-showprofile")
        .click();

      cy.sel("profile-timezone")
        .select("Africa/Cairo")

      cy.sel("profile-save")
        .click()

      cy.showUserMenu()
        .sel("header-showprofile")
        .click();

      cy.sel("profile-timezone")
        .should("have.value", "Africa/Cairo");
    })

    it("User can set appointment email alerts", () => {

      cy.visit("/");
      cy.showUserMenu();

      cy.sel("header-showprofile")
        .click();

      cy.sel("profile-enable-appointment-email-notifications")
        .check()

      cy.sel("profile-save")
        .click()

      cy.showUserMenu()
        .sel("header-showprofile")
        .click();

      cy.sel("profile-enable-appointment-email-notifications")
        .should("be.checked");
    })

    it("User can set appointment browser alerts", () => {

      cy.visit("/");
      cy.showUserMenu();

      cy.sel("header-showprofile")
        .click();

      cy.sel("profile-enable-appointment-browser-notifications")
        .check()

      cy.sel("profile-save")
        .click()

      cy.showUserMenu()
        .sel("header-showprofile")
        .click();

      cy.sel("profile-enable-appointment-browser-notifications")
        .should("be.checked");
    })

    it("User cannot update password with same password", () => {

        cy.visit("/");

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

    it("User cannot update password with value less then minimum required", () => {

        cy.visit("/");

        cy.showUserMenu();

        cy.sel("header-showprofile")
          .click();

        cy.sel("profile-password")
          .type("abc");

        cy.sel("profile-save")
          .click()

        cy.sel("validation-item-text")
          .first()
          .should("have.text", "Password must be at least 14 characters long.");
    })
});
