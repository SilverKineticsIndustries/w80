import React, { useState, useReducer, useContext, memo, useCallback } from 'react';
import { createUseStyles } from 'react-jss';
import ModalWrapper from '../../common/ModalWrapper';
import ValidationPanel from '../../common/ValidationPanel';
import {  useDispatch } from 'react-redux';
import { rejectApplication } from '../../store/applications/thunks';
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

const rejectionReducer = (state, action) => {
    switch (action.type) {
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

const RejectionModal = ({applicationId, onClose}) =>
{
    const classes = styles();
    const dispatch = useDispatch();
    const [validationErrors, setValidationErrors] = useState([]);
    const { t } = useTranslation(null, { keyPrefix: "application" });
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);

    const [rejection, setRejection] = useReducer(rejectionReducer, {
        method: "",
        reason: "",
        responseText: "",
    });

    const onFieldChange = useCallback((e) => {
        onUpdateField(e, setRejection);
    },[setRejection]);

    const onCancel = useCallback((e) => {
        e.preventDefault();
        onClose();
    },[onClose]);

    const onReject = useCallback((e) => {
        e.preventDefault();
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await rejectApplication(dispatch, getState, applicationId, rejection),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => onClose(),
                (err) => setValidationErrors(err),
                t.target)));
    },[onClose, t.target, dispatch, applicationId, rejection, setLoading, setServerErrorMessage]);

    return (
        <ModalWrapper>
            <fieldset className={classes.fieldSetContainer}>
                <div className={classes.formHeader}>
                    {t("rejection-information")}
                </div>
                <form>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("rejection-method")}:</div>
                            <select name="method" value={rejection.method} onChange={onFieldChange} required={true}
                                className={classes.inputControl} autoFocus={true} autoComplete="false">
                                <option key={0} value=''>{t("rejection-select-method")}</option>
                                <option key={1} value='Email'>{t("rejection-method-email")}</option>
                                <option key={2} value='Phone'>{t("rejection-method-phone")}</option>
                                <option key={3} value='InPerson'>{t("rejection-method-in-person")}</option>
                            </select>
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("rejection-reason")}:</div>
                            <textarea name="reason" type="text" required={true} value={rejection.reason}
                                onChange={onFieldChange} className={classes.inputControl} autoComplete="false"
                                rows={4} />
                        </label>
                    </div>
                    <div className={classes.fieldContainer}>
                        <label>
                            <div className={classes.labelText}>{t("rejection-response-text")}:</div>
                            <textarea name="responseText" type="text" required={true} value={rejection.responseText}
                                onChange={onFieldChange} className={classes.inputControl} autoComplete="false"
                                rows={10} />
                        </label>
                    </div>
                    {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                    <div className="center">
                        <button className='editor-button' onClick={onReject}>{t("reject")}</button>
                        <button className='editor-button' onClick={onCancel}>{t("cancel")}</button>
                    </div>
                </form>
            </fieldset>
        </ModalWrapper>
    )
}

export default memo(RejectionModal);