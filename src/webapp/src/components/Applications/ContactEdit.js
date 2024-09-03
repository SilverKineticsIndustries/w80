import React, { useReducer, memo, useCallback } from 'react'
import { createUseStyles } from 'react-jss'
import { onUpdateField } from '../../helpers/common'
import { useTranslation } from 'react-i18next'

const styles = createUseStyles({
    wrapper: {
        marginBottom: "10px",
        textAlign: "center"
    },
    row: {
        paddingBottom: "6px"
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
        width: "100px"
    },
    contactParameter: {
        marginLeft: "6px"
    }
})

const contactReducer = (state, action) => {
    switch (action.type) {
        case 'update':
            return {...state, [action.name]: action.payload};
        default:
            return state;
    }
}

const ContactEdit = ({contact, onUpdate, onCancel, dataTestInstanceId=""}) =>
{
    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "application" });
    const [contactItem, setContactItem] = useReducer(contactReducer, structuredClone(contact));

    const onFieldChange = useCallback((e) => {
        onUpdateField(e, setContactItem);
    }, [setContactItem]);

    return (
        <div className={classes.wrapper}>
            <div className={classes.editor}>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("contact-name")}:</span>
                        <input value={contactItem.contactName || ""} onChange={onFieldChange}
                            type="text" name="contactName" style={{width: '262px'}} data-test={`contact-edit-${dataTestInstanceId}-name`} />
                    </label>
                </div>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("contact-type")}:</span>
                        <select value={contactItem.type || "Email"} onChange={onFieldChange} name="type" data-test={`contact-edit-${dataTestInstanceId}-type`}>
                            <option value='Email'>{t("contact-type-email")}</option>
                            <option value='Phone'>{t("contact-type-phone")}</option>
                            <option value='Web'>{t("contact-type-web")}</option>
                            <option value='Other'>{t("contact-type-other")}</option>
                        </select>
                        <input type="text" className={classes.contactParameter} value={contactItem.contactParameter || ""}
                            onChange={onFieldChange} name="contactParameter" data-test={`contact-edit-${dataTestInstanceId}-parameter`} />
                    </label>
                </div>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("contact-role")}:</span>
                        <select value={contactItem.role || "Recruiter"} onChange={onFieldChange} name="role"
                            style={{width: '262px'}} data-test={`contact-edit-${dataTestInstanceId}-role`} >
                            <option value='Recruiter'>{t("contact-role-recruiter")}</option>
                            <option value='HiringManager'>{t("contact-role-hiring-manager")}</option>
                            <option value='HumanResources'>{t("contact-role-human-resources")}</option>
                            <option value='TeamMember'>{t("contact-role-team-member")}</option>
                            <option value='Other'>{t("contact-role-other")}</option>
                        </select>
                    </label>
                </div>
            </div>
            <div className="center">
                <button className="editor-button" onClick={() => onUpdate(contactItem)} data-test={`contact-edit-${dataTestInstanceId}-save`}>
                    {t("contact-update")}
                </button>
                <button className="editor-button" onClick={onCancel} data-test={`contact-edit-${dataTestInstanceId}-cancel`}>
                    {t("contact-cancel")}
                </button>
            </div>
        </div>
    )
}

export default memo(ContactEdit);