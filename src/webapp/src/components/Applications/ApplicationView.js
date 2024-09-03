import React, { useContext, useState, memo, useCallback } from 'react';
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
            return t("work-setting-hybrid");
        case 'OnSite':
            return t("work-setting-onsite");
        case 'Remote':
            return t("work-setting-remote");
        default:
            return;
    }
}

const displayPositionType = (t, val) =>
{
    switch (val)
    {
        case 'Fulltime':{
            return t("position-type-fulltime");
        }
        case 'Parttime': {
            return t("position-type-parttime");
        }
        case 'Contract': {
            return t("position-type-contract");
        }
        case 'Temporary': {
            return t("position-type-temporary");
        }
        default:
            return;
    }
}

const nextAppointment = (application) => {
    if (!application.appointments)
        return;

    const sorted = application.appointments.filter((x) => new Date(x.endDateTimeUTC) > new Date()).sort(sortDatesAsc)
    if (sorted)
        return sorted[0];
}

const industry = (val, industries) => {
    if (val)
        return industries.find(x => x.value === val)?.name;
}

const ApplicationView = ({id, onBeginEdit, allowEdit = false,
    allowDelete=false, allowArchive=false, allowUnarchive=false, allowReject=false, allowExpand=false,
    allowStateChange=false, allowAccept=false }) => {

    const classes = styles();
    const dispatch = useDispatch();
    const [expanded, setExpanded] = useState(false);
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const industries = useSelector(selectIndusties);
    const { t } = useTranslation(null, { keyPrefix: "application"});
    const [showRejectionModal, setShowRejectionModal] = useState(false);
    const [showAcceptanceModal, setShowAcceptanceModal] = useState(false);
    const application = useSelector(state => structuredClone(selectApplicationById(state, id)));

    const onDeleteClick = useCallback((e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await deactivateApplication(dispatch, getState, application.id),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, null, null, e))
        );
    },[dispatch, application.id, setLoading, setServerErrorMessage]);

    const onArchiveClick = useCallback((e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await archiveApplication(dispatch, getState, application.id),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, null, null, e))
        );
    },[dispatch, application.id, setLoading, setServerErrorMessage]);

    const onUnarchiveClick = useCallback((e) => {
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await unarchiveApplication(dispatch, getState, application.id),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, null, null, e))
        );
    },[dispatch, application.id, setLoading, setServerErrorMessage]);

    const onRejectedClick = useCallback(() => {
        setShowRejectionModal(true);
    },[]);

    const onAcceptedClick = useCallback(() => {
        setShowAcceptanceModal(true);
    },[]);

    const nextAppnt = nextAppointment(application);

    return(
        <div className={classes.wrapper}>
            <div className={classes.headerInfoContainer}>
                <div className="flex-1">
                    {!application.rejection.rejectedUTC && !application.acceptance.acceptedUTC &&
                        <States app={application} allowStateChange={allowStateChange} />
                    }
                    {application.rejection.rejectedUTC && <RejectionInfo rejection={application.rejection} /> }
                    {application.acceptance.acceptedUTC && <AcceptanceInfo acceptance={application.acceptance} />}
                </div>
                <div className="flex-5">
                    <div className="nowrap">
                        <div className={classes.viewFieldName}>{t("company-name")}:</div>
                        <span className={classes.viewFieldValue} style={{fontWeight: "bolder" }}
                            data-test={`application-view-${id}-company-name`}>
                            {application.companyName}
                        </span>
                    </div>
                    <div className="nowrap">
                        <span className={classes.viewFieldName}>{t("role")}</span>
                        <span className={classes.viewFieldValue}
                            data-test={`application-view-${id}-role`}>
                            {application.role}
                        </span>
                    </div>
                    <div className="nowrap">
                        <span className={classes.viewFieldName}>{t("compensation")}:</span>
                        <span className={classes.viewFieldValue}>
                            <CompensationView min={application.compensationMin} max={application.compensationMax}
                                type={application.compensationType} dataTestPrefix={`application-view-${id}`}/>
                        </span>
                    </div>
                </div>
                <div className="flex-2">
                    <div>
                        {nextAppnt &&
                            <div>
                                <fieldset>
                                    <legend style={{margin:"0 auto"}}>Next Appointment</legend>
                                    <CalendarLink appointment={nextAppnt} />
                                </fieldset>
                            </div>
                        }
                    </div>
                </div>
                <div>
                    {allowEdit && <ToolButton onClick={() => onBeginEdit(id)}
                        img={edit} tooltip={t("edit-application")} height="36px" width="36px" dataTest={`application-view-${id}-edit`} />}
                    {allowDelete && <ToolButton onClick={(e) => onDeleteClick(e)}
                        img={remove} tooltip={t("delete-application")} height="36px" width="36px" confirmationMessage={t("confirm-delete-application")} dataTest={`application-view-${id}-delete`} />}
                    {allowArchive && <ToolButton onClick={(e) => onArchiveClick(e)}
                        img={archive} tooltip={t("archive-application")} height="36px" width="36px" confirmationMessage={t("confirm-archive-application")} dataTest={`application-view-${id}-archive`} />}
                    {allowUnarchive && <ToolButton onClick={(e) => onUnarchiveClick(e)}
                        img={unarchive} tooltip={t("unarchive-application")} height="36px" width="36px" confirmationMessage={t("confirm-unarchive-application")} dataTest={`application-view-${id}-unarchive`} />}
                    {allowAccept && <ToolButton onClick={(e) => onAcceptedClick(e)}
                        img={accept} tooltip={t("accept-application")} height="36px" width="36px" dataTest={`application-view-${id}-accept`} />}
                    {allowReject && <ToolButton onClick={(e) => onRejectedClick(e)}
                        img={reject} tooltip={t("reject-application")} height="36px" width="36px" dataTest={`application-view-${id}-reject`}  />}
                    {allowExpand && !expanded && <ToolButton onClick={() => setExpanded(true)}
                        img={expand} height="36px" width="36px" dataTest={`application-view-${id}-expand`}  />}
                    {allowExpand && expanded && <ToolButton onClick={() => setExpanded(false)}
                        img={collapse} height="36px" width="36px" dataTest={`application-view-${id}-collapse`}  />}
                </div>
            </div>
            {expanded &&
                <>
                    <div className={classes.expandedInfoContainer}>
                        <div className="flex-4">
                            <fieldset>
                                <legend>{t("role-description")}</legend>
                                <div className={classes.roleDescriptionContent} data-test={`application-view-${id}-role-description`}>
                                    {(application.roleDescription || "").replace('\n', '<br/>')}
                                </div>
                            </fieldset>
                        </div>
                        <div className="flex-2">
                            <div className={classes.expandedRightSideContainer}>
                                <div className="nowarp">
                                    <span className={classes.viewFieldName}>{t("position-type")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-position-type`}>
                                        {displayPositionType(t, application.positionType)}
                                    </span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("work-setting")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-work-setting`}>
                                        {displayWorkSetting(t, application.workSetting)}
                                    </span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("industry")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-industry`}>
                                        {industry(application.industry, industries)}
                                    </span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("hq-location")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-hq-location`}>
                                        {application.hqLocation}
                                    </span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("position-location")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-position-location`}>
                                        {application.positionLocation}
                                    </span>
                                </div>
                                <div className="nowrap">
                                    <span className={classes.viewFieldName}>{t("travel-requirements")}:</span>
                                    <span className={classes.viewFieldValue} data-test={`application-view-${id}-travel-requirements`}>
                                        {application.travelRequirements}
                                    </span>
                                </div>
                                <fieldset style={{ "width": "100%"}}>
                                    <legend>{t("posting-source")}</legend>
                                    <div>
                                        <span data-test={`application-view-${id}-source`}>{application.sourceOfJobPosting}</span>
                                        <OpenUrl value={application.sourceOfJobPosting} />
                                    </div>
                                </fieldset>
                                <fieldset style={{ "width": "100%"}}>
                                    <legend>{t("additional-info")}</legend>
                                    <div className={classes.additionalInfoValue} data-test={`application-view-${id}-additional-info`}>
                                        {(application.additionalInfo || "").replace('\n', "<br/>")}
                                    </div>
                                </fieldset>
                            </div>
                        </div>
                    </div>
                    <div className={classes.contacts}>
                        <fieldset style={{ width: "100%" }}>
                            <legend>{t("contacts")}</legend>
                            <ContactList contacts={application.contacts} allowEdit={false} dataTestInstanceId={id} />
                        </fieldset>
                    </div>
                </>
            }
            {showRejectionModal && <RejectionModal applicationId={application.id} onClose={() => setShowRejectionModal(false)}/>}
            {showAcceptanceModal && <AcceptanceModal applicationId={application.id} onClose={() => setShowAcceptanceModal(false)}/>}
        </div>
    )
}

export default memo(ApplicationView);