import React, { useReducer, memo } from 'react'
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

const ContactEdit = ({contact, onUpdate, onCancel}) =>
{
    const classes = styles();
    const { t } = useTranslation();
    const [contactItem, setContactItem] = useReducer(contactReducer, structuredClone(contact));

    const onFieldChange = (e) => {
        onUpdateField(e, setContactItem);
    }

    return (
        <div className={classes.wrapper}>
            <div className={classes.editor}>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("application.contact-name")}:</span>
                        <input value={contactItem.contactName || ""} onChange={onFieldChange}
                            type="text" name="contactName" style={{width: '262px'}} />
                    </label>
                </div>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("application.contact-type")}:</span>
                        <select value={contactItem.type || "Email"} onChange={onFieldChange} name="type">
                            <option value='Email'>{t("application.contact-type-email")}</option>
                            <option value='Phone'>{t("application.contact-type-phone")}</option>
                            <option value='Web'>{t("application.contact-type-web")}</option>
                            <option value='Other'>{t("application.contact-type-other")}</option>
                        </select>
                        <input type="text" className={classes.contactParameter} value={contactItem.contactParameter || ""} onChange={onFieldChange} name="contactParameter" />
                    </label>
                </div>
                <div className={classes.row}>
                    <label className="nowrap">
                        <span className={classes.editorLabel}>{t("application.contact-role")}:</span>
                        <select value={contactItem.role || "Recruiter"} onChange={onFieldChange} name="role" style={{width: '262px'}} >
                            <option value='Recruiter'>{t("application.contact-role-recruiter")}</option>
                            <option value='HiringManager'>{t("application.contact-role-hiring-manager")}</option>
                            <option value='HumanResources'>{t("application.contact-role-human-resources")}</option>
                            <option value='TeamMember'>{t("application.contact-role-team-member")}</option>
                            <option value='Other'>{t("application.contact-role-other")}</option>
                        </select>
                    </label>
                </div>
            </div>
            <div className="center">
                <button className="editor-button" onClick={() => onUpdate(contactItem)}>{t("application.contact-update")}</button>
                <button className="editor-button" onClick={onCancel}>{t("application.contact-cancel")}</button>
            </div>
        </div>
    )
}

export default memo(ContactEdit);