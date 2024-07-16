import React, { useContext, useState, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useSelector, useDispatch } from 'react-redux';
import { selectApplicationById } from '../../store/applications/selectors';
import { deactivateApplication, archiveApplication, unarchiveApplication } from '../../store/applications/thunks';
import States from './States';
import { StatusContext } from '../../App';
import CompensationView from './CompensationView';
import CalendarLink from '../Calendar/CalendarLink';
import { sortDatesAsc } from '../../helpers/dates';
import ContactList from './ContactList';
import { selectIndusties } from '../../store/industries/selectors';
import ToolButton from '../../common/ToolButton';
import edit from '../../assets/edit.png'
import remove from '../../assets/remove.png';
import archive from '../../assets/archive.png';
import unarchive from '../../assets/unarchive.png';
import reject from '../../assets/reject.png';
import accept from '../../assets/accept.png'
import expand from '../../assets/expand.png';
import collapse from '../../assets/collapse.png';
import RejectionModal from './RejectionModal'
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import RejectionInfo from './RejectionInfo'
import AcceptanceInfo from './AcceptanceInfo'
import AcceptanceModal from './AcceptanceModal';
import OpenUrl from '../../common/OpenUrl';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    wrapper: {
        backgroundColor: "var(--very-dark)",
        border: "1px solid var(--dark)",
        margin: "4px",
        borderRadius: "10px",
        padding: "6px",
        "&:hover": {
            boxShadow: "rgba(100, 100, 111, 0.3) 0px 7px 29px 0px"
        }
    },
    viewFieldName: {
        width: "200px",
        marginRight: "10px",
        whiteSpace: "nowrap",
        textAlign: "right",
        display: "inline-block"
    },
    viewFieldValue: {
        fontFamily: "sans-serif",
        whiteSpace: "nowrap",
        overflow: "hidden",
        textOverflow: "ellipsis"
    },
    roleDescriptionContent: {
        whiteSpace: "pre-wrap",
        maxHeight: "400px",
        overflow: "scroll",
    },
    additionalInfoValue: {
        whiteSpace: "pre-wrap",
        maxHeight: "200px",
        overflow: "scroll",
    },
    headerInfoContainer: {
        gap: "10px",
        display: "flex",
        flexWrap: "wrap",
        alignItems: "center",
        justifyContent: "space-around"
    },
    expandedInfoContainer: {
        gap: "10px",
        display: "flex",
        flexWrap: "wrap",
        justifyContent: "space-around"
    },
    expandedRightSideContainer: {
        gap: "4px",
        display: "flex",
        flexWrap: "wrap"
    },
    contacts: {
        gap: "10px",
        display: "flex",
        flexWrap: "wrap",
        alignItems: "center",
        justifyContent: "space-around"
    }
})

const displayWorkSetting = (t, val) =>
{
    switch (val)
    {
        case 'Hybrid':
            return t("application.work-setting-hybrid");
        case 'OnSite':
            return t("application.work-setting-onsite");
        case 'Remote':
            return t("application.work-setting-remote");
        default:
            return;
    }
}

const displayPositionType = (t, val) =>
{
    switch (val)
    {
        case 'Fulltime':{
            return t("application.position-type-fulltime");
        }
        case 'Parttime': {
            return t("application.position-type-parttime");
        }
        case 'Contract': {
            return t("application.position-type-contract");
        }
        case 'Temporary': {
            return t("application.position-type-temporary");
        }
        default:
            return;
    }
}

const ApplicationView = ({id, onBeginEdit, allowEdit = false,
    allowDelete=false, allowArchive=false, allowUnarchive=false, allowReject=false, allowExpand=false,
    allowStateChange=false, allowAccept=false }) => {

    const classes = styles();
    const dispatch = useDispatch();
    const { t } = useTranslation();
    const [expanded, setExpanded] = useState(false);
    const statusContext = useContext(StatusContext);
    const industries = useSelector(selectIndusties);
    const [showRejectionModal, setShowRejectionModal] = useState(false);
    const [showAcceptanceModal, setShowAcceptanceModal] = useState(false);
    const application = useSelector(state => structuredClone(selectApplicationById(state, id)));

    const onDeleteClick = (e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await deactivateApplication(dispatch, getState, application.id),
            apiDecoratorOptions(statusContext, null, null, e))
        );
    }

    const onArchiveClick = (e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await archiveApplication(dispatch, getState, application.id),
            apiDecoratorOptions(statusContext, null, null, e))
        );
    }

    const onUnarchiveClick = (e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await unarchiveApplication(dispatch, getState, application.id),
            apiDecoratorOptions(statusContext, null, null, e))
        );
    }

    const onRejectedClick = (e) => {
        setShowRejectionModal(true);
    }

    const onAcceptedClick = (e) => {
        setShowAcceptanceModal(true);
    }

    const nextAppointment = () => {
        if (!application.appointments)
            return;

        const sorted = application.appointments.filter((x) => new Date(x.endDateTimeUTC) > new Date()).sort(sortDatesAsc)
        if (sorted)
            return sorted[0];
    }

    const industry = (val) => {
        if (val)
            return industries.find(x => x.value === val)?.name;
    }

    return(
        <div className={classes.wrapper}>
            <div className={classes.headerInfoContainer}>
                <div style={{ "flex": 1 }}>
                    {!application.rejection.rejectedUTC && !application.acceptance.acceptedUTC &&
                        <States app={application} allowStateChange={allowStateChange} />
                    }
                    {application.rejection.rejectedUTC && <RejectionInfo rejection={application.rejection} /> }
                    {application.acceptance.acceptedUTC && <AcceptanceInfo acceptance={application.acceptance} />}
                </div>
                <div style={{ "flex": 5 }}>
                    <div className="nowrap">
                        <div className={classes.viewFieldName}>{t("application.company-name")}:</div>
                        <span className={classes.viewFieldValue} style={{fontWeight: "bolder" }}>{application.companyName}</span>
                    </div>
                    <div className="nowrap">
                        <span className={classes.viewFieldName}>{t("application.role")}</span>
                        <span className={classes.viewFieldValue}>{application.role}</span>
                    </div>
                    <div className="nowrap">
                        <span className={classes.viewFieldName}>{t("application.compensation")}:</span>
                        <span className={classes.viewFieldValue}><CompensationView min={application.compensationMin} max={application.compensationMax} type={application.compensationType} /></span>
                    </div>
                </div>
                <div style={{ "flex": 2 }}>
                    <div>
                        {nextAppointment() &&
                            <div>
                                <fieldset>
                                    <legend style={{margin:"0 auto"}}>Next Appointment</legend>
                                    <CalendarLink appointment={nextAppointment()} />
                                </fieldset>
                            </div>
                        }
                    </div>
                </div>
                <div>
                    {allowEdit && <ToolButton onClick={() => onBeginEdit(id)} img={edit} tooltip={t("application.edit-application")} height="36px" width="36px" />}
                    {allowDelete && <ToolButton onClick={(e) => onDeleteClick(e)} img={remove} tooltip={t("application.delete-application")} height="36px" width="36px" confirmationMessage={t("application.confirm-delete-application")} />}
                    {allowArchive && <ToolButton onClick={(e) => onArchiveClick(e)} img={archive} tooltip={t("application.archive-application")} height="36px" width="36px" confirmationMessage={t("application.confirm-archive-application")} />}
                    {allowUnarchive && <ToolButton onClick={(e) => onUnarchiveClick(e)} img={unarchive} tooltip={t("application.unarchive-application")} height="36px" width="36px" confirmationMessage={t("application.confirm-unarchive-application")} />}
                    {allowAccept && <ToolButton onClick={(e) => onAcceptedClick(e)} img={accept} tooltip={t("application.accept-application")} height="36px" width="36px" />}
                    {allowReject && <ToolButton onClick={(e) => onRejectedClick(e)} img={reject} tooltip={t("application.reject-application")} height="36px" width="36px" />}
                    {allowExpand && !expanded && <ToolButton onClick={() => setExpanded(true)} img={expand} height="36px" width="36px" />}
                    {allowExpand && expanded && <ToolButton onClick={() => setExpanded(false)} img={collapse} height="36px" width="36px" />}
                </div>
            </div>
            {expanded &&
                <React.Fragment>
                    <div className={classes.expandedInfoContainer}>
                        <div style={{ "flex": 4 }}>
                            <fieldset>
                                <legend>{t("application.role-description")}</legend>
                                <div className={classes.roleDescriptionContent}>
                                    {(application.roleDescription || "").replace('\n', '<br/>')}
                                </div>
                            </fieldset>
                        </div>
                        <div style={{ "flex": 2 }}>
                            <div className={classes.expandedRightSideContainer}>
                                <div className="nowarp">
                                    <span className={classes.viewFieldName}>{t("application.position-type")}:</span>
                                    <span className={classes.viewFieldValue}>{displayPositionType(t, application.positionType)}</span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("application.work-setting")}:</span>
                                    <span className={classes.viewFieldValue}>{displayWorkSetting(t, application.workSetting)}</span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("application.industry")}:</span>
                                    <span className={classes.viewFieldValue}>{industry(application.industry)}</span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("application.hq-location")}:</span>
                                    <span className={classes.viewFieldValue}>{application.hqLocation}</span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("application.position-location")}:</span>
                                    <span className={classes.viewFieldValue}>{application.positionLocation}</span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("application.travel-requirements")}:</span>
                                    <span className={classes.viewFieldValue}>{application.travelRequirements}</span>
                                </div>
                                <fieldset style={{ "width": "100%"}}>
                                    <legend>{t("application.posting-source")}</legend>
                                    <div>
                                        {application.sourceOfJobPosting}<OpenUrl value={application.sourceOfJobPosting} />
                                    </div>
                                </fieldset>
                                <fieldset style={{ "width": "100%"}}>
                                    <legend>{t("application.additional-info")}</legend>
                                    <div className={classes.additionalInfoValue}>
                                        {(application.additionalInfo|| "").replace('\n', '<br/>')}
                                    </div>
                                </fieldset>
                            </div>
                        </div>
                    </div>
                    <div className={classes.contacts}>
                        <fieldset style={{ width: "100%" }}>
                            <legend>{t("application.contacts")}</legend>
                                <ContactList contacts={application.contacts} allowEdit={false} />
                        </fieldset>
                    </div>
                </React.Fragment>
            }
            {showRejectionModal && <RejectionModal applicationId={application.id} onClose={() => setShowRejectionModal(false)}/>}
            {showAcceptanceModal && <AcceptanceModal applicationId={application.id} onClose={() => setShowAcceptanceModal(false)}/>}
        </div>
    )
}

export default memo(ApplicationView);