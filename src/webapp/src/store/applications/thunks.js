import { get, post } from '../../helpers/api';

export async function createNewApplication(dispatch, getState, init = () => {}) {
    const response = await get('/application/create');
    response.data.isNew = true;
    init(response.data);
    dispatch({ type: 'applications/create', payload: response.data });
}

export async function refreshApplication(dispatch, getState, id) {
    const response = await get(`/application/?id=${id}`);
    dispatch({ type: 'applications/refresh', payload: response.data });
}

export async function queryApplicationsForUser(dispatch, getState, userId) {
    const response = await get(`/application/foruser?userId=${userId}`);
    dispatch({ type: 'applications/query', payload: response.data });
}

export async function upsertApplication(dispatch, getState, app ) {
    const response = await post('/application', app);
    if (app.isNew) delete app.isNew;
    dispatch({ type: 'applications/upsert', payload: response.data });
}

export async function updateApplicationCalendarAppointments(dispatch, getState, app) {
    const response = await post('/application', app);
    if (app.isNew) delete app.isNew;
    dispatch({ type: 'applications/updateCalendarAppointments', payload: response.data });
}

export async function changeApplicationState(dispatch, getState, app) {
    const response = await post('/application', app);
    dispatch({ type: 'applications/changeState', payload: response.data });
}

export async function deactivateApplication(dispatch, getState, id) {
    const response = await post(`/application/deactivate?id=${id}`);
    dispatch({ type: 'applications/deactivate', payload: response.data});
}

export async function rejectApplication(dispatch, getState, id, rejection) {
    const response = await post(`/application/reject?id=${id}`, rejection);
    dispatch({ type: 'applications/reject', payload: response.data});
}

export async function acceptApplication(dispatch, getState, id, acceptance) {
    const response = await post(`/application/accept?id=${id}`, acceptance);
    dispatch({ type: 'applications/accept', payload: response.data});
}

export async function archiveApplication(dispatch, getState, id) {
    const response = await post(`/application/archive?id=${id}`);
    dispatch({ type: 'applications/archive', payload: response.data});
}

export async function unarchiveApplication(dispatch, getState, id) {
    const response = await post(`/application/unarchive?id=${id}`);
    dispatch({ type: 'applications/unarchive', payload: response.data});
}

export async function markSent(dispatch, getState, sentAppEvents) {
    const response = await post(`/application/application/marksent`, sentAppEvents);
    dispatch({ type: 'applications/marksent', payload: response.data});
}
