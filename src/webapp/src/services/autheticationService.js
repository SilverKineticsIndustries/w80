import { get, post } from "../helpers/api"

export function generateInvitationCode(email) {
    return get("/authentication/invitation/generate?email=" + encodeURIComponent(email));
}

export function resendEmailVerificationCode(email) {
    return get("/authentication/emailConfirmation/resend?email=" + encodeURIComponent(email));
}

export function verifyEmailVerificationCode(code) {
    return post("/authentication/emailConfirmation", { "code": code}, true);
}

export function login(email, password, captcha=null) {
    return post("/authentication/login", { email: email, password: password, captcha: captcha}, true);
}

export function processInvitation(invitationCode, email, password) {
    return post("/authentication/invitation", { invitationCode: invitationCode, email: email, password: password}, true);
}