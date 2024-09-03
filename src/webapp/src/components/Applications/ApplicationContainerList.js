import React, { useState, useContext, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useSelector, useDispatch } from 'react-redux';
import { createNewApplication } from '../../store/applications/thunks';
import { selectNewlyAddedApplication } from '../../store/applications/selectors';
import ApplicationContainer from './ApplicationContainer';
import { StatusContext } from '../../App';
import ToolButton from '../../common/ToolButton';
import add from '../../assets/add.png';
import { useTranslation } from 'react-i18next';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import ApplicationSearchBar from './ApplicationSearchBar';

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
    headerText: {
        color: "var(--regular-text)",
        fontSize: "1.5em",
        marginLeft: "auto",
        textAlign: "right",
        fontWeight: "bold",
        marginRight: "10px"
    }
})

const ApplicationContainerList = ({selector, headerLabel, allowNew, ...props}) =>
{
    const classes = styles();
    const dispatch = useDispatch();
    const [filter, setFilter] = useState({});
    const { t } = useTranslation(null, { keyPrefix: "application" });
    const newAppAdded = useSelector(selectNewlyAddedApplication);
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const applicationIds = useSelector((state) => selector(state, filter)) || [];

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

    return(
        <div className={classes.wrapper}>
            <div className={classes.toolbar}>
                {allowNew && <ToolButton onClick={onNewClick} disabled={newAppAdded} img={add} dataTest="application-add-new" tooltip={t("create-new-app")} />}
                <ApplicationSearchBar onFilterChange={(e) => setFilter(e)} isDisabled={newAppAdded} />
                {headerLabel && <div className={classes.headerText}>{headerLabel}</div>}
            </div>
            <div className={classes.applications}>
                {applicationIds.map((id, idx) =>
                    <React.Fragment key={idx}>
                        <ApplicationContainer key={id} id={id} {...props} />
                        {(applicationIds.length - 1 !== idx) && <hr key={`hr-${idx}`}/>}
                    </React.Fragment>
                )}
            </div>
        </div>
    )
}

export default memo(ApplicationContainerList);