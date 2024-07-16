import React, { useState, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { useSelector, useDispatch } from 'react-redux';
import { createNewApplication } from '../../store/applications/thunks';
import { selectNewlyAddedApplication } from '../../store/applications/selectors';
import ApplicationContainer from './ApplicationContainer';
import { StatusContext } from '../../App';
import { sortDatesAsc } from '../../helpers/dates';
import ToolButton from '../../common/ToolButton';
import add from '../../assets/add.png';
import { useTranslation } from 'react-i18next';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';

const styles = createUseStyles({
    wrapper: {
        marginTop: "6px"
    },
    toolbar: {
        display: "flex",
        padding: "10px",
        alignItems: "center",
        justifyContent: "flex-start",
        backgroundColor: "var(--very-dark)",
        border: "1px solid var(--dark)",
        borderRadius: "10px",
        flexWrap: "wrap",
        marginRight: "8px",
        "@media (max-width: 900px)": {
            flexDirection: "row-reverse"
        }
    },

    applications: {
        marginTop: "6px",
        marginRight: "4px"
    },
    toolbaractions: {
        flexBasis: "100px",
        '& input[type="textbox"]': {
            width: "100%"
        }
    },
    toolbarfilter: {
        paddingTop: "6px",
        flexBasis: "100px",
        whiteSpace: "nowrap"
    },
    toolbarsearch: {
        display: "flex",
        flexWrap: "wrap",
        whiteSpace: "nowrap",
        "& div": {
            paddingRight: "10px"
        },
        "& label": {
            marginRight: "4px",
            marginLeft: "8px",
            display: "inline-block",
            textAlign: "right"
        }
    },
    headerText: {
        color: "var(--regular-text)",
        fontSize: "1.5em",
        marginLeft: "auto",
        textAlign: "right",
        fontWeight: "bold",
        marginRight: "10px"
    }
})

const SortType = {
    State: 'State',
    CompanyName: 'CompanyName',
    Compensation: 'Compensation',
    NextAppointment: 'NextAppointment',
};

const termFilter = (term) => {
    return (app) => {
        if (term)
            return app.role.includes(term)
                || app.companyName.includes(term)
                || app.roleDescription.includes(term);
        else
            return app;
    }
}

const getSortOrder = (sortOrder, invertSort = false) =>
{
    switch (sortOrder) {
        case SortType.NextAppointment: {
            return (a, b) =>
            {
                const fielda = invertSort ? a.appointments: b.appointments;
                const fieldb = invertSort ? b.appointments: a.appointments;
                return  ((fielda || []).length > 0 ? fieldb.sort(sortDatesAsc)[0].datetimeutc : 0)
                        - ((fieldb || []).length > 0 ? fielda.sort(sortDatesAsc)[0].datetimeutc : 0)
            }
        }
        case SortType.CompanyName: {
            return (a, b) =>
            {
                const fielda = invertSort ? a.companyName : b.companyName;
                const fieldb = invertSort ? b.companyName : a.companyName;
                if (fielda < fieldb)
                    return -1;
                if (fielda > fieldb)
                    return 1;
                return 0;
            }
        }
        case SortType.Compensation: {
            return (a, b) =>
            {
                // TODO: Apply invertSort
                return a.compensationMax && b.compensationMax
                    ? a.compensationMax - b.compensationMax
                    : a.compensationMin && b.compensationMin
                        ? a.compensationMin - b.compensationMin
                        : a.compensationMax && b.compensationMin
                            ? a.compensationMax - b.compensationMin
                            : a.compensationMin && b.compensationMax
                                ? a.compensationMin - b.compensationMax
                                : a.id - b.id;
            }
        }
        default: { // sort by 'state'
            return (a, b) =>
            {
                const fieldA = invertSort ? a.states: b.states;
                const fieldB = invertSort ? b.states: a.states;
                return ((fieldA || []).find(x => x.isCurrent)?.seqNo || 0)
                        - ((fieldB || []).find(x => x.isCurrent)?.seqNo || 0);
            }
        }
    }
}

export default function ApplicationList({selector, allowNew=true, allowEdit=true, allowDelete=true,
    allowArchive=true, allowUnarchive=true, allowReject=true, allowExpand=true, allowStateChange=true,
    allowAccept=true, headerLabel=''})
{
    const classes = styles();
    const dispatch = useDispatch();
    const { t } = useTranslation();
    const [term, setTerm] = useState('');
    const statusContext = useContext(StatusContext);
    const [invertSort, setInvertSort] = useState(false);
    const [sortType, setSortType] = useState(SortType.State);
    const newAppAdded = useSelector(selectNewlyAddedApplication);
    const applicationIds = useSelector((state) => selector(state, getSortOrder(sortType, invertSort), termFilter(term)));

    const onNewClick = (e) => {
        e.preventDefault();
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await createNewApplication(dispatch, getState, (data) => {
                data.positionType = 'Fulltime';
                data.workSetting = 'OnSite';
                data.compensationType = 'Salary';
            }),
            apiDecoratorOptions(statusContext, null, null, e.target))
        );
    }

    return(
        <div className={classes.wrapper}>
            <div className={classes.toolbar}>
                {allowNew && <ToolButton onClick={onNewClick} disabled={newAppAdded} img={add} tooltip={t("application.create-new-app")} />}
                <div className={classes.toolbarsearch}>
                    <div>
                        <label htmlFor="sortTypeFilter">{t("application.sort-by")}</label>
                        <select id="sortTypeFilter" onChange={(e) => setSortType(e.target.value)} value={sortType} disabled={newAppAdded}>
                            <option value={SortType.State}>{t("application.sort-by-state")}</option>
                            <option value={SortType.NextAppointment}>{t("application.sort-by-next-appointment")}</option>
                            <option value={SortType.CompanyName}>{t("application.sort-by-company-name")}</option>
                            <option value={SortType.Compensation}>{t("application.sort-by-compensation")}</option>
                        </select>
                    </div>
                    <div>
                        <label htmlFor="sortInvert">{t("application.invert-sort")}</label>
                        <input id="sortInvert" type="checkbox" value={invertSort} onChange={(e) => setInvertSort(e.target.checked)}/>
                    </div>
                    <div>
                        <label htmlFor="searchTerm">{t("application.search")}</label>
                        <input id="searchTerm" type="text" width="250px" disabled={newAppAdded} value={term}
                            onChange={(e) => setTerm(e.target.value)} autoComplete="false" />
                    </div>
                    <div>
                        <button onClick={() => setTerm()}>{t("application.clear-search")}</button>
                    </div>
                </div>
                {headerLabel &&
                    <div className={classes.headerText}>
                        {headerLabel}
                    </div>}
            </div>
            <div className={classes.applications}>
                {
                    (applicationIds || []).map((id, idx) =>
                        <React.Fragment>
                        <ApplicationContainer key={idx} id={id} allowEdit={allowEdit} allowDelete={allowDelete}
                            allowArchive={allowArchive} allowReject={allowReject} allowExpand={allowExpand}
                            allowUnarchive={allowUnarchive} allowStateChange={allowStateChange}
                            allowAccept={allowAccept} />
                            <hr />
                        </React.Fragment>
                    )
                }
            </div>
        </div>
    )
}