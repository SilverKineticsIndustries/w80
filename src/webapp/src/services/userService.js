import { get, post } from "../helpers/api"

export function getUserProfile(id = "") {
    if (id)
        return get("/user/profile/" + id)
    else
        return get("/user/profile");
}

export function updateUserProfile(userProfile) {
    return post("/user/profile/", userProfile);
}

export function getUsers() {
    return get("/user/all")
}

export function upsertUser(user) {
    return post("/user/", user);
}

export function deactivateUser(userId) {
    return post("/user/deactivate?userId=" + userId);
}