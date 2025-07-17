import React, { useState, useEffect, useContext, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import { selectNewlyAddedApplication } from '../../store/applications/selectors';
import { StatusContext } from '../../App';
import add from '../../assets/add.png';
import { createNewApplication } from '../../store/applications/thunks';
import ToolButton from '../../common/ToolButton';
import { UserContext } from '../../App';
import { updateApplicationSortAndFilter } from '../../services/userService';
import { getFilter, SortType } from '../../services/applicationService';

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
        alignItems: "center",
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

const ApplicationSearchBar = ({ onFilterChanged, allowNew = true, allowSorting = false }) => {
    const classes = styles();
    const { currentUser, setCurrentUser } = useContext(UserContext);
    const initial = getFilter(currentUser.applicationSearchAndSortJSON);
    const [term, setTerm] = useState("");
    const [sort, setSort] = useState(initial.sort);
    const [invertSort, setInvertSort] = useState(initial.invertSort);
    const newAppAdded = useSelector(selectNewlyAddedApplication);

    const dispatch = useDispatch();
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const { t } = useTranslation(null, { keyPrefix: "application" });

    const onNewClick = (e) => {
        e.preventDefault();
        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await createNewApplication(dispatch, getState, (data) => {
                data.positionType = 'Fulltime';
                data.workSetting = 'OnSite';
                data.compensationType = 'Salary';
            }),
            apiDecoratorOptions({ setLoading, setServerErrorMessage }, null, null, e.target))
        );
    }

    useEffect(() => {

        const filter = getFilter(initial, sort, term, invertSort);
        onFilterChanged(filter);

        if (allowSorting)
        {
            delete filter.term;
            const applicationSearchAndSort = JSON.stringify(filter)
            setCurrentUser({ ...currentUser, applicationSearchAndSortJSON: applicationSearchAndSort});
            (async () => await updateApplicationSortAndFilter(currentUser.id, applicationSearchAndSort))();
        }
        // eslint-disable-next-line
    }, [invertSort, term, sort]);

    const isDisabled = () => newAppAdded

    return (
        <div className={classes.wrapper}>
            <div>
                {allowNew && !isDisabled() && <ToolButton onClick={onNewClick} img={add} dataTest="application-add-new" tooltip={t("create-new-app")} />}
            </div>
            { allowSorting &&
                <>
                    <div>
                        <label htmlFor="sortTypeFilter">{t("sort-by")}</label>
                        <select id="sortTypeFilter" data-test="sort-type-filter" value={sort} disabled={isDisabled()}
                            onChange={(e) => { setSort(e.target.value) }}>
                            <option value={SortType.State}>{t("sort-by-state")}</option>
                            <option value={SortType.NextAppointment}>{t("sort-by-next-appointment")}</option>
                            <option value={SortType.CompanyName}>{t("sort-by-company-name")}</option>
                            <option value={SortType.Compensation}>{t("sort-by-compensation")}</option>
                        </select>
                    </div>
                    <div>
                        <label htmlFor="sortInvert">{t("invert-sort")}</label>
                        <input id="sortInvert" data-test="sort-type-invert-filter" type="checkbox" checked={invertSort} disabled={isDisabled()}
                            onChange={(e) => { setInvertSort(e.target.checked) }} />
                    </div>
                </>
            }
            <div>
                <label htmlFor="searchTerm">{t("search")}</label>
                <input id="searchTerm" type="text" width="250px" disabled={isDisabled()} value={term}
                    onChange={(e) => { setTerm(e.target.value) }} autoComplete="false" />
            </div>
            <div>
                <button onClick={() => { setTerm("") }} disabled={isDisabled()}>{t("clear-search")}</button>
            </div>
        </div>
    )
}

export default memo(ApplicationSearchBar);