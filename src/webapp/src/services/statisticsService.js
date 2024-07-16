import { get } from "../helpers/api"

function getStatistics(id = "") {
    if (id)
        return get("/statistics?userId=" + id)
    else
        return get("/statistics");
}

export { getStatistics }