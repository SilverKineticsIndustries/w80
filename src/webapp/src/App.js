import React, { useState, useEffect, createContext } from 'react';
import { createUseStyles } from 'react-jss';
import { Routes, Route } from 'react-router-dom';
import Header from './common/Header';
import Login from './components/Login/Login';
import LeftMenu from './common/LeftMenu';
import OpenApplicationList from './components/Applications/OpenApplicationList';
import Statistics from './components/Statistics/Statistics';
import ArchivedApplicationList from './components/Applications/ArchivedApplicationList';
import RejectedApplicationList from './components/Applications/RejectedApplicationList';
import Calendar from './components/Calendar/Calendar';
import { getUserFromAccessToken } from './helpers/accessTokensStorage';
import RequireAuthRoute from './common/RequireAuthRoute';
import { queryApplicationsForUser } from './store/applications/thunks';
import { queryIndustries } from './store/industries/thunks';
import { useDispatch } from 'react-redux';
import StatusPanel from './common/StatusPanel';
import { apiDispatchDecorator, apiDecoratorOptions } from './helpers/api';
import AcceptedApplicationList from './components/Applications/AcceptedApplicationList';
import { useTranslation } from 'react-i18next';
import Notifications from './components/Notifications/Notifications';
import { getUserCulture } from './helpers/common';

const UserContext = createContext();
const StatusContext = createContext();

const styles = createUseStyles({
  appBody: {
    display: "flex",
    "@media (max-width: 720px)": {
      flexWrap: "wrap",
      justifyContent: "center"
    }
  },
  appContent: {
    width: "100%"
  }
})

export default function App()
{
  const classes = styles();
  const dispatch = useDispatch();
  const { i18n } = useTranslation();
  const [loading, setLoading] = useState(0);
  const [serverErrorMessage, setServerErrorMessage] = useState();
  const [currentUser, setCurrentUser] = useState(getUserFromAccessToken());

  useEffect(() => {
    if (currentUser)
    {
      var culture = getUserCulture();
      i18n.changeLanguage(culture);

      dispatch(apiDispatchDecorator(
        async (dispatch, getState) => await queryApplicationsForUser(dispatch, getState, currentUser.id),
        apiDecoratorOptions({ setLoading, setServerErrorMessage }))
      );
      dispatch(apiDispatchDecorator(
        async (dispatch, getState) => await queryIndustries(dispatch, getState),
        apiDecoratorOptions({ setLoading, setServerErrorMessage }))
      );
    }
  },[currentUser, dispatch, i18n]);

  return (
    <StatusContext.Provider value={{loading, setLoading, serverErrorMessage, setServerErrorMessage}}>
      <UserContext.Provider value={{currentUser, setCurrentUser}}>
        <div>
          <StatusPanel />
          {currentUser && <Header /> }
          {currentUser && <Notifications /> }
          <div className={classes.appBody}>
            {currentUser && <LeftMenu /> }
            <div className={classes.appContent}>
              <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/open" element={<RequireAuthRoute><OpenApplicationList/></RequireAuthRoute>} />
                <Route path="/calendar" element={<RequireAuthRoute><Calendar/></RequireAuthRoute>} />
                <Route path="/archived" element={<RequireAuthRoute><ArchivedApplicationList/></RequireAuthRoute>} />
                <Route path="/accepted" element={<RequireAuthRoute><AcceptedApplicationList/></RequireAuthRoute>} />
                <Route path="/rejected" element={<RequireAuthRoute><RejectedApplicationList/></RequireAuthRoute>} />
                <Route path="/statistics" element={<RequireAuthRoute><Statistics/></RequireAuthRoute>} />
              </Routes>
            </div>
          </div>
        </div>
      </UserContext.Provider>
    </StatusContext.Provider>
  );
}

export { UserContext, StatusContext }