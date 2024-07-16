import React, { useReducer, useEffect, useState, useContext } from 'react'
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

export default function UserManagementEdit({user, onPostUpdate, onCancel})
{
    const classes = styles();
    const { t } = useTranslation();
    const statusContext = useContext(StatusContext);
    const [cultures, setCultures] = useState([]);
    const [timezones, setTimezones] = useState([]);
    const [validationErrors, setValidationErrors] = useState([]);
    const [userItem, setUserItem] = useReducer(userReducer, structuredClone(user));

    const onFieldChange = (e) => {
        onUpdateField(e, setUserItem);
    }

    useEffect(() => {
        apiDirectDecorator(
            async () => await get('/options/cultures'),
            apiDecoratorOptions(statusContext, (data) => setCultures(data.sort(sortByName))))();

        apiDirectDecorator(
            async () => await get('/options/timezones'),
            apiDecoratorOptions(statusContext, (data) => setTimezones(data.sort(sortByName))))();
    },[user]);

    const onUpsertClick = (e) => {
        e.preventDefault();
        apiDirectDecorator(
            async () => await upsertUser(userItem),
            apiDecoratorOptions(
                statusContext,
                (data) => onPostUpdate(data),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    }

    return (
        <div className={classes.wrapper}>
            <form>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("user-management.role")}:</div>
                        <select name="role" required={true} value={userItem.role || ""} data-test="user-management-role"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" >
                                <option value='User' key='User'>{t("user-management.user-role")}</option>
                                <option value='Administrator' key='Administrator'>{t("user-management.admin-role")}</option>
                        </select>
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("user-management.email")}:</div>
                        <input name="email" type="text" required={true} value={userItem.email || ""} data-test="user-management-email"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                            <MaxLength val={userItem.email} max={100} />
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("user-management.nickname")}:</div>
                        <input name="nickname" type="text" required={true} value={userItem.nickname || ""} data-test="user-management-nickname"
                            onChange={onFieldChange} className={classes.inputControl} autoComplete="false" />
                            <MaxLength val={userItem.nickname} max={50} />
                    </label>
                </div>
                <div className={classes.fieldContainer}>
                    <label>
                        <div className={classes.labelText}>{t("user-management.timezone")}:</div>
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
                        <div className={classes.labelText}>{t("user-management.culture")}:</div>
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
                        { userItem.id ? t("user-management.update") : t("user-management.insert") }
                    </button>
                    <button className='editor-button' onClick={onCancel} data-test="user-management-cancel">{t("user-management.cancel")}</button>
                </div>
            </form>
        </div>
    )
}