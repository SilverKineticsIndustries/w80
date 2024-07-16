import { get } from '../../helpers/api';

export async function queryIndustries(dispatch, getState) {
    const response = await get('/options/industries');
    dispatch({ type: 'industries/query', payload: response.data });
}
