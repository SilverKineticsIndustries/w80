import { createSelector } from '@reduxjs/toolkit'

export const selectDict = state => state.industries.dict;
export const selectIndusties = createSelector(
  [selectDict],
  (industriesDict) => Object.values(industriesDict)
);