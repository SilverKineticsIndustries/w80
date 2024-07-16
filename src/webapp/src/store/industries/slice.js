import { createSlice } from "@reduxjs/toolkit";

export const industries = createSlice({
    name: "industries",
    initialState: {
        dict: {},
    },
    reducers: {
        query: (state, action) => {
            state.dict = Object.fromEntries(action.payload.map(x => [x.value, x]));
        }
    }
});

export default industries.reducer;