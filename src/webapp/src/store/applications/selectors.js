export const selectApplicationById = (state, id) => state.applications.dict[id];
export const selectNewlyAddedApplication = state => Object.values(state.applications.dict).find(x => x.isNew);
export const selectListOfApplicationIdAndApplicationCompany = state => Object.values(state.applications.dict).map((x) => ({"id": x.id, "companyName": x.companyName}));
export const selectCalendarAppointmentsForApplications = state => Object.values(state.applications.dict).reduce((arr, curr) => arr.concat(curr.appointments.map((x) => ({...x, "startDateTimeUTC": new Date(x.startDateTimeUTC), "endDateTimeUTC": new Date(x.endDateTimeUTC), "applicationId": curr.id}) )), [])

const _selectCurrentApplicationIds = (state, appFilter, sort, termFilter) =>
{
    let items = Object.values(state.applications.dict).filter(appFilter);
    if (termFilter)
        items = items.filter(termFilter);

    if (sort)
        return items.sort(sort).map(x => x.id);
    else
        return items.map(x => x.id);
}

export const selectAcceptedApplicationIds = (state, sort, termFilter) => {
    return _selectCurrentApplicationIds(state, (x) => x.acceptance?.acceptedUTC && !x.deactivatedUTC, sort, termFilter)
}

export const selectCurrentApplicationIds = (state, sort, termFilter) => {
    return _selectCurrentApplicationIds(state, (x) => !x.rejection?.rejectedUTC && !x.acceptance?.acceptedUTC && !x.deactivatedUTC && !x.archivedUTC, sort, termFilter)
}

export const selectArchivedApplicationIds = (state, sort, termFilter) => {
    return _selectCurrentApplicationIds(state, (x) => !x.deactivatedUTC && x.archivedUTC, sort, termFilter)
}

export const selectRejectedApplicationIds = (state, sort, termFilter) => {
    return _selectCurrentApplicationIds(state, (x) => x.rejection.rejectedUTC && !x.deactivatedUTC && !x.archivedUTC, sort, termFilter)
}