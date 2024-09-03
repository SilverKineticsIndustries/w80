import React, { useState, memo } from 'react';
import { useSelector } from 'react-redux';
import ApplicationEdit from './ApplicationEdit';
import ApplicationView from './ApplicationView';
import { selectApplicationById } from '../../store/applications/selectors';

const ApplicationContainer = ({id, ...props}) =>
{
    const [editable, setEditable] = useState();
    const application = useSelector(state => selectApplicationById(state, id));
    const isInEditMode = application.isNew || editable === id;

    return (
        <>
            {!isInEditMode
                ? <ApplicationView id={id} onBeginEdit={(id) => setEditable(id)} {...props} />
                : <ApplicationEdit id={id} onCompleteEdit={() => setEditable(null)} {...props} />}
        </>
    )
}

export default memo(ApplicationContainer);