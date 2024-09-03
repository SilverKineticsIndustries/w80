import { createSelector } from '@reduxjs/toolkit'

const selectApplicationsDict = state => state.applications.dict;

export const selectListOfApplicationIdAndApplicationCompany = createSelector(
  [selectApplicationsDict],
  (applicationsDict) => Object.values(applicationsDict)
    .map(({ companyName, id }) => ({ companyName, id })),
);

export const selectCalendarAppointmentsForApplications = createSelector(
  [selectApplicationsDict],
  (applicationsDict) => Object.values(applicationsDict)
    .reduce((arr, curr) => arr.concat(curr.appointments.map((x) => ({...x, "startDateTimeUTC": new Date(x.startDateTimeUTC), "endDateTimeUTC": new Date(x.endDateTimeUTC), "applicationId": curr.id}) )), [])
);

export const selectNewlyAddedApplication = createSelector(
    [selectApplicationsDict],
    (applicationsDict) => Object.values(applicationsDict).find(x => x.isNew)
);

export const selectApplicationById = (state, id) => state.applications.dict[id];

const _selectCurrentApplicationIds = (state, selection, filter) =>
{
    let items = Object.values(state.applications.dict).filter(selection);
    if (filter.term)
        items = items.filter(filter.term);

    if (filter.sort)
        return items.sort(filter.sort).map(x => x.id);
    else
        return items.map(x => x.id);
}

export const selectAcceptedApplicationIds = (state, filter) => {
    return _selectCurrentApplicationIds(state, (x) => x.acceptance?.acceptedUTC && !x.deactivatedUTC, filter)
}

export const selectOpenApplicationIds = (state, filter) => {
    return _selectCurrentApplicationIds(state, (x) => !x.rejection?.rejectedUTC && !x.acceptance?.acceptedUTC && !x.deactivatedUTC && !x.archivedUTC, filter)
}

export const selectArchivedApplicationIds = (state, filter) => {
    return _selectCurrentApplicationIds(state, (x) => !x.deactivatedUTC && x.archivedUTC, filter)
}

export const selectRejectedApplicationIds = (state, filter) => {
    return _selectCurrentApplicationIds(state, (x) => x.rejection.rejectedUTC && !x.deactivatedUTC && !x.archivedUTC, filter)
}