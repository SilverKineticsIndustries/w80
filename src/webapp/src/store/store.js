import { configureStore } from '@reduxjs/toolkit';
import applicationReducer from './applications/slice';
import industryReducer from './industries/slice';

export default configureStore({
    reducer: {
        applications: applicationReducer,
        industries: industryReducer
    }
})

