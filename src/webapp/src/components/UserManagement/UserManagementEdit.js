import React, { useReducer, useEffect, useState, useContext, useCallback, memo } from 'react'
import { upsertUser } from '../../services/userService'
import { createUseStyles } from 'react-jss'
import { get } from '../../helpers/api';
import { StatusContext } from '../../App';
import { sortByName, onUpdateField } from '../../helpers/common';
import { useTranslation } from 'react-i18next';
import ValidationPanel from '../../common/ValidationPanel';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api';
import MaxLength from '../../common/MaxLength';

const styles = createUseStyles({
    wrapper: {
        marginBottom: "10px",
        textAlign: "center"
    },
    editorLabel: {
        width: "50px",
        marginRight: "6px",
        display: "inline-block",
        textAlign: "right"
    },
    editorInputContainer: {
        width: "200px"
    },
    inputControl: {
        width: "250px"
    },
    editorButtons: {
        marginTop: "8px",
        textAlign: "center"
    },
    fieldContainer: {
        margin: "10px"
    },
    labelText: {
        display: "inline-block",
        textAlign: "right",
        width: "150px",
        marginRight: "8px",
        cursor: "pointer"
    }
})

const userReducer = (state, action) => {
    switch (action.type) {
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

const UserManagementEdit = ({user, onPostUpdate, onCancel}) =>
{
    const classes = styles();
    const [cultures, setCultures] = useState([]);
    const [timezones, setTimezones] = useState([]);
    const [validationErrors, setValidationErrors] = useState([]);
    const { t } = useTranslation(null, { keyPrefix: "user-management" });
    const [userItem, setUserItem] = useReducer(userReducer, structuredClone(user));
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);

    const onFieldChange = useCallback((e) => {
        onUpdateField(e, setUserItem);
    },[setUserItem]);

    useEffect(() => {
        apiDirectDecorator(
            async () => await get('/options/cultures'),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, (data) => setCultures(data.sort(sortByName))))();

        apiDirectDecorator(
            async () => await get('/options/timezones'),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, (data) => setTimezones(data.sort(sortByName))))();
    },[user, setLoading, setServerErrorMessage]);

    const onUpsertClick = useCallback((e) => {
        e.preventDefault();
        apiDirectDecorator(
            async () => await upsertUser(userItem),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                (data) => onPostUpdate(data),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    },[onPostUpdate, setValidationErrors, userItem, setLoading, setServerErrorMessage]);

    return (
        <div className={classes.wrapper}>
            <form>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("role")}:</div>
                        <select name="role" required={true} value={userItem.role || ""} data-test="user-management-role"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                <option value='User' key='User'>{t("user-role")}</option>
                                <option value='Administrator' key='Administrator'>{t("admin-role")}</option>
                        </select>
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("email")}:</div>
                        <input name="email" type="text" required={true} value={userItem.email || ""} data-test="user-management-email"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                            <MaxLength val={userItem.email} max={100} />
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("nickname")}:</div>
                        <input name="nickname" type="text" required={true} value={userItem.nickname || ""} data-test="user-management-nickname"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                            <MaxLength val={userItem.nickname} max={50} />
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("timezone")}:</div>
                        <select name="timeZone" required={true} value={userItem.timeZone || ""} data-test="user-management-timezone"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                {timezones.map((t, idx) =>
                                    <option value={t.value} key={idx}>{t.name}</option>
                                )}
                        </select>
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("culture")}:</div>
                        <select name="culture" required={true} value={userItem.culture || ""} data-test="user-management-culture"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                {cultures.map((t, idx) =>
                                    <option value={t.value} key={idx}>{t.name}</option>
                                )}
                        </select>
                    </label>
                </div>
                {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                <div className={classes.editorButtons}>
                    <button className='editor-button' onClick={onUpsertClick} data-test="user-management-upsert">
                        { userItem.id ? t("update") : t("insert") }
                    </button>
                    <button className='editor-button' onClick={onCancel} data-test="user-management-cancel">{t("cancel")}</button>
                </div>
            </form>
        </div>
    )
}

export default memo(UserManagementEdit);