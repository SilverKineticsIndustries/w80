import React, { useEffect, useState, useReducer, useContext, memo } from 'react';
import dayjs from "dayjs";
import "dayjs/locale/de";
import { createUseStyles } from 'react-jss';
import { get } from '../../helpers/api';
import { getUserProfile, updateUserProfile } from '../../services/userService';
import ValidationPanel from '../../common/ValidationPanel';
import ModalWrapper from '../../common/ModalWrapper';
import { sortByName, onUpdateField } from '../../helpers/common';
import { setBrowserNotificationsEnabledFlag } from '../../helpers/users';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api';
import { StatusContext } from '../../App';
import { useTranslation } from 'react-i18next';
import MaxLength from '../../common/MaxLength';

const styles = createUseStyles({
    fieldSetContainer: {
        border: "none"
    },
    labelText: {
        display: "inline-block",
        textAlign: "right",
        width: "160px",
        marginRight: "8px",
        cursor: "pointer"
    },
    inputControl: {
        width: "250px",
    },
    formHeader: {
        textAlign: "center",
        borderBottom: "1px solid var(--semi-dark)",
        paddingBottom: "6px"
    },
    formGrid: {
        display: "flex",
        flexDirection: "column",
        "& > div": {
            margin: "4px"
        }
    }
})

const userProfileReducer = (state, action) => {
    switch (action.type) {
        case 'load':
            return action.payload;
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

async function requestNotificationPermissions(user) {

    if (!user.enableAppointmentBrowserNotifications)
       return;

    if (!Notification.permission || Notification.permission !== "granted")
        await Notification.requestPermission();
}

const UserProfile = ({onClose}) =>
{
    const classes = styles();
    const [cultures, setCultures] = useState([]);
    const [timezones, setTimezones] = useState([]);
    const [validationErrors, setValidationErrors] = useState([]);
    const { t, i18n } = useTranslation(null, { keyPrefix: "user-profile"});
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);

    const [userProfile, setUserProfile] = useReducer(userProfileReducer, {
        email: "",
        nickname: "",
        timeZone: "",
        culture: "",
        enableAppointmentEmailNotifications: false,
        enableAppointmentBrowserNotifications: false
    });

    const onCancel = (e) => {
        e.preventDefault();
        onClose();
    };

    const onFieldChange = (e) => {
        onUpdateField(e, setUserProfile);
    }

    const onUpdate = (e) => {
        e.preventDefault();
        setBrowserNotificationsEnabledFlag(userProfile.enableAppointmentBrowserNotifications);
        requestNotificationPermissions(userProfile)
        .then(() => {
            apiDirectDecorator(
                async () => await updateUserProfile(userProfile),
                apiDecoratorOptions(
                    { setLoading, setServerErrorMessage },
                    () => {
                        const updated = userProfile.culture?.split('-')[0];
                        i18n.changeLanguage(updated);
                        dayjs.locale(updated);
                        onClose();
                    },
                    (valErrors) => setValidationErrors(valErrors),
                    e.target
                ))()
        })
    }

    useEffect(() => {
        apiDirectDecorator(
            async () => await getUserProfile(),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, (data) => setUserProfile({ type: "load", payload: data })))();

        apiDirectDecorator(
            async () => await get("/options/cultures"),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, (data) => setCultures(data.sort(sortByName))))();

        apiDirectDecorator(
            async () => await get("/options/timezones"),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, (data) => setTimezones(data.sort(sortByName))))();
    },[setLoading, setServerErrorMessage]);

    return (
        <ModalWrapper>
            <fieldset className={classes.fieldSetContainer}>
                <div className={classes.formHeader}>
                    {t("user-profile")}
                </div>
                <form>
                    <div className={classes.formGrid}>
                        <div>
                            <label>
                                <div className={classes.labelText}>{t("email")}:</div>
                                <input name="email" type="text" required={true} value={userProfile.email || ""} data-test="profile-email"
                                    onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                                    <MaxLength val={userProfile.email} max={100} />
                            </label>
                        </div>
                        <div>
                            <label>
                                <div className={classes.labelText}>{t("nickname")}:</div>
                                <input name="nickname" type="text" value={userProfile.nickname || ""} data-test="profile-nickname"
                                    onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                                    <MaxLength val={userProfile.email} max={50} />
                            </label>
                        </div>
                        <div>
                            <label>
                                <div className={classes.labelText}>{t("password")}:</div>
                                <input name="password" type="password" required={false} value={userProfile.password || ""} data-test="profile-password"
                                    onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                            </label>
                        </div>
                        <div>
                            <label>
                                <div className={classes.labelText}>{t("timezone")}:</div>
                                <select name="timeZone" required={true} value={userProfile.timeZone || ""} data-test="profile-timezone"
                                    onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                        {timezones.map((t) =>
                                            <option value={t.value} key={t.value}>{t.name}</option>
                                        )}
                                </select>
                            </label>
                        </div>
                        <div>
                            <label>
                                <div className={classes.labelText}>{t("culture")}:</div>
                                <select name="culture" required={true} value={userProfile.culture || ""} data-test="profile-culture"
                                    onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                        {cultures.map((t) =>
                                            <option value={t.value} key={t.value}>{t.name}</option>
                                        )}
                                </select>
                            </label>
                        </div>
                        <div>
                            <fieldset>
                                <legend>{t("appointment-notifications")}</legend>
                                <div className="center">
                                    <label>
                                        <span className={classes.labelText}>{t("email-notifications")}:</span>
                                        <input type="checkbox" checked={userProfile.enableAppointmentEmailNotifications || false} data-test="profile-enable-appointment-email-notifications"
                                            name="enableAppointmentEmailNotifications" onChange={onFieldChange} autoComplete="false" />
                                    </label>
                                </div>
                                <div className="center">
                                    <label>
                                        <span className={classes.labelText}>{t("browser-notifications")}:</span>
                                        <input type="checkbox" checked={userProfile.enableAppointmentBrowserNotifications || false} data-test="profile-enable-appointment-browser-notifications"
                                            name="enableAppointmentBrowserNotifications" onChange={onFieldChange} autoComplete="false" />
                                    </label>
                                </div>
                            </fieldset>
                        </div>
                        {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                        <div className="editor-buttons-container">
                            <button className="editor-button" onClick={onUpdate} data-test="profile-save">{t("save")}</button>
                            <button className="editor-button" onClick={onCancel} data-test="profile-cancel">{t("close")}</button>
                        </div>
                    </div>
                </form>
            </fieldset>
        </ModalWrapper>
    )
}

export default memo(UserProfile);