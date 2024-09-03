import React, { useState, useContext, useCallback, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useDispatch } from 'react-redux';
import { changeApplicationState } from '../../store/applications/thunks';
import { StatusContext } from '../../App';
import StateCircle from './StateCircle';
import { apiDispatchDecorator, apiDecoratorOptions } from '../../helpers/api';

const styles = createUseStyles({
    wrapper: {
        display: "inline-block",
    }
})

const isRejected = (app) => !!app?.rejection?.rejectedUTC;

const States = ({app, allowStateChange=false}) =>
{
    const classes = styles();
    const dispatch = useDispatch();
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const [selectedState, setSelectedState] = useState(app.states.find((x) => x.isCurrent));
    const sortedStates = app.states.toSorted((a,b) => { return a.seqNo - b.seqNo });

    const onApplicationStateChange = useCallback((e) => {
        let changed =  structuredClone(app);
        changed.states.find((x) => x.isCurrent).isCurrent = false;
        changed.states.find((x) => x.id === e.target.value).isCurrent = true;

        dispatch(apiDispatchDecorator(
            async (dispatch, getState) => await changeApplicationState(dispatch, getState, changed),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => setSelectedState(changed.states.find((x) => x.isCurrent)),
                null, e.target
            )));
    }, [app, dispatch, setLoading, setServerErrorMessage]);

    return(
        <div className={classes.wrapper}>
            {!isRejected(app) &&
                <>
                    <StateCircle color={selectedState.hexColor} />
                    {allowStateChange &&
                        <div className="center">
                            <select onChange={onApplicationStateChange} value={selectedState.id}>
                                {
                                    sortedStates.map((x, idx) => {
                                        if (x.id === selectedState.id)
                                            return <option key={idx} value={x.id}>&#x27A4;&nbsp;{x.name}</option>;
                                        else
                                            return <option key={idx} value={x.id}>&nbsp;&nbsp;&nbsp;{x.name}</option>;
                                    })
                                }
                            </select>
                        </div>
                    }
                    {!allowStateChange && <div className="center">{sortedStates.find((x) => x.isCurrent)?.name}</div>}
                </>
            }
            {isRejected(app) && <StateCircle color="var(--error-text)" />}
        </div>
    )
}

export default memo(States);