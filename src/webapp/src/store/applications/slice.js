import { createSlice } from '@reduxjs/toolkit';

export const applications = createSlice({
    name: "applications",
    initialState: {
        dict: {},
    },
    reducers: {
        create: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        upsert: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        query: (state, action) => {
            state.dict = Object.fromEntries(action.payload.map(x => [x.id, x]));
        },
        refresh: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        deactivate: (state, action) => {
            const id = action.payload.id;
            if (state.dict[id])
                delete state.dict[id];
        },
        clear: (state, action) => {
            if (state.dict[action.payload])
                delete state.dict[action.payload]
        },
        archive: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        reject: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        accept: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        unarchive: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        changeState: (state, action) => {
            var updatedState = action.payload.states.find(y => y.isCurrent);
            state.dict[action.payload.id].states.find(y => y.id === updatedState.id).isCurrent = true;
            state.dict[action.payload.id].states.find(y => y.isCurrent === true).isCurrent = false;
        },
        updateCalendarAppointments: (state, action) => {
            state.dict[action.payload.id] = action.payload;
        },
        marksent: (state, action) => {
            for(const appId in action.payload)
            {
                const appointments = state.dict[appId].appointments;
                action.payload[appId].forEach((i) => {
                    const match = appointments.find(x => x.id === i);
                    if (match)
                        match.browserNotificationSent = true;
                });
            }
        }
    }
})
export default applications.reducer;
