import React, { useState, useContext, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useSelector } from 'react-redux';
import ApplicationContainer from './ApplicationContainer';
import ApplicationSearchBar from './ApplicationSearchBar';
import { getFilter } from '../../services/applicationService';
import { UserContext } from '../../App';

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

const ApplicationContainerList = ({ selector, headerLabel, allowNew, ...props }) => {
    const classes = styles();

    const { currentUser } = useContext(UserContext);
    const init = getFilter(currentUser.applicationSearchAndSortJSON);
    const [filter, setFilter] = useState(init);
    const applicationIds = useSelector((state) => selector(state, filter)) || [];

    return (
        <div className={classes.wrapper}>
            <div className={classes.toolbar}>
                <ApplicationSearchBar onFilterChanged={(e) => setFilter(e)} allowNew={allowNew} allowSorting={props.allowSorting} />
                {headerLabel && <div className={classes.headerText}>{headerLabel}</div>}
            </div>
            <div className={classes.applications}>
                {applicationIds.map((id, idx) =>
                    <React.Fragment key={idx}>
                        <ApplicationContainer key={id} id={id} {...props} />
                        {(applicationIds.length - 1 !== idx) && <hr key={`hr-${idx}`} />}
                    </React.Fragment>
                )}
            </div>
        </div>
    )
}

export default memo(ApplicationContainerList);