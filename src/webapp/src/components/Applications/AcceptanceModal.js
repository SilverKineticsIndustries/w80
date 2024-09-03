import React, { useState, useReducer, useContext, useCallback } from 'react';
import { createUseStyles } from 'react-jss';
import ModalWrapper from '../../common/ModalWrapper';
import ValidationPanel from '../../common/ValidationPanel';
import { useDispatch } from 'react-redux';
import { acceptApplication } from '../../store/applications/thunks';
import { StatusContext } from '../../App';
import { useTranslation } from 'react-i18next';
import { onUpdateField } from '../../helpers/common';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';

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
        width: "250px",
    },
    formHeader: {
        textAlign: "center",
        borderBottom: "1px solid var(--semi-dark)",
        paddingBottom: "6px"
    }
})

const acceptanceReducer = (state, action) => {
    switch (action.type) {
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

export default function AcceptanceModal({applicationId, onClose})
{
    const classes = styles();
    const dispatch = useDispatch();
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const [validationErrors, setValidationErrors] = useState([]);
    const { t } = useTranslation(null, { keyPrefix: "application" });

    const [acceptance, setAcceptance] = useReducer(acceptanceReducer, {
        method: "",
        responseText: "",
        archiveOpenApplications: true
    });

    const onFieldChange = (e) => {
        onUpdateField(e, setAcceptance);
    }

    const onCancel = (e) => {
        e.preventDefault();
        onClose();
    }

    const onAccept = useCallback((e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await acceptApplication(dispatch, getState, applicationId, acceptance),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => onClose(),
                (valErr) => setValidationErrors(valErr),
                e.target))
        );
    },[applicationId, acceptance, dispatch, onClose, setLoading, setServerErrorMessage]);

    return (
        <ModalWrapper>
            <fieldset className={classes.fieldSetContainer}>
                <div className={classes.formHeader}>
                    {t("acceptance-information")}
                </div>
                <form>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("acceptance-method")}:</div>
                            <select name="method" value={acceptance.method} onChange={onFieldChange} required={true}
                                className={classes.inputControl} autoFocus={true} autoComplete="false">
                                <option key={0} value=''>{t("acceptance-select-method")}</option>
                                <option key={1} value='Email'>{t("acceptance-method-email")}</option>
                                <option key={2} value='Phone'>{t("acceptance-method-phone")}</option>
                                <option key={3} value='InPerson'>{t("acceptance-method-in-person")}</option>
                            </select>
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("acceptance-response-text")}:</div>
                            <textarea name="responseText" type="text" required={true} value={acceptance.responseText}
                                onChange={onFieldChange} className={classes.inputControl} autoComplete="false" rows={10} />
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("acceptance-archive-apps")}</div>
                            <input type="checkbox" required={true} value={acceptance.archiveOpenApplications}
                                onChange={onFieldChange} className={classes.inputControl} />
                        </label>
                    </div>
                    {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                    <div className="center">
                        <button className='editor-button' onClick={onAccept}>{t("acceptance-accept")}</button>
                        <button className='editor-button' onClick={onCancel}>{t("acceptance-cancel")}</button>
                    </div>
                </form>
            </fieldset>
        </ModalWrapper>
    )
}