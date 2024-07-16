import React, { useState, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import CompensationEdit from './CompensationEdit';
import ContactList from './ContactList';
import { useDispatch, useSelector } from 'react-redux';
import { selectApplicationById } from '../../store/applications/selectors';
import { upsertApplication, refreshApplication } from '../../store/applications/thunks';
import { StatusContext } from '../../App';
import ValidationPanel from '../../common/ValidationPanel';
import { selectIndusties } from '../../store/industries/selectors';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import MaxLength from '../../common/MaxLength';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    wrapper: {
        backgroundColor: "var(--very-dark)",
        border: "1px solid var(--dark)",
        borderRadius: "10px",
        paddingTop: "20px",
        paddingBottom: "10px"
    },
    travelRequirements: {
        resize: "none",
        width: "100%"
    },
    additionalInfo: {
        resize: "none",
        height: "130px",
        width: "100%"
    },
    postingSource: {
        resize: "none",
        width: "100%"
    },
    compensation: {
        marginRight: "6px",
        marginLeft: "6px"
    },
    fieldGroup: {
        width: "80vw",
        margin: "auto",
        marginTop: "10px"
    },
    field: {
        display: "flex",
        flexDirection: "row",
        flexWrap: "wrap",
        justifyContent: "center",
        marginBottom: "6px"
    },
    labelWithControl: {
        display: "flex",
        minWidth: "40vw"
    },
    labelContainer: {
        flex: 1,
        textAlign: "right",
        paddingRight: "6px",
        whiteSpace: "nowrap"
    },
    controlContainer: {
        flex: 2,
        "& input,select": {
            width: "100%"
        }
    }
})

export default function ApplicationEdit({id, onCompleteEdit}) {

    const classes = styles();
    const dispatch = useDispatch();
    const { t } = useTranslation();
    const statusContext = useContext(StatusContext);
    const industries = useSelector(selectIndusties);
    const [validationErrors, setValidationErrors] = useState([]);
    const [application, setApplication] = useState(structuredClone(useSelector(state => selectApplicationById(state, id))));

    const onFieldChange = (e, isNumeric) => {
        setApplication(state => ({
            ...state,
            [e.target.name]: isNumeric ? Number(e.target.value) : e.target.value
        }));
    }

    const onSaveClick = (e) => {
        e.preventDefault();
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await upsertApplication(dispatch, getState, application),
            apiDecoratorOptions(statusContext, onCompleteEdit, setValidationErrors, e.target))
        );
    }

    const onCancelClick = (e) => {
        e.preventDefault();
        if (application.isNew)
        {
            dispatch({ type: 'applications/clear', payload: application.id });
            onCompleteEdit();
        }
        else
            dispatch(apiDispatchDecorator(
                async (dispatch, getState) => await refreshApplication(dispatch, getState, application.id),
                apiDecoratorOptions(statusContext, onCompleteEdit, null, e.target)));
    }

    const onContactsUpdate = (updatedContacts) => {
        setApplication(state => ({
            ...state,
            contacts: updatedContacts
        }));
    }

    return(
        <div className={classes.wrapper}>
            <fieldset className={classes.fieldGroup}>
                <legend>Company Information</legend>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="company-name">{t("application.company-name")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="company-name" onChange={onFieldChange} value={application.companyName || ""} name="companyName"
                                type="text" required={true} autoComplete="false" />
                                <MaxLength val={application.companyName} max={150} />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="industry">{t("application.industry")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="industry" onChange={onFieldChange} value={application.industry || ""}
                                name="industry">
                                {
                                    industries.sort((a,b) => a.name.localeCompare(b.name)).map((i, idx) =>
                                        <option key={idx} value={i.value}>{i.name}</option>
                                    )
                                }
                            </select>
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="hq-location">{t("application.hq-location")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="hq-location" onChange={onFieldChange} value={application.hqLocation || ""}
                                name="hqLocation" type="text"
                                autoComplete="false" />
                        </div>
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>Role Information</legend>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="company-role">{t("application.role")}</label>
                        <div className={classes.controlContainer}>
                            <input id="company-role" onChange={onFieldChange} value={application.role || ""}
                                name="role" type="text" required={true}
                                autoComplete="false" />
                                <MaxLength val={application.role} max={100} />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="position-type">{t("application.position-type")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="position-type" onChange={onFieldChange} value={application.positionType || ""}
                                name="positionType" autoComplete="false">
                                <option value="Fulltime">{t("application.position-type-fulltime")}</option>
                                <option value="Parttime">{t("application.position-type-parttime")}</option>
                                <option value="Contract">{t("application.position-type-contract")}</option>
                                <option value="Temporary">{t("application.position-type-temporary")}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="work-settings">{t("application.work-setting")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="work-settings" onChange={onFieldChange} value={application.workSetting || ""}
                                name="workSetting" autoComplete="false">
                                <option value="OnSite">{t("application.work-setting-onsite")}</option>
                                <option value="Hybrid">{t("application.work-setting-hybrid")}</option>
                                <option value="Remote">{t("application.work-setting-remote")}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="position-location">{t("application.position-location")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="position-location" onChange={onFieldChange} value={application.positionLocation || ""}
                                name="positionLocation" type="text"
                                autoComplete="false" />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <div className={classes.compensation}>
                            <CompensationEdit application={application} onFieldChange={onFieldChange} />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <label>
                            {t("application.travel-requirements")}
                            <textarea onChange={onFieldChange} value={application.travelRequirements || ""}
                                name="travelRequirements" className={classes.travelRequirements} rows={1}
                                autoComplete="false" />
                        </label>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <label>
                            {t("application.source")}
                            <input onChange={onFieldChange} value={application.sourceOfJobPosting || ""}
                                name="sourceOfJobPosting" className={classes.postingSource} type="text"
                                autoComplete="false" />
                        </label>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <label>
                            <div>{t("application.additional-info")}</div>
                            <textarea onChange={onFieldChange} value={application.additionalInfo || ""}
                                name="additionalInfo" className={classes.additionalInfo}
                                autoComplete="false" />
                        </label>
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>{t("application.contacts")}</legend>
                <div className={classes.field}>
                    <div style={{ "minWidth": "60vw"}}>
                        <ContactList contacts={application.contacts} onUpdate={onContactsUpdate} />
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>{t("application.role-description")}</legend>
                <textarea onChange={onFieldChange} value={application.roleDescription || ""}
                    name="roleDescription" rows={25} max={4000} style={{width: "100%"}}
                    autoComplete="false" />
            </fieldset>
            {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
            <div className="center">
                <button className="editor-button" onClick={onSaveClick}>{t("application.save")}</button>
                <button className="editor-button" onClick={onCancelClick}>{t("application.cancel")}</button>
            </div>
        </div>
    )
}