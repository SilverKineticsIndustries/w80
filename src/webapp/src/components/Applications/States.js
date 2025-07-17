import React, { useState, useContext, useCallback, memo } from 'react';
import { Tooltip } from 'react-tooltip';
import { createUseStyles } from 'react-jss';
import { useDispatch } from 'react-redux';
import { changeApplicationState } from '../../store/applications/thunks';
import { StatusContext } from '../../App';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';
import { v4 as uuidv4 } from 'uuid';
import { useTranslation } from 'react-i18next'

const styles = createUseStyles({
    wrapper: {
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "space-around"
    },
    prevStateContainer: {
        flexGrow: 1,
        width: "40px"
    },
    applStateContainer: {
        flexGrow: 4,
        width: "60px",
        textAlign: "center"
    },
    nextStateContainer: {
        flexGrow: 1,
        width: "40px"
    },
    stateChangeButton: {
        color: "#222",
        fontSize: "40px",
        paddingLeft: "10px"
    }
})

const getStateName = (states, id) => states.find(x => x.id === id)?.name;
const getStateImage = (states, id) => states.find(x => x.id === id)?.resource;
const getApplState = (states, id, isPrevious=false) => {
    let idx = states.findIndex(x => x.id === id);
    if (idx >= 0)
    {
        if (!isPrevious)
            idx += 1;
        else
            idx -= 1;
        if (states.length > idx && idx > -1)
            return states[idx].id;
    }
}

const States = ({app, allowStateChange=false}) =>
{
    const labelUniqueid = uuidv4();
    const prevStateUniqueid = uuidv4();
    const nextStateUniqueid = uuidv4();

    const classes = styles();
    const dispatch = useDispatch();
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const sortedStates = app.states.toSorted((a,b) => { return a.seqNo - b.seqNo });
    const [selectedState, setSelectedState] = useState(app.states.find((x) => x.isCurrent));
    const nextState = getApplState(sortedStates, selectedState.id);
    const prevState = getApplState(sortedStates, selectedState.id, true);
    const { t } = useTranslation(null, { keyPrefix: "application" });

    const onStateChange = useCallback((e, stateId) => {
        if (!stateId)
            return;

        let changed =  structuredClone(app);
        changed.states.forEach((x) => x.isCurrent = false);
        changed.states.find((x) => x.id === stateId).isCurrent = true;

        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await changeApplicationState(dispatch, getState, changed),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => setSelectedState(changed.states.find((x) => x.isCurrent)),
                null, e.target
            )));
    },[app, dispatch, setLoading, setServerErrorMessage])

    const onMoveToNextState = (e) => { onStateChange(e, nextState); }
    const onMoveToPrevState = (e) => { onStateChange(e, prevState); }

    const currentStateName = getStateName(sortedStates, selectedState.id);
    const nextStateName = nextState ? getStateName(sortedStates, nextState) : null;
    const prevStateName = prevState ? getStateName(sortedStates, prevState) : null;

    const stateImage = `${process.env.PUBLIC_URL}/states/${getStateImage(sortedStates, selectedState.id)}`;

    return(
        <div className={classes.wrapper}>
            <div className={classes.prevStateContainer}>
                {allowStateChange && prevState &&
                    <>
                        <button data-tooltip-id={prevStateUniqueid}
                                className={`${classes.stateChangeButton} toolbar-button`}
                                data-test={`prev-state-${app.id}-button`}
                                onClick={onMoveToPrevState}>
                            &#9667;
                        </button>
                        <Tooltip id={prevStateUniqueid} delayShow="100" place="top">{t("move-to-prev-state", {prevStateName: prevStateName})}</Tooltip>
                    </>
                }
            </div>
            <div className={classes.applStateContainer}>
                <img data-tooltip-id={labelUniqueid}
                    height={44} width={44}
                    src={stateImage}
                    data-test={`state-label-${app.id}`}
                    alt={currentStateName} />
                <Tooltip id={labelUniqueid} delayShow="100" place="top">{currentStateName}</Tooltip>
            </div>
            <div className={classes.nextStateContainer}>
                {allowStateChange && nextState &&
                    <>
                        <button data-tooltip-id={nextStateUniqueid}
                                className={`${classes.stateChangeButton} toolbar-button`}
                                data-test={`next-state-${app.id}-button`}
                                onClick={onMoveToNextState}>
                            &#9657;
                        </button>
                        <Tooltip id={nextStateUniqueid} delayShow="100" place="top">{t("move-to-next-state", {nextStateName: nextStateName})}</Tooltip>
                    </>
                }
            </div>
        </div>
    )
}

export default memo(States);