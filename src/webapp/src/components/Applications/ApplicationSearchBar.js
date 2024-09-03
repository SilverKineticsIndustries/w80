import React, { useState, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { sortDatesAsc } from '../../helpers/dates';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    toolbarfilter: {
        paddingTop: "6px",
        flexBasis: "100px",
        whiteSpace: "nowrap"
    },
    toolbaractions: {
        flexBasis: "100px",
        '& input[type="textbox"]': {
            width: "100%"
        }
    },
    wrapper: {
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
            return (app.role || "").includes(term)
                || (app.companyName || "").includes(term)
                || (app.roleDescription || "").includes(term);
        else
            return app;
    }
}

const sortOrder = (sortOrder, invertSort = false) =>
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

const ApplicationSearchBar = ({onFilterChange, isDisabled}) =>
{
    const classes = styles();
    const [term, setTerm] = useState("");
    const [sort, setSort] = useState(SortType.State);
    const [invertSort, setInvertSort] = useState(false);
    const { t } = useTranslation(null, { keyPrefix: "application" });

    const filterChanged = () => {
        onFilterChange({
            term: termFilter(term),
            sort: sortOrder(sort, invertSort),
            invertSort: invertSort
        })
    }

    return (
        <div className={classes.wrapper}>
            <div>
                <label htmlFor="sortTypeFilter">{t("sort-by")}</label>
                <select id="sortTypeFilter" value={sort} disabled={isDisabled}
                    onChange={(e) => { setSort(e.target.value); filterChanged()}}>
                    <option value={SortType.State}>{t("sort-by-state")}</option>
                    <option value={SortType.NextAppointment}>{t("sort-by-next-appointment")}</option>
                    <option value={SortType.CompanyName}>{t("sort-by-company-name")}</option>
                    <option value={SortType.Compensation}>{t("sort-by-compensation")}</option>
                </select>
            </div>
            <div>
                <label htmlFor="sortInvert">{t("invert-sort")}</label>
                <input id="sortInvert" type="checkbox" value={invertSort} disabled={isDisabled}
                    onChange={(e) => { setInvertSort(e.target.checked); filterChanged()}}/>
            </div>
            <div>
                <label htmlFor="searchTerm">{t("search")}</label>
                <input id="searchTerm" type="text" width="250px" disabled={isDisabled} value={term}
                    onChange={(e) => { setTerm(e.target.value); filterChanged()}} autoComplete="false" />
            </div>
            <div>
                <button onClick={() => { setTerm(""); filterChanged()}} disabled={isDisabled}>{t("clear-search")}</button>
            </div>
        </div>
    )
}

export default memo(ApplicationSearchBar);