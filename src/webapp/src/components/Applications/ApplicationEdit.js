import React, { useState, useContext, useCallback, memo } from 'react';
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

const ApplicationEdit = ({id, onCompleteEdit}) => {

    const classes = styles();
    const dispatch = useDispatch();
    const industries = useSelector(selectIndusties);
    const [validationErrors, setValidationErrors] = useState([]);
    const { t } = useTranslation(null, { keyPrefix: "application" });
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const [application, setApplication] = useState(structuredClone(useSelector(state => selectApplicationById(state, id))));

    const onFieldChange = (e, isNumeric) => {
        setApplication(state => ({
            ...state,
            [e.target.name]: isNumeric ? Number(e.target.value) : e.target.value
        }));
    }

    const onSaveClick = useCallback((e) => {
        e.preventDefault();
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await upsertApplication(dispatch, getState, application),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, onCompleteEdit, setValidationErrors, e.target))
        );
    },[dispatch, application, onCompleteEdit, setValidationErrors, setLoading, setServerErrorMessage]);

    const onCancelClick = useCallback((e) => {
        e.preventDefault();
        if (application.isNew)
        {
            dispatch({ type: 'applications/clear', payload: application.id });
            onCompleteEdit();
        }
        else
            dispatch(apiDispatchDecorator(
                async (dispatch, getState) => await refreshApplication(dispatch, getState, application.id),
                apiDecoratorOptions({ setLoading, setServerErrorMessage }, onCompleteEdit, null, e.target)));
    },[application.id, application.isNew, dispatch, onCompleteEdit, setLoading, setServerErrorMessage]);

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
                        <label className={classes.labelContainer} htmlFor="company-name">{t("company-name")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="company-name" onChange={onFieldChange} value={application.companyName || ""} name="companyName"
                                type="text" required={true} autoComplete="false" data-test="application-edit-company-name" />
                                <MaxLength val={application.companyName} max={150} />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="industry">{t("industry")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="industry" onChange={onFieldChange} value={application.industry || ""}
                                data-test="application-edit-industry" name="industry">
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
                        <label className={classes.labelContainer} htmlFor="hq-location">{t("hq-location")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="hq-location" onChange={onFieldChange} value={application.hqLocation || ""}
                                name="hqLocation" type="text" data-test="application-edit-hq-location"
                                autoComplete="false" />
                        </div>
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>Role Information</legend>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="company-role">{t("role")}</label>
                        <div className={classes.controlContainer}>
                            <input id="company-role" onChange={onFieldChange} value={application.role || ""}
                                name="role" type="text" required={true} data-test="application-edit-company-role"
                                autoComplete="false" />
                                <MaxLength val={application.role} max={100} />
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="position-type">{t("position-type")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="position-type" onChange={onFieldChange} value={application.positionType || ""}
                                name="positionType" data-test="application-edit-position-type" autoComplete="false">
                                <option value="Fulltime">{t("position-type-fulltime")}</option>
                                <option value="Parttime">{t("position-type-parttime")}</option>
                                <option value="Contract">{t("position-type-contract")}</option>
                                <option value="Temporary">{t("position-type-temporary")}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="work-settings">{t("work-setting")}:</label>
                        <div className={classes.controlContainer}>
                            <select id="work-settings" onChange={onFieldChange} value={application.workSetting || ""}
                                name="workSetting" data-test="application-edit-work-settings" autoComplete="false">
                                <option value="OnSite">{t("work-setting-onsite")}</option>
                                <option value="Hybrid">{t("work-setting-hybrid")}</option>
                                <option value="Remote">{t("work-setting-remote")}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div className={classes.field}>
                    <div className={classes.labelWithControl}>
                        <label className={classes.labelContainer} htmlFor="position-location">{t("position-location")}:</label>
                        <div className={classes.controlContainer}>
                            <input id="position-location" onChange={onFieldChange} value={application.positionLocation || ""}
                                name="positionLocation" type="text" data-test="application-edit-position-location"
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
                            {t("travel-requirements")}
                            <textarea onChange={onFieldChange} value={application.travelRequirements || ""}
                                name="travelRequirements" className={classes.travelRequirements} rows={1}
                                data-test="application-edit-travel-requirements" autoComplete="false" />
                        </label>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <label>
                            {t("source")}
                            <input onChange={onFieldChange} value={application.sourceOfJobPosting || ""}
                                name="sourceOfJobPosting" className={classes.postingSource} type="text"
                                data-test="application-edit-source" autoComplete="false" />
                        </label>
                    </div>
                </div>
                <div className={classes.field}>
                    <div style={{ "minWidth": "40vw"}}>
                        <label>
                            <div>{t("additional-info")}</div>
                            <textarea onChange={onFieldChange} value={application.additionalInfo || ""}
                                name="additionalInfo" className={classes.additionalInfo}
                                data-test="application-edit-additional-info" autoComplete="false" />
                        </label>
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>{t("contacts")}</legend>
                <div className={classes.field}>
                    <div style={{ "minWidth": "60vw"}}>
                        <ContactList contacts={application.contacts} onUpdate={onContactsUpdate} />
                    </div>
                </div>
            </fieldset>
            <fieldset className={classes.fieldGroup}>
                <legend>{t("role-description")}</legend>
                <textarea onChange={onFieldChange} value={application.roleDescription || ""}
                    name="roleDescription" rows={25} max={4000} style={{width: "100%"}}
                    data-test="application-edit-role-description" autoComplete="false" />
            </fieldset>
            {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
            <div className="center">
                <button className="editor-button" onClick={onSaveClick} data-test="application-edit-save">{t("save")}</button>
                <button className="editor-button" onClick={onCancelClick} data-test="application-edit-cancel">{t("cancel")}</button>
            </div>
        </div>
    )
}

export default memo(ApplicationEdit);