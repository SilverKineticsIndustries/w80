import React, { useRef, memo } from "react";
import { useSelector } from "react-redux";
import { useDispatch } from "react-redux";
import { useTranslation } from 'react-i18next';
import { markSent } from "../../store/applications/thunks";
import { getBrowserNotificationsEnabledFlag } from "../../helpers/users";
import { numbOfMinutesFromNowToDate } from "../../helpers/dates";
import { apiDispatchDecorator, apiDecoratorOptions } from "../../helpers/api";
import { selectCalendarAppointmentsForApplications, selectListOfApplicationIdAndApplicationCompany} from "../../store/applications/selectors";

const icon = process.env.PUBLIC_URL + "/favicon.ico";
const checkInterval = Number(process.env.REACT_APP_NOTIFICATION_CHECK_IN_MILLISECONDS);
const alertThresholdInMinutes = Number(process.env.REACT_APP_BROWSER_NOTIFICATIONS_THRESHOLD_IN_MINUTES);

const Notifications = () =>
{
    const interval = useRef(null);
    const dispatch = useDispatch();
    const { t } = useTranslation();
    const title = t('alert');

    const appointments = useSelector(selectCalendarAppointmentsForApplications);
    const appIdAndCompanyNameList = useSelector(selectListOfApplicationIdAndApplicationCompany);

    if (getBrowserNotificationsEnabledFlag()
        && checkInterval
        && checkInterval > 1000
        && Notification.permission
        && Notification.permission === "granted"
        && appointments
        && appointments.length > 0)
    {
        const run = (events) =>
        {
            const toUpdate = {};
            events.forEach((appointment) => {
                if (!appointment.browserNotificationSent)
                {
                    const diffMinutes = numbOfMinutesFromNowToDate(appointment.startDateTimeUTC);
                    const absMinutes = Math.abs(diffMinutes);
                    if (diffMinutes < 0 && absMinutes < alertThresholdInMinutes)
                    {
                        const appId = appointment.applicationId;
                        const companyName = appIdAndCompanyNameList.find(x => x.id === appId).companyName ?? t('notifications.unknown-company');
                        const body = t('notifications.have-appointment', { companyName: companyName, minutes: Math.floor(absMinutes) });
                        new Notification(title, { body, icon });
                        toUpdate[appId] = [...(toUpdate[appId] || []), appointment.id];
                    }
                }
            });

            if (Object.keys(toUpdate).length > 0) {
                dispatch(apiDispatchDecorator(
                    async (dispatch, getState) => await markSent(dispatch, getState, toUpdate),
                    apiDecoratorOptions())
                );
            }
        }

        if (interval.current)
            clearInterval(interval.current);

        interval.current = setInterval(function (){
            try {
                run(appointments);
            } catch (e) {
                console.log(e);
            }
        }, checkInterval)
    }

    return (
        <React.Fragment/>
  );
}

export default memo(Notifications);

