import { sortDatesAsc } from "../helpers/dates";

const SortType = {
    State: 'State',
    CompanyName: 'CompanyName',
    Compensation: 'Compensation',
    NextAppointment: 'NextAppointment',
};

function getSortByNextAppointmentFunc(opts = {}) {
    return (a, b) =>
    {
        const fielda = opts.invertSort ? b.appointments: a.appointments;
        const fieldb = opts.invertSort ? a.appointments: b.appointments;
        const fieldaDT = new Date((fielda || []).length > 0 ? (fielda.sort(sortDatesAsc) || [])[0]?.startDateTimeUTC: 0);
        const fieldbDT = new Date((fieldb || []).length > 0 ? (fieldb.sort(sortDatesAsc) || [])[0]?.startDateTimeUTC: 0);
        if (fieldbDT < fieldaDT)
            return -1;
        if (fieldbDT > fieldaDT)
            return 1;
        return 0;
    }
}

function getSortByCompanyNameFunc(opts = {})  {
    return (a, b) =>
    {
        const fielda = opts.invertSort ? a.companyName : b.companyName;
        const fieldb = opts.invertSort ? b.companyName : a.companyName;
        if (fieldb < fielda)
            return -1;
        if (fieldb > fielda)
            return 1;
        return 0;
    }
}

function getSortByCompensationFunc(opts = {}) {
    return (a, b) =>
    {
        let fielda, fieldb;
        const aHasMin = !!a.compensationMin;
        const aHasMax = !!a.compensationMax;
        const bHasMin = !!b.compensationMin;
        const bHasMax = !!b.compensationMax;

        if (aHasMax && bHasMax)
        {
            fielda = opts.invertSort ? a.compensationMax: b.compensationMax;
            fieldb = opts.invertSort ? b.compensationMax: a.compensationMax;
        }
        else if (aHasMax && bHasMin)
        {
            fielda = opts.invertSort ? a.compensationMax: b.compensationMin;
            fieldb = opts.invertSort ? b.compensationMin: a.compensationMax;
        }
        else if (aHasMin && bHasMax)
        {
            fielda = opts.invertSort ? a.compensationMin: b.compensationMax;
            fieldb = opts.invertSort ? b.compensationMax: a.compensationMin;
        }
        else
        {
            fielda = opts.invertSort ? a.compensationMin: b.compensationMin;
            fieldb = opts.invertSort ? b.compensationMin: a.compensationMin;
        }

        if (fieldb < fielda)
            return -1;
        if (fieldb > fielda)
            return 1;
        return 0;
    }
}

function getSortByStateFunc(opts = {}) {
    return (a, b) =>
    {
        const fielda = opts.invertSort ? a.states: b.states;
        const fieldb = opts.invertSort ? b.states: a.states;
        return ((fielda || []).find(x => x.isCurrent)?.seqNo || 0)
               - ((fieldb || []).find(x => x.isCurrent)?.seqNo || 0);
    }
}

const getSortOrder = (sortOrder, invertSort = false) =>
{
    const opts = { invertSort: invertSort };
    switch (sortOrder) {
        case SortType.NextAppointment:
            return getSortByNextAppointmentFunc(opts);
        case SortType.CompanyName:
            return getSortByCompanyNameFunc(opts);
        case SortType.Compensation:
            return getSortByCompensationFunc(opts);
        default:  // sort by 'state'
            return getSortByStateFunc(opts);
    }
}

const getSearchApplicationsByTermFunc = (term) => {
    return (app) => {
        if (term)
            return (app.role || "").includes(term)
                || (app.companyName || "").includes(term)
                || (app.roleDescription || "").includes(term);
        else
            return app;
    }
}

const getFilter = (json, sort="", term="", invertSort=null) =>
{
    let filter;
    if (typeof json === 'object' && json)
        filter = json;
    else
        filter = json ? JSON.parse(json) : {};
    filter.sort = sort || filter.sort || SortType.State;
    filter.term = term || filter.term;
    filter.invertSort = (invertSort === null || invertSort === undefined) ? filter.invertSort : invertSort;
    filter.sortFunc = getSortOrder(filter.sort, filter.invertSort);
    filter.termFunc = getSearchApplicationsByTermFunc(filter.term);
    return filter;
}

export { getFilter, SortType }