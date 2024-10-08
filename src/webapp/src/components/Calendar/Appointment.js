import React, { useState, useReducer, useContext, memo, useCallback } from 'react';
import { createUseStyles } from 'react-jss';
import ModalWrapper from '../../common/ModalWrapper';
import ValidationPanel from '../../common/ValidationPanel';
import { useSelector, useDispatch, useStore } from 'react-redux';
import { selectListOfApplicationIdAndApplicationCompany } from '../../store/applications/selectors';
import { updateApplicationCalendarAppointments } from '../../store/applications/thunks';
import { dateTimeToInputDateTimeLocalString } from '../../helpers/dates';
import { StatusContext } from '../../App';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import { createdValidationError, onUpdateField } from '../../helpers/common';
import { v4 as uuidv4 } from 'uuid';
import { useTranslation } from 'react-i18next';
import MaxLength from '../../common/MaxLength';

const styles = createUseStyles({
    fieldSetContainer: {
        border: "none"
    },
    fieldContainer: {
        margin: "10px",
        whiteSpace: "nowrap"
    },
    labelText: {
        display: "inline-block",
        textAlign: "right",
        width: "150px",
        marginRight: "8px",
        cursor: "pointer",
        verticalAlign: "top"
    },
    inputControl: {
        width: "300px",
    },
    formHeader: {
        textAlign: "center",
        borderBottom: "1px solid var(--semi-dark)",
        paddingBottom: "6px"
    }
})

const appointmentReducer = (state, action) => {
    switch (action.type) {
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

const Appointment = ({selectedAppointment, onClose}) =>
{
    const classes = styles();
    const store = useStore();
    const dispatch = useDispatch();
    const { t } = useTranslation(null, { keyPrefix: "calendar" });
    const [validationErrors, setValidationErrors] = useState([]);
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);

    const appIdAndCompanyNameList = useSelector(selectListOfApplicationIdAndApplicationCompany);
    const [appointment, setAppointment] = useReducer(appointmentReducer, selectedAppointment || {
        id: "",
        applicationId: "",
        description: "",
        startDateTimeUTC: "",
        endDateTimeUTC: ""
    });

    const onFieldChange = useCallback((e) => {
        onUpdateField(e, setAppointment);
    },[setAppointment]);

    const onCancel = useCallback((e) => {
        e.preventDefault();
        onClose();
    },[onClose]);

    const onModify = useCallback((e, isDelete=false) => {
        e.preventDefault();
        setValidationErrors([]);

        if (!appointment.applicationId) {
            setValidationErrors((state) => [...state, createdValidationError(t("application-cannot-be-empty"))]);
            return false;
        }
        if (!appointment.description) {
            setValidationErrors((state) => [...state, createdValidationError(t("description-cannot-be-empty"))]);
            return false;
        }

        const sourceApp = store.getState().applications.dict[appointment.applicationId];
        const app = structuredClone(sourceApp);
        if (!app.appointments)
            app.appointments = [];
        if (!appointment.id)
            appointment.id = uuidv4();

        let match = app.appointments.find((x) => x.id === appointment.id);
        if (!match)
            app.appointments.push(appointment);

        else
        {
            if (!isDelete)
                app.appointments[app.appointments.indexOf(match)] = {...match, ...appointment};
            else
                app.appointments = app.appointments.filter((x) => x.id !== appointment.id);
        }

        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await updateApplicationCalendarAppointments(dispatch, getState, app),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => onClose(),
                (err) => setValidationErrors(err),
                e.target))
        );
    },[appointment, dispatch, onClose, store, t, setLoading, setServerErrorMessage]);

    return (
        <ModalWrapper>
            <fieldset className={classes.fieldSetContainer}>
                <div className={classes.formHeader}>
                    {t("add-edit-appointment")}
                </div>
                <form>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("start")}</div>
                            <span>
                                <input type="datetime-local" value={dateTimeToInputDateTimeLocalString(appointment.startDateTimeUTC)}
                                    onChange={onFieldChange} name="startDateTimeUTC" />
                            </span>
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("end")}</div>
                            <span>
                                <input type="datetime-local" value={dateTimeToInputDateTimeLocalString(appointment.endDateTimeUTC)}
                                    onChange={onFieldChange} name="endDateTimeUTC" />
                            </span>
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("application")}</div>
                            <select name="applicationId" onChange={onFieldChange} className={classes.inputControl}
                                value={appointment.applicationId || ""} autoComplete='false'>
                                <option>{t("select-application")}</option>
                                {
                                    appIdAndCompanyNameList.map((i, idx) =>
                                        <option key={idx} value={i.id}>{i.companyName}</option>
                                    )
                                }
                            </select>
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("appointment-description")}</div>
                            <textarea name="description" type="text" required={true} value={appointment.description || ""}
                                onChange={onFieldChange} className={classes.inputControl} autoComplete='false'
                                autoFocus={true} rows={8} />
                                <MaxLength val={appointment.description} max={200} />
                        </label>
                    </div>
                    {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                    <div className="center">
                        <button className="editor-button" onClick={onModify}>{t("save")}</button>
                        <button className="editor-button" onClick={(e) => onModify(e, true)}>{t("delete")}</button>
                        <button className="editor-button" onClick={onCancel}>{t("cancel")}</button>
                    </div>
                </form>
            </fieldset>
        </ModalWrapper>
    )
}

export default memo(Appointment);